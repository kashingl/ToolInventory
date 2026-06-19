namespace ToolInventory.API.Common;

public static class InputNormalizer
{
    public static string NormalizeName(string value) => value.Trim();

    public static string? NormalizeOptionalText(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim();
    }

    public static string? NormalizeBarcode(string? value)
        => NormalizeOptionalText(value)?.ToUpperInvariant();
}
