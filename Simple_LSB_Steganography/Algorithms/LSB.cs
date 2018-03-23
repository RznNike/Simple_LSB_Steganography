using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Simple_LSB_Steganography
{
    /// <summary>
    /// Реализация алгоритма Least Significant Bit (LSB)
    /// </summary>
    public class LSB : Algorithm
    {
        /// <summary>
        /// Терминальный символ
        /// </summary>
        private static readonly byte[] TERMINATOR = { 255, 255 };

        /// <summary>
        /// Обрабатываемое изображение
        /// </summary>
        private Bitmap Image { get; set; }

        /// <summary>
        /// Данные изображения, блокированные в памяти
        /// </summary>
        private BitmapData ImageData { get; set; }

        /// <summary>
        /// Конвертация изображения (в потоке) в пиксели (массив чисел)
        /// </summary>
        /// <param name="parImage">Изображение</param>
        /// <returns>Массив пикселей в числовом представлении</returns>
        protected override int[] StreamToPixels(Stream parImage)
        {
            // Инициализация изображения
            Image = new Bitmap(parImage);
            FixPixelFormat(Image);

            // Блокирование данных изображения в памяти
            ImageData = Image.LockBits
            (
                new Rectangle(0, 0, Image.Width, Image.Height),
                ImageLockMode.ReadWrite,
                PixelFormat.Format32bppArgb
            );

            // Копирование данных изображения в массив чисел
            int[] pixels = new int[ImageData.Width * ImageData.Height];
            Marshal.Copy(ImageData.Scan0, pixels, 0, pixels.Length);

            return pixels;
        }

        /// <summary>
        /// Приведение формата пикселей изображения к 32bppArgb
        /// </summary>
        /// <param name="parImage"></param>
        private void FixPixelFormat(Bitmap parImage)
        {
            if (parImage.PixelFormat != PixelFormat.Format32bppArgb)
            {
                parImage = parImage.Clone(new Rectangle(0, 0, parImage.Width, parImage.Height), PixelFormat.Format32bppArgb);
            }
        }

        /// <summary>
        /// Конвертация пикселей (массива чисел) в изображение (в потоке)
        /// </summary>
        /// <param name="parPixels">Массив пикселей в числовом представлении</param>
        /// <returns>Изображение</returns>
        protected override Stream PixelsToStream(int[] parPixels)
        {
            Stream result;

            // Попытка конвертации
            try
            {
                ReleaseImage(parPixels, true);
                result = new MemoryStream();
                Image.Save(result, ImageFormat.Bmp);
            }
            // Ошибка возникает, если изображение удалено из памяти.
            // Это говорит о том, что вложение сообщения было прервано.
            catch
            {
                result = null;
            }

            return result;
        }

        /// <summary>
        /// Разблокировка данных изображения в памяти
        /// </summary>
        /// <param name="parPixels">Массив измененных пикселей изображения.
        /// Может быть null, если <paramref name="parApplyChanges"/> = false</param>
        /// <param name="parApplyChanges">Флаг сохранения изменений пикселей</param>
        private void ReleaseImage(int[] parPixels, bool parApplyChanges)
        {
            if (parApplyChanges)
            {
                Marshal.Copy(parPixels, 0, ImageData.Scan0, parPixels.Length);
            }
            Image.UnlockBits(ImageData);
        }

        /// <summary>
        /// Кодирование битов сообщения внутри пикселей
        /// </summary>
        /// <param name="parMessage">Сообщение</param>
        /// <param name="parPixels">Массив пикселей</param>
        protected override void Encode(byte[] parMessage, int[] parPixels)
        {
            byte[] modifiedMessage;

            // Проверка длины сообщения на вместимость в выбранное изображение
            if ((Image.Width * Image.Height) < (parMessage.Length * 8))
            {
                Image = null;
                return;
            }
            // Формирование сообщения с терминальным символом в конце
            else if ((Image.Width * Image.Height) >= (parMessage.Length * 8 + 2))
            {
                modifiedMessage = new byte[parMessage.Length + 2];
                parMessage.CopyTo(modifiedMessage, 0);
                TERMINATOR.CopyTo(modifiedMessage, parMessage.Length);
            }
            else
            {
                modifiedMessage = parMessage;
            }

            // Помещение сообщения в пиксели
            // Цикл по байтам сообщения
            for (int i = 0; i < modifiedMessage.Length; i++)
            {
                // Цикл по битам в байте
                for (int j = 0; j < 8; j++)
                {
                    // Номер текущего пикселя в изображении
                    int index = i * 8 + j;
                    // Маска (0 или 1) для младшего бита в пикселе
                    int mask = (modifiedMessage[i] >> j) & 1;
                    // Очистка младшего бита и применение маски
                    parPixels[index] = (parPixels[index] >> 1 << 1) | mask;
                }
            }
        }

        /// <summary>
        /// Декодирование битов сообщения из массива пикселей
        /// </summary>
        /// <param name="parPixels">Массив пикселей</param>
        /// <returns>Сообщение</returns>
        protected override byte[] Decode(int[] parPixels)
        {
            List<byte> message = new List<byte>();

            // Цикл по символам (1 символ = 16 бит в кодировке UTF-16)
            for (int i = 0; i < parPixels.Length / 16; i++)
            {
                byte[] currentChar = new byte[2] { 0, 0 };

                // Цикл по байтам в символе
                for (int k = 0; k < 2; k++)
                {
                    // Цикл по битам в байте
                    for (int j = 0; j < 8; j++)
                    {
                        // Номер текущего пикселя в изображении
                        int index = (2 * i + k) * 8 + j;
                        // Младший бит в пикселе
                        int bit = (parPixels[index] & 1);
                        // Внесение бита в формируемый байт символа
                        currentChar[k] |= (byte)(bit << j);
                    }
                }

                // Если получен терминальный символ - остановка
                if (AreCharsEqual(currentChar, TERMINATOR))
                {
                    break;
                }
                // Иначе - добавление символа в сообщение
                else
                {
                    message.AddRange(currentChar);
                }
            }

            ReleaseImage(null, false);

            return message.ToArray();
        }

        /// <summary>
        /// Сравнение двух символов в кодировке UTF-16, представленных в виде набора бит
        /// </summary>
        /// <param name="parChar1">Символ 1</param>
        /// <param name="parChar2">Символ 2</param>
        /// <returns>Равны ли символы</returns>
        private bool AreCharsEqual(byte[] parChar1, byte[] parChar2)
        {
            return Encoding.Unicode.GetChars(parChar1)[0] == Encoding.Unicode.GetChars(parChar2)[0];
        }
    }
}
