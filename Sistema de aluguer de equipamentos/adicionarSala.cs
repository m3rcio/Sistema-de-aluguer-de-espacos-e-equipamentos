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

    
    public partial class adicionarSala : Form
    {
        Form2 _form2 = new Form2();
        List<Espaco> listaEspacos = new List<Espaco>();
        public adicionarSala()
        {
            InitializeComponent();
            cmbOcupado.Items.Add("Ocupado");
            cmbOcupado.Items.Add("Disponível");
            cmbOcupado.SelectedIndex = 1;
        }

        private void adicionarSala_Load(object sender, EventArgs e)
        {

        }

        public class Espaco
        {
            public int Id { get; set; }
            public string Nome { get; set; }
            public bool EstaOcupado { get; set; }
            public Image Imagem { get; set; }
            public string Localidade { get; set; }
            public string Contacto { get; set; }

            public Espaco(int id,string nome, bool estaOcupado, Image imagem, string localidade, string contacto)
            {
                Id = id;
                Nome = nome;
                EstaOcupado = estaOcupado;
                Imagem = imagem;
                Localidade = localidade;
                Contacto = contacto;
            }

            public override string ToString()
            {
                return Nome + " - " + (EstaOcupado ? "Ocupado" : "Disponível");
            }

          
        }

        private void SalvarNoBanco(Espaco espaco)
        {
            string connStr = "server=localhost;user=root;password=;database=sre;";
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                conn.Open();
                string query = "INSERT INTO espacos (Nome, EstaOcupado, Imagem, Localidade, Contacto) VALUES (@Nome, @EstaOcupado, @Imagem, @Localidade, @Contacto)";
                MySqlCommand cmd = new MySqlCommand(query, conn);

                cmd.Parameters.AddWithValue("@Nome", espaco.Nome);
                cmd.Parameters.AddWithValue("@EstaOcupado", espaco.EstaOcupado);
                cmd.Parameters.AddWithValue("@Imagem", ImageToByteArray(espaco.Imagem));
                cmd.Parameters.AddWithValue("@Localidade", espaco.Localidade);
                cmd.Parameters.AddWithValue("@Contacto", espaco.Contacto);

                cmd.ExecuteNonQuery();
            }
        }

        private byte[] ImageToByteArray(Image image)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                return ms.ToArray();
            }
        }


        private void button1_Click(object sender, EventArgs e)
        {

           
            if (string.IsNullOrWhiteSpace(txtNome.Text)) return;

            Espaco novo = new Espaco(
     id: 0,
     nome: txtNome.Text,
     estaOcupado: cmbOcupado.SelectedItem?.ToString() == "Ocupado",
     imagem: picImagem.Image,
     localidade: txtLocal.Text,
     contacto: txtContacto.Text
 );
            if (string.IsNullOrWhiteSpace(txtNome.Text) ||
    string.IsNullOrWhiteSpace(txtLocal.Text) ||
    string.IsNullOrWhiteSpace(txtContacto.Text))
            {
                MessageBox.Show("Por favor, preencha todos os campos obrigatórios.", "Campos obrigatórios", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            else
            {
                SalvarNoBanco(novo);

                txtNome.Text = "";
                txtLocal.Text = "";
                txtContacto.Text = "";
                picImagem.Image = null;
                this._form2.AdicionarEspacoAoLayout(novo);
            }
        

        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            this.Close();
            this._form2.Show();
        }

        private void picImagem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

           
            openFileDialog.Filter = "Arquivos de Imagem|*.jpg;*.jpeg;*.png;*.bmp;*.gif";

            openFileDialog.Title = "Selecione uma imagem";

       
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                
                picImagem.Image = new System.Drawing.Bitmap(openFileDialog.FileName);

              
                picImagem.SizeMode = PictureBoxSizeMode.StretchImage;
            }
        }
    }


}
