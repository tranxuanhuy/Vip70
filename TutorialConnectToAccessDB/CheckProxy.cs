using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace TutorialConnectToAccessDB
{
    class CheckProxy
    {
        public void GetCityOfSockHttpRequest(System.Windows.Forms.Label label3=null,string proxyfile="")
        {
            if (proxyfile=="")
            {
                proxyfile = "rawSocks.txt"; 
            }
            File.Delete("proxyWithCityHttpRequest.txt");
            int block = 50;
            for (int j = 0; j <= File.ReadAllLines(proxyfile).Count() / block; j++)
            {
                if (label3!=null)
                {
                    label3.Invoke((System.Windows.Forms.MethodInvoker)(() => label3.Text = "checking..." + j + "/" + File.ReadAllLines(proxyfile).Count() / block)); 
                }
                ChangeProxy.ResetProxySockEntireComputer();

                CheckProxyThread[] thr = new CheckProxyThread[1000];
                Thread[] tid = new Thread[1000];

                //MyThread thr1 = new MyThread();
                //MyThread thr2 = new MyThread();

                //Thread tid1 = new Thread(new ThreadStart(thr1.Thread1));
                //Thread tid2 = new Thread(new ThreadStart(thr2.Thread1));

                //tid1.Start();
                //tid2.Start();
                System.IO.DirectoryInfo downloadedMessageInfo = new DirectoryInfo("temp\\");

                foreach (FileInfo file in downloadedMessageInfo.GetFiles())
                {
                    file.Delete();
                }

                int num;
                if (j == File.ReadAllLines(proxyfile).Count() / block)
                    num = File.ReadAllLines(proxyfile).Count();
                else
                    num = (j + 1) * block;
                for (int i = j * block; i < num; i++)
                {
                    string proxy = ReadProxyAtLine(i + 1, proxyfile);
                    thr[i] = new CheckProxyThread();
                    tid[i] = new Thread(new ThreadStart(thr[i].SockCheckHttpRequest));
                    tid[i].Name = proxy;
                    tid[i].Start();
                }

                for (int i = j * block; i < num; i++)
                {
                    tid[i].Join();
                }

                if (label3 != null)
                           {
                    label3.Invoke((System.Windows.Forms.MethodInvoker)(() => label3.Text = "finish check !!!")); 
                }

            }
        }
        public string ReadProxyAtLine(int p, string file)
        {
            string proxy = File.ReadLines(file).Skip(p - 1).First();
            string[] aproxy = proxy.Split(':');
            return aproxy[0] + ':' + aproxy[1];
        }
    }
}
