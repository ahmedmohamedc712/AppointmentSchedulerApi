using System;
using System.Runtime.CompilerServices;
using AppointmentScheduler.Data;
using AppointmentScheduler.Data.DTOs;
using AppointmentScheduler.Exceptions;
using AppointmentScheduler.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace AppointmentScheduler.Services;

public class UsersService(AppDbContext context, IPasswordHasher passwordHasher, IJwtProvider jwtProvider) : IUsersService
{
    public async Task<string> Signup(SignupRequest request)
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
        await context.SaveChangesAsync();

        return jwtProvider.Create(user);
    }
    public async Task<string> Login(LoginRequest request)
    {
        var isValidEmail = IsValidEmail(request.Email);
        if(!isValidEmail)
            throw new BadRequestException("Invalid Email format.");
        
        var user = await context.Users.FirstOrDefaultAsync(x => x.Email == request.Email);
        if(user == null || !passwordHasher.VerifyPassword(user.PasswordHashed, request.Password))
        {
            throw new BadRequestException("Invalid Credentials.");
        }

        return jwtProvider.Create(user);
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