
//author EtaYuy | mdsaputra.wordpress.com | Meihta Dwiguna Saputra

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.OleDb;
using System.IO;
using Microsoft.Win32;
using System.Threading;
using System.Runtime.InteropServices;
using System.Data.SqlClient;
using System.Net;
using System.Diagnostics;

namespace TutorialConnectToAccessDB
{
    public partial class FormMain : Form
    {
        [DllImport("wininet.dll")]
        public static extern bool InternetSetOption(IntPtr hInternet, int dwOption, IntPtr lpBuffer, int dwBufferLength);
        public const int INTERNET_OPTION_SETTINGS_CHANGED = 39;
        public const int INTERNET_OPTION_REFRESH = 37;
        static bool settingsReturn, refreshReturn;

        private OleDbConnection bookConn;
        private OleDbCommand oleDbCmd = new OleDbCommand();
        //parameter from mdsaputra.udl
        private String connParam = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=|DataDirectory|\book.mdb;Persist Security Info=False";
        private string proxyfile;

        //sqlcommand with where condition match country or state
        private bool stateSelected = false;
        private TcpForwarderSlim tcpForward;
        private bool useProxifier=false;
        private bool systemTime=false;
        private string ProxifierPath;
        public FormMain()
        {
            //create connection using parameter from mdsaputra.udl
            bookConn = new OleDbConnection(connParam);
            InitializeComponent();
        }

        private void buttonInsert_Click(object sender, EventArgs e)
        {

            InsertProxyToDB();
                    }

        private void InsertProxyToDB(string websource=null)
        {
            if (websource==null)
            {
                websource = comboBox1.GetItemText(comboBox1.SelectedItem); 
            }

            bookConn.Open();
            oleDbCmd.Connection = bookConn;
            //oleDbCmd.CommandText = "delete from book";
            //oleDbCmd.ExecuteNonQuery();

            if (File.Exists("proxyWithCityHttpRequest.txt"))
            {
                for (int i = 0; i <= File.ReadAllLines("proxyWithCityHttpRequest.txt").Count() - 1; i++)
                {
                    string proxy = ReadProxyAtLine(i + 1, "proxyWithCityHttpRequest.txt");
                    proxy += "\t" + websource;
                    try
                    {
                        oleDbCmd.CommandText = "insert into book (IP, country,state,city,GMT,websource) values ('" + proxy.Replace("\t", "\',\'") + "');";
                        int temp = oleDbCmd.ExecuteNonQuery();
                        //if (temp > 0)
                        //{
                        //    textBoxBookName.Text = null;
                        //    textBoxDescription.Text = null;
                        //    MessageBox.Show("Record Successfuly Added");
                        //}
                        //else
                        //{
                        //    MessageBox.Show("Record Fail to Added");
                        //}
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);

                    }
                } 
            }
            bookConn.Close();
            Listview1Reload("select distinct country from book order by country desc");
            GridviewReload("select * from book where country='United States (US)'");
            label3.Invoke((System.Windows.Forms.MethodInvoker)(() => label3.Text = "Record Successfuly Added!"));
        }

        

        private void button1_Click(object sender, EventArgs e)
        {
            loadProxyFileDownloadFromWeb();
        }

