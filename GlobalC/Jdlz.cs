using System;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using System.Linq;

namespace GlobalC
{
    static class Jdlz
    {
        [DllImport("jdlz.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern int unjdlz_clean(string inData, [MarshalAs(UnmanagedType.VBByRefStr)]ref string outData);

        [DllImport("jdlz.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern int jdlzpack(string inData, [MarshalAs(UnmanagedType.VBByRefStr)]ref string outData, int inputLength);

        public static byte[] Unpack(byte[] input)
        {
            BinaryReader br = new BinaryReader(new MemoryStream(input), Encoding.Default);
            if (new string(br.ReadChars(4)) != "JDLZ")
                throw new Exception("This is not a JDLZ archive");
            br.ReadInt32();
            int bufferLength = br.ReadInt32();
            br.Close();
            string inData = new string(Encoding.Default.GetChars(input));
            string outData = new string(new char[bufferLength]);
            int outLength = unjdlz_clean(inData, ref outData);
            if (outLength == 0)
                throw new Exception("This JDLZ archive is not valid");
            return Encoding.Default.GetBytes(outData);
        }

        public static byte[] Compress(byte[] input)
        {
            string inData = new string(Encoding.Default.GetChars(input));
            string outData = new string(new char[input.Length]);
            int outLength = jdlzpack(inData, ref outData, input.Length);
            if (outLength == 0)
                throw new Exception("Failed to compress data");
            return Encoding.Default.GetBytes(outData.Take(outLength).ToArray());
        }
    }
}
