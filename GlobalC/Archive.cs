using System;
using System.Linq;
using System.Text;
using System.IO;
using System.ComponentModel;
using System.Windows.Forms;

namespace GlobalC
{
    class Archive
    {
        PreHeader preheader;
        [Category("Info")]
        public ArchiveType Type { get; private set; }
        int num1;
        [DisplayName("Unpacked Size")]
        [Category("Size")]
        public int UnpackedSize { get; private set; }
        [Category("Size")]
        public int Size { get; private set; }
        [Category("Info")]
        public uint Offset { get; private set; }
        byte[] data;
        byte[] ending;
        int id;
        [Category("Info")]
        public bool Compressed
        {
            get { return Type != ArchiveType.RAWW; }
        }
        [Category("Info")]
        [Browsable(false)]
        public int PreviousSize
        {
            get { return preheader.PreviousSize; }
            set { preheader.PreviousSize = value; }
        }
        MainVault vault;

        public Archive(BinaryReader br, int id, MainVault sourceVault)
        {
            preheader = new PreHeader(br);
            Offset = (uint)br.BaseStream.Position;
            ReadHeader(br);
            data = br.ReadBytes(Size);
            ending = br.ReadBytes(preheader.FullSize - Size - preheader.Size);
            this.id = id;
            vault = sourceVault;
        }

        private void ReadHeader(BinaryReader br)
        {
            Type = (ArchiveType)br.ReadInt32();
            num1 = br.ReadInt32();
            UnpackedSize = br.ReadInt32();
            Size = br.ReadInt32();
            br.BaseStream.Seek(Offset, SeekOrigin.Begin);
        }

        public void Write(BinaryWriter bw)
        {
            preheader.Write(bw);
            bw.Write(data);
            bw.Write(ending);
        }

        public void Unpack()
        {
            if (Type == ArchiveType.JDLZ)
            {
                data = Jdlz.Unpack(data);
                Type = ArchiveType.RAWW;
                preheader.UnpackedSize = data.Length;
                preheader.FullSize = data.Length + preheader.FullSize - Size + 16;
                if (id + 1 != vault.Length)
                    vault.Archives[id + 1].PreviousSize = preheader.FullSize;
                Size = data.Length + 16;
                byte[] header = new byte[16];
                BinaryWriter bw = new BinaryWriter(new MemoryStream(header));
                bw.Write((int)Type);
                num1 = 4097;
                bw.Write(num1);
                bw.Write(UnpackedSize);
                bw.Write(Size);
                bw.Close();
                data = header.Concat(data).ToArray();
            }
            else
            {
                MessageBox.Show(string.Format("Archive with type \"{0}\" can't be unpacked", Type));
            }
        }

        public void Compress()
        {
            if (!Compressed)
            {
                data = Jdlz.Compress(data.Skip(16).Take(data.Length - 16).ToArray());
                Type = ArchiveType.JDLZ;
                preheader.FullSize = data.Length + preheader.FullSize - Size;
                Size = data.Length;
            }
            else
            {
                MessageBox.Show("This archive is already compressed");
            }
        }

        public void Replace(string fileName)
        {
            BinaryReader br = new BinaryReader(File.Open(fileName, FileMode.Open), Encoding.Default);
            ReadHeader(br);
            data = br.ReadBytes(Size);
            preheader.FullSize = data.Length + preheader.FullSize - Size;
            Size = data.Length;
            br.Close();
        }

        public void Export(string fileName)
        {
            BinaryWriter bw = new BinaryWriter(File.Open(fileName, FileMode.Create), Encoding.Default);
            bw.Write(data);
            bw.Close();
        }

        struct PreHeader
        {
            public readonly int Size;
            int num1;
            public int UnpackedSize;
            public int FullSize;
            int num2;
            public int PreviousSize;
            int num3;

            public PreHeader(BinaryReader br)
            {
                Size = 24;
                num1 = br.ReadInt32();
                UnpackedSize = br.ReadInt32();
                FullSize = br.ReadInt32();
                num2 = br.ReadInt32();
                PreviousSize = br.ReadInt32();
                num3 = br.ReadInt32();
            }

            public void Write(BinaryWriter bw)
            {
                bw.Write(num1);
                bw.Write(UnpackedSize);
                bw.Write(FullSize);
                bw.Write(num2);
                bw.Write(PreviousSize);
                bw.Write(num3);
            }
        }

        public override string ToString()
        {
            return string.Format("Archive {0} ({1})", id, Type);
        }
    }

    public enum ArchiveType
    {
        JDLZ = 1514947658,
        HUFF = 1179014472,
        RAWW = 1465336146,
        Unknown
    }
}
