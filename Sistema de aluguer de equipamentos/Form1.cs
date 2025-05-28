using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;


namespace Sistema_de_aluguer_de_equipamentos
{
    public partial class Form1 : Form
    {
        

        public Form1()
        {
            InitializeComponent();
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Form2 form2 = new Form2();
            if(textBox1.Text=="jose" && textBox2.Text == "1234")
            {
                MessageBox.Show("login feito!");
                this.Hide();
                form2.Show();
            }else
            {
                MessageBox.Show("erro no login!");
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            TestarConexao();
        }

        private void TestarConexao()
        {
            string connStr = "server=localhost;user=root;password=;database=sre;";
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();

                    string query = "SELECT * FROM espacos";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    MySqlDataReader reader = cmd.ExecuteReader();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Erro: " + ex.Message, "Erro");
                }
            }
        }
    }
}
