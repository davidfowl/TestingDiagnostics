using System;
using System.Threading.Tasks;
using Xunit;

namespace XUnitTestProject1
{
    public class UnitTest1
    {
        [Fact]
        public async Task HangTestRunner()
        {
            var tcs = new TaskCompletionSource<object>();

            await tcs.Task;
        }
    }
}
