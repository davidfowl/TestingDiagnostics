using System;
using Xunit;
using Xunit.Abstractions;

namespace XUnitTestProject2
{
    public class UnitTest1
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public UnitTest1(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void Test1()
        {
            _testOutputHelper.WriteLine("This is line from a test!");

            _testOutputHelper.WriteLine("This is line from another line from a test!");

            Assert.True(true);
        }
    }
}
