using MiniPie.Converter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace MiniPie.Tests.Converter
{
    public class BoolToLoginTextConverterTest
    {
        BoolToLoginTextConverter converter = new BoolToLoginTextConverter();

        [Fact(DisplayName = "Convets forward")]
        public void TestConvertForward()
        {
            Assert.Equal("Logout", converter.Convert(true, null, null, null));
            Assert.Equal("Login", converter.Convert(false, null, null, null));
        }

        [Fact(DisplayName = "Convets back")]
        public void TestConvertBack()
        {
            Assert.Throws<NotImplementedException>(() => converter.ConvertBack(true, null, null, null));
            Assert.Throws<NotImplementedException>(() => converter.ConvertBack(false, null, null, null));
        }
    }
}
