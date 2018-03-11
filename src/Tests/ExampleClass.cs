namespace Tests
{
    using System.Threading.Tasks;

    public class TestClass
    {
        // Do not modify the line numbers in this class

        public async Task TestMethod()
        {
            await DoNothing().ConfigureAwait(false);
            
            DoNothing().Wait();

            var a = await ReturnSomething().ConfigureAwait(false);
            var b = ReturnSomething().Result;

            Task.Run(async () => await DoNothing().ConfigureAwait(false)).Wait();
            await Task.Run(async () => await DoNothing()).ConfigureAwait(false);
            await Task.Run(async () => await DoNothing().ConfigureAwait(false));
            await Task.Run(async () => await DoNothing());
        }

        private Task DoNothing() => Task.CompletedTask;
        private Task<int> ReturnSomething() => Task.FromResult(1);
    }
}
