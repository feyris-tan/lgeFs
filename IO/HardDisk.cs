using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DNA;

namespace LgeFs
{
    public abstract class HardDisk
    {
        internal abstract BinaryEndianReader GetStream();
        protected abstract byte[] GetSector(int no);

        private BinaryEndianReader str;
        private LgeDirectoryInfo rootDir;

        protected void AfterConstruction()
        {
            byte[] partTable = GetSector(0);
            if (!(partTable[0x1FE] == 0x55 && partTable[0x1C1] == 0x0B))
            {
                throw new NotSupportedException("Partition Table not found in sector 0");
            }
            FirstSector = BitConverter.ToInt32(partTable, 0x1C2 + 4);
            int p0Size = BitConverter.ToInt32(partTable, 0x1C2 + 8);
            int off1 = BitConverter.ToInt32(partTable, 0x1C2 + 12);
            if (off1 != 0) off1 = BitConverter.ToInt32(partTable, 0x1C2 + 0);

            str = GetStream();
            str.BaseStream.Position = FirstSector * 512;
            BootSector = new BootSector(str);
            FatStartSector = BootSector.HiddenSectors + FirstSector;

            str.BaseStream.Position = FatStartSector * SectorSize;
            byte mId = str.ReadByte();
            if (mId != BootSector.MediaCode)
            {
                throw new ArgumentNullException("Media ID error.");
            }

            if (off1 != 0)
            {
                str.BaseStream.Position = (FirstSector + 1) * 512;
                BootSector ExtendedBootSector = new BootSector(str);
                int ExtendedFatStartSector = ExtendedBootSector.HiddenSectors + FirstSector + off1;
                long clu1 = p0Size + FirstSector + ExtendedBootSector.HiddenSectors + ExtendedBootSector.RootSector;
                str.BaseStream.Position = clu1 * 512;
            }

            Cluster1Start = FatStartSector + BootSector.RootSector;

            rootDir = new LgeDirectoryInfo(this, 1, "/");
        }

        public int SectorSize { get; protected set; }

        public int FirstSector { get; protected set; }

        public BootSector BootSector { get; protected set; }

        public int FatStartSector { get; protected set; }

        public int FatRootSector
        {
            get
            {
                return BootSector.RootSector + FatStartSector;
            }
        }

        public int Cluster1Start { get; protected set; }

        internal void GoToCluster(int number)
        {
            long clusterOffset = (number - 1);
            clusterOffset *= this.BootSector.SectorsPerCluster;
            clusterOffset += this.Cluster1Start;
            clusterOffset *= 512;
            str.BaseStream.Position = clusterOffset;
        }

        public LgeDirectoryInfo RootDirectory
        {
            get
            {
                return rootDir;
            }
        }
    }
}
