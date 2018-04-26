﻿using System;
using System.Management.Automation;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace GetUrl
{
    [Cmdlet(VerbsCommon.Get, "Url")]
    public class GetUrlCommand : PSCmdlet
    {
        [Parameter(Position = 0, Mandatory = true)]
        public string Url { get; set; } = string.Empty;

        protected override void EndProcessing()
        {

            var client = new HttpClient();
            client.BaseAddress = new Uri(Url);
            client.DefaultRequestHeaders.Accept.Clear();
            try
            {
                var task = client.GetAsync("/");
                task.Wait();

                var response = task.Result;
                if(response.IsSuccessStatusCode)
                {
                    WriteObject(response);
                }
            }
            catch(Exception e)
            {
                this.WriteObject(e.Message);
            }

            base.EndProcessing();
        }
    }
}
