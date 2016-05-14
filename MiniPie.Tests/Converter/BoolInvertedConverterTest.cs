using MiniPie.Converter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace MiniPie.Tests.Converter
{
    public class BoolInvertedConverterTest
    {
        BoolInvertedConverter _converter = new BoolInvertedConverter(); 

        [Fact(DisplayName ="Convets forward")]
        public void TestConvertForward()
        {
            Assert.Equal(false, _converter.Convert(true, null, null, null));
            Assert.Equal(true, _converter.Convert(false, null, null, null));
        }

        [Fact(DisplayName = "Convets back")]
        public void TestConvertBackward()
        {
            Assert.Equal(false, _converter.ConvertBack(true, null, null, null));
            Assert.Equal(true, _converter.ConvertBack(false, null, null, null));
        }
    }
}
