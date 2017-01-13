using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Newtonsoft.Json;

namespace MvcApp
{
  public class Program
  {
    const string CLIENT_ID     = "c9aa845538a191f78b06";
    const string CLIENT_SECRET = "48c23e11279ee02ada0544c15b137693b8cde93d";
    const string REDIRECT_URI  = "http://127.0.0.1:5555/";

    private static int    _completedCount;
    private static bool   _complete;
    private static string _statusMessage = "Starting up...";

    public static void Main(string[] args)
    {
      // TODO: more robust args specify .tdl path or explicit count
      if (args.Length == 0 || !Int32.TryParse(args[0], out _completedCount))
        throw new Exception("Pass completed count as argument.");

      WriteStatus($"Submitting completed task count {_completedCount} for {DateTime.Today:d}");

      // TODO: save auth token and avoid re-auth once retrieved
      WriteStatus("Starting http listener.");
      var host = new WebHostBuilder().UseKestrel()
                                     .Configure(app => app.Run(ProccessRequest))
                                     .UseUrls(REDIRECT_URI)
                                     .Build();

      new TaskFactory().StartNew(() => host.Run());


      WriteStatus("Browsing to authentication URL.");
      var authUrl = $"https://exist.io/oauth2/authorize?response_type=code&client_id={CLIENT_ID}&redirect_uri={REDIRECT_URI}&scope=read+write";
      OpenBrowser(authUrl);

      while (!_complete)
        System.Threading.Thread.Sleep(200);

      System.Threading.Thread.Sleep(5000);
      //Console.WriteLine("Press any key to exit...");
      //Console.ReadKey();
    }

    private static async Task ProccessRequest(HttpContext context)
    {
      // Ignore favicon request
      if (context.Request.Path.Value.Contains("favicon.ico"))
        return;

      bool isValid = true;

      // Check for errors
      if (context.Request.Query.ContainsKey("error"))
      {
        WriteStatus($"OAuth authorization error: {context.Request.Query["error"]}.");
        isValid = false;
      }

      if (!context.Request.Query.ContainsKey("code"))
      {
        if (!_complete)
          WriteStatus($"Malformed authorization response. {context.Request.GetDisplayUrl()}");

        isValid = false;
      }

      string refreshMeta = _complete ? null : $"<meta http-equiv='refresh' content='1;url={REDIRECT_URI}'>";

      await context.Response.WriteAsync($"<html>{refreshMeta}<head></head><body>{_statusMessage}</body></html>");

      if (!isValid)
        return;

      string oauthCode = context.Request.Query["code"].ToString();

      using (var client = new HttpClient())
      {
        var tokenResponseMessage = await client.PostAsync(
          new Uri("https://exist.io/oauth2/access_token"),
          new FormUrlEncodedContent(new Dictionary<string, string>
          {
            { "grant_type",    "authorization_code" },
            { "code",          oauthCode },
            { "client_id",     CLIENT_ID },
            { "client_secret", CLIENT_SECRET },
            { "redirect_uri",  REDIRECT_URI }
          }));

        var tokenResponseBody = await tokenResponseMessage.Content.ReadAsStringAsync();
        var tokenResponse     = JsonConvert.DeserializeObject<TokenResponse>(tokenResponseBody);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenResponse.access_token);

        var attributeUpdateRequests        = new[] { new AttributeUpdateRequest("tasks_completed", DateTime.Today, _completedCount) };
        var attributeUpdateResponseMessage = await client.PostAsync(
          new Uri("https://exist.io/api/1/attributes/update/"),
          new StringContent(JsonConvert.SerializeObject(attributeUpdateRequests), Encoding.UTF8, "application/json"));

        WriteStatus($"Got response {attributeUpdateResponseMessage.StatusCode:D} {attributeUpdateResponseMessage.ReasonPhrase} from updating tasks_completed.");
      }

      _complete = true;
    }

    private static void WriteStatus(string statusMessage)
    {
      Console.WriteLine(statusMessage);
      _statusMessage += String.Concat("<BR/>", statusMessage);
    }

    private static void OpenBrowser(string url)
    {
      try
      {
        Process.Start(url);
      }
      catch
      {
        // hack because of this: https://github.com/dotnet/corefx/issues/10361
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
          url = url.Replace("&", "^&");
          Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
          Process.Start("xdg-open", url);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
          Process.Start("open", url);
        }
        else
        {
          throw;
        }
      }
    }
  }

  [SuppressMessage("ReSharper", "InconsistentNaming")]
  public class TokenResponse
  {

    public string access_token { get; set; }
    public string token_type { get; set; }
    public long expires_in { get; set; }
    public string refresh_token { get; set; }
    public string scope { get; set; }
  }

  [SuppressMessage("ReSharper", "InconsistentNaming")]
  public class AttributeUpdateRequest
  {
    public string name { get; set; }
    public string date { get; set; }
    public int value { get; set; }

    public AttributeUpdateRequest(string name, DateTime date, int value)
    {
      this.name  = name;
      this.date  = date.ToString("yyyy-MM-dd");
      this.value = value;
    }
  }
}
