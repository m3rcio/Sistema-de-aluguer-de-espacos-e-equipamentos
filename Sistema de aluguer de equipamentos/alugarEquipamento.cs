using System;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using System.IO;
using System.Collections.Generic;

namespace Sistema_de_aluguer_de_equipamentos
{
    public partial class alugarEquipamentos : Form
    {
        public class Equipamento
        {
            public int Id { get; set; }
            public string Nome { get; set; }
            public bool EstaOcupado { get; set; }
            public String Descricao { get; set; }
            public Image Imagem { get; set; }
            public string ContactoResponsavel { get; set; }
        }

        public class AluguelEquipamento
        {
            public int CodigoEquipamento { get; set; }
            public string Ordenante { get; set; }
            public string Contacto { get; set; }
            public string BI { get; set; }
            public int DuracaoHoras { get; set; }
        }

        public alugarEquipamentos()
        {
            InitializeComponent();
        }

        private void alugarEquipamentos_Load(object sender, EventArgs e)
        {
            CarregarEquipamentosDoBanco();

            Timer timer = new Timer();
            timer.Interval = 60000;
            timer.Tick += (s, args) =>
            {
                VerificarEquipamentosParaLiberar();
                CarregarEquipamentosDoBanco();
            };
            timer.Start();
        }

        private void AdicionarEquipamentoAoLayout(Equipamento equipamento)
        {
            Panel card = new Panel();
            card.Width = 220;
            card.Height = 300;
            card.BorderStyle = BorderStyle.None;
            card.Margin = new Padding(10);
            card.BackColor = Color.Gray;
            card.Padding = new Padding(5);

         
            PictureBox pic = new PictureBox();
            pic.Image = equipamento.Imagem;
            pic.Width = card.Width - 10;
            pic.Height = 150;
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
            infoPanel.AutoScroll = true; 

            int labelTop = 5;

         
            Label lblNome = new Label();
            lblNome.Text = "Nome: " + equipamento.Nome;
            lblNome.AutoSize = true;
            lblNome.Top = labelTop;
            lblNome.Left = 5;
            lblNome.Font = new Font(lblNome.Font, FontStyle.Bold);
            labelTop += lblNome.Height + 3;

            
            Label lblId = new Label();
            lblId.Text = "Nº: " + equipamento.Id;
            lblId.AutoSize = true;
            lblId.Top = labelTop;
            lblId.Left = 5;
            lblId.ForeColor = Color.Black;
            labelTop += lblId.Height + 3;

          
            Label lblDescricao = new Label();
            lblDescricao.Text = "Descrição: " + equipamento.Descricao;
            lblDescricao.AutoSize = false;
            lblDescricao.Width = infoPanel.Width - 10;
            lblDescricao.Top = labelTop;
            lblDescricao.Left = 5;
            lblDescricao.ForeColor = Color.Black;
            lblDescricao.MaximumSize = new Size(infoPanel.Width - 10, 0); 
            labelTop += lblDescricao.Height + 4;

          
            Label lblEstado = new Label();
            lblEstado.Text = "Estado: " + (equipamento.EstaOcupado ? "OCUPADO" : "DISPONÍVEL");
            lblEstado.AutoSize = true;
            lblEstado.Top = labelTop;
            lblEstado.Left = 5;
            lblEstado.ForeColor = equipamento.EstaOcupado ? Color.Red : Color.Green;
            labelTop += lblEstado.Height + 5;

      
            Label lblContacto = new Label();
            lblContacto.Text = "Contacto: " + equipamento.ContactoResponsavel;
            lblContacto.AutoSize = true;
            lblContacto.Top = labelTop;
            lblContacto.Left = 5;
            lblContacto.Font = new Font(lblContacto.Font.FontFamily, 8);
            labelTop += lblContacto.Height + 6;

            
            if (labelTop > infoPanel.Height)
            {
                infoPanel.Height = labelTop + 10;
                card.Height = pic.Height + infoPanel.Height + 20;
            }

         
            infoPanel.Controls.Add(lblNome);
            infoPanel.Controls.Add(lblId);
            infoPanel.Controls.Add(lblDescricao);
            infoPanel.Controls.Add(lblEstado);
            infoPanel.Controls.Add(lblContacto);

            card.Controls.Add(pic);
            card.Controls.Add(infoPanel);

            flpEquipamentos.Controls.Add(card);
        }

        private void CarregarEquipamentosDoBanco()
        {
            flpEquipamentos.Controls.Clear();
            string connStr = "server=localhost;user=root;password=;database=sre;";

            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                conn.Open();
                string query = "SELECT Id, Nome,Descricao, EstaOcupado, Imagem, ContactoResponsavel FROM equipamentos";

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var equipamento = new Equipamento
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            Nome = reader["Nome"].ToString(),
                            Descricao = reader["Descricao"].ToString(),
                            EstaOcupado = Convert.ToBoolean(reader["EstaOcupado"]),
                            ContactoResponsavel = reader["ContactoResponsavel"].ToString()
                        };

                        if (reader["Imagem"] != DBNull.Value)
                        {
                            byte[] imagemBytes = (byte[])reader["Imagem"];
                            using (MemoryStream ms = new MemoryStream(imagemBytes))
                            {
                                equipamento.Imagem = Image.FromStream(ms);
                            }
                        }

