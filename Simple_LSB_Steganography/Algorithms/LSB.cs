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

        protected override int[] StreamToPixels(Stream parImage)
        {
            Image = new Bitmap(parImage);
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

        private void FixPixelFormat(Bitmap parImage)
        {
            if (parImage.PixelFormat != PixelFormat.Format32bppArgb)
            {
                parImage = parImage.Clone(new Rectangle(0, 0, parImage.Width, parImage.Height), PixelFormat.Format32bppArgb);
            }
        }

        protected override Stream PixelsToStream(int[] parPixels)
        {
            Stream result;

            try
            {
                ReleaseImage(parPixels, true);
                result = new MemoryStream();
                Image.Save(result, ImageFormat.Bmp);
            }
            catch
            {
                result = null;
            }

            return result;
        }

        private void ReleaseImage(int[] parPixels, bool parApplyChanges)
        {
            if (parApplyChanges)
            {
                Marshal.Copy(parPixels, 0, ImageData.Scan0, parPixels.Length);
            }
            Image.UnlockBits(ImageData);
        }

        protected override void Encode(byte[] parMessage, int[] parPixels)
        {
            byte[] modifiedMessage;
            if ((Image.Width * Image.Height) < (parMessage.Length * 8))
            {
                Image = null;
                return;
            }
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

            for (int i = 0; i < parMessage.Length; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    int index = i * 8 + j;
                    int mask = (parMessage[i] >> j) & 1;
                    parPixels[index] = (parPixels[index] >> 1 << 1) | mask;
                }
            }
        }
        
        protected override byte[] Decode(int[] parPixels)
        {
            List<byte> message = new List<byte>();

            for (int i = 0; i < parPixels.Length / 16; i++)
            {
                byte[] currentChar = new byte[2] { 0, 0 };
                for (int k = 0; k < 2; k++)
                {
                    for (int j = 0; j < 8; j++)
                    {
                        int index = (2 * i + k) * 8 + j;
                        int bit = (parPixels[index] & 1);
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
            ReleaseImage(parPixels, false);

            return message.ToArray();
        }

        private bool AreCharsEqual(byte[] parChar1, byte[] parChar2)
        {
            return Encoding.Unicode.GetChars(parChar1)[0] == Encoding.Unicode.GetChars(parChar2)[0];
        }
    }
}
