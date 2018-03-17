using System.IO;

namespace Simple_LSB_Steganography
{
    public abstract class Algorithm
    {
        public Stream PutMessage(Stream parImage, byte[] parMessage)
        {
            int[] pixels = StreamToPixels(parImage);
            Encode(parMessage, pixels);

            return PixelsToStream(pixels);
        }

        public byte[] GetMessage(Stream parImage)
        {
            int[] pixels = StreamToPixels(parImage);

            return Decode(pixels);
        }

        /// <summary>
        /// Writes pixels to an image
        /// </summary>
        /// <param name="parPixels">Pixels of an original image</param>
        /// <returns></returns>
        protected abstract Stream PixelsToStream(int[] parPixels);

        /// <summary>
        /// Encodes the <paramref name="parMessage"/> to the <paramref name="parPixels"/>
        /// </summary>
        /// <param name="parMessage">Message to be encoded</param>
        /// <param name="parPixels">Pixels of an original image</param>
        protected abstract void Encode(byte[] parMessage, int[] parPixels);

        /// <summary>
        /// Executes pixels from the <paramref name="parImage"/>
        /// </summary>
        /// <param name="parImage">Container of a message</param>
        /// <returns></returns>
        protected abstract int[] StreamToPixels(Stream parImage);

        protected abstract byte[] Decode(int[] parPixels);
    }
}
