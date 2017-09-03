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
            Type type = typeof(GrantType);

            //Tries to find a DescriptionAttribute for a potential friendly name
            //for the enum
            MemberInfo[] memberInfo = type.GetMember(token.ToString());
            var attribute = memberInfo[0].GetCustomAttribute<DescriptionAttribute>(false);

            //Pull out the description value
            return attribute.Description;
        }
    }

    
}