// Copyright (c) 2025 Koji Hasegawa.
// This software is released under the MIT License.

using System.IO;
using System.Threading.Tasks;
using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using HttpMultipartParser;
using UnityEngine;
using UnityEngine.Scripting;

namespace FileServer
{
    public class UploadController : WebApiController
    {
        internal static string RootPath { private get; set; }

        [Route(HttpVerbs.Post, "/upload")]
        [Preserve]
        public async Task Upload()
        {
            var referrer = Request.UrlReferrer; // Note: using for redirect after upload
            if (referrer == null)
            {
                throw HttpException.BadRequest("Referer header is required");
            }

            var directory = Path.Join(RootPath, referrer.LocalPath);

            var parser = await MultipartFormDataParser.ParseAsync(Request.InputStream);
            foreach (var file in parser.Files)
            {
                var savePath = Path.Join(directory, file.FileName);
                // TODO: duplicate file name
                Debug.Log($"Received file: {file.FileName}, saving to: {savePath}");

                await using var fileStream = File.Create(savePath);
                await file.Data.CopyToAsync(fileStream);
            }

            Debug.Log($"Redirecting to: {referrer}");
            HttpContext.Redirect(referrer.ToString());
        }
    }
}
