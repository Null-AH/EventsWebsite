using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventApi.Interfaces;

namespace EventApi.Services
{
    public class FontManagerService : IFontManagerService
    {
        private const string DefaultFontTheme = "Noto Naskh Arabic";
        private readonly Dictionary<string, string> _fontMap = new(System.StringComparer.OrdinalIgnoreCase)
        {
            { "Almarai", "Almarai-Regular.ttf" },
            { "Amiri", "Amiri-Regular.ttf" },
            { "Aref Ruqaa", "ArefRuqaa-Regular.ttf" },
            { "Cairo", "Cairo-Regular.ttf" },
            { "Changa", "Changa-Regular.ttf" },
            { "El Messiri", "ElMessiri-Regular.ttf" },
            { "Harmattan", "Harmattan-Regular.ttf" },
            { "IBM Plex Sans Arabic", "IBMPlexSansArabic-Regular.ttf" },
            { "Jomhuria", "Jomhuria-Regular.ttf" },
            { "Katibeh", "Katibeh-Regular.ttf" },
            { "Lateef", "Lateef-Regular.ttf" },
            { "Mada", "Mada-Regular.ttf" },
            { "Markazi Text", "MarkaziText-Regular.ttf" },
            { "Mirza", "Mirza-Regular.ttf" },
            { "Noto Kufi Arabic", "NotoKufiArabic-Regular.ttf" },
            { "Noto Naskh Arabic", "NotoNaskhArabic-Regular.ttf" },
            { "Rakkas", "Rakkas-Regular.ttf" },
            { "Reem Kufi", "ReemKufi-Regular.ttf" },
            { "Scheherazade New", "ScheherazadeNew-Regular.ttf" },
            { "Tajawal", "Tajawal-Regular.ttf" }            
        };

        public IEnumerable<string> GetAvailableFonts()
        {
            return _fontMap.Keys.OrderBy(name => name);
        }

        public string GetFontPath(string? fontName)
        {
            if (string.IsNullOrEmpty(fontName) || !_fontMap.ContainsKey(fontName))
            {
                fontName = DefaultFontTheme;
            }

            return _fontMap[fontName];
        }
    }
}