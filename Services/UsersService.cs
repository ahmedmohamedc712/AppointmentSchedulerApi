using System;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using AppointmentScheduler.Data;
using AppointmentScheduler.Data.DTOs;
using AppointmentScheduler.Exceptions;
using AppointmentScheduler.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using static AppointmentScheduler.Controllers.UsersController;

namespace AppointmentScheduler.Services;

public class UsersService(AppDbContext context,
    IPasswordHasher passwordHasher,
    IJwtProvider jwtProvider,
    ICurrentUserAccessor currentUserAccessor) : IUsersService
{
    public async Task<Response> Signup(SignupRequest request)
    {
        var isValidEmail = IsValidEmail(request.Email);
        if(!isValidEmail)
            throw new BadRequestException("Invalid Email format.");  
        
        if(request.Password != request.ConfirmPassword)
            throw new BadRequestException("Confirm password must match the password.");

        var isFound = await context.Users.AnyAsync(x => x.Email == request.Email);
        if(isFound)
            throw new ConflictException("There is already an account using this Email.");
        
        var hashedPassword = passwordHasher.HashPassword(request.Password);

        User user = new()
        {
            Name = request.Name,
            Email = request.Email,
            PasswordHashed = hashedPassword
        };  
        await context.Users.AddAsync(user);

        var jwtToken = jwtProvider.Create(user);
        var refreshToken = jwtProvider.GenerateRefreshToken();

        var refreshTokenObj = new RefreshToken()
        {
            Token = refreshToken,
            Expires = DateTime.UtcNow.AddDays(7),
            User = user
        };

        await context.RefreshTokens.AddAsync(refreshTokenObj);
        await context.SaveChangesAsync();

        return new Response(jwtToken, refreshToken);
    }
    public async Task<Response> Login(LoginRequest request)
    {
        var isValidEmail = IsValidEmail(request.Email);
        if(!isValidEmail)
            throw new BadRequestException("Invalid Email format.");
        
        var user = await context.Users.FirstOrDefaultAsync(x => x.Email == request.Email);
        if(user == null || !passwordHasher.VerifyPassword(user.PasswordHashed, request.Password))
        {
            throw new BadRequestException("Invalid Credentials.");
        }

        var jwtToken = jwtProvider.Create(user);
        var refreshToken = jwtProvider.GenerateRefreshToken();

        var refreshTokenObj = new RefreshToken()
        {
            Token = refreshToken,
            Expires = DateTime.UtcNow.AddDays(7),
            User = user
        };

        await context.RefreshTokens.AddAsync(refreshTokenObj);
        await context.SaveChangesAsync();

        return new Response(jwtToken, refreshToken);
    }

    public async Task<Response> LoginUserWithRefreshToken(string refreshToken)
    {
        var refreshTokenObj = await context.RefreshTokens
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Token == refreshToken);

        if (refreshTokenObj == null || refreshTokenObj.Expires < DateTime.UtcNow)
        {
            throw new BadRequestException("Refresh token has expired.");
        }
        var accessToken = jwtProvider.Create(refreshTokenObj.User);

        refreshTokenObj.Token = jwtProvider.GenerateRefreshToken();
        refreshTokenObj.Expires = DateTime.UtcNow.AddDays(7);

        await context.SaveChangesAsync();

        return new Response(accessToken, refreshTokenObj.Token);
    }
    public async Task RevokeRefreshTokens(int userId)
    {
        if(userId != currentUserAccessor.GetCurrentUserId())
        {
            throw new BadRequestException("You can't do this.");
        }
        await context.RefreshTokens
            .Where(x => x.UserId == userId)
            .ExecuteDeleteAsync();
    }
    private bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}