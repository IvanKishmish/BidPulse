namespace BidPulse.Contracts.Categories;

public sealed record CreateCategoryRequest(
    string Name,
    string Description);