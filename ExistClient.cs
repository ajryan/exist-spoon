using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;

namespace ExistSpoon
{
    public class ExistClient
    {
        private readonly string _oauthToken;

        public ExistClient(string oauthToken)
        {
            _oauthToken = oauthToken;
        }

        public void SubmitCompletions(List<DateCompletionCount> completions)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _oauthToken);

                ProgressAggregator.Publish($"Submitting completion updates.");

                var attributeUpdateRequests = completions.Select(
                    completion => new AttributeUpdateRequest("tasks_completed", completion.Date, completion.CompletedCount)).ToArray();

                var attributeUpdateResponseMessage = client.PostAsync(
                    new Uri("https://exist.io/api/1/attributes/update/"),
                    new StringContent(JsonConvert.SerializeObject(
                        attributeUpdateRequests),
                        Encoding.UTF8,
                        "application/json")).Result;

                ProgressAggregator.Publish($"Submitted. Got response {attributeUpdateResponseMessage.StatusCode:D} {attributeUpdateResponseMessage.ReasonPhrase} from updating tasks_completed.");
            }
        }

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public class AttributeUpdateRequest
        {
            public string name { get; set; }
            public string date { get; set; }
            public int value { get; set; }

            public AttributeUpdateRequest(string name, DateTime date, int value)
            {
                this.name = name;
                this.date = date.ToString("yyyy-MM-dd");
                this.value = value;
            }
        }
    }
}
