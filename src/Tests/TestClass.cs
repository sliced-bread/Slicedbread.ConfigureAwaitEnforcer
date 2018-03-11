namespace Tests
{
    using System.Threading.Tasks;

    public class TestClass
    {
        // Do not modify the line numbers in this class

        public async Task TestMethod()
        {
            await DoNothing().ConfigureAwait(false);
            await DoNothing();
        }


        private Task DoNothing()
        {
            return Task.CompletedTask;
        }
    }
}
