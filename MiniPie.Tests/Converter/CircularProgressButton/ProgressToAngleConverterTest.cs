using MiniPie.Controls.CircularProgressButton;
using MiniPie.Converter.CircularProgressButton;
using NSubstitute;
using System;
using System.Collections.Generic;
using Xunit;

namespace MiniPie.Tests.Converter.CircularProgressButton
{
    public class ProgressToAngleConverterTest
    {
        ProgressToAngleConverter _converter = new ProgressToAngleConverter();
        IProgressReporter _reporter = Substitute.For<IProgressReporter>();

        public ProgressToAngleConverterTest()
        {
            _reporter.Maximum.Returns(100);
            _reporter.Minimum.Returns(0);
        }

        public static IEnumerable<object[]> TestData
        {
            get
            {
                // Or this could read from a file. :)
                return new[]
                {
                new object[] { 10.1, 36.36},
                new object[] { 16.7, 60.12},
                new object[] { 50, 180.0},
                new object[] { 98.1, 353.16 },
                new object[] { 75, 270.0 }
            };
            }
        }

        [Theory(DisplayName ="Can convert")]
        [MemberData(nameof(TestData))]
        public void TestConvert(double data, double expected)
        {
            var array = new object[] { data, _reporter };
            double result = (double)_converter.Convert(array, null, null, null);
            Assert.Equal(expected, result, 2);
;       }

        [Theory(DisplayName = "Can convert back")]
        [MemberData(nameof(TestData))]
#pragma warning disable RECS0154 // Parameter is never used
        public void TestConvertBack(double expected, double data)
#pragma warning restore RECS0154 // Parameter is never used
        {
            var array = new object[] { data, _reporter };
            Assert.Throws<NotImplementedException>(() => _converter.ConvertBack(array, null, null, null));
        }
    }
}
