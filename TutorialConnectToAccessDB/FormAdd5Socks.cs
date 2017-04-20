using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace TutorialConnectToAccessDB
{
    public partial class FormAdd5Socks : Form
    {
        bool checkProxy = false;

        public bool CheckProxy
        {
            get { return checkProxy; }
            set { checkProxy = value; }
        }

        public FormAdd5Socks()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            File.WriteAllText("rawSocks.txt", richTextBox1.Text);
            new CheckProxy().GetCityOfSockHttpRequest();
            checkProxy = true;
            this.Close();
        }

        
        
    }
}
