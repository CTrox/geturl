using System;
using System.Management.Automation;

namespace GetUrl
{
    [Cmdlet(VerbsCommon.Get, "Url")]
    public class GetUrlCommand : PSCmdlet
    {
        [Parameter(Position=1)]
        public string Message { get; set; } = string.Empty;

        protected override void EndProcessing()
        {

            var HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("http://google.ch");
            client.DefaultRequestHeaders.Accept.Clear();
            try {
              HttpResponseMessage response = client.GetAsync("/").Wait();
              if (response.IsSuccessStatusCode)
              {
                this.WriteObject(response);
              }
            }

            base.EndProcessing();
        }
    }
}
