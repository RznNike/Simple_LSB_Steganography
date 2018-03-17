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

        public override Stream PutMessage(Stream parImage, byte[] parMessage)
        {
            Bitmap image = new Bitmap(parImage);
            byte[] message;

            if ((image.Width * image.Height) < (parMessage.Length * 8))
            {
                return null;
            }
            else if ((image.Width * image.Height) >= (parMessage.Length * 8 + 2))
            {
                message = new byte[parMessage.Length + 2];
                parMessage.CopyTo(message, 0);
                TERMINATOR.CopyTo(message, parMessage.Length);
            }
            else
            {
                message = parMessage;
            }

            BitmapData bdImage = image.LockBits(new Rectangle(0, 0, image.Width, image.Height),
                                                      ImageLockMode.ReadWrite,
                                                      PixelFormat.Format32bppArgb);
            int[] bitsImage = new int[bdImage.Stride / 4 * bdImage.Height];
            Marshal.Copy(bdImage.Scan0, bitsImage, 0, bitsImage.Length);
            
            for (int i = 0; i < message.Length; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    int index = i * 8 + j;
                    int mask = (message[i] >> j) & 1 | (bitsImage[index] >> 1 << 1);
                    bitsImage[index] &= mask;
                }
            }
            
            Marshal.Copy(bitsImage, 0, bdImage.Scan0, bitsImage.Length);
            image.UnlockBits(bdImage);

            Stream result = new MemoryStream();
            image.Save(result, ImageFormat.Bmp);
            return result;
        }

        public override byte[] GetMessage(Stream parImage)
        {
            Bitmap image = new Bitmap(parImage);
            List<byte> message = new List<byte>();

            BitmapData bdImage = image.LockBits(new Rectangle(0, 0, image.Width, image.Height),
                                                      ImageLockMode.ReadWrite,
                                                      PixelFormat.Format32bppArgb);
            int[] bitsImage = new int[bdImage.Stride / 4 * bdImage.Height];
            Marshal.Copy(bdImage.Scan0, bitsImage, 0, bitsImage.Length);

            for (int i = 0; i < bitsImage.Length / 16; i++)
            {
                byte[] currentChar = new byte[2] { 0, 0 };
                for (int k = 0; k < 2; k++)
                {
                    for (int j = 0; j < 8; j++)
                    {
                        int index = (2 * i + k) * 8 + j;
                        int bit = (bitsImage[index] & 1);
                        currentChar[k] |= (byte)(bit << j);
                    }
                }
                if (IsCharsEqual(currentChar, TERMINATOR))
                {
                    break;
                }
                else
                {
                    message.AddRange(currentChar);
                }
            }

            image.UnlockBits(bdImage);
           
            return message.ToArray();
        }

        private bool IsCharsEqual(byte[] parChar1, byte[] parChar2)
        {
            return Encoding.Unicode.GetChars(parChar1)[0] == Encoding.Unicode.GetChars(parChar2)[0];
        }
    }
}
