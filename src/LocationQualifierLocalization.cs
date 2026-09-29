// SPDX-License-Identifier: MPL-2.0

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CompactCraftingTooltips
{
    internal static class LocationQualifierLocalization
    {
        private static readonly Dictionary<string, string> Cache =
            new Dictionary<string, string>(
                StringComparer.OrdinalIgnoreCase);

        internal static string RefugeeCamp(string rawLanguage)
        {
            string code = Normalize(rawLanguage);
            string value;

            if (Cache.TryGetValue(code, out value))
                return value;

            value = Load(code);
            if (string.IsNullOrWhiteSpace(value) &&
                !string.Equals(
                    code,
                    "en",
                    StringComparison.OrdinalIgnoreCase))
            {
                value = Load("en");
            }

            if (string.IsNullOrWhiteSpace(value))
                value = "Refugee camp";

            Cache[code] = value;
            return value;
        }

        internal static string Normalize(string rawLanguage)
        {
            string code = string.IsNullOrWhiteSpace(rawLanguage)
                ? "en"
                : rawLanguage
                    .Trim()
                    .ToLowerInvariant()
                    .Replace('-', '_');

            if (code == "zh" ||
                code.StartsWith("zh_", StringComparison.Ordinal))
            {
                return "zh_cn";
            }

            if (code == "pt" ||
                code.StartsWith("pt_", StringComparison.Ordinal))
            {
                return "pt_br";
            }

            switch (code)
            {
                case "en":
                case "fr":
                case "de":
                case "es":
                case "ko":
                case "ja":
                case "ru":
                case "it":
                case "pl":
                    return code;
                default:
                    return "en";
            }
        }

        private static string Load(string code)
        {
            Assembly assembly = typeof(LocationQualifierLocalization).Assembly;
            string suffix =
                ".refugee_camp." +
                code +
                ".txt";

            string resource = assembly
                .GetManifestResourceNames()
                .FirstOrDefault(
                    x => x.EndsWith(
                        suffix,
                        StringComparison.OrdinalIgnoreCase));

            if (resource == null)
                return null;

            using (Stream stream =
                assembly.GetManifestResourceStream(resource))
            {
                if (stream == null)
                    return null;

                using (StreamReader reader =
                    new StreamReader(
                        stream,
                        Encoding.UTF8,
                        true))
                {
                    return reader
                        .ReadToEnd()
                        .Trim(
                            '\uFEFF',
                            '\r',
                            '\n',
                            ' ',
                            '\t');
                }
            }
        }
    }
}
