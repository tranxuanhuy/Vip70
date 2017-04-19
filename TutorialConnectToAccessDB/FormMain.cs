
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
        public FormMain()
        {
            //create connection using parameter from mdsaputra.udl
            bookConn = new OleDbConnection(connParam);
            InitializeComponent();
        }

        private void buttonInsert_Click(object sender, EventArgs e)
        {

            bookConn.Open();
            oleDbCmd.Connection = bookConn;
            oleDbCmd.CommandText = "delete from book";
            oleDbCmd.ExecuteNonQuery();

            for (int i = 0; i <= File.ReadAllLines("proxyWithCityHttpRequest.txt").Count()-1; i++)
            {
                string proxy = ReadProxyAtLine(i + 1, "proxyWithCityHttpRequest.txt");
                try
                {
                    oleDbCmd.CommandText = "insert into book (IP, country,state,city) values ('" + proxy.Replace("\t", "\',\'") + "');";
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
            bookConn.Close();
            ListviewReload("select distinct country from book order by country desc");
            GridviewReload("select * from book where country='United States'"); 
            label3.Invoke((System.Windows.Forms.MethodInvoker)(() => label3.Text = "Record Successfuly Added!"));
                    }

        private void buttonShowAll_Click(object sender, EventArgs e)
        {
            dataGridView1.DataSource = null;
            dataGridView1.Rows.Clear();
            dataGridView1.Refresh();

            OleDbDataAdapter dAdapter = new OleDbDataAdapter("select * from book", connParam);
            OleDbCommandBuilder cBuilder = new OleDbCommandBuilder(dAdapter);
            
            DataTable dataTable = new DataTable();
            DataSet ds = new DataSet();
            
            dAdapter.Fill(dataTable);

            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                dataGridView1.Rows.Add(dataTable.Rows[i][1], dataTable.Rows[i][2], dataTable.Rows[i][3], dataTable.Rows[i][4]);
            }
                
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Stream myStream = null;
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            openFileDialog1.InitialDirectory = "c:\\";
            openFileDialog1.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 2;
            openFileDialog1.RestoreDirectory = true;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    proxyfile=openFileDialog1.FileName;
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
            GetCityOfProxyHttpRequest(label3);
        }

        //filechooser txt file => proxyWithCityHttpRequest.txt
    public void GetCityOfProxyHttpRequest(System.Windows.Forms.Label label3)
    {
        File.Delete("proxyWithCityHttpRequest.txt");
        int block = 50;
        for (int j = 0; j <= File.ReadAllLines(proxyfile).Count() / block; j++)
        {
            label3.Invoke((System.Windows.Forms.MethodInvoker)(() => label3.Text = "checking..." + j + "/" + File.ReadAllLines(proxyfile).Count() / block));
            ResetProxySockEntireComputer();

            MyThread[] thr = new MyThread[1000];
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
                thr[i] = new MyThread();
                tid[i] = new Thread(new ThreadStart(thr[i].Thread1));
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

    public void ResetProxySockEntireComputer()
    {
        RegistryKey registry = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Internet Settings", true);
        registry.SetValue("ProxyEnable", 0);
        // These lines implement the Interface in the beginning of program 
        // They cause the OS to refresh the settings, causing IP to realy update
        settingsReturn = InternetSetOption(IntPtr.Zero, INTERNET_OPTION_SETTINGS_CHANGED, IntPtr.Zero, 0);
        refreshReturn = InternetSetOption(IntPtr.Zero, INTERNET_OPTION_REFRESH, IntPtr.Zero, 0);
    }
    public string ReadProxyAtLine(int p, string file)
    {
        string proxy = File.ReadLines(file).Skip(p - 1).First();
        string[] aproxy = proxy.Split(':');
        return aproxy[0] + ':' + aproxy[1];
    }

    private void FormMain_Load(object sender, EventArgs e)
    {
        try
        {
            File.Copy("demo.ppx", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Proxifier\Profiles\demo.ppx");

        }
        catch (Exception)
        {
           
        }
        ListviewReload("select distinct country from book order by country desc");
        GridviewReload("select * from book where country='United States'"); 
    }

    private void ListviewReload(string sqlcommand)
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
            dataGridView1.Rows.Add(dataTable.Rows[i][1], dataTable.Rows[i][2], dataTable.Rows[i][3], dataTable.Rows[i][4]);
        }
    }

    private void button3_Click(object sender, EventArgs e)
    {
        ListviewReload("select distinct state from book where country='" + listView1.SelectedItems[0].Text + "'");
        stateSelected = true;
    }

    private void button2_Click(object sender, EventArgs e)
    {
        ListviewReload("select distinct country from book");
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
        ProxifierReload("127.0.0.1",9951);
    }

    private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
    {
        ProxifierReload(dataGridView1.CurrentCell.Value.ToString().Split(':')[0],1085);
    }

    private void ProxifierReload(string proxy,int port)
    {
        string filename = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)+@"\Proxifier\Profiles\demo.ppx";
        lineChanger("<Address>" + proxy + "</Address>", filename, 20);
        lineChanger("<Port>" + port + "</Port>", filename, 21);
        var proc = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = System.Environment.GetFolderPath(
        System.Environment.SpecialFolder.ProgramFiles) +@"\Proxifier\Proxifier.exe",
                Arguments = filename,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true,

                //WorkingDirectory = @"C:\MyAndroidApp\"
            }
        };

        proc.Start();
    }
        
    }
    
}