                        AdicionarEquipamentoAoLayout(equipamento);
                    }
                }
            }
        }

        public void VerificarEquipamentosParaLiberar()
        {
            string connStr = "server=localhost;user=root;password=;database=sre;";
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                conn.Open();

                string query = @"SELECT a.codigo_equipamento, a.data_aluguel, a.duracao_horas
                   FROM aluguelequipamento a
                   INNER JOIN equipamentos e ON a.codigo_equipamento = e.Id
                   WHERE e.EstaOcupado = 1";

                MySqlCommand cmd = new MySqlCommand(query, conn);
                MySqlDataReader reader = cmd.ExecuteReader();

                List<int> equipamentosParaLiberar = new List<int>();

                while (reader.Read())
                {
                    DateTime dataAluguel = Convert.ToDateTime(reader["data_aluguel"]);
                    int duracao = Convert.ToInt32(reader["duracao_horas"]);
                    int codigo = Convert.ToInt32(reader["codigo_equipamento"]);
                    DateTime fimPrevisto = dataAluguel.AddHours(duracao);

                    if (DateTime.Now >= fimPrevisto)
                    {
                        equipamentosParaLiberar.Add(codigo);
                    }
                }

                reader.Close();

                foreach (int id in equipamentosParaLiberar)
                {
                   
                    string update = "UPDATE equipamentos SET EstaOcupado = 0 WHERE Id = @id";
                    MySqlCommand cmdUpdate = new MySqlCommand(update, conn);
                    cmdUpdate.Parameters.AddWithValue("@id", id);
                    cmdUpdate.ExecuteNonQuery();

                   
                    string deleteQuery = "DELETE FROM aluguelequipamento WHERE codigo_equipamento = @id";
                    MySqlCommand cmdDelete = new MySqlCommand(deleteQuery, conn);
                    cmdDelete.Parameters.AddWithValue("@id", id);
                    cmdDelete.ExecuteNonQuery();
                }
            }
        }


        private void btnAlugar_Click(object sender, EventArgs e)
        {
            
        }


        private void RegistrarAluguel(AluguelEquipamento aluguel)
        {
            string connStr = "server=localhost;user=root;password=;database=sre;";

            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                conn.Open();

                using (MySqlTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {
                    
                        string checkQuery = "SELECT COUNT(*) FROM equipamentos WHERE Id = @id AND EstaOcupado = 0";
                        MySqlCommand checkCmd = new MySqlCommand(checkQuery, conn, transaction);
                        checkCmd.Parameters.AddWithValue("@id", aluguel.CodigoEquipamento);
                        int count = Convert.ToInt32(checkCmd.ExecuteScalar());

                        if (count == 0)
                        {
                            MessageBox.Show("Equipamento não encontrado ou já está ocupado.");
                            return;
                        }

                       
                        string insertQuery = @"INSERT INTO aluguelequipamento 
                    (codigo_equipamento, ordenante, contacto, bi, data_aluguel, duracao_horas)
                    VALUES (@id, @ordenante, @contacto, @bi, NOW(), @duracao)";

                        MySqlCommand insertCmd = new MySqlCommand(insertQuery, conn, transaction);
                        insertCmd.Parameters.AddWithValue("@id", aluguel.CodigoEquipamento);
                        insertCmd.Parameters.AddWithValue("@ordenante", aluguel.Ordenante);
                        insertCmd.Parameters.AddWithValue("@contacto", aluguel.Contacto);
                        insertCmd.Parameters.AddWithValue("@bi", aluguel.BI);
                        insertCmd.Parameters.AddWithValue("@duracao", aluguel.DuracaoHoras);
                        insertCmd.ExecuteNonQuery();

                      
                        string updateQuery = "UPDATE equipamentos SET EstaOcupado = 1 WHERE Id = @id";
                        MySqlCommand updateCmd = new MySqlCommand(updateQuery, conn, transaction);
                        updateCmd.Parameters.AddWithValue("@id", aluguel.CodigoEquipamento);
                        updateCmd.ExecuteNonQuery();

                        transaction.Commit();
                        MessageBox.Show("Equipamento alugado com sucesso!");

                        txtCodigoEquipamento.Text = "";
                        pictureBox1 = null;
                        txtOrdenante.Text = "";
                        txtContacto.Text = "";
                        txtBI.Text = "";
                        txtDuracao.Text = "";
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        MessageBox.Show($"Erro ao registrar aluguel: {ex.Message}");
                    }
                }
            }
        }

      

        private void button1_Click(object sender, EventArgs e)
        {
            adicionarEquipamento _adicionarEquipamento = new adicionarEquipamento();
            this.Hide();
            _adicionarEquipamento.ShowDialog();
        }

        private void btnAlugar_Click_1(object sender, EventArgs e)
        {
            if (
               string.IsNullOrWhiteSpace(txtOrdenante.Text) ||
               string.IsNullOrWhiteSpace(txtContacto.Text) ||
               string.IsNullOrWhiteSpace(txtBI.Text) ||
               string.IsNullOrWhiteSpace(txtDuracao.Text))
            {
                MessageBox.Show("Por favor, preencha todos os campos.");
                return;
            }

            var aluguel = new AluguelEquipamento
            {
                CodigoEquipamento = int.Parse(txtCodigoEquipamento.Text),
                Ordenante = txtOrdenante.Text,
                Contacto = txtContacto.Text,
                BI = txtBI.Text,
                DuracaoHoras = int.Parse(txtDuracao.Text)
            };

            RegistrarAluguel(aluguel);

            CarregarEquipamentosDoBanco();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Form2 alugarEspacos = new Form2();
            this.Hide();
            alugarEspacos.ShowDialog();
        }
    }
}