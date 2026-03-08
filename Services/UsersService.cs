using System;
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
}