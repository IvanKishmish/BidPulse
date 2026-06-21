using System.Text.RegularExpressions;
using BidPulse.Domain.Entities.Models;
using BidPulse.Domain.Enums;
using ErrorOr;

namespace BidPulse.Domain.Entities;

public sealed class User : Entity
{
    public string NickName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public decimal Balance { get; private set; }
    public Role Role { get; private set; } = Role.User;
    
    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$", 
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private User(){}//ef

    private User(Guid id, UserCreationParams args)
    : base(id)
    {
        NickName = args.NickName;
        Email = args.Email;
        PasswordHash = args.PasswordHash;
        Balance = args.Balance;
        Role = args.Role;
    }

    public static ErrorOr<User> Create(UserCreationParams args)
    {
        var validationResult = ValidateInvariants(args.NickName, args.Email);
        if (validationResult.IsError)
            return validationResult.Errors;
        
        if (string.IsNullOrWhiteSpace(args.PasswordHash))
            return Error.Validation("User.PasswordRequired", "Password hash cannot be empty.");

        return new User(Guid.CreateVersion7(), args);
    }

    public ErrorOr<Updated> UpdateProfile(string name, string email)
    {
        var validationResult = ValidateInvariants(name, email);
        if (validationResult.IsError)
            return validationResult.Errors;

        NickName = name;
        Email = email;

        return Result.Updated;
    }

    public ErrorOr<Updated> ChangeRole(Role newRole)
    {
        Role = newRole;
            
        return Result.Updated;
    }
    
    public ErrorOr<Updated> AddToBalance(decimal amount)
    {
        if (amount <= 0)
            return Error.Validation("User.InvalidAmount", "Amount to add must be greater than zero.");
 
        Balance += amount;
        return Result.Updated;
    }
    
    public ErrorOr<Updated> DeductFromBalance(decimal amount)
    {
        if (amount <= 0)
            return Error.Validation("User.InvalidAmount", "Amount to deduct must be greater than zero.");
 
        if (Balance < amount)
            return Error.Validation("User.InsufficientFunds", "Insufficient balance to complete the operation.");
 
        Balance -= amount;
        return Result.Updated;
    }
    
    private static ErrorOr<Success> ValidateInvariants(string name, string email)
    {
        var errors = new List<Error>();
        
        if (string.IsNullOrWhiteSpace(name) || name.Length < 6)
            errors.Add(Error.Validation("User.InvalidName", "User name must be at least 6 characters long."));

        if (string.IsNullOrWhiteSpace(email) || !EmailRegex.IsMatch(email))
            errors.Add(Error.Validation("User.InvalidEmail", "A valid email address is required."));

        return errors.Count > 0 ? errors : Result.Success;
    }
}