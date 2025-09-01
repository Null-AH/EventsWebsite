using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventApi.Interfaces
{
    public interface IFontManagerService
    {
        public IEnumerable<string> GetAvailableFonts();
        public string GetFontPath(string? fontName);
    }
}