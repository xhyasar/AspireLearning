using AspireLearning.ServiceDefaults.GlobalEnum;

namespace AspireLearning.ServiceDefaults.GlobalUtility;

public static class LanguageParser
{
    public static LanguageEnum Parse(string acceptLanguage)
    {
        if (string.IsNullOrWhiteSpace(acceptLanguage))
            return LanguageEnum.TR; // Default language

        try
        {
            var languages = acceptLanguage.Split(',', StringSplitOptions.RemoveEmptyEntries);
            foreach (var lang in languages)
            {
                var langCode = lang.Split(';')[0].Trim().ToUpperInvariant();
                if (langCode.Length > 2)
                    langCode = langCode.Substring(0, 2);

                if (Enum.TryParse<LanguageEnum>(langCode, ignoreCase: true, out var parsedLanguage))
                    return parsedLanguage;
            }
        }
        catch (Exception ex)
        {
            // Log exception and return default language
        }

        return LanguageEnum.TR; // Default fallback
    }
}

