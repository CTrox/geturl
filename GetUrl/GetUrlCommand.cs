using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Text;

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

        [Alias("X")]
        [Parameter()]
        public string Request
        {
            get; set;
        } = "GET";



        protected override void ProcessRecord()
        {
            if(BodyLineFromPipeline != null)
                _bodyLinesFromPipeLine.Add(BodyLineFromPipeline);

            base.ProcessRecord();
        }

        protected override void EndProcessing()
        {
            var url = EnsureHttpScheme(Url);
            string bodyFromPipeline;

            if(_bodyLinesFromPipeLine.Count > 0)
                bodyFromPipeline = String.Join("\n", _bodyLinesFromPipeLine);
            else
                bodyFromPipeline = null;

            if(bodyFromPipeline != null && Body != null)
                throw new ArgumentException("Either use pipelineing or specify the body parameter. But don't do both.");

            Exec(url, Request, bodyFromPipeline ?? Body);

            base.EndProcessing();
        }

        private Uri EnsureHttpScheme(string url)
        {
            try {
                var uri = new Uri(url);
                if (uri.Scheme != "http" && uri.Scheme != "https") {
                  return new Uri("http://" + url);
                }
                return uri;
            } catch(UriFormatException) {
                // scheme not supplied, prepending http
                return new Uri("http://" + url);
            }
        }

        private void Exec(Uri url, string request, string body = null)
        {
            using(var client = new HttpClient())
            {
                HttpResponseMessage httpResponse;
                {
                    Task<HttpResponseMessage> requestTask;
                    switch (request)
                    {
                        case "GET":
                            requestTask = client.GetAsync(url);
                            break;
                        case "POST":
                            var postContent = new StringContent(body, Encoding.UTF8, "application/json");
                            requestTask = client.PostAsync(url, postContent ?? throw new InvalidOperationException($"Body must be set when using {request}."));
                            break;
                        case "PUT":
                            var putContent = new StringContent(body, Encoding.UTF8, "application/json");
                            requestTask = client.PutAsync(url, putContent);
                            break;
                        default:
                            throw new InvalidOperationException($"Request Method {request} not implemented");
                    }

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
