using ErrorOr;

namespace BidPulse.Domain.Entities;

public sealed class Category : Entity
{
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;

    private Category()
    { }//ef

    private Category(Guid id, string name, string description) : base(id)
    {
        Name = name;
        Description = description;
    }

    public static ErrorOr<Category> Create(string name, string description)
    {
        var validationResult = ValidateInvariants(name, description);

        if (validationResult.IsError)
            return validationResult.Errors;
        
        return new Category(Guid.CreateVersion7(), name, description);
    }

    public ErrorOr<Updated> UpdateCategory(string name, string description)
    {
        var validationResult = ValidateInvariants(name, description);

        if (validationResult.IsError)
            return validationResult.Errors;
        
        Name = name;
        Description = description;

        return Result.Updated;
    }

    private static ErrorOr<Success> ValidateInvariants(string name, string description)
    {
        var errors = new List<Error>();
        
        if(string.IsNullOrWhiteSpace(name))
            errors.Add(Error.Validation("Category.RequiredName", "Category name cannot be empty"));
        
        if(string.IsNullOrWhiteSpace(description))
            errors.Add(Error.Validation("Category.RequiredDescription", "Category description cannot be empty"));

        return errors.Count > 0 ? errors : Result.Success;
    }
}