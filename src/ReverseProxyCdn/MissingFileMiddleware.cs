using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;
using Microsoft.Dnx.Runtime;
using Microsoft.Framework.Configuration;

namespace ReverseProxyCdn
{
    public class MissingFileMiddleware
    {
        private RequestDelegate _next;
        private IConfigurationRoot _config;
        private string _domain, _port, _path;
        private IApplicationEnvironment _env;

        public MissingFileMiddleware(RequestDelegate next, IConfigurationRoot root, IApplicationEnvironment env)
        {
            _next = next;
            _config = root;
            _env = env;
        }

        public async Task Invoke(HttpContext context)
        {
            context.Response.Headers.Remove("Server");

            GetDomainAndPath(context);
            if(!ValidateCaller())
            {
                context.Response.StatusCode = 401;
                return;
            }
            var url = GetRemoteUrl(context);

            if(!ValidateFile())
            {
                context.Response.StatusCode = 403;
            }
            await DownloadAndServeFile(context, url);
        }

        private void GetDomainAndPath(HttpContext context)
        {
            string filePath = context.Request.Path;
            int end = filePath.IndexOf("/", 1, StringComparison.Ordinal) - 1;

            int portIndex = filePath.IndexOf(':');

            _domain = portIndex > -1 ? filePath.Substring(1, portIndex - 1) : filePath.Substring(1, end);
            _port = portIndex > -1 ? filePath.Substring(portIndex + 1, end - portIndex) : "80";
            _path = filePath.Substring(end + 1);
        }

        private bool ValidateCaller()
        {
            var domain = _domain.Replace(":" + _port, "");
            var validDomain = _config["domains"].Split(',');
            if(!validDomain.Any(a => string.Equals(a.Trim(), domain, StringComparison.OrdinalIgnoreCase)))
            {
                //todo: probably return 401?
                return false;
            }
            return true;
        }

        private bool ValidateFile()
        {
            var ext = Path.GetExtension(_path).ToLowerInvariant();
            var extensions = _config["extensions"].Split(',').Select(a => a.Trim());
            if(!extensions.Contains(ext))
            {
                //todo: probably return 403?
                return false;
            }
            return true;
        }

        private Uri GetRemoteUrl(HttpContext context)
        {
            Uri url;

            if(!Uri.TryCreate(context.Request.Scheme + "://" + _domain + ":" + _port + _path, UriKind.Absolute, out url))
            {
                return null;
                //todo probably return error code?
            }

            return url;
        }

        private async Task DownloadAndServeFile(HttpContext context, Uri remote)
        {
            var path = Path.Combine(_env.ApplicationBasePath, "wwwroot", _domain, _path.Trim('/'));
            FileInfo file = new FileInfo(path);
            if(file.Exists)
            {
                return;
            }
            using(HttpClient client = new HttpClient())
            {
                var msg = new HttpRequestMessage() { RequestUri = remote };
                msg.Headers.Add("User-Agent", "Reverse Proxy 1.0 (http://m82.be)");

                var resp = await client.SendAsync(msg);
                if(resp.StatusCode != HttpStatusCode.OK)
                {
                    context.Response.StatusCode = (int)resp.StatusCode;
                }
                var buffer = await resp.Content.ReadAsByteArrayAsync();
                await SaveFile(file, buffer);
                await context.Response.Body.WriteAsync(buffer, 0, buffer.Length);

                context.Response.StatusCode = 200;
            }
        }

        private async Task SaveFile(FileInfo file, byte[] buffer)
        {
            file.Directory.Create();

            using(FileStream fileStream = new FileStream(file.FullName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None, 4096, true))
            {
                await fileStream.WriteAsync(buffer, 0, buffer.Length);
            }
        }
    }
}