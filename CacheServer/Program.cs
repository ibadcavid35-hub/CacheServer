using System.Net;

namespace CacheServer
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var listener = new HttpListener();
            listener.Prefixes.Add(@"http://localhost:27001/");
            listener.Start();
            Console.WriteLine("Listener started...");

            while (true)
            {
                var context = await listener.GetContextAsync();

            }
        }
    }
}
