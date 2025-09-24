// Copyright (c) 2025 Koji Hasegawa.
// This software is released under the MIT License.

using System.Diagnostics;
using EmbedIO;
using EmbedIO.Actions;
using EmbedIO.Files;
using EmbedIO.WebApi;
using Swan.Logging;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace FileServer
{
    public class FileServer
    {
        public string IPAddress { get; set; }
        public string Port { get; set; }
        public bool OpenBrowser { get; set; }

        private WebServer _server;

        public bool IsRunning => _server != null;

        public void StartServer()
        {
            if (IsRunning)
            {
                Debug.Log("Server is already running.");
                return;
            }

            var url = $"http://{IPAddress}:{Port}/";
            _server = CreateWebServer(url);
            _server.RunAsync();

            if (OpenBrowser)
            {
                // Open web browser locally
                var browser = new Process()
                {
                    StartInfo = new ProcessStartInfo(url) { UseShellExecute = true }
                };
                browser.Start();
            }

            Debug.Log($"Server started at {url}");
        }

        public void StopServer()
        {
            _server?.Dispose();
            _server = null;

            Debug.Log("Server stopped.");
        }

        private static WebServer CreateWebServer(string url)
        {
            UploadController.RootPath = Application.persistentDataPath; // Note: Can only be get on the main thread

            var server = new WebServer(o => o
                    .WithUrlPrefix(url)
                    .WithMode(HttpListenerMode.EmbedIO))
                // First, we will configure our web server by adding Modules.
                .WithLocalSessionManager()
                .WithWebApi("/api", m => m.WithController<UploadController>())
                .WithStaticFolder(
                    "/",
                    Application.persistentDataPath,
                    true,
                    m => m
                        .WithDirectoryLister(UploadableDirectoryLister.Instance)
                        .WithoutDefaultDocument())
                // Add static files after other modules to avoid conflicts
                .WithModule(new ActionModule("/", HttpVerbs.Any, ctx => ctx.SendDataAsync(new { Message = "Error" })));

            // Listen for state changes.
            server.StateChanged += (s, e) => $"WebServer New State - {e.NewState}".Info();

            return server;
        }
    }
}
