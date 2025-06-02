using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SmartInventoryApi.DTOs; // Đảm bảo bạn đã có ForgotPasswordRequest, ForgotPasswordResponse, ResetPasswordRequest
using SmartInventoryApi.Models;
using SmartInventoryApi.Services;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http; // Added for StatusCodes

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly InventoryManagementDbContext _context;
    private readonly JwtSettings _jwtSettings;

    public AuthController(
        IUserService userService,
        InventoryManagementDbContext context,
        IOptions<JwtSettings> jwtSettings)
    {
        _userService = userService;
        _context = context;
        _jwtSettings = jwtSettings.Value;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(u =>
                u.Username == request.Username && u.IsActive);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
                return Unauthorized("Tên đăng nhập hoặc mật khẩu không hợp lệ");

            var token = GenerateJwtToken(user);

            return Ok(new LoginResponse
            {
                Token = token,
                FullName = user.FullName,
                Role = user.UserRole
            });
        }
        catch (Exception ex)
        {
            // Ghi log lỗi ở đây nếu cần
            // Consider using a proper logging framework, e.g., Serilog or Microsoft.Extensions.Logging
            Console.WriteLine($"Login error: {ex.ToString()}");
            return StatusCode(StatusCodes.Status500InternalServerError, "Lỗi máy chủ nội bộ");
        }
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] CreateUserDto createUserDto)
    {
        try
        {
            var user = await _userService.CreateUserAsync(createUserDto);
            // Trả về thông tin user vừa tạo, thường không bao gồm mật khẩu
            return CreatedAtAction(nameof(GetProfile), new { id = user.Id }, new { user.Id, user.Username, user.FullName, user.Email, user.UserRole });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            // Ghi log lỗi ở đây nếu cần
            Console.WriteLine($"Register error: {ex.ToString()}");
            return StatusCode(StatusCodes.Status500InternalServerError, "Lỗi máy chủ nội bộ");
        }
    }

    [HttpGet("profile")]
    [Authorize]
    public async Task<IActionResult> GetProfile()
    {
        try
        {
            // Lấy username từ claims của token JWT đã xác thực
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized("Không thể xác định người dùng từ token.");
            }

            var user = await _userService.GetCurrentUserAsync(username);
            return Ok(user);
        }
        catch (KeyNotFoundException)
        {
            return NotFound("Người dùng không tồn tại");
        }
        catch (Exception ex)
        {
            // Ghi log lỗi ở đây nếu cần
            Console.WriteLine($"GetProfile error: {ex.ToString()}");
            return StatusCode(StatusCodes.Status500InternalServerError, "Lỗi máy chủ nội bộ");
        }
    }

    [HttpPut("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
    {
        try
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var userId))
            {
                return Unauthorized("Không thể xác định ID người dùng từ token.");
            }

            await _userService.ChangePasswordAsync(userId, changePasswordDto);
            return NoContent(); // Thành công, không có nội dung trả về
        }
        catch (KeyNotFoundException)
        {
            return NotFound("Người dùng không tồn tại");
        }
        catch (UnauthorizedAccessException ex) // Lỗi này được ném từ UserService nếu mật khẩu hiện tại sai
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            // Ghi log lỗi ở đây nếu cần
            Console.WriteLine($"ChangePassword error: {ex.ToString()}");
            return StatusCode(StatusCodes.Status500InternalServerError, "Lỗi máy chủ nội bộ");
        }
    }

    // Endpoint yêu cầu gửi OTP để quên mật khẩu
    [HttpPost("forgot-password")]
    [AllowAnonymous] // Cho phép truy cập mà không cần token
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _userService.ForgotPasswordAsync(request.Email);

            // Luôn trả về thông báo thành công chung chung để tránh lộ thông tin email có tồn tại hay không
            return Ok(new ForgotPasswordResponse
            {
                Success = true,
                Message = "Nếu email của bạn tồn tại trong hệ thống, một mã OTP sẽ được gửi đến."
            });
        }
        catch (Exception ex)
        {
            // Ghi log lỗi ở đây nếu cần
            Console.WriteLine($"Lỗi khi yêu cầu quên mật khẩu: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Đã xảy ra lỗi khi xử lý yêu cầu của bạn.", details = ex.Message });
        }
        // Added to satisfy compiler error CS0161: "not all code paths return a value".
        // This line should theoretically be unreachable if the try-catch block is exhaustive.
        // However, to ensure the method always returns a value as per its signature Task<IActionResult>.
        return StatusCode(StatusCodes.Status500InternalServerError, "Lỗi không mong muốn: Luồng xử lý đã thoát khỏi try-catch trong ForgotPassword.");
    }

    // Endpoint đặt lại mật khẩu bằng OTP
    [HttpPost("reset-password")]
    [AllowAnonymous] // Cho phép truy cập mà không cần token
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request) // Sử dụng ResetPasswordRequest đã cập nhật với Otp
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Truyền request.Otp (đã đổi tên từ Token trong DTO)
            var result = await _userService.ResetPasswordAsync(request.Email, request.Otp, request.NewPassword);

            if (!result)
            {
                return BadRequest(new { message = "OTP không hợp lệ, đã hết hạn hoặc email không đúng." });
            }

            return Ok(new { message = "Mật khẩu của bạn đã được đặt lại thành công." });
        }
        catch (Exception ex)
        {
            // Ghi log lỗi ở đây nếu cần
            Console.WriteLine($"Lỗi khi đặt lại mật khẩu: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Đã xảy ra lỗi khi đặt lại mật khẩu.", details = ex.Message });
        }
    }

    // Endpoint này không còn cần thiết cho quy trình OTP, có thể xóa hoặc đánh dấu là không dùng nữa.
    // Nếu bạn muốn giữ lại để tương thích ngược hoặc mục đích khác, hãy cân nhắc.
    // Hiện tại, tôi sẽ trả về lỗi 404 để cho biết nó không được sử dụng.
    [HttpPost("validate-reset-token")]
    [AllowAnonymous]
    public IActionResult ValidateResetToken([FromBody] ValidateTokenRequest request) // DTO này có thể cần được cập nhật hoặc loại bỏ
    {
        // Vì chúng ta dùng OTP, endpoint này có thể không còn phù hợp.
        // Bạn có thể tạo một endpoint mới `validate-otp` nếu cần thiết.
        return NotFound(new { message = "Endpoint này không còn được sử dụng trong quy trình đặt lại mật khẩu bằng OTP." });
    }


    // Phương thức private để tạo JWT token
    private string GenerateJwtToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtSettings.Key);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username), // Thường dùng Username cho ClaimTypes.Name
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.UserRole)
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpireMinutes),
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
