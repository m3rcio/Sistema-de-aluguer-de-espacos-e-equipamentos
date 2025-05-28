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
using System.IO;

namespace Sistema_de_aluguer_de_equipamentos
{
    public partial class adicionarEquipamento : Form
    {
        public adicionarEquipamento()
        {
            InitializeComponent();
            cmbOcupado.Items.Add("Ocupado");
            cmbOcupado.Items.Add("Disponível");
            cmbOcupado.SelectedIndex = 1;
        }

        public class Equipamento
        {
            public int Id { get; set; }
            public string Nome { get; set; }
            public string Descricao { get; set; }
            public bool EstaOcupado { get; set; }
            public Image Imagem { get; set; }
            public string ContactoResponsavel { get; set; }

            public Equipamento(int id, string nome, string descricao, bool estaOcupado, Image imagem, string contactoResponsavel)
            {
                Id = id;
                Nome = nome;
                Descricao = descricao;
                EstaOcupado = estaOcupado;
                Imagem = imagem;
                ContactoResponsavel = contactoResponsavel;
            }
        }

        private void btnCarregarImagem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Imagens|*.jpg;*.jpeg;*.png;*.bmp";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                picImagem.Image = Image.FromFile(openFileDialog.FileName);
            }
        }

        private void adicionarEquipamento_Load(object sender, EventArgs e)
        {

        }

        private void adicionarBtn_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNome.Text) ||
                string.IsNullOrWhiteSpace(txtDescricao.Text) ||
                string.IsNullOrWhiteSpace(txtContacto.Text))
            {
                MessageBox.Show("Por favor, preencha todos os campos obrigatórios.", "Campos obrigatórios", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Equipamento novo = new Equipamento(
                id: 0,
                nome: txtNome.Text,
                descricao: txtDescricao.Text,
                estaOcupado: cmbOcupado.SelectedItem?.ToString() == "Ocupado",
                imagem: picImagem.Image,
                contactoResponsavel: txtContacto.Text
            );

            SalvarNoBanco(novo);

            // Limpar campos
            txtNome.Text = "";
            txtDescricao.Text = "";
            txtContacto.Text = "";
            picImagem.Image = null;

            MessageBox.Show("Equipamento adicionado com sucesso!");

        }

        private void SalvarNoBanco(Equipamento equipamento)
        {
            string connStr = "server=localhost;user=root;password=;database=sre;";
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                conn.Open();
                string query = "INSERT INTO equipamentos (Nome, Descricao, EstaOcupado, Imagem, ContactoResponsavel) " +
                               "VALUES (@Nome, @Descricao, @EstaOcupado, @Imagem, @ContactoResponsavel)";
                MySqlCommand cmd = new MySqlCommand(query, conn);

                cmd.Parameters.AddWithValue("@Nome", equipamento.Nome);
                cmd.Parameters.AddWithValue("@Descricao", equipamento.Descricao);
                cmd.Parameters.AddWithValue("@EstaOcupado", equipamento.EstaOcupado);
                cmd.Parameters.AddWithValue("@Imagem", ImageToByteArray(equipamento.Imagem));
                cmd.Parameters.AddWithValue("@ContactoResponsavel", equipamento.ContactoResponsavel);

                cmd.ExecuteNonQuery();
            }
        }

        private byte[] ImageToByteArray(Image image)
        {
            if (image == null) return null;
            using (MemoryStream ms = new MemoryStream())
            {
                image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                return ms.ToArray();
            }
        }

        private void picImagem_Click_1(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Arquivos de Imagem|*.jpg;*.jpeg;*.png;*.bmp;*.gif";
            openFileDialog.Title = "Selecione uma imagem";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                picImagem.Image = new Bitmap(openFileDialog.FileName);
                picImagem.SizeMode = PictureBoxSizeMode.StretchImage;
            }
        }

        private void picImagem_Click(object sender, EventArgs e)
        {
           
        }

        private void button1_Click(object sender, EventArgs e)
        {
            alugarEquipamentos _alugarEquipamento = new alugarEquipamentos();
            this.Hide();
            _alugarEquipamento.ShowDialog();
        }

        
    }
}
