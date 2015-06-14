using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DNA;
using System.IO;

namespace LgeFs
{
    public class LgeDirectoryInfo
    {
        internal LgeDirectoryInfo(HardDisk hdd, int cluster,string intname)
        {
            BinaryEndianReader ber = hdd.GetStream();
            entries = null;
            fileCount = 0;
            dirCount = 0;
            relatedDisk = hdd;
            name = intname;
            entries = new List<LgeDirEntry>();

            hdd.GoToCluster(cluster);
            
            while (true)
            {
                LgeDirEntry entry = new LgeDirEntry(ber);
                if (string.IsNullOrEmpty(entry.Name)) break;
                if (entry.Name == ".") continue;
                if (entry.Name == "..") continue;
                if (entry.IsDeleted) continue;
                if (entry.IsDirectory) dirCount++;
                else fileCount++;
                entries.Add(entry);
            }
        }

        int dirCount, fileCount;
        List<LgeDirEntry> entries;
        HardDisk relatedDisk;
        string name;

        public LgeDirectoryInfo[] GetDirectories()
        {
            LgeDirectoryInfo[] subdirs = new LgeDirectoryInfo[dirCount];
            int ptr = 0;
            foreach (LgeDirEntry lde in entries)
            {
                if (lde.IsDirectory)
                {
                    subdirs[ptr++] = new LgeDirectoryInfo(relatedDisk, lde.StartingCluster, name + lde.Name + "/");
                }
            }
            return subdirs;
        }

        public LgeFileInfo[] GetFiles()
        {
            LgeFileInfo[] files = new LgeFileInfo[fileCount];
            int ptr = 0;
            foreach (LgeDirEntry lde in entries)
            {
                if (!lde.IsDirectory)
                {
                    files[ptr++] = new LgeFileInfo(this, lde, relatedDisk);
                }
            }
            return files;
        }

        public override string ToString()
        {
            return name;
        }
    }

    public class LgeFileInfo
    {
        internal LgeFileInfo(LgeDirectoryInfo parentDir, LgeDirEntry data, HardDisk relatedDisk)
        {
            this.parentDir = parentDir;
            this.data = data;
            this.relatedDisk = relatedDisk;
        }

        LgeDirectoryInfo parentDir;
        LgeDirEntry data;
        HardDisk relatedDisk;

        public override string ToString()
        {
            return parentDir.ToString() + data.Name;
        }

        public string FullName
        {
            get
            {
                return ToString();
            }
        }

        public string Name
        {
            get
            {
                return data.Name;
            }
        }

        public long Length
        {
            get
            {
                if (data.SizeHi != 0) throw new NotImplementedException("8 byte size");
                return (long)data.Size;
            }
        }

        public byte[] GetByteArray()
        {
            relatedDisk.GoToCluster(data.StartingCluster);
            if (data.SizeHi != 0) throw new NotImplementedException("8 byte size");
            byte[] content = relatedDisk.GetStream().ReadBytes(data.Size);
            return content;
        }

        public Stream GetStream()
        {
            relatedDisk.GoToCluster(data.StartingCluster);
            BinaryEndianReader ber = relatedDisk.GetStream();

            return new Substream(ber.BaseStream.Position, this.Length, ber.BaseStream);
        }
    }

    class LgeDirEntry
    {
        internal LgeDirEntry(BinaryEndianReader ber)
        {
            byte[] name = ber.ReadBytes(45 * 2);
            if (name[1] == 0xE5) IsDeleted = true;
            Name = Encoding.ASCII.GetString(name).Replace("\0", "");
            Attribute = ber.ReadInt16LE();
            StartingCluster = ber.ReadInt32LE();
            Time = ber.ReadInt32LE();
            Date = ber.ReadInt32LE();
            Size = ber.ReadInt32LE();
            SizeHi = ber.ReadInt32LE();
            Time2 = ber.ReadInt32LE();
            ber.ReadInt32();    //???
            ber.ReadInt32();    //???
            ber.ReadInt32();    //???

            if ((Name.Length >= 4) && (!IsDirectory))
            {
                Name = Name.Substring(0, Name.Length - 4) + "." + Name.Substring(Name.Length - 3);
            }
        }

        public string Name { get; private set; }

        private short Attribute { get; set; }

        public int StartingCluster { get; private set; }

        private int Time { get;  set; }

        private int Date { get;  set; }

        public int Size { get; private set; }

        public int SizeHi { get; private set; }

        private int Time2 { get; set; }

        public bool IsDirectory 
        {
            get
            {
                return (Attribute & 16) != 0;
            }
        }

        public bool IsDeleted { get; private set; }
    }
}
