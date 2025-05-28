using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Sistema_de_aluguer_de_equipamentos.adicionarSala;
using MySql.Data.MySqlClient;
using System.IO;

namespace Sistema_de_aluguer_de_equipamentos
{
    public partial class Form2 : Form
    {


        public void AdicionarEspacoAoLayout(Espaco espaco)
        {
           
            Panel card = new Panel();
            card.Width = 220;  
            card.Height = 300; 
            card.BorderStyle = BorderStyle.None;
            card.Margin = new Padding(10);
            card.BackColor = Color.Gray;
            card.Padding = new Padding(5);

         
            PictureBox pic = new PictureBox();
            pic.Image = espaco.Imagem;
            pic.Width = card.Width - 10;
            pic.Height = 180;           
            pic.SizeMode = PictureBoxSizeMode.Zoom;
            pic.Top = 5;
            pic.Left = 5;
            pic.BackColor = Color.White;

         
            Panel infoPanel = new Panel();
            infoPanel.Width = card.Width - 10;
            infoPanel.Height = card.Height - pic.Height - 15;
            infoPanel.Top = pic.Bottom + 5;
            infoPanel.Left = 5;
            infoPanel.BackColor = Color.LightGray;

          
            int labelTop = 5; 

            Label lblNome = new Label();
            lblNome.Text = "Nome: " + espaco.Nome;
            lblNome.AutoSize = true;
            lblNome.Top = labelTop;
            lblNome.Left = 5;
            lblNome.ForeColor = Color.Black;
            lblNome.Font = new Font(lblNome.Font, FontStyle.Bold);
            labelTop += lblNome.Height + 2;

            Label lblId = new Label();
            lblId.Text = "Nº: " + espaco.Id;
            lblId.AutoSize = true;
            lblId.Top = labelTop;
            lblId.Left = 5;
            lblId.ForeColor = Color.Black;
            labelTop += lblId.Height + 2;

            Label lblEstado = new Label();
            lblEstado.Text = "Estado: " + (espaco.EstaOcupado ? "Ocupado" : "Disponível");
            lblEstado.AutoSize = true;
            lblEstado.Top = labelTop;
            lblEstado.Left = 5;
            lblEstado.ForeColor = espaco.EstaOcupado ? Color.Red : Color.Green;
            labelTop += lblEstado.Height + 2;

            Label lblLocal = new Label();
            lblLocal.Text = "Local: " + espaco.Localidade;
            lblLocal.AutoSize = true;
            lblLocal.Top = labelTop;
            lblLocal.Left = 5;
            lblLocal.ForeColor = Color.Black;
            labelTop += lblLocal.Height + 2;

            Label lblContacto = new Label();
            lblContacto.Text = "Contacto: " + espaco.Contacto;
            lblContacto.AutoSize = true;
            lblContacto.Top = labelTop;
            lblContacto.Left = 5;
            lblContacto.ForeColor = Color.Black;

       
            infoPanel.Controls.Add(lblNome);
            infoPanel.Controls.Add(lblId);
            infoPanel.Controls.Add(lblEstado);
            infoPanel.Controls.Add(lblLocal);
            infoPanel.Controls.Add(lblContacto);

            card.Controls.Add(pic);
            card.Controls.Add(infoPanel);

            flpEspacos.Controls.Add(card);
        }



        public Form2()
        {
           
            InitializeComponent();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            Timer timer = new Timer();
            timer.Interval = 60000; 
            timer.Tick += (s, args) =>
            {
                VerificarEspacosParaLiberar();
                CarregarEspacosDoBanco();
            };
            timer.Start();

            CarregarEspacosDoBanco();
        }


        public class Aluguel
        {
            public int CodigoEspaco { get; set; }
            public int Id { get; set; }
            public string Ordenante { get; set; }
            public string Contacto { get; set; }
            public string BI { get; set; }
            public int DuracaoHoras { get; set; }

            public Aluguel(int codigoEspaco, string ordenante, string contacto, string bi, int duracaoHoras)
            {
                CodigoEspaco = codigoEspaco;
                Ordenante = ordenante;
                Contacto = contacto;
                BI = bi;
                DuracaoHoras = duracaoHoras;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            adicionarSala _adicionarSala = new adicionarSala();
            this.Close();
            _adicionarSala.Show();
        }

        private void flpEspacos_Paint(object sender, EventArgs e)
        {
            
        }

        private void CarregarEspacosDoBanco()
        {
            flpEspacos.Controls.Clear();
            string connStr = "server=localhost;user=root;password=;database=sre;";
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                conn.Open();
                string query = "SELECT * FROM espacos";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                MySqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    int id = Convert.ToInt32(reader["Id"]);
                    string nome = reader["Nome"].ToString();
                    bool estaOcupado = Convert.ToBoolean(reader["EstaOcupado"]);
                    string localidade = reader["Localidade"].ToString();
                    string contacto = reader["Contacto"].ToString();

                    byte[] imagemBytes = (byte[])reader["Imagem"];
                    Image imagem;
                    using (MemoryStream ms = new MemoryStream(imagemBytes))
                    {
                        imagem = Image.FromStream(ms);
                    }

                    Espaco espaco = new Espaco(id,nome, estaOcupado, imagem, localidade, contacto);
                    AdicionarEspacoAoLayout(espaco);
                }
            }
        }

