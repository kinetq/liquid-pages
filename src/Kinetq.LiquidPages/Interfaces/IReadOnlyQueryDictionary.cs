namespace Kinetq.LiquidPages.Interfaces;

// In Kinetq.LiquidPages.Abstractions
public interface IReadOnlyQueryDictionary
{
    string? this[string key] { get; }
    bool TryGetValue(string key, out string? value);
    bool ContainsKey(string key);
}