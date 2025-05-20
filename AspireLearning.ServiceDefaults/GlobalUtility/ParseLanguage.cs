using AspireLearning.ServiceDefaults.GlobalEnum;

namespace AspireLearning.ServiceDefaults.GlobalUtility;

public static class LanguageParser
{
    public static Language Parse(string acceptLanguage)
    {
        if (string.IsNullOrWhiteSpace(acceptLanguage))
            return Language.TR; // Default language

        try
        {
            var languages = acceptLanguage.Split(',', StringSplitOptions.RemoveEmptyEntries);
            foreach (var lang in languages)
            {
                var langCode = lang.Split(';')[0].Trim().ToUpperInvariant();
                if (langCode.Length > 2)
                    langCode = langCode.Substring(0, 2);

                if (Enum.TryParse<Language>(langCode, ignoreCase: true, out var parsedLanguage))
                    return parsedLanguage;
            }
        }
        catch
        {
            // Log exception and return default language
        }

        return Language.TR; // Default fallback
    }
}

