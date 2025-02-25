﻿using System.Collections.Generic;

public static class LanguageValidator
{
    private static readonly HashSet<string> LimbiSuportate = new() { "ro", "en" };

    public static bool EsteLimbajSuportat(string language)
    {
        return !string.IsNullOrEmpty(language) && LimbiSuportate.Contains(language.ToLower());
    }
}
