using System.IO;

namespace Simple_LSB_Steganography
{
    public abstract class Algorithm
    {
        public abstract Stream PutMessage(Stream parImage, byte[] parMessage);

        public abstract byte[] GetMessage(Stream parImage);
    }
}
