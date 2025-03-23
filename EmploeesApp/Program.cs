using EmploeesApp.Services;

namespace EmploeesApp
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var argsService = new ArgsService(args);
            await argsService.RunTask();
        }
    }
}
