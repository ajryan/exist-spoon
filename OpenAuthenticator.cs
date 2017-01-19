using System.IO;

namespace ExistSpoon
{
    public class OpenAuthenticator
    {
        private readonly OauthHost _oauthHost;

        public bool FlushingProgress => _oauthHost.FlushingProgress;

        public OpenAuthenticator(string clientId, string clientSecret, string redirectUri)
        {
            _oauthHost = new OauthHost(clientId, clientSecret, redirectUri);
        }

        private static string OauthTokenPath => Path.Combine(Path.GetTempPath(), "existio_oauthtoken.txt");

        public string Authenticate()
        {
            string oauthToken;

            if (File.Exists(OauthTokenPath))
            {
                ProgressAggregator.Publish($"Using existing oauth token from {OauthTokenPath}.");
                oauthToken = File.ReadAllText(OauthTokenPath);
            }
            else
            {
                ProgressAggregator.Publish("Initiating OAuth flow.");

                oauthToken = _oauthHost.Authenticate().Result;
                File.WriteAllText(OauthTokenPath, oauthToken);
            }

            return oauthToken;
        }
    }
}
