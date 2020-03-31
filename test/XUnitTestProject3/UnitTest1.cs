using System;
using System.Threading.Tasks;
using Xunit;

namespace XUnitTestProject3
{
    public class UnitTest1
    {
        static UnitTest1()
        {
            AppDomain.CurrentDomain.FirstChanceException += (sender, e) =>
            {
                if (e.Exception is TimeoutException)
                {
                    // Capture a dump?
                }
            };
        }

        [Fact]
        public async Task Test1()
        {
            var tcs = new TaskCompletionSource<object>();

            await tcs.Task.OrTimeout();
        }
    }
}
