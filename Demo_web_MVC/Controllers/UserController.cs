using Demo_web_MVC.Data.AppDatabase;
using Demo_web_MVC.Models;
using Demo_web_MVC.Models.ViewModel;
using Demo_web_MVC.Service.Sendemail;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System.Security.Claims;
using System.Security.Cryptography;

namespace Demo_web_MVC.Controllers
{

    public class UserController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IEmailServices _emailService;
        private readonly ILogger<UserController> _logger;
        private readonly AppDatabase _context;
        public UserController( IConfiguration configuration,ILogger<UserController> logger, AppDatabase context, IEmailServices emailService)
        {
            _logger = logger;
            _context = context;
            _emailService = emailService;
            _configuration = configuration;
        }
        public IActionResult Index()
        {

            return View();
        }

        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]

        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Model không hợp lệ khi đăng ký.");
                    return View(model);
                }

                if (await _context.Users.AnyAsync(x => x.Username == model.Username))
                {
                    _logger.LogWarning($"Tên đăng nhập đã tồn tại: {model.Username}");
                    ModelState.AddModelError("Username", "Tên đã tồn tại!");
                    return View(model);
                }

                if (await _context.Users.AnyAsync(x => x.Email == model.Email))
                {
                    _logger.LogWarning($"Email đã tồn tại: {model.Email}");
                    ModelState.AddModelError("Email", "Email đã tồn tại!");
                    return View(model);
                }

                var role = await _context.Roles
                    .FirstAsync(r => r.Code == "USER");

                var user = new User
                {
                    Username = model.Username!,
                    Email = model.Email!,
                    FullName = model.FullName,
                    IsActive = false,
                    CreatedAt = DateTime.UtcNow
                };

                var hasher = new PasswordHasher<User>();
                user.PasswordHash = hasher.HashPassword(user, model.Password!);

                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Người dùng {user.Username} đã được tạo thành công.");

                var userRole = new UserRole
                {
                    UserId = user.Id,
                    RoleId = role.Id
                };

                _context.UserRoles.Add(userRole);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Phân quyền cho người dùng {user.Username} thành công.");

                var token = new UserToken
                {
                    UserId = user.Id,
                    Token = Guid.NewGuid().ToString(),
                    Type = TokenType.EmailConfirm,
                    ExpiredAt = DateTime.UtcNow.AddHours(24),
                    CreatedAt = DateTime.UtcNow
                };

                _context.userTokens.Add(token);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Token xác nhận email cho người dùng {user.Username} đã được tạo.");


                var baseUrl = _configuration["AppSettings:BaseUrl"];

                var confirmUrl =
                    $"{baseUrl}/User/Confirm?token={token.Token}";

                await _emailService.SendEmailAsync(
                    user.Email,
                    "Xác thực tài khoản",
                    $@"
                    <p>Xin chào {user.FullName},</p>
                    <p>Vui lòng nhấn vào liên kết bên dưới để xác thực tài khoản:</p>
                    <a href='{confirmUrl}'>Xác thực tài khoản</a>
                    "
                );

                _logger.LogInformation("Email xác nhận tài khoản đã được gửi đến {Email}", user.Email);
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Đã xảy ra lỗi trong quá trình đăng ký tài khoản.");
                _logger.LogError(ex.Message, ex);
                return StatusCode(500, "Đã xảy ra lỗi hệ thống, vui lòng thử lại sau.");
            }
        }

        public async Task<IActionResult> Confirm(string token)
        {
            try
            {
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("Token không được để trống");
                    return BadRequest("Token không được để trống");
                }

                var tokenEntity = await _context.userTokens
                    .Include(t => t.User)
                    .FirstOrDefaultAsync(t =>
                        t.Token == token &&
                        t.Type == TokenType.EmailConfirm &&
                        !t.IsUsed &&
                        t.ExpiredAt > DateTime.UtcNow
                    );

                if (tokenEntity == null)
                {
                    _logger.LogWarning($"Token không hợp lệ: {token}");
                    return BadRequest("Token không hợp lệ");
                }

                // Xác thực email thành công
                tokenEntity.User.IsActive = true;
                tokenEntity.User.EmailConfirmedAt = DateTime.UtcNow;

                tokenEntity.IsUsed = true;
                tokenEntity.UsedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Xác thực email thành công cho người dùng: {tokenEntity.User.Username}");
                return RedirectToAction("Login", "User");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Đã xảy ra lỗi khi xác thực token email.");
                return StatusCode(500, "Đã xảy ra lỗi hệ thống, vui lòng thử lại sau.");
            }
        }

        public IActionResult Login()
        {

            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {

            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Đăng nhập không hợp lệ do dữ liệu không hợp lệ.");
                    return View(model);
                }

                var input = model.UsernameOrEmail!.ToLower();
                // 1. Tìm user trong DB
                var user = await _context.Users
                     .Include(u => u.UserRoles)
                         .ThenInclude(ur => ur.Role)
                     .FirstOrDefaultAsync(x =>
                         x.Username == input ||
                         x.Email == input);

                if (user == null)
                {
                    _logger.LogWarning($"Không tìm thấy người dùng với tên đăng nhập hoặc email: {input}");
                    ModelState.AddModelError("", "Sai tên đăng nhập hoặc mật khẩu");
                    return View(model);
                }

                if (!user.IsActive)
                {
                    _logger.LogWarning($"Tài khoản không kích hoạt: {input}");
                    ModelState.AddModelError("", "Tài khoản chưa được kích hoạt, vui lòng kiểm tra email");
                    return View(model);
                }

                // 2. Check lockout
                if (user.LockoutUntil != null && user.LockoutUntil > DateTime.UtcNow)
                {
                    _logger.LogWarning($"Tài khoản bị khóa: {input} đến {user.LockoutUntil:HH:mm:ss}");
                    ModelState.AddModelError("",
                        $"Tài khoản bị khóa đến {user.LockoutUntil:HH:mm:ss}");
                    return View("Lockout");
                }

                // 3. Verify password
                var hasher = new PasswordHasher<User>();
                var result = hasher.VerifyHashedPassword(
                    user,
                    user.PasswordHash,
                    model.Password!
                );

                // 4. Sai mật khẩu
                if (result != PasswordVerificationResult.Success &&
                    result != PasswordVerificationResult.SuccessRehashNeeded)
                {
                    user.FailedLoginCount++;

                    if (user.FailedLoginCount >= 3)
                    {
                        user.LockoutUntil = DateTime.UtcNow.AddMinutes(5);
                        user.FailedLoginCount = 0;
                        _logger.LogWarning($"Tài khoản {input} bị khóa do nhập sai mật khẩu quá 3 lần.");
                    }

                    await _context.SaveChangesAsync();

                    if (user.LockoutUntil != null && user.LockoutUntil > DateTime.UtcNow)
                    {
                        return View("Lockout");
                    }

                    _logger.LogWarning($"Sai tên đăng nhập hoặc mật khẩu cho {input}");
                    ModelState.AddModelError("", "Sai tên đăng nhập hoặc mật khẩu");
                    return View(model);
                }

                // 5. Đăng nhập thành công → reset lockout
                user.FailedLoginCount = 0;
                user.LockoutUntil = null;
                await _context.SaveChangesAsync();

                var roleCode = user.UserRoles
                       .Select(ur => ur.Role.Code)
                       .FirstOrDefault();

                if (roleCode == null)
                {
                    _logger.LogWarning($"Tài khoản {input} chưa được phân quyền.");
                    ModelState.AddModelError("", "Tài khoản chưa được phân quyền");
                    return View(model);
                }

                // 6. Sign in
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim("FullName", user.FullName ?? ""),
                };

                foreach (var role in user.UserRoles.Select(ur => ur.Role.Code))
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }

                var identity = new ClaimsIdentity(
                    claims,
                    CookieAuthenticationDefaults.AuthenticationScheme
                );
                var principal = new ClaimsPrincipal(identity);
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(identity)
                );

                // Ghi lại các claims khi đăng nhập thành công
                foreach (var claim in principal.Claims)
                {
                    _logger.LogInformation("Claim Type: {Type}, Value: {Value}", claim.Type, claim.Value);
                }

                _logger.LogInformation($"Đăng nhập thành công cho người dùng: {input}");

                if (user.DateOfBirth == null)
                {
                    return RedirectToAction("UpdateBirthday", "User");
                }
                return RedirectToAction("Index", "Product");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Đã xảy ra lỗi trong quá trình đăng nhập.");
                ModelState.AddModelError("", "Đã xảy ra lỗi trong quá trình đăng nhập, vui lòng thử lại sau.");
                return View(model); // Trang lỗi chung khi có exception xảy ra
            }
        }
        public IActionResult ForgotPassword()
        {
            return View("ForgotPassword");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(string email)
        {

            try
            {
                if (string.IsNullOrEmpty(email))
                {
                    _logger.LogWarning("Tham số email trống hoặc null.");
                    return View();
                }

                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == email);

                if (user == null)
                {
                    _logger.LogWarning($"Không tìm thấy người dùng với email: {email}");
                    return View("ForgotPasswordConfirmation");
                }

                if (!user.IsActive)
                {
                    _logger.LogWarning($"Người dùng với email {email} không hoạt động.");
                    return View("ForgotPasswordConfirmation");
                }

                if (user.UpdatedAt > DateTime.UtcNow.AddMinutes(-5))
                {
                    _logger.LogWarning($"Người dùng với email {email} đã yêu cầu mật khẩu mới trong vòng 5 phút qua.");
                    return View("ForgotPasswordConfirmation");
                }

                // 1. Sinh mật khẩu mới
                string newPassword = GeneratePassword(10);

                // 2. Hash và cập nhật mật khẩu
                var hasher = new PasswordHasher<User>();
                user.PasswordHash = hasher.HashPassword(user, newPassword);
                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // 3. Gửi email với mật khẩu mới
                await _emailService.SendEmailAsync(
                    user.Email,
                    "Mật khẩu mới của bạn",
                    $@"
                    <p>Xin chào {user.FullName},</p>
                    <p>Với tài khoản {user.Username} và Email là : {user.Email}</p>
                    <p>Bạn đã yêu cầu đặt lại mật khẩu. </p>
                    <p>Mật khẩu mới của bạn là:</p>
                    <h3>{newPassword}</h3>
                    <p>Vui lòng đăng nhập và đổi mật khẩu ngay.</p>
                    "
                );

                _logger.LogInformation($"Mật khẩu mới đã được gửi thành công tới email {email}.");
                return RedirectToAction("Login", "User");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Đã xảy ra lỗi trong quá trình xử lý yêu cầu quên mật khẩu.");
                return View("Error"); // Trang lỗi chung khi có exception xảy ra
            }
        }
        public static string GeneratePassword(int length = 10)
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789@#$!";
            var bytes = new byte[length];

            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);

            return new string(bytes.Select(b => chars[b % chars.Length]).ToArray());
        }
       
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            return RedirectToAction("Login", "User");
        }
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return PartialView("ChangePassword");
        }
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model) 
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ChangePassword failed. ModelState is invalid.");
                return View(model);

            }
            var hasher = new PasswordHasher<User>();

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                _logger.LogWarning("User not authenticated. UserId is null.");
                return NotFound();

            }

                
            var user = await _context.Users.FindAsync(int.Parse(userId));
            if (user == null)
            {
                _logger.LogWarning($"User not found for UserId: {userId}");
                return NotFound();
            }
                
            var verify = hasher.VerifyHashedPassword(
                    user,
                    user.PasswordHash,
                    model.OldPassword!
                );

            if (verify == PasswordVerificationResult.Failed)
            {
                _logger.LogWarning($"Password verification failed for UserId: {userId}");
                ModelState.AddModelError("", "Mật khẩu hiện tại không đúng");
                return View(model);
            }
            if (model.OldPassword == model.NewPassword)
            {
                _logger.LogWarning($"New password is the same as the old password for UserId: {userId}");
                ModelState.AddModelError("", "Mật khẩu mới phải khác mật khẩu hiện tại");
                return View(model);
            }

            user.PasswordHash = hasher.HashPassword(user, model.NewPassword!);
                user.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();
            _logger.LogInformation($"Password changed successfully for UserId: {userId}");
            return RedirectToAction("Index", "Home");
        }
        [HttpGet]
        public IActionResult UpdateBirthday()
        {
            return View();
        }
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> UpdateBirthday(UpdateBirthdayViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = GetUserIdFromClaims();

            if (userId == null)
            {
                return RedirectToAction("Login", "User");
            }

            var user = await _context.Users.FindAsync(userId.Value);

            if (user == null)
            {
                return NotFound();
            }

            // Nếu đã có ngày sinh thì không cho đổi lại
            if (user.DateOfBirth != null)
            {
                return RedirectToAction("Index", "Product");
            }

            // Lưu ngày sinh vào DB
            user.DateOfBirth = model.DateOfBirth;
            user.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Product");
        }
        private int? GetUserIdFromClaims()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return null;
            }
            return userId;
        }
    }
}
