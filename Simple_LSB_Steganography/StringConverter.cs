using System.Text;

namespace Simple_LSB_Steganography
{
    public static class StringConverter
    {
        public static byte[] StringToBin(string parString)
        {
            return Encoding.Unicode.GetBytes(parString);
        }

        public static string BinToString(byte[] parBin)
        {
            return Encoding.Unicode.GetString(parBin);
        }
    }
}
