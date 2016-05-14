using MiniPie.Converter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Xunit;

namespace MiniPie.Tests.Converter
{
    public class VisibilityConveterTest
    {
        VisibilityConverter converter = new VisibilityConverter(); 

        [Theory]
        [InlineData(false, Visibility.Collapsed)]
        [InlineData(true, Visibility.Visible)]
        public void TestConvertNotInversed(bool testData, Visibility expected)
        {
            Assert.Equal(expected, converter.Convert(testData, null, null, null));
        }

        [Theory]
        [InlineData(true, Visibility.Collapsed)]
        [InlineData(false, Visibility.Visible)]
        public void TestConvertInversed(bool testData, Visibility expected)
        {
            Assert.Equal(expected, converter.Convert(testData, null, true, null));
        }
    }
}
