using System;
using System.ComponentModel;
using System.Reflection;

namespace MiniPie.Core.SpotifyWeb.Models
{
    public enum GrantType
    {
        [Description("authorization_code")]
        AuthorizationCode,
        [Description("refresh_token")]
        RefreshToken
    }

    public static class TokenAuthorizationTypeExtentions
    {
        public static string GetDescription(this GrantType token)
        {
            Type type = token.GetType();
            if (!type.IsEnum)
            {
                throw new ArgumentException("EnumerationValue must be of Enum type", "enumerationValue");
            }

            //Tries to find a DescriptionAttribute for a potential friendly name
            //for the enum
            MemberInfo[] memberInfo = type.GetMember(token.ToString());
            if (memberInfo.Length > 0)
            {
                var attribute = memberInfo[0].GetCustomAttribute<DescriptionAttribute>(false);

                if (attribute != null)
                {
                    //Pull out the description value
                    return attribute.Description;
                }
            }
            //If we have no description attribute, just return the ToString of the enum
            return token.ToString();
        }
    }

    
}