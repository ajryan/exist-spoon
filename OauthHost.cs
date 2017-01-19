using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Newtonsoft.Json;

namespace ExistSpoon
{
    public class OauthHost
    {
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _redirectUri;

        private int _progressStack = 0;
        public bool FlushingProgress { get; private set; }

        public OauthHost(string clientId, string clientSecret, string redirectUri)
        {
            _clientId = clientId;
            _clientSecret = clientSecret;
            _redirectUri = redirectUri;
        }

        public Task<string> Authenticate()
        {
            ProgressAggregator.Published += OnProgress;

            var tcs = new TaskCompletionSource<string>();

            ProgressAggregator.Publish("Starting http listener.");

            var host = new WebHostBuilder().UseKestrel()
                                           .Configure(app => app.Run(context => ProccessRequest(context, tcs)))
                                           .UseUrls(_redirectUri)
                                           .Build();

            // TODO: testhost?
            new TaskFactory().StartNew(() => host.Run());

            ProgressAggregator.Publish("Browsing to authentication URL.");

            var authUrl = $"https://exist.io/oauth2/authorize?response_type=code&client_id={_clientId}&redirect_uri={_redirectUri}&scope=read+write";
            Utils.OpenBrowser(authUrl);

            return tcs.Task;
        }

        private async Task ProccessRequest(HttpContext context, TaskCompletionSource<string> tcs)
        {
            if (context.Request.Path.Value.Contains("favicon.ico"))
                return;

            bool isValid = true;
            bool isCompleted = tcs.Task.IsCompleted;

            if (context.Request.Query.ContainsKey("error"))
            {
                ProgressAggregator.Publish($"OAuth authorization error: {context.Request.Query["error"]}.");
                isValid = false;
            }

            if (!context.Request.Query.ContainsKey("code"))
            {
                if (!isCompleted)
                    ProgressAggregator.Publish($"Malformed authorization response. {context.Request.GetDisplayUrl()}");

                isValid = false;
            }

            string refreshMeta = (_progressStack == 0)
                    ? null
                    : $"<meta http-equiv='refresh' content='1;url={_redirectUri}'>";

            if (_progressStack == 0)
                FlushingProgress = false;
            else
                _progressStack = 0;

            string responseBody = $@"
<html>
  <head>
    {refreshMeta}
    <style type='text/css'>
      body {{ font-family: sans-serif; }}
    </style>
  </head>
  <body>{_statusMessage}</body>
</html>";

            await context.Response.WriteAsync(responseBody);

            if (isValid)
            {
                string oauthCode = context.Request.Query["code"].ToString();
                var accessToken = await GetAccessToken(oauthCode);

                tcs.SetResult(accessToken);
            }
        }

        private async Task<string> GetAccessToken(string oauthCode)
        {
            using (var client = new HttpClient())
            {
                var tokenResponseMessage = await client.PostAsync(
                    new Uri("https://exist.io/oauth2/access_token"),
                    new FormUrlEncodedContent(new Dictionary<string, string>
                    {
                        {"grant_type", "authorization_code"},
                        {"code", oauthCode},
                        {"client_id", _clientId},
                        {"client_secret", _clientSecret},
                        {"redirect_uri", _redirectUri}
                    }));

                var tokenResponseBody = await tokenResponseMessage.Content.ReadAsStringAsync();
                var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(tokenResponseBody);

                return tokenResponse.access_token;
            }
        }

        private string _statusMessage = "";

        private void OnProgress(string status)
        {
            _statusMessage += $"<BR/>{status}";
            FlushingProgress = true;
            _progressStack++;
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
}
