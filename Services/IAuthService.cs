using NexusIMS.API.DTOs.UserDTOs;

namespace NexusIMS.API.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterDto model);
        Task<AuthResponseDto> LoginAsync(LoginDto model);
    }
}
