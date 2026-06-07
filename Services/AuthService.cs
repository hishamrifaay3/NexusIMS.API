using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using NexusIMS.API.DTOs.UserDTOs;
using NexusIMS.API.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace NexusIMS.API.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        public AuthService(UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto model)
        {

            var userExists = await _userManager.FindByEmailAsync(model.Email);
            if (userExists != null)
            {
                return new AuthResponseDto { IsSuccess = false, Message = "البريد الإلكتروني مسجل بالفعل!" };
            }
            var roleExists = await _roleManager.RoleExistsAsync(model.Role);
            if (!roleExists)
            {
                return new AuthResponseDto { IsSuccess = false, Message = "الصلاحية المحددة غير موجودة بالنظام!" };
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                WarehouseId = model.WarehouseId,
                IsActive = true,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return new AuthResponseDto { IsSuccess = false, Message = $"فشل إنشاء الحساب: {errors}" };
            }

            await _userManager.AddToRoleAsync(user, model.Role);

            return new AuthResponseDto
            {
                IsSuccess = true,
                Message = "تم إنشاء حساب الموظف بنجاح.",
                FullName = user.FullName,
                Role = model.Role,
                WarehouseId = model.WarehouseId
            };
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return new AuthResponseDto { IsSuccess = false, Message = "خطأ في البريد الإلكتروني أو كلمة المرور!" };
            }

            if (!user.IsActive)
            {
                return new AuthResponseDto 
                { IsSuccess = false, Message = "هذا الحساب موقوف حالياً، راجع مدير النظام." };
            }

  
            var isPasswordValid = await _userManager.CheckPasswordAsync(user, model.Password);
            if (!isPasswordValid)
            {
                return new AuthResponseDto 
                { IsSuccess = false, Message = "خطأ في البريد الإلكتروني أو كلمة المرور!" };
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            var mainRole = userRoles.FirstOrDefault();

  
            var token = await GenerateJwtTokenAsync(user, mainRole!);

            return new AuthResponseDto
            {
                IsSuccess = true,
                Message = "تم تسجيل الدخول بنجاح.",
                Token = token,
                FullName = user.FullName,
                Role = mainRole!,
                WarehouseId = user.WarehouseId
            };
        }

        private async Task<string> GenerateJwtTokenAsync(ApplicationUser user , string role)
        {
            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier,user.Id),
                new Claim(ClaimTypes.Email,user.Email!),
                new Claim("FullName",user.Id),
                new Claim(ClaimTypes.Role,role),
                new Claim("WarehouseId", user.WarehouseId?.ToString() ?? "")
            };
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));

            var token = new JwtSecurityToken(
                    issuer: _configuration["Jwt:Issuer"],
                    audience: _configuration["Jwt:Audience"],
                    expires: DateTime.UtcNow.AddDays(Convert.ToDouble(_configuration["Jwt:DurationInDays"])),
                    claims: authClaims,
                    signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }


    }
}
