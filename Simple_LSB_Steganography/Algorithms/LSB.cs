using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Simple_LSB_Steganography
{
    public class LSB : Algorithm
    {
        private static readonly byte[] TERMINATOR = { 255, 255 };

        private Bitmap Image { get; set; }
        private BitmapData ImageData { get; set; }

        private bool AreCharsEqual(byte[] parChar1, byte[] parChar2)
        {
            return Encoding.Unicode.GetChars(parChar1)[0] == Encoding.Unicode.GetChars(parChar2)[0];
        }

        private void FixPixelFormat(Bitmap parImage)
        {
            if (parImage.PixelFormat != PixelFormat.Format32bppArgb)
            {
                parImage = parImage.Clone(new Rectangle(0, 0, parImage.Width, parImage.Height), PixelFormat.Format32bppArgb);
            }
        }

        protected override Stream ToStream(int[] pixels)
        {
            ReleaseImage(pixels);

            Stream result = new MemoryStream();
            Image.Save(result, ImageFormat.Bmp);

            return result;
        }

        private void ReleaseImage(int[] pixels)
        {
            Marshal.Copy(pixels, 0, ImageData.Scan0, pixels.Length);
            Image.UnlockBits(ImageData);
        }

        protected override void Encode(byte[] message, int[] pixels)
        {
            byte[] modifiedMessage;
            if ((Image.Width * Image.Height) < (message.Length * 8))
            {
                return;
            }
            else if ((Image.Width * Image.Height) >= (message.Length * 8 + 2))
            {
                modifiedMessage = new byte[message.Length + 2];
                message.CopyTo(modifiedMessage, 0);
                TERMINATOR.CopyTo(modifiedMessage, message.Length);
            }
            else
            {
                modifiedMessage = message;
            }

            for (int i = 0; i < message.Length; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    int index = i * 8 + j;
                    int mask = (message[i] >> j) & 1;
                    pixels[index] = (pixels[index] >> 1 << 1) | mask;
                }
            }
        }

        protected override int[] GetPixels(Stream image)
        {
            Image = new Bitmap(image);
            FixPixelFormat(Image);

            ImageData = Image.LockBits
            (
                new Rectangle(0, 0, Image.Width, Image.Height),
                ImageLockMode.ReadWrite,
                PixelFormat.Format32bppArgb
            );
            int[] pixels = new int[ImageData.Width * ImageData.Height];
            Marshal.Copy(ImageData.Scan0, pixels, 0, pixels.Length);

            return pixels;
        }

        internal override byte[] Decode(int[] pixels)
        {
            List<byte> message = new List<byte>();

            for (int i = 0; i < pixels.Length / 16; i++)
            {
                byte[] currentChar = new byte[2] { 0, 0 };
                for (int k = 0; k < 2; k++)
                {
                    for (int j = 0; j < 8; j++)
                    {
                        int index = (2 * i + k) * 8 + j;
                        int bit = (pixels[index] & 1);
                        currentChar[k] |= (byte)(bit << j);
                    }
                }
                if (AreCharsEqual(currentChar, TERMINATOR))
                {
                    break;
                }
                else
                {
                    message.AddRange(currentChar);
                }
            }
            ReleaseImage(pixels);

            return message.ToArray();
        }
    }
}
