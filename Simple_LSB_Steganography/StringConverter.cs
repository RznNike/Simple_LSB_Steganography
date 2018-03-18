using System.Text;

namespace Simple_LSB_Steganography
{
    /// <summary>
    /// Конвертер между строковыми и бинарными данными
    /// </summary>
    public static class StringConverter
    {
        /// <summary>
        /// Конвертация строки в байты
        /// </summary>
        /// <param name="parString">Строка</param>
        /// <returns>Байты</returns>
        public static byte[] StringToBin(string parString)
        {
            return Encoding.Unicode.GetBytes(parString);
        }

        /// <summary>
        /// Конвертация байтов в строку
        /// </summary>
        /// <param name="parBin">Байты</param>
        /// <returns>Строка</returns>
        public static string BinToString(byte[] parBin)
        {
            return Encoding.Unicode.GetString(parBin);
        }
    }
}
