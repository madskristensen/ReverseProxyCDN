using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;

public class MissingFileModule : IHttpModule
{
    private static readonly string[] _extensions = ConfigurationManager.AppSettings.Get("extensions").Split(' ');
    private string _domain, _port, _path;

    public void Init(HttpApplication app)
    {
        var asyncHelper = new EventHandlerTaskAsyncHelper(OnEndRequestAsync);
        app.AddOnEndRequestAsync(asyncHelper.BeginEventHandler, asyncHelper.EndEventHandler);
        app.PreSendRequestHeaders += context_PreSendRequestHeaders;
    }

    void context_PreSendRequestHeaders(object sender, EventArgs e)
    {
        HttpApplication application = (HttpApplication)sender;
        application.Context.Response.Headers.Remove("Server");
    }

    private async Task OnEndRequestAsync(object sender, EventArgs e)
    {
        HttpContext context = ((HttpApplication)sender).Context;

        if (context.Response.StatusCode == 404)
        {
            GetDomainAndPath(context);
            ValidateCaller();
            ValidateFile();

            Uri url = GetRemoteUrl(context);

            await DownloadAndServeFile(context, url);
        }
    }

    private void GetDomainAndPath(HttpContext context)
    {
        string filePath = context.Request.FilePath;
        int end = filePath.IndexOf("/", 1, StringComparison.Ordinal) - 1;

        int portIndex = filePath.IndexOf(':');

        _domain = portIndex > -1 ? filePath.Substring(1, portIndex - 1) : filePath.Substring(1, end);
        _port = portIndex > -1 ? filePath.Substring(portIndex + 1, end - portIndex) : "80";
        _path = filePath.Substring(end + 1);
    }

    private void ValidateCaller()
    {
        string domain = _domain.Replace(":" + _port, "");

        if (!"true".Equals(ConfigurationManager.AppSettings.Get(domain), StringComparison.OrdinalIgnoreCase))
            throw new HttpException(401, "Unauthorized");
    }

    private void ValidateFile()
    {
        string ext = Path.GetExtension(_path).ToLowerInvariant();

        if (!_extensions.Contains(ext))
            throw new HttpException(403, "File extension not supported");
    }

    private Uri GetRemoteUrl(HttpContext context)
    {
        Uri url;


        if (!Uri.TryCreate(context.Request.Url.Scheme + "://" + _domain + ":" + _port + _path, UriKind.Absolute, out url))
            throw new HttpException(406, "Requesting URL was not formatted correctly");

        return url;
    }

    private async Task DownloadAndServeFile(HttpContext context, Uri remote)
    {
        FileInfo file = new FileInfo(context.Server.MapPath("~/" + _domain + _path));

        try
        {
            using (WebClient client = new WebClient())
            {
                client.Headers.Add("User-Agent", "Reverse Proxy 1.0 (http://m82.be)");
                byte[] buffer = await client.DownloadDataTaskAsync(remote);
                await SaveFile(file, buffer);

                context.Response.BinaryWrite(buffer);
                context.Response.ContentType = client.ResponseHeaders["content-type"];
                context.Response.StatusCode = 200;
            }
        }
        catch (WebException exception)
        {
            var response = exception.Response as HttpWebResponse;
            throw new HttpException((int)response.StatusCode, exception.Message);
        }
    }

    private async Task SaveFile(FileInfo file, byte[] buffer)
    {
        file.Directory.Create();

        using (FileStream fileStream = new FileStream(file.FullName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None, 4096, true))
        {
            await fileStream.WriteAsync(buffer, 0, buffer.Length);
        }
    }

    public void Dispose()
    {
    }
}