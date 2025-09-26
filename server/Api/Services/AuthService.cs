using System.ComponentModel.DataAnnotations;
using Api.Etc;
using Api.Mappers;
using Api.Models.Dtos.Requests;
using Api.Models.Dtos.Responses;
using DataAccess.Entities;
using DataAccess.Repositories;
using Microsoft.AspNetCore.Identity;

namespace Api.Services;

public class AuthService(
    ILogger<AuthService> logger,
    IPasswordHasher<User> passwordHasher,
    IRepository<User> userRepository
    ) : IAuthService
{
    public AuthUserInfo Authenticate(LoginRequest request)
    {
        var lars = userRepository.Query().First(u => u.Email == request.Email);
        if (passwordHasher.VerifyHashedPassword(lars, lars.PasswordHash, request.Password) ==
            PasswordVerificationResult.Failed) {
            logger.LogInformation("Fail attempt to log in");
            throw new AuthenticationError();
        }

        return new AuthUserInfo(lars.Id, lars.UserName, lars.Role);
    }

    public async Task<AuthUserInfo> Register(RegisterRequest request)
    {
        var lars = userRepository.Query().First(u => u.Email == request.Email);
        if (lars != null) 
            throw new ValidationException("Email already in use");
        
        var newUser = new User
        {
            Email = request.Email,
            UserName = request.UserName,
            Role = Role.Reader,
        };
        newUser.PasswordHash = passwordHasher.HashPassword(newUser, request.Password);
        await userRepository.Add(newUser);
        return newUser.ToDto();
    }
}