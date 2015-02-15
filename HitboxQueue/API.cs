using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using Newtonsoft.Json.Linq;

namespace HitboxQueue
{
    public static class API
    {
        public static object Get(string url)
        {
            try
            {
                WebClient c = new WebClient();

                var responseFromServer = c.DownloadString(url);

                c.Dispose();
                //Trace.WriteLine(String.Format("GetApi Data: {0}", responseFromServer));
                return responseFromServer;
            }
            catch (WebException exception)
            {
                using (var reader = new StreamReader(exception.Response.GetResponseStream()))
                {
                    var responseText = reader.ReadToEnd();
                    return responseText;
                }
            }
        }
    }
}