        public void VerificarEspacosParaLiberar()
        {
            string connStr = "server=localhost;user=root;password=;database=sre;";
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                conn.Open();

                string query = @"SELECT a.codigo_espaco, a.data_aluguel, a.duracao_horas
                         FROM alugueis a
                         INNER JOIN espacos e ON a.codigo_espaco = e.Id
                         WHERE e.EstaOcupado = 1";

                MySqlCommand cmd = new MySqlCommand(query, conn);
                MySqlDataReader reader = cmd.ExecuteReader();

                List<int> espacosParaLiberar = new List<int>();

                while (reader.Read())
                {
                    DateTime dataAluguel = Convert.ToDateTime(reader["data_aluguel"]);
                    int duracao = Convert.ToInt32(reader["duracao_horas"]);
                    int codigo = Convert.ToInt32(reader["codigo_espaco"]);
                    DateTime fimPrevisto = dataAluguel.AddHours(duracao);

            

                    if (DateTime.Now >= fimPrevisto)
                    {
                        espacosParaLiberar.Add(codigo);
                    }
                }

                reader.Close();

                foreach (int id in espacosParaLiberar)
                {
                    string update = "UPDATE espacos SET EstaOcupado = 0 WHERE Id = @id";
                    MySqlCommand cmdUpdate = new MySqlCommand(update, conn);
                    cmdUpdate.Parameters.AddWithValue("@id", id);
                    cmdUpdate.ExecuteNonQuery();

                   
                    string deleteQuery = "DELETE FROM alugueis WHERE codigo_espaco = @id";
                    MySqlCommand cmdDelete = new MySqlCommand(deleteQuery, conn);
                    cmdDelete.Parameters.AddWithValue("@id", id);
                    cmdDelete.ExecuteNonQuery();
                }
            }
        }

        private void flpEspacos_Paint(object sender, PaintEventArgs e)
        {

        }


        public void AdicionarAluguel(Aluguel aluguel)
        {
            string connStr = "server=localhost;user=root;password=;database=sre;";
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                conn.Open();

                string checkQuery = "SELECT COUNT(*) FROM espacos WHERE Id = @codigo";
                MySqlCommand checkCmd = new MySqlCommand(checkQuery, conn);
                checkCmd.Parameters.AddWithValue("@codigo", aluguel.CodigoEspaco);
                int count = Convert.ToInt32(checkCmd.ExecuteScalar());
                if (count == 0)
                {
                    MessageBox.Show("O espaço com este código não existe.");
                    return;
                }
                string checkEspaco = "SELECT EstaOcupado FROM espacos WHERE Id = @codigo";
                MySqlCommand cmd = new MySqlCommand(checkEspaco, conn);
                cmd.Parameters.AddWithValue("@codigo", aluguel.CodigoEspaco);

                object result = cmd.ExecuteScalar();

                bool estaOcupado = Convert.ToBoolean(result);
                if (estaOcupado)
                {
                    MessageBox.Show("Equipamento não encontrado ou já está ocupado.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                DateTime dataAluguel = DateTime.Now;

                string insertQuery = @"INSERT INTO alugueis (codigo_espaco, ordenante, contacto, bi, data_aluguel, duracao_horas)
                               VALUES (@codigo, @ordenante, @contacto, @bi, @data, @duracao)";
                MySqlCommand cmdInsert = new MySqlCommand(insertQuery, conn);
                cmdInsert.Parameters.AddWithValue("@codigo", aluguel.CodigoEspaco);
                cmdInsert.Parameters.AddWithValue("@ordenante", aluguel.Ordenante);
                cmdInsert.Parameters.AddWithValue("@contacto", aluguel.Contacto);
                cmdInsert.Parameters.AddWithValue("@bi", aluguel.BI);
                cmdInsert.Parameters.AddWithValue("@data", dataAluguel);
                cmdInsert.Parameters.AddWithValue("@duracao", aluguel.DuracaoHoras);
                cmdInsert.ExecuteNonQuery();

            
                string updateQuery = "UPDATE espacos SET EstaOcupado = 1 WHERE Id = @codigo";
                MySqlCommand cmdUpdate = new MySqlCommand(updateQuery, conn);
                cmdUpdate.Parameters.AddWithValue("@codigo", aluguel.CodigoEspaco);
                cmdUpdate.ExecuteNonQuery();
                MessageBox.Show("Aluguel registrado com sucesso!");
            }
        }


        private void button2_Click_1(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCodigoEspaco.Text) ||
       string.IsNullOrWhiteSpace(txtOrdenante.Text) ||
       string.IsNullOrWhiteSpace(txtContacto.Text) ||
       string.IsNullOrWhiteSpace(txtBI.Text) ||
       string.IsNullOrWhiteSpace(txtDuracao.Text))
            {
                MessageBox.Show("Por favor, preencha todos os campos.");
                return;
            }

            Aluguel novo = new Aluguel(
                codigoEspaco: int.Parse(txtCodigoEspaco.Text),
                ordenante: txtOrdenante.Text,
                contacto: txtContacto.Text,
                bi: txtBI.Text, 
                duracaoHoras: int.Parse(txtDuracao.Text)
            );

            AdicionarAluguel(novo);
            CarregarEspacosDoBanco();

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {
           
        }

        private void button3_Click(object sender, EventArgs e)
        {
           
        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            alugarEquipamentos _alugarEquipamentos = new alugarEquipamentos();
            this.Hide();
            _alugarEquipamentos.ShowDialog();
        }
    }
}
