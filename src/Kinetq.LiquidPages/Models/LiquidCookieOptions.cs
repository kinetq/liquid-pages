namespace Kinetq.LiquidPages.Models;

public sealed class LiquidCookieOptions
{
    public string? Domain { get; init; }

    public string? Path { get; init; }

    public DateTimeOffset? Expires { get; init; }

    public bool Secure { get; init; }

    public LiquidSameSiteMode SameSite { get; init; } = LiquidSameSiteMode.None;

    public bool HttpOnly { get; init; }

    public TimeSpan? MaxAge { get; init; }

    public bool IsEssential { get; init; }
}

public enum LiquidSameSiteMode
{
    Unspecified,
    None,
    Lax,
    Strict
}
