using System;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace endgame_poc
{
    class Program
    {
        static void Main(string[] args)
        {
            var server = FluentMockServer.Start();

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

            Console.WriteLine("Press any key to stop server...");
            Console.ReadLine();
        }
    }
}
