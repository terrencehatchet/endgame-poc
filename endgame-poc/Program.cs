using System;
using System.Threading;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using WireMock.Settings;
using System.IO;

namespace endgame_poc
{
    class Program
    {
        static void Main(string[] args)
        {
            var server = FluentMockServer.Start(new FluentMockServerSettings
            {
                Urls = new[] {"http://+:8080"},
                StartAdminInterface = true,
                ReadStaticMappings = true,
            });

            // Set the maping folder relative to this one
            string mappingsFolder = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "endgame-poc", "__admin", "mappings");
            System.Console.WriteLine("mappingsFolder folder: " + mappingsFolder);
            server.ReadStaticMappings(mappingsFolder);

            /* server
                .Given(
                    Request.Create().WithPath("* /api/v1/Applications/* /Mortgage/status").UsingGet()
                )
                .RespondWith(
                    Response.Create()
                        .WithStatusCode(200)
                        .WithHeader("Content-Type", "text/plain")
                        .WithBody("Hello world!!!!!")
                );*/

            Thread.Sleep(Timeout.Infinite);

            //Console.WriteLine("Press any key to stop server...");
            //Console.ReadLine();
        }
    }
}