        private void loadProxyFileDownloadFromWeb()
        {
            
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            openFileDialog1.InitialDirectory = "c:\\";
            openFileDialog1.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 2;
            openFileDialog1.RestoreDirectory = true;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    proxyfile = openFileDialog1.FileName;
                    backgroundWorker1.RunWorkerAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                }
            }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            //GetCityOfProxyHttpRequest(label3);
            new CheckProxy().GetCityOfSockHttpRequest(label3,proxyfile);
        }

        //filechooser txt file => proxyWithCityHttpRequest.txt
    public void GetCityOfProxyHttpRequest(System.Windows.Forms.Label label3)
    {
        File.Delete("proxyWithCityHttpRequest.txt");
        int block = 50;
        for (int j = 0; j <= File.ReadAllLines(proxyfile).Count() / block; j++)
        {
            label3.Invoke((System.Windows.Forms.MethodInvoker)(() => label3.Text = "checking..." + j + "/" + File.ReadAllLines(proxyfile).Count() / block));
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
                tid[i] = new Thread(new ThreadStart(thr[i].ProxyCheckHttpRequest));
                tid[i].Name = proxy;
                tid[i].Start();
            }

            for (int i = j * block; i < num; i++)
            {
                tid[i].Join();
            }

            label3.Invoke((System.Windows.Forms.MethodInvoker)(() => label3.Text = "finish check !!!"));

        }
    }

    public string ReadProxyAtLine(int p, string file)
    {
        string proxy = File.ReadLines(file).Skip(p - 1).First();
        string[] aproxy = proxy.Split(':');
        return aproxy[0] + ':' + aproxy[1];
    }

    private void FormMain_Load(object sender, EventArgs e)
    {
        //mo firefox de nghe ket noi autoit
        bool firefoxExist=false;
        foreach (System.Diagnostics.Process myProc in System.Diagnostics.Process.GetProcesses())
        {
            if (myProc.ProcessName == "firefox")
            {
                firefoxExist = true;
            }
        }
        if (!firefoxExist)
        {
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = System.Environment.GetFolderPath(
            System.Environment.SpecialFolder.ProgramFiles) + @"\Mozilla Firefox\firefox.exe",
                    Arguments = "https://addons.mozilla.org/vi/firefox/addon/mozrepl/",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,

                    //WorkingDirectory = @"C:\MyAndroidApp\"
                }
            };

            proc.Start(); 
        }

        //config lan dau: chon duong dan dden vip72
        if (!File.Exists("config"))
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            openFileDialog1.InitialDirectory = "c:\\";
            openFileDialog1.Filter = "exe files (*.exe)|Proxifier.exe";
            openFileDialog1.FilterIndex = 2;
            openFileDialog1.RestoreDirectory = true;
            openFileDialog1.Title = "chon duong dan den file Proxifier.exe";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                using (System.IO.StreamWriter file = new System.IO.StreamWriter("config"))
                {
                    ProxifierPath = openFileDialog1.FileName;
                    file.Write(ProxifierPath);
                }

            } 
        }
        else
        {
            ProxifierPath = File.ReadLines("config").First();
        }
        //load combobox
        comboBox1.DisplayMember = "Text";
        comboBox1.ValueMember = "Value";

        var items = new[] { 
    new { Text = "proxyelite", Value = "proxyelite" }, 
    //new { Text = "report B", Value = "reportB" }, 
    //new { Text = "report C", Value = "reportC" },
    //new { Text = "report D", Value = "reportD" },
    new { Text = "5socks", Value = "5socks" }
};

        comboBox1.DataSource = items;
        
        try
        {
            File.Copy("demo.ppx", (ProxifierPath + @"\Profiles\demo.ppx").Replace("\\Proxifier.exe", ""));
            
        }
        catch (Exception)
        {
           
        }
        Listview1Reload("select distinct country from book order by country desc");
      GridviewReload("select * from book where country='United States (US)'"); 
    }

    private void Listview1Reload(string sqlcommand)
    {
        listView1.Clear();
        listView1.Refresh();

        OleDbDataAdapter dAdapter = new OleDbDataAdapter(sqlcommand, connParam);
        OleDbCommandBuilder cBuilder = new OleDbCommandBuilder(dAdapter);

        DataTable dataTable = new DataTable();
        DataSet ds = new DataSet();

        dAdapter.Fill(dataTable);

        for (int i = 0; i < dataTable.Rows.Count; i++)
        {
            //dataGridView1.Rows.Add(dataTable.Rows[i][1], dataTable.Rows[i][2], dataTable.Rows[i][3], dataTable.Rows[i][4]);
            ListViewItem listitem = new ListViewItem(dataTable.Rows[i][0].ToString());
            listView1.Items.Add(listitem);
        }
    }

   

    private void listView1_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (listView1.SelectedItems.Count > 0)
        {
            if (!stateSelected)
            {
                GridviewReload("select * from book where country='" + listView1.SelectedItems[0].Text + "'");
            }
            else
            {
                //where and country = US ?
                GridviewReload("select * from book where state='" + listView1.SelectedItems[0].Text + "'");
            } 
        }
    }

    private void GridviewReload(string sqlcommand)
    {
        dataGridView1.DataSource = null;
        dataGridView1.Rows.Clear();
        dataGridView1.Refresh();

        OleDbDataAdapter dAdapter = new OleDbDataAdapter(sqlcommand, connParam);
        OleDbCommandBuilder cBuilder = new OleDbCommandBuilder(dAdapter);

        DataTable dataTable = new DataTable();
        DataSet ds = new DataSet();

        dAdapter.Fill(dataTable);

        for (int i = 0; i < dataTable.Rows.Count; i++)
        {
            dataGridView1.Rows.Add(dataTable.Rows[i][1], dataTable.Rows[i][2], dataTable.Rows[i][3], dataTable.Rows[i][4], dataTable.Rows[i][5], dataTable.Rows[i][6]);
        }
        label4.Text = dataTable.Rows.Count + " records";
    }

    private void button3_Click(object sender, EventArgs e)
    {
        Listview1Reload("select distinct state from book where country='" + listView1.SelectedItems[0].Text + "'");
        stateSelected = true;
    }

    private void button2_Click(object sender, EventArgs e)
    {
        Listview1Reload("select distinct country from book order by country desc");
        stateSelected = false;
    }

    private void lineChanger(string newText, string fileName, int line_to_edit)
    {
        string[] arrLine = File.ReadAllLines(fileName);
        arrLine[line_to_edit - 1] = newText;
        File.WriteAllLines(fileName, arrLine);
    }


    private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (useProxifier)
        {
            ProxifierReload("127.0.0.1", 9951); 
        }
        else
        {
            ChangeProxy.ResetProxySockEntireComputer();
        }
    }

    private void ProxifierReload(string proxy,int port)
    {
        string filename = (ProxifierPath + @"\Profiles\demo.ppx").Replace("\\Proxifier.exe","") ;
        lineChanger("<Address>" + proxy + "</Address>", filename, 28);
        lineChanger("<Port>" + port + "</Port>", filename, 29);
        var proc = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = ProxifierPath,
                Arguments = string.Format("\"{0}\"", filename),
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true,

                //WorkingDirectory = @"C:\MyAndroidApp\"
            }
        };

        proc.Start();
    }

    private void checkBox1_CheckedChanged(object sender, EventArgs e)
    {
        if (checkBox1.Checked)
        {
            useProxifier = true;
            //reset value of 3 browsers
            ChangeProxy.ResetProxySockEntireComputer();

            foreach (System.Diagnostics.Process myProc in System.Diagnostics.Process.GetProcesses())
            {
                if (myProc.ProcessName == "firefox")
                {
                    System.Diagnostics.Process cmd;
                    cmd = System.Diagnostics.Process.Start("resetSocksFF.exe");
                    cmd.WaitForExit();
                }
            }
            
        }
        else
        {
            useProxifier = false;
            foreach (System.Diagnostics.Process myProc in System.Diagnostics.Process.GetProcesses())
            {
                if (myProc.ProcessName == "Proxifier")
                {
                    myProc.Kill();
                }
            }
        }
    }


    private void comboBox1_SelectionChangeCommitted(object sender, EventArgs e)
    {
        ComboBox cmb = (ComboBox)sender;
        int selectedIndex = cmb.SelectedIndex;

        if (selectedIndex==0)
        {
            loadProxyFileDownloadFromWeb();
        }
        else if (selectedIndex==1)
	{
        FormAdd5Socks f2=new FormAdd5Socks();
        f2.ShowDialog();
        if (f2.CheckProxy)
        {
            InsertProxyToDB(); 
        }
	}
        
        
    }

    private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
    {
        InsertProxyToDB(); 
    }

    private void listView2_SelectedIndexChanged(object sender, EventArgs e)
    {

    }

    private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
    {
        string proxy = dataGridView1.CurrentCell.Value.ToString();
        if (useProxifier)
        {
            ProxifierReload(proxy.Split(':')[0], int.Parse(proxy.Split(':')[1]));
        }
            //neu ko dung proxifier
        else
        {
            if (checkBox3.Checked)
            {
              ChangeProxy.SetSockEntireComputer(proxy);
            }
         
            //set socks for firefox, ff can't use system sock error
            if (checkBox4.Checked)
            {
                System.Diagnostics.Process cmd;
                using (System.IO.StreamWriter file = new System.IO.StreamWriter("autoItReadProxy.txt"))
                {
                    file.Write(proxy);
                }
                cmd = System.Diagnostics.Process.Start("changeSocksFF.exe");
                cmd.WaitForExit(); 
            }
        }

        //change systemtime
        if (systemTime) { ChangeProxy.ChangeTimezone(dataGridView1.CurrentRow.Cells[5].Value.ToString()); }
        else
        {
            ChangeProxy.ResetTimezone();
        }
        label4.Text = proxy;
    }

    private void buttonCheck5Socks_Click(object sender, EventArgs e)
    {
        string sqlcommand = "select * from book where websource='5socks'";
        OleDbDataAdapter dAdapter = new OleDbDataAdapter(sqlcommand, connParam);
        OleDbCommandBuilder cBuilder = new OleDbCommandBuilder(dAdapter);

        DataTable dataTable = new DataTable();
        DataSet ds = new DataSet();

        dAdapter.Fill(dataTable);
        using (System.IO.StreamWriter file = new System.IO.StreamWriter("rawSocks.txt"))
        {
            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                file.WriteLine(dataTable.Rows[i][1]); 
            }
        }

        //xoa khoi DB roi sau khi ktra thi insert sock con song vao lai
        bookConn.Open();
        oleDbCmd.Connection = bookConn;
        oleDbCmd.CommandText = "delete from book where websource='5socks'";
        oleDbCmd.ExecuteNonQuery();
        bookConn.Close();
        new CheckProxy().GetCityOfSockHttpRequest();
        InsertProxyToDB("5socks");
        MessageBox.Show("Done");
    }

    private void checkBox2_CheckedChanged(object sender, EventArgs e)
    {
if (checkBox2.Checked)
{
    systemTime = true;
}
else
{
    systemTime = false;
}
    }

    private void checkBox4_CheckedChanged(object sender, EventArgs e)
    {
        if (!checkBox4.Checked)
        {
            System.Diagnostics.Process cmd;
            cmd = System.Diagnostics.Process.Start("resetSocksFF.exe");
            cmd.WaitForExit(); 
        }
    }

    private void checkBox3_CheckedChanged(object sender, EventArgs e)
    {
        if (!checkBox3.Checked)
        {
            ChangeProxy.ResetProxySockEntireComputer(); 
        }
    }

    private void button4_Click(object sender, EventArgs e)
    {
        ChangeProxy.SetSockEntireComputer("1.1.1.1:1");
    }

 

   
        
    }
    
}
