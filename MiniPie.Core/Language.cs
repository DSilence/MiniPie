using System.Collections.Generic;
using System.Globalization;

namespace MiniPie.Core
{
    public class Language
    {
        public Language(CultureInfo cultureInfo, string uiName)
        {
            CultureInfo = cultureInfo;
            UiName = uiName;
        }
        public CultureInfo CultureInfo { get; set; }
        public string UiName { get; set; }

        public static Language Russian = new Language(new CultureInfo("RU"), "Русский");
        public static Language English = new Language(new CultureInfo("EN"), "English");

        public static List<Language> Languages = new List<Language>
        {
            English, Russian
        }; 

        public override string ToString()
        {
            return UiName;
        }
    }
}
