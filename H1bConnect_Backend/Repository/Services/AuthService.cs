using H1bConnect_Backend.JwtToken;
using H1bConnect_Backend.Models.Entities.DTO;
using H1bConnect_Backend.Models.Entities;
using MongoDB.Driver;
using H1bConnect_Backend.Data;
using H1bConnect_Backend.Repository.IServices;

namespace H1bConnect_Backend.Repository.Services
{
    public class AuthService : IAuthService
    {
        private readonly IMongoCollection<User> _users;

        public AuthService(QuadrantDbContext dbContext)
        {
            _users = dbContext.Users;

            // Ensure a unique index on the Email field
            var indexKeys = Builders<User>.IndexKeys.Ascending(u => u.Email);
            var indexOptions = new CreateIndexOptions { Unique = true };
            var model = new CreateIndexModel<User>(indexKeys, indexOptions);
            _users.Indexes.CreateOne(model);
        }

        public async Task<bool> RegisterUserAsync(SignUpDTO signUpDTO)
        {
            if (signUpDTO.Password != signUpDTO.ConfirmPassword)
            {
                throw new ArgumentException("Passwords do not match.");
            }

            var existingUser = await _users.Find(u => u.Email == signUpDTO.Email).FirstOrDefaultAsync();
            if (existingUser != null)
            {
                throw new InvalidOperationException("User already exists.");
            }

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(signUpDTO.Password);
            var user = new User
            {
                Email = signUpDTO.Email,
                PasswordHash = hashedPassword
            };

            await _users.InsertOneAsync(user);
            return true;
        }

        public async Task<string> AuthenticateUserAsync(LoginDTO loginDTO, IConfiguration configuration)
        {
            var user = await _users.Find(u => u.Email == loginDTO.Email).FirstOrDefaultAsync();
            if (user == null || !BCrypt.Net.BCrypt.Verify(loginDTO.Password, user.PasswordHash))
            {
                throw new UnauthorizedAccessException("Invalid email or password.");
            }

            return JwtTokenHelper.GenerateToken(user, configuration);
        }
    }

}
