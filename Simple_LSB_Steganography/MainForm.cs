using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Simple_LSB_Steganography
{
    public partial class MainForm : Form
    {
        private Stream _originalImage = null;
        private Stream _resultImage = null;
        private string _fileExtension = null;

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
                    _fileExtension = Path.GetExtension(openFileDialog.FileName).TrimStart('.');
                }
                catch
                {
                    MessageBox.Show("Can't open image.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
        }

        private void tsmSaveResult_Click(object sender, EventArgs e)
        {
            if (_resultImage == null)
            {
                MessageBox.Show("You should apply algorithm to get result.", "No result", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            saveFileDialog.DefaultExt = _fileExtension;
            saveFileDialog.Filter = string.Format("Image|*.{0}", _fileExtension);
            saveFileDialog.FileName = openFileDialog.FileName;
            DialogResult choice = saveFileDialog.ShowDialog();
            if (choice == DialogResult.OK)
            {
                try
                {
                    Image image = Image.FromStream(_resultImage);
                    image.Save(saveFileDialog.FileName);
                }
                catch
                {
                    MessageBox.Show("Can't save image.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
        }

        private void tsmApplyAlgorithm_Click(object sender, EventArgs e)
        {
            if (_originalImage == null)
            {
                MessageBox.Show("You should select original image first.", "No target image", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            pboxResult.Image = new Bitmap(pboxOriginal.Image);
        }
    }
}
