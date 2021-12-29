using System;
using System.Windows.Forms;

namespace WPF_KINL_Server
{
    public partial class Server : Form
    {
        public Server()
        {
            InitializeComponent();
        }
        int i = 0;
        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void serverResetButton_Click(object sender, EventArgs e)
        {
            serverStateLabel.Text = "ServerClose";
            i++;
            dataGridView1.Rows.Add("??", "???", "??????", "제발!", "맞냐", i);
        }

        private void RefreshButton_Click(object sender, EventArgs e)
        {
            if (dataGridView1.Rows.Count > 1)
            {
                dataGridView1.Rows.Remove(dataGridView1.Rows[3]);
            }
        }
        private void dataGridView1_CellContentClick_1(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}
