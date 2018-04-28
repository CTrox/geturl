using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Linq;

namespace GetUrl
{
    [Cmdlet(VerbsCommon.Get, "Url")]
    public class GetUrlCommand : PSCmdlet
    {
        private readonly List<string> _bodyLinesFromPipeLine = new List<string>();



        [Parameter(Mandatory = true, Position = 1)]
        public string Url
        {
            get; set;
        }

        [Parameter(ValueFromPipeline = true)]
        public string BodyLineFromPipeline
        {
            get; set;
        }

        [Alias("B")]
        [Parameter()]
        public string Body
        {
            get; set;
        }



        protected override void ProcessRecord()
        {
            if(BodyLineFromPipeline != null)
                _bodyLinesFromPipeLine.Add(BodyLineFromPipeline);

            base.ProcessRecord();
        }

        protected override void EndProcessing()
        {
            string url = ensureScheme(Url);
            string bodyFromPipeline;

            if(_bodyLinesFromPipeLine.Count > 0)
                bodyFromPipeline = String.Join("\n", _bodyLinesFromPipeLine);
            else
                bodyFromPipeline = null;

            if(bodyFromPipeline != null && Body != null)
                throw new ArgumentException("Either use pipelineing or specify the body parameter. But don't do both.");

            Exec(url, bodyFromPipeline ?? Body);

            base.EndProcessing();
        }

        private string ensureScheme(string url)
        {
            try {
                var uri = new Uri(url);
            } catch(UriFormatException) {
              // scheme not supplied, prepending http
              return "http://" + url;
            }
            return url;
        }

        private void Exec(string url, string body = null)
        {
            if(url == null)
                throw new ArgumentNullException(nameof(url));

            using(var client = new HttpClient())
            {
                HttpResponseMessage httpResponse;
                {
                    var requestTask = client.GetAsync(url);
                    requestTask.Wait();
                    httpResponse = requestTask.Result;
                }

                if(httpResponse.IsSuccessStatusCode)
                {
                    string stringResponse;
                    {
                        var readTask = httpResponse.Content.ReadAsStringAsync();
                        readTask.Wait();
                        stringResponse = readTask.Result;
                    }

                    foreach(var responseLine in Regex.Split(stringResponse, "\r\n|\r|\n"))
                    {
                        WriteObject(responseLine);
                    }
                }
                else
                {
                    throw new InvalidOperationException($"Bad Request: {httpResponse.StatusCode}");
                }
            }
        }
    }
}
