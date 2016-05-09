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
        BoolInvertedConverter converter = new BoolInvertedConverter(); 

        [Fact(DisplayName ="Convets forward")]
        public void TestConvertForward()
        {
            Assert.Equal(false, converter.Convert(true, null, null, null));
            Assert.Equal(true, converter.Convert(false, null, null, null));
        }

        [Fact(DisplayName = "Convets back")]
        public void TestConvertBackward()
        {
            Assert.Equal(false, converter.ConvertBack(true, null, null, null));
            Assert.Equal(true, converter.ConvertBack(false, null, null, null));
        }
    }
}
