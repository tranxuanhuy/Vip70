using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

namespace TutorialConnectToAccessDB
{
    public class CheckProxyThread
    {

        public void ProxyCheckHttpRequest()
        {
            Thread thr = Thread.CurrentThread;
            string proxy = thr.Name;

            string MyProxyHostString = proxy.Split(':')[0];
            int MyProxyPort = int.Parse(proxy.Split(':')[1]);
            Demo w = new Demo();
            try
            {
                // Create a request for the URL. 
                WebRequest request = WebRequest.Create("https://whoer.net/");
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
                string info = ProcessReponseString(thr, responseFromServer);
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

        public void SockCheckHttpRequest()
        {
            Thread thr = Thread.CurrentThread;
            string proxy = thr.Name;

            string MyProxyHostString = proxy.Split(':')[0];
            int MyProxyPort = int.Parse(proxy.Split(':')[1]);

            Demo w = new Demo();
            try
            {
                Chilkat.Http http = new Chilkat.Http();
                http.SocksHostname = MyProxyHostString;
                http.SocksPort = MyProxyPort;
                ////http.SocksUsername = "myProxyLogin";
                ////http.SocksPassword = "myProxyPassword";
                ////  Set the SOCKS version to 4 or 5 based on the version
                ////  of the SOCKS proxy server:
                http.SocksVersion = 5;
                bool success1;
                ////  Any string unlocks the component for the 1st 30-days.
                success1 = http.UnlockComponent("Anything for 30-day trial");
                if (success1 != true)
                {
                    Console.WriteLine(http.LastErrorText);
                    return;
                }
                ////  Send the HTTP GET and return the content in a string.
                string responseFromServer = http.QuickGetStr("https://whoer.net/");
                // Display the content.
                string info = ProcessReponseString(thr, responseFromServer);
                w.WriteToFileThreadSafe(proxy + "\t" + info, "proxyWithCityHttpRequest.txt");
            }
            catch (Exception e)
            {
                w.WriteToFileThreadSafe(proxy, "proxyWithCityHttpRequest.txt");
                Console.WriteLine("{0} Second exception caught.", e);
            }
            
        }

        private static string ProcessReponseString(Thread thr, string responseFromServer)
        {
            //Console.WriteLine(responseFromServer);
            string filename = thr.Name.Replace(".", " ");
            filename = filename.Replace(":", " ");
            filename = "temp\\output" + filename + ".txt";
            File.WriteAllText(filename, responseFromServer);

            int counter = 0;
            string line;

            System.IO.StreamReader file =
new System.IO.StreamReader(filename);

            while ((line = file.ReadLine()) != null)
            {
                if (line.Contains("Country:"))
                {
                    break;
                }

                counter++;
            }

            //Console.WriteLine("Line number: {0}", counter);

            file.Close();
            int offset = counter;
            string info = File.ReadLines(filename).Skip(offset + 7).First() + File.ReadLines(filename).Skip(offset + 16).First() + File.ReadLines(filename).Skip(offset + 23).First() +"\t"+ File.ReadLines(filename).Skip(offset + 96).First().Split('-')[1].Split(' ')[0];
            info = info.Replace("<span class=\"cont\">", "\t");
            
                info = info.Replace("<span class=\"disabled\">", "");
            info = info.Replace("</span>", "\t");
            info = info.Replace("N/A\t", "N/A");
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
            return info;
        }
    }

   
}
