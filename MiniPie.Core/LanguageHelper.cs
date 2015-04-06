using System.Collections.Generic;
using System.Globalization;

namespace MiniPie.Core
{
	public static class LanguageHelper
	{
		public static readonly Language Russian = new Language(new CultureInfo("RU"), "Русский");
		public static readonly Language English = new Language(new CultureInfo("EN"), "English");

		public static readonly List<Language> Languages = new List<Language>
		{
			English, Russian
		};
	}
}