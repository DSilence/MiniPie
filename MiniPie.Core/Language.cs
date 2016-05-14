using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;

namespace MiniPie.Core
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Language
    {
        public Language(CultureInfo cultureInfo, string uiName)
        {
            CultureInfo = cultureInfo;
            UiName = uiName;
        }

        [JsonProperty]
        public CultureInfo CultureInfo { get; set; }
        [JsonProperty]
        public string UiName { get; set; }

        public override string ToString()
        {
            return UiName;
        }

        protected bool Equals(Language other)
        {
            return Equals(CultureInfo, other.CultureInfo) && string.Equals(UiName, other.UiName);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Language) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((CultureInfo != null ? CultureInfo.GetHashCode() : 0)*397) ^ (UiName != null ? UiName.GetHashCode() : 0);
            }
        }
    }
}
