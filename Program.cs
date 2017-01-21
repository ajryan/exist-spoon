using System;
using System.IO;
using System.Threading;

namespace ExistSpoon
{
    public class Program
    {
        private const string ClientId     = "c9aa845538a191f78b06";
        private const string ClientSecret = "48c23e11279ee02ada0544c15b137693b8cde93d";
        private const string RedirectUri  = "http://127.0.0.1:5555/";

        public static int Main(string[] args)
        {
            ProgressAggregator.Published += OnProgress;

            if (args.Length == 0 || !File.Exists(args[0]))
            {
                ProgressAggregator.Publish("First argument must be path to .TDL task list.");
                return -1;
            }

            var tdlPath = args[0];

            ProgressAggregator.Publish($"Submitting completed task count from {tdlPath}");

            var authenticator = new OpenAuthenticator(ClientId, ClientSecret, RedirectUri);

            string oauthToken = authenticator.Authenticate();
            var completions = new TdlFile(tdlPath).PerseCompletions();

            new ExistClient(oauthToken).SubmitCompletions(completions);

            while (authenticator.FlushingProgress)
                Thread.Sleep(10);

            return 0;
        }

        private static void OnProgress(string message)
        {
            Console.WriteLine(message);
        }
    }
}
