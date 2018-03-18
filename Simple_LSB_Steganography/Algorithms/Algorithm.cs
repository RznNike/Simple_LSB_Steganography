using System.IO;

namespace Simple_LSB_Steganography
{
    /// <summary>
    /// Базовый класс алгоритмов стеганографии
    /// </summary>
    public abstract class Algorithm
    {
        /// <summary>
        /// Помещение сообщения в изображение
        /// </summary>
        /// <param name="parImage">Изображение-контейнер</param>
        /// <param name="parMessage">Сообщение</param>
        /// <returns>Изображение с помещенным в него сообщением</returns>
        public Stream PutMessage(Stream parImage, byte[] parMessage)
        {
            int[] pixels = StreamToPixels(parImage);
            Encode(parMessage, pixels);

            return PixelsToStream(pixels);
        }

        /// <summary>
        /// Извлечение сообщения из изображения
        /// </summary>
        /// <param name="parImage">Изображение со скрытым в нем сообщением</param>
        /// <returns>Сообщение</returns>
        public byte[] GetMessage(Stream parImage)
        {
            int[] pixels = StreamToPixels(parImage);

            return Decode(pixels);
        }

        /// <summary>
        /// Конвертация изображения (в потоке) в пиксели (массив чисел)
        /// </summary>
        /// <param name="parImage">Изображение</param>
        /// <returns>Массив пикселей в числовом представлении</returns>
        protected abstract int[] StreamToPixels(Stream parImage);

        /// <summary>
        /// Конвертация пикселей (массива чисел) в изображение (в потоке)
        /// </summary>
        /// <param name="parPixels">>Массив пикселей в числовом представлении</param>
        /// <returns>Изображение</returns>
        protected abstract Stream PixelsToStream(int[] parPixels);

        /// <summary>
        /// Кодирование битов сообщения внутри пикселей
        /// </summary>
        /// <param name="parMessage">Сообщение</param>
        /// <param name="parPixels">Массив пикселей</param>
        protected abstract void Encode(byte[] parMessage, int[] parPixels);

        /// <summary>
        /// Декодирование битов сообщения из массива пикселей
        /// </summary>
        /// <param name="parPixels">Массив пикселей</param>
        /// <returns>Сообщение</returns>
        protected abstract byte[] Decode(int[] parPixels);
    }
}
