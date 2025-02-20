using System.Collections.Generic;

public static class LanguageValidator
{
    private static readonly HashSet<string> LimbiSuportate = new() { "ro", "en" };

    public static bool EsteLimbajSuportat(string limba)
    {
        return !string.IsNullOrEmpty(limba) && LimbiSuportate.Contains(limba.ToLower());
    }
}
