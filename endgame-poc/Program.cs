using System;
using System.Threading;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using WireMock.Settings;

namespace endgame_poc
{
    class Program
    {
        static void Main(string[] args)
        {
            var server = FluentMockServer.Start(new FluentMockServerSettings
            {
                Urls = new[] {"http://+:8080"}
            });

            server
                .Given(
                    Request.Create().WithPath("/some/thing").UsingGet()
                )
                .RespondWith(
                    Response.Create()
                        .WithStatusCode(200)
                        .WithHeader("Content-Type", "text/plain")
                        .WithBody("Hello world!!!!!")
                );

            Thread.Sleep(Timeout.Infinite);

            //Console.WriteLine("Press any key to stop server...");
            //Console.ReadLine();
        }
    }
}
