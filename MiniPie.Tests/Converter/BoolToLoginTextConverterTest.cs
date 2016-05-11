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
        BoolToLoginTextConverter _converter = new BoolToLoginTextConverter();

        [Fact(DisplayName = "Convets forward")]
        public void TestConvertForward()
        {
            Assert.Equal("Logout", _converter.Convert(true, null, null, null));
            Assert.Equal("Login", _converter.Convert(false, null, null, null));
        }

        [Fact(DisplayName = "Convets back")]
        public void TestConvertBack()
        {
            Assert.Throws<NotImplementedException>(() => _converter.ConvertBack(true, null, null, null));
            Assert.Throws<NotImplementedException>(() => _converter.ConvertBack(false, null, null, null));
        }
    }
}
