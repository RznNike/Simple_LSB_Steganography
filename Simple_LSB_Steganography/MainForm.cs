using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace Simple_LSB_Steganography
{
    public partial class MainForm : Form
    {
        private Stream _originalImage = null;
        private Stream _resultImage = null;

        public MainForm()
        {
            InitializeComponent();
        }

        private void tsmExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void tsmOpenImage_Click(object sender, EventArgs e)
        {
            openFileDialog.Multiselect = false;
            DialogResult choice = openFileDialog.ShowDialog();
            if (choice == DialogResult.OK)
            {
                try
                {
                    _originalImage = new MemoryStream(File.ReadAllBytes(openFileDialog.FileName));
                    pboxOriginal.Image = new Bitmap(_originalImage);
                }
                catch
                {
                    MessageBox.Show("Не получилось открыть изображение.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
        }

        private void tsmOpenImageToDecode_Click(object sender, EventArgs e)
        {
            openFileDialog.Multiselect = false;
            DialogResult choice = openFileDialog.ShowDialog();
            if (choice == DialogResult.OK)
            {
                try
                {
                    _resultImage = new MemoryStream(File.ReadAllBytes(openFileDialog.FileName));
                    pboxResult.Image = new Bitmap(_resultImage);
                }
                catch
                {
                    MessageBox.Show("Не получилось открыть изображение.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
        }

        private void tsmSaveResult_Click(object sender, EventArgs e)
        {
            if (_resultImage == null)
            {
                MessageBox.Show("Сначала примените алгоритм для получения результата", "Нечего сохранять",
                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            saveFileDialog.DefaultExt = "png";
            saveFileDialog.Filter = string.Format("Image|*.{0}", "png");
            saveFileDialog.FileName = openFileDialog.FileName;
            DialogResult choice = saveFileDialog.ShowDialog();
            if (choice == DialogResult.OK)
            {
                try
                {
                    Image image = Image.FromStream(_resultImage);
                    image.Save(saveFileDialog.FileName, ImageFormat.Png);
                }
                catch
                {
                    MessageBox.Show("Не получилось сохранить изображение.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
        }

        private void tsmApplyAlgorithm_Click(object sender, EventArgs e)
        {
            if (_originalImage == null)
            {
                MessageBox.Show("Сначала загрузите изображение.", "Нет целевого изображения", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            if (tbInputText.Text == "")
            {
                MessageBox.Show("Сначала введите текст сообщения.", "Нет сообщения", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            byte[] binMessage = StringConverter.StringToBin(tbInputText.Text);
            Algorithm algorithm = new LSB();
            _resultImage = algorithm.PutMessage(_originalImage, binMessage);
            pboxResult.Image = Image.FromStream(_resultImage);

            btnDecodeMessage_Click(sender, e);
        }

        private void btnDecodeMessage_Click(object sender, EventArgs e)
        {
            if (_resultImage == null)
            {
                MessageBox.Show("Сначала загрузите изображение.", "Нет целевого изображения", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            Algorithm algorithm = new LSB();
            byte[] binMessage = algorithm.GetMessage(_resultImage);
            string message = StringConverter.BinToString(binMessage);
            tbOutputText.Text = message;
        }
    }
}
