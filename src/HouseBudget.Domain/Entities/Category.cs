using HouseBudget.Domain.Enums;
using HouseBudget.Domain.Exceptions;

namespace HouseBudget.Domain.Entities;

public sealed class Category : BaseEntity
{
    private readonly List<Category> _subCategories = new();

    public Guid? UserId { get; private set; }
    public Guid? ParentCategoryId { get; private set; }
    public string Name { get; private set; } = default!;
    public string? Description { get; private set; }
    public CategoryType Type { get; private set; }
    public string Color { get; private set; } = "#607D8B";
    public string? IconName { get; private set; }
    public bool IsSystem { get; private set; }
    public int SortOrder { get; private set; }

    public Category? ParentCategory { get; private set; }
    public IReadOnlyCollection<Category> SubCategories => _subCategories.AsReadOnly();

    private Category() { }

    public static Category CreateSystem(string name, CategoryType type, string color, string? iconName = null)
    {
        return new Category
        {
            Name = name,
            Type = type,
            Color = color,
            IconName = iconName,
            IsSystem = true
        };
    }

    public static Category CreateForUser(Guid userId, string name, CategoryType type, string color, string? iconName = null, Guid? parentCategoryId = null)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new DomainException("Category name is required.");
        return new Category
        {
            UserId = userId,
            Name = name.Trim(),
            Type = type,
            Color = color,
            IconName = iconName,
            ParentCategoryId = parentCategoryId
        };
    }

    public void Update(string name, string? description, string color, string? iconName, CategoryType type)
    {
        if (IsSystem) throw new DomainException("Cannot modify a system category.");
        if (string.IsNullOrWhiteSpace(name)) throw new DomainException("Category name is required.");
        Name = name.Trim();
        Description = description;
        Color = color;
        IconName = iconName;
        Type = type;
        MarkUpdated();
    }
}
