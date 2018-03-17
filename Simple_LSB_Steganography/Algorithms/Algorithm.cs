using System.IO;

namespace Simple_LSB_Steganography
{
    public abstract class Algorithm
    {
        public Stream PutMessage(Stream memorizedImage, byte[] parMessage)
        {
            int[] pixels = GetPixels(memorizedImage);
            Encode(parMessage, pixels);

            return ToStream(pixels);
        }

        /// <summary>
        /// Writes pixels to an image
        /// </summary>
        /// <param name="pixels">Pixels of an original image</param>
        /// <returns></returns>
        protected abstract Stream ToStream(int[] pixels);
        /// <summary>
        /// Encodes the <paramref name="message"/> to the <paramref name="pixels"/>
        /// </summary>
        /// <param name="message">Message to be encoded</param>
        /// <param name="pixels">Pixels of an original image</param>
        protected abstract void Encode(byte[] message, int[] pixels);
        /// <summary>
        /// Executes pixels from the <paramref name="image"/>
        /// </summary>
        /// <param name="image">Container of a message</param>
        /// <returns></returns>
        protected abstract int[] GetPixels(Stream image);

        public byte[] GetMessage(Stream image)
        {
            int[] pixels = GetPixels(image);

            return Decode(pixels);
        }

        internal abstract byte[] Decode(int[] pixels);
    }
}
