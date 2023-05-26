using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace RoseOnlineBot
{
    public partial class SelectProcess : Form
    {
        public SelectProcess(Process[] processes)
        {
            InitializeComponent();
            foreach (Process p in processes)
            {
                comboBox1.Items.Add(p.Id);
            }
        }
    }
}
