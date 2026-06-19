namespace Kinetq.LiquidPages.Interfaces;

// In Kinetq.LiquidPages.Abstractions
public interface IReadOnlyHeaderDictionary
{
    string? this[string key] { get; }
    bool TryGetValue(string key, out string? value);
    IEnumerable<KeyValuePair<string, string>> GetAll(); // if needed
}