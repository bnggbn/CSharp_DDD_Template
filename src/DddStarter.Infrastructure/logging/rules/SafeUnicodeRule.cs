using System.Text;
using DddStarter.Infrastructure.Logging.Abstractions;

namespace DddStarter.Infrastructure.Logging.Rules;

public sealed class SafeUnicodeRule : ILogSanitizationRule
{
    public string Apply(string input)
    {
        string value = input ?? string.Empty;
        StringBuilder sb = new(value.Length);

        for (int i = 0; i < value.Length; i++)
        {
            int rune;
            if (char.IsHighSurrogate(value[i]) && i + 1 < value.Length && char.IsLowSurrogate(value[i + 1]))
            {
                rune = char.ConvertToUtf32(value[i], value[i + 1]);
                i++;
            }
            else
            {
                rune = value[i];
            }

            if (rune == 0x09) { sb.Append(@"\t"); continue; }
            if (rune == 0x0A) { sb.Append(@"\n"); continue; }
            if (rune == 0x0D) { sb.Append(@"\r"); continue; }

            if (IsAllowedRune(rune))
            {
                if (rune <= 0xFFFF) sb.Append((char)rune);
                else sb.Append(char.ConvertFromUtf32(rune));
            }
            else
            {
                sb.Append(EscapeRune(rune));
            }
        }

        return sb.ToString();
    }

    private static bool IsAllowedRune(int r)
    {
        if (r < 0x20 || r == 0x7F) return false;
        if (r >= 0x80 && r <= 0x9F) return false;
        if (r >= 0xD800 && r <= 0xDFFF) return false;
        if (r >= 0xE000 && r <= 0xF8FF) return false;

        return r switch
        {
            0x034F or 0x061C or 0x200B or 0x200C or 0x200D or 0x200E or 0x200F or
            0x202A or 0x202B or 0x202C or 0x202D or 0x202E or
            0x2066 or 0x2067 or 0x2068 or 0x2069 or
            0xFEFF => false,
            _ => true
        };
    }

    private static string EscapeRune(int rune)
    {
        return rune <= 0xFFFF
            ? @"\u" + rune.ToString("x4")
            : @"\u{" + rune.ToString("x") + "}";
    }
}