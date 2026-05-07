using System.Globalization;

namespace HouseBudget.Mobile.Converters;

/// <summary>
/// Converts a bool to one of two colors passed as ConverterParameter="TrueColor|FalseColor".
/// Accepts hex strings (e.g. "#FF0000") or named MAUI colors (e.g. "Red", "Transparent").
/// </summary>
public sealed class BoolToColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var parts = (parameter as string ?? "").Split('|');
        string pick = (value is true) ? (parts.ElementAtOrDefault(0) ?? "") : (parts.ElementAtOrDefault(1) ?? "");
        return ParseColor(pick) ?? Colors.Transparent;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();

    private static Color? ParseColor(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return null;
        if (s.StartsWith('#')) return Color.FromArgb(s);
        return s.ToLowerInvariant() switch
        {
            "transparent" => Colors.Transparent,
            "white" => Colors.White,
            "black" => Colors.Black,
            "grey" or "gray" => Colors.Grey,
            _ => Color.FromArgb(s)
        };
    }
}

/// <summary>Inverts a bool value.</summary>
public sealed class InverseBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is not true;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is not true;
}

// Alias so both naming conventions work in XAML
public sealed class InvertedBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is not true;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is not true;
}

/// <summary>Returns true when int > 0, false otherwise.</summary>
public sealed class IntToBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is int i && i > 0;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

/// <summary>Converts a 0-100 percentage to a 0.0-1.0 Progress value.</summary>
public sealed class PercentToProgressConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is decimal d) return (double)Math.Min(d / 100m, 1m);
        if (value is double dbl) return Math.Min(dbl / 100.0, 1.0);
        return 0.0;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

/// <summary>Returns "Income" → green, "Expense" → red, else grey.</summary>
public sealed class TypeToColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => (value as string) switch
        {
            "Income" => Color.FromArgb("#4CAF50"),
            "Expense" => Color.FromArgb("#F44336"),
            _ => Color.FromArgb("#1A1A2E")
        };

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

/// <summary>Returns "+" for Income, "-" for Expense.</summary>
public sealed class TypeToSignConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => (value as string) == "Income" ? "+" : "-";

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

/// <summary>Maps category name to a representative emoji.</summary>
public sealed class CategoryToEmojiConverter : IValueConverter
{
    private static readonly Dictionary<string, string> Map = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Salary"] = "💼", ["Freelance"] = "💻", ["Investment"] = "📈",
        ["Business"] = "🏢", ["Rental"] = "🏠", ["Other Income"] = "💵",
        ["Housing"] = "🏠", ["Utilities"] = "⚡", ["Groceries"] = "🛒",
        ["Dining Out"] = "🍽️", ["Transportation"] = "🚗", ["Healthcare"] = "🏥",
        ["Education"] = "📚", ["Entertainment"] = "🎬", ["Shopping"] = "🛍️",
        ["Personal Care"] = "💆", ["Insurance"] = "🛡️", ["Subscriptions"] = "📱",
        ["Travel"] = "✈️", ["Gifts & Donations"] = "🎁", ["Savings"] = "💰",
        ["Debt Payment"] = "💳", ["Pets"] = "🐾", ["Transfer"] = "🔄"
    };

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => Map.TryGetValue(value as string ?? "", out var emoji) ? emoji : "💸";

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

/// <summary>Converts a bool to one of two strings. ConverterParameter="TrueText|FalseText".</summary>
public sealed class BoolToStringConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var parts = (parameter as string ?? "|").Split('|');
        return (value is true) ? (parts.ElementAtOrDefault(0) ?? "") : (parts.ElementAtOrDefault(1) ?? "");
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

/// <summary>Returns true when string is non-empty.</summary>
public sealed class StringToBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => !string.IsNullOrEmpty(value as string);

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

/// <summary>Returns "Get Started" for price 0, else "Subscribe".</summary>
public sealed class PriceToActionConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is decimal d && d == 0 ? "Get Started" : "Subscribe";

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

/// <summary>Returns a brand color per subscription tier name.</summary>
public sealed class TierToColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => (value as string) switch
        {
            "Free" => Color.FromArgb("#607D8B"),
            "Premium" => Color.FromArgb("#E94560"),
            "Business" => Color.FromArgb("#9C27B0"),
            _ => Color.FromArgb("#1A1A2E")
        };

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
