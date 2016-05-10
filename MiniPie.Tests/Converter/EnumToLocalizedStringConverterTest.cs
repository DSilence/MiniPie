using MiniPie.Converter;
using MiniPie.Core.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace MiniPie.Tests.Converter
{
    public class EnumToLocalizedStringConverterTest
    {
        EnumToLocalizedStringConverter converter = new EnumToLocalizedStringConverter();

        [Theory(DisplayName = "Can convert single value")]
        [InlineData(ApplicationSize.Small, "Small")]
        [InlineData(LockScreenBehavior.PauseUnpause, "Pause when locked. Unpause when unlocked if paused by locking")]
        public void TestConvert(Enum value, string expected)
        {
            Assert.Equal(expected, converter.Convert(value, typeof(string), null, null));
        }

        [Theory(DisplayName = "Can convert multiple values")]
        [InlineData(typeof (ApplicationSize), new[] { "Large", "Medium", "Small" })]
        public void TestConvertMultiple(Type value, IEnumerable<string> expected)
        {
            var result = (List<string>)converter.Convert(value, typeof(IEnumerable), null, null);
            Assert.Equal(expected, result);
        }

        [Theory(DisplayName = "Can convert single back")]
        [InlineData("Small", ApplicationSize.Small)]
        [InlineData("Medium", ApplicationSize.Medium)]
        [InlineData("Large", ApplicationSize.Large)]
        public void TestConvertBack(string value, Enum expected)
        {
            Assert.Equal(expected, converter.ConvertBack(value, expected.GetType(), null, null));
        }
    }
}
