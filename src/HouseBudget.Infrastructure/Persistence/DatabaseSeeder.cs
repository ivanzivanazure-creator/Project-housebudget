using HouseBudget.Domain.Entities;
using HouseBudget.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace HouseBudget.Infrastructure.Persistence;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        if (await context.Categories.AnyAsync(c => c.IsSystem)) return;

        var categories = new[]
        {
            Category.CreateSystem("Salary", CategoryType.Income, "#4CAF50", "work"),
            Category.CreateSystem("Freelance", CategoryType.Income, "#8BC34A", "laptop"),
            Category.CreateSystem("Investment", CategoryType.Income, "#009688", "trending_up"),
            Category.CreateSystem("Business", CategoryType.Income, "#2196F3", "business"),
            Category.CreateSystem("Rental", CategoryType.Income, "#3F51B5", "home"),
            Category.CreateSystem("Other Income", CategoryType.Income, "#607D8B", "attach_money"),
            Category.CreateSystem("Housing", CategoryType.Expense, "#F44336", "home"),
            Category.CreateSystem("Utilities", CategoryType.Expense, "#FF9800", "bolt"),
            Category.CreateSystem("Groceries", CategoryType.Expense, "#CDDC39", "shopping_cart"),
            Category.CreateSystem("Dining Out", CategoryType.Expense, "#FF5722", "restaurant"),
            Category.CreateSystem("Transportation", CategoryType.Expense, "#9C27B0", "directions_car"),
            Category.CreateSystem("Healthcare", CategoryType.Expense, "#E91E63", "local_hospital"),
            Category.CreateSystem("Education", CategoryType.Expense, "#673AB7", "school"),
            Category.CreateSystem("Entertainment", CategoryType.Expense, "#00BCD4", "movie"),
            Category.CreateSystem("Shopping", CategoryType.Expense, "#FF4081", "shopping_bag"),
            Category.CreateSystem("Personal Care", CategoryType.Expense, "#EC407A", "spa"),
            Category.CreateSystem("Insurance", CategoryType.Expense, "#78909C", "security"),
            Category.CreateSystem("Subscriptions", CategoryType.Expense, "#29B6F6", "subscriptions"),
            Category.CreateSystem("Travel", CategoryType.Expense, "#26A69A", "flight"),
            Category.CreateSystem("Gifts & Donations", CategoryType.Expense, "#EF5350", "card_giftcard"),
            Category.CreateSystem("Savings", CategoryType.Expense, "#66BB6A", "savings"),
            Category.CreateSystem("Debt Payment", CategoryType.Expense, "#FFA726", "payment"),
            Category.CreateSystem("Pets", CategoryType.Expense, "#8D6E63", "pets"),
            Category.CreateSystem("Other Expense", CategoryType.Expense, "#BDBDBD", "more_horiz"),
            Category.CreateSystem("Transfer", CategoryType.Both, "#90A4AE", "swap_horiz")
        };

        await context.Categories.AddRangeAsync(categories);
        await context.SaveChangesAsync();
    }
}
