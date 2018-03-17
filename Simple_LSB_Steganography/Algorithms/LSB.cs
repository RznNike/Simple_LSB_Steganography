using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple_LSB_Steganography
{
    public class LSB : Algorithm
    {
        public override Stream PutMessage(Stream parImage, byte[] parMessage)
        {
            return null;
        }

        public override byte[] GetMessage(Stream parImage)
        {
            return null;
        }
    }
}
