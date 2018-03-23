using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace Simple_LSB_Steganography
{
    /// <summary>
    /// Форма приложения
    /// </summary>
    public partial class MainForm : Form
    {
        /// <summary>
        /// Оригинальное изображение
        /// </summary>
        private Stream _originalImage = null;

        /// <summary>
        /// Изображение со сскрытым сообщением
        /// </summary>
        private Stream _resultImage = null;

        /// <summary>
        /// Реализация стеганографического алгоритма
        /// </summary>
        private Algorithm _algorithm = new LSB();

        /// <summary>
        /// Конструктор формы
        /// </summary>
        public MainForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Обработка нажатия на кнопку выхода
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsmExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        /// <summary>
        /// Обработка нажатия на кнопку открытия исходного изображения
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsmOpenImage_Click(object sender, EventArgs e)
        {
            LoadImage(ref _originalImage, pboxOriginal);
        }

        /// <summary>
        /// Обработка нажатия на кнопку открытия изображения со скрытым сообением
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsmOpenImageToDecode_Click(object sender, EventArgs e)
        {
            LoadImage(ref _resultImage, pboxResult);
            btnDecodeMessage_Click(sender, e);
        }

        /// <summary>
        /// Загрузка изображения в память и на форму
        /// </summary>
        /// <param name="refImageStream">Переменная для хранения изображения</param>
        /// <param name="parPictureBox">PictureBox для показа изображения</param>
        private void LoadImage(ref Stream refImageStream, PictureBox parPictureBox)
        {
            openFileDialog.Multiselect = false;
            DialogResult choice = openFileDialog.ShowDialog();
            if (choice == DialogResult.OK)
            {
                try
                {
                    refImageStream = new MemoryStream(File.ReadAllBytes(openFileDialog.FileName));
                    parPictureBox.Image = new Bitmap(refImageStream);
                }
                catch
                {
                    MessageBox.Show("Не получилось открыть изображение.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
        }

        /// <summary>
        /// Обработка нажатия на кнопку сохранения результата. Сохранение в PNG.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
            saveFileDialog.FileName = string.Format("{0}_stego", openFileDialog.FileName);
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

        /// <summary>
        /// Обработка нажатия на кнопку применения алгоритма
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
            _resultImage = _algorithm.PutMessage(_originalImage, binMessage);

            if (_resultImage == null)
            {
                MessageBox.Show("Не получилось вложить сообщение. Попробуйте уменьшить длину сообщения.", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            pboxResult.Image = Image.FromStream(_resultImage);
            btnDecodeMessage_Click(sender, e);
        }

        /// <summary>
        /// Обработка нажатия на кнопку извлечения сообщения из изображения
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDecodeMessage_Click(object sender, EventArgs e)
        {
            if (_resultImage == null)
            {
                MessageBox.Show("Сначала загрузите изображение.", "Нет целевого изображения", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            try
            {
                byte[] binMessage = _algorithm.GetMessage(_resultImage);
                string message = StringConverter.BinToString(binMessage);
                tbOutputText.Text = message;
            }
            catch
            {
                MessageBox.Show("Не удалось извлечь сообщение.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        /// <summary>
        /// Обработка нажатия на пункт меню "О программе"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsmAboutProgram_Click(object sender, EventArgs e)
        {
            Form aboutForm = new AboutForm();
            aboutForm.ShowDialog();
        }
    }
}
