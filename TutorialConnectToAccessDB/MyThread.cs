using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

namespace TutorialConnectToAccessDB
{
    public class MyThread
    {

        public void Thread1()
        {
            Thread thr = Thread.CurrentThread;
            string proxy = thr.Name;

            string MyProxyHostString = proxy.Split(':')[0];
            int MyProxyPort = int.Parse(proxy.Split(':')[1]);
            Demo w = new Demo();
            try
            {
                // Create a request for the URL. 
                WebRequest request = WebRequest.Create("http://ip-score.com");
                request.Proxy = new WebProxy(MyProxyHostString, MyProxyPort);
                // If required by the server, set the credentials.
                request.Credentials = CredentialCache.DefaultCredentials;
                // Get the response.
                WebResponse response = request.GetResponse();
                // Display the status.
                Console.WriteLine(((HttpWebResponse)response).StatusDescription);
                // Get the stream containing content returned by the server.
                Stream dataStream = response.GetResponseStream();
                // Open the stream using a StreamReader for easy access.
                StreamReader reader = new StreamReader(dataStream);
                // Read the content.
                string responseFromServer = reader.ReadToEnd();
                // Display the content.
                //Console.WriteLine(responseFromServer);
                string filename = thr.Name.Replace(".", " ");
                filename = filename.Replace(":", " ");
                filename = "temp\\output" + filename + ".txt";
                File.WriteAllText(filename, responseFromServer);
                string info = File.ReadLines(filename).Skip(145).First() + File.ReadLines(filename).Skip(146).First() + File.ReadLines(filename).Skip(147).First();
                info = info.Remove(0, info.IndexOf("png\">") + 6);
                info = info.Replace("</p>							<p><em>State:</em> ", "\t");
                info = info.Replace("</p>							<p><em>City:</em> ", "\t");
                info = info.Replace("</p>", "");
                //using (System.IO.StreamWriter file = new System.IO.StreamWriter("proxyWithCityHttpRequest.txt", true))
                //{
                //    try
                //    {
                //        file.WriteLine(proxy + "\t" + info);
                //    }
                //    catch (Exception e)
                //    {
                //        Console.WriteLine("{0} Second exception caught.", e);
                //    }
                //}   
                w.WriteToFileThreadSafe(proxy + "\t" + info, "proxyWithCityHttpRequest.txt");
            }
            catch (Exception e)
            {
                w.WriteToFileThreadSafe(proxy, "proxyWithCityHttpRequest.txt");
                Console.WriteLine("{0} Second exception caught.", e);
            }

            // Clean up the streams and the response.
            //reader.Close();
            //response.Close();
        }
    }
}
