using H1bConnect_Backend.Models.Entities.DTO;

namespace H1bConnect_Backend.Repository.IServices
{
    public interface IAuthService
    {
        Task<bool> RegisterUserAsync(SignUpDTO signUpDTO);
        Task<string> AuthenticateUserAsync(LoginDTO loginDTO, IConfiguration configuration);
    }
}
