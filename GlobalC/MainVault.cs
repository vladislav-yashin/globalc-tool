using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace GlobalC
{
    class MainVault : IDisposable
    {
        BinaryReader br;
        int num1;
        int num2;
        uint fileSize;
        int num3;
        public List<Archive> Archives { get; private set; }
        public string FileName { get; private set; }
        public int Length
        {
            get { return Archives.Count(); }
        }

        public MainVault(string fileName)
        {
            FileName = fileName;
            OpenFile(fileName);
        }

        ~MainVault()
        {
            br.Close();
        }

        private void OpenFile(string fileName)
        {
            if (br != null)
                br.Close();
            br = new BinaryReader(File.Open(fileName, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite), Encoding.Default);
            ReadHeader();
            Archives = new List<Archive>();
            ReadArchives();
            br.Close();
        }

        private void ReadHeader()
        {
            num1 = br.ReadInt32();
            num2 = br.ReadInt32();
            fileSize = br.ReadUInt32();
            num3 = br.ReadInt32();
        }

        private void ReadArchives()
        {
            int i = 0;
            do
            {
                Archives.Add(new Archive(br, i, this));
                i++;
            }
            while (br.BaseStream.Position != br.BaseStream.Length);
        }

        public void Write()
        {
            br.Close();
            BinaryWriter bw = new BinaryWriter(File.Create(FileName), Encoding.Default);
            bw.Write(num1);
            bw.Write(num2);
            bw.Write(fileSize);
            bw.Write(num3);
            Archives.ForEach(i => i.Write(bw));
            uint trueSize = (uint)bw.BaseStream.Length - 16;
            bw.BaseStream.Seek(8L, SeekOrigin.Begin);
            bw.Write(trueSize);
            bw.Close();
            OpenFile(FileName);
        }

        public void Dispose()
        {
            if (br != null)
                br.Close();
            GC.SuppressFinalize(this);
        }
    }
}
