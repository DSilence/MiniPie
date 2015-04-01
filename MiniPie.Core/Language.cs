using System;
using System.Collections.Generic;
using System.Globalization;
using MiniPie.Core.Extensions;
using Newtonsoft.Json;

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

		private sealed class CultureInfoUiNameEqualityComparer : IEqualityComparer<Language>
		{
			public bool Equals(Language x, Language y)
			{
				if (ReferenceEquals(x, y)) return true;
				if (ReferenceEquals(x, null)) return false;
				if (ReferenceEquals(y, null)) return false;
				if (x.GetType() != y.GetType()) return false;
				return Equals(x.CultureInfo, y.CultureInfo) && string.Equals(x.UiName, y.UiName);
			}

			public int GetHashCode(Language obj)
			{
				unchecked
				{
					return ((obj.CultureInfo != null ? obj.CultureInfo.GetHashCode() : 0)*397) ^ (obj.UiName != null ? obj.UiName.GetHashCode() : 0);
				}
			}
		}
    }
}
