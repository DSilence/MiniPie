using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace MiniPie.Core.Extensions {
    public static class StringExtensions {
        public static string ToSHA1(this string str) {
            var sb = new StringBuilder();
#pragma warning disable CA5350 // Do not use insecure cryptographic algorithm SHA1.
            SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(str)).ToList().ForEach(b => sb.Append(b.ToString("x2")));
#pragma warning restore CA5350 // Do not use insecure cryptographic algorithm SHA1.
            return sb.ToString();
        }
    }
}
