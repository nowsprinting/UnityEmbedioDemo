// Copyright (c) 2025 Koji Hasegawa.
// This software is released under the MIT License.

using System.Diagnostics;
using EmbedIO;
using EmbedIO.Actions;
using EmbedIO.Files;
using Swan.Logging;
using UnityEngine;

namespace FileServer
{
    public class WebServerController : MonoBehaviour
    {
        public string url = "http://localhost:9696/";
        public bool openBrowser = true;

        private WebServer _server;

        private void Start()
        {
            _server = CreateWebServer(url);
            _server.RunAsync();

            if (openBrowser)
            {
                // Open web browser locally
                var browser = new Process()
                {
                    StartInfo = new ProcessStartInfo(url) { UseShellExecute = true }
                };
                browser.Start();
            }
        }

        private void OnDestroy()
        {
            _server?.Dispose();
        }

        private static WebServer CreateWebServer(string url)
        {
            var server = new WebServer(o => o
                    .WithUrlPrefix(url)
                    .WithMode(HttpListenerMode.EmbedIO))
                // First, we will configure our web server by adding Modules.
                .WithLocalSessionManager()
                .WithStaticFolder(
                    "/",
                    Application.persistentDataPath,
                    true,
                    m => m
                        .WithDirectoryLister(DirectoryLister.Html)
                        .WithoutDefaultDocument())
                // Add static files after other modules to avoid conflicts
                .WithModule(new ActionModule("/", HttpVerbs.Any, ctx => ctx.SendDataAsync(new { Message = "Error" })));

            // Listen for state changes.
            server.StateChanged += (s, e) => $"WebServer New State - {e.NewState}".Info();

            return server;
        }
    }
}
