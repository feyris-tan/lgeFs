using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DNA;
namespace LgeFs
{
    public class BootSector
    {
        internal BootSector(BinaryEndianReader ber)
        {
            ber.ReadBytes(3);
            SystemID = Encoding.ASCII.GetString(ber.ReadBytes(8));
            SectorSize = ber.ReadInt16LE();
            ber.ReadByte();
            HiddenSectors = ber.ReadInt16LE();
            FATs = ber.ReadByte();
            ber.ReadInt32();    //ignored;
            MediaCode = ber.ReadByte();
            ber.ReadInt16();    //ignored3
            SectorsPerTrack = ber.ReadInt16LE();
            ber.ReadInt16();    //ignored4
            ber.ReadInt32();    //ignored5
            TotalSectors = ber.ReadInt32LE();
            FatLength = ber.ReadInt32LE();
            ber.ReadInt64();    //ignored
            FsInfoSec = ber.ReadInt16LE();
            Neco = ber.ReadInt16LE();
            RootSector = ber.ReadInt32LE();
            ber.ReadBytes(15);  //ignored
            Label = Encoding.ASCII.GetString(ber.ReadBytes(11));
            FatId = Encoding.ASCII.GetString(ber.ReadBytes(8));
            ber.ReadBytes(6);   //ignored
            SectorsPerCluster = ber.ReadInt16LE();
        }

        public string SystemID { get; private set; }

        public short SectorSize { get; private set; }

        public short HiddenSectors { get; private set; }

        public byte FATs { get; private set; }

        public byte MediaCode { get; private set; }

        public short SectorsPerTrack { get; private set; }

        public int TotalSectors { get; private set; }

        public int FatLength { get; private set; }

        public short FsInfoSec { get; private set; }

        public short Neco { get; private set; }

        public int RootSector { get; private set; }

        public string Label { get; private set; }

        public string FatId { get; private set; }

        public short SectorsPerCluster { get; private set; }
    }
}
