using AspireLearning.ServiceDefaults.GlobalEnum;

namespace AspireLearning.ServiceDefaults.GlobalUtility;

public static class LanguageParser
{
    public static LanguageEnum Parse(string acceptLanguage)
    {
        if (string.IsNullOrWhiteSpace(acceptLanguage))
            return LanguageEnum.TR; // Default language

        // Extract the primary language tag from Accept-Language (e.g., "en-US,en;q=0.9")
        var languages = acceptLanguage.Split(',', StringSplitOptions.RemoveEmptyEntries);

        foreach (var lang in languages)
        {
            var langCode = lang.Split(';')[0].Trim().ToUpperInvariant();

            // Consider only the first two-letter language code (ISO 639-1)
            if (langCode.Length > 2)
                langCode = langCode.Substring(0, 2);

            // Try parsing to enum
            if (Enum.TryParse<LanguageEnum>(langCode, ignoreCase: true, out var parsedLanguage))
                return parsedLanguage;
        }

        return LanguageEnum.TR; // Default fallback
    }
}
