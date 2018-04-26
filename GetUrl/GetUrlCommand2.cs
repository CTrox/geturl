using System;
using System.Collections.Generic;
using System.Management.Automation;

namespace GetUrl
{
    [Cmdlet(VerbsCommon.Get, "Url2")]
    public class GetUrlCommand2 : PSCmdlet
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
            string bodyFromPipeline;

            if(_bodyLinesFromPipeLine.Count > 0)
                bodyFromPipeline = String.Join("\n", _bodyLinesFromPipeLine);
            else
                bodyFromPipeline = null;

            if(bodyFromPipeline != null && Body != null)
                throw new ArgumentException("Either use pipelineing or specify the body manually. Both operations together are not supported.");

            Exec(Url, bodyFromPipeline ?? Body);

            base.EndProcessing();
        }

        private void Exec(string url, string body = null)
        {
            if(url == null)
                throw new ArgumentNullException(nameof(url));

            WriteObject($"Url: {url}");
            WriteObject($"Body: {body ?? "null"}");
        }
    }
}
