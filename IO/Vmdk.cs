using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IzayoiTwo.IO;
using System.IO;

namespace LgeFs
{
    public class Vmdk : HardDisk
    {
        SequenceStream parent;
        const string rant = "Are you sure the VMDK File was created using HDClone in BitImage mode?";

        public Vmdk(FileInfo fi)
        {
            SectorSize = 512;
            DirectoryInfo di = fi.Directory;
            if (fi.Length > 5000)
            {
                throw new ArgumentNullException("That VMDK looks really large. " + rant);
            }

            List<Stream> extents = new List<Stream>();
            string[] vmdkCommands = File.ReadAllLines(fi.FullName);
            foreach (string vmdkCommand in vmdkCommands)
            {
                string[] args = vmdkCommand.Split(' ');
                if (args[0] == "RW")
                {
                    if (args[2] == "FLAT")
                    {
                        args[3] = args[3].Replace("\"", "");
                        extents.Add(File.OpenRead(Path.Combine(di.FullName, args[3])));
                    }
                    else
                    {
                        throw new NotSupportedException(string.Format("The VMDK Extent {0} is not stored in FLAT mode. {1}", args[3], rant));
                    }
                }
            }
            if (extents.Count == 0)
            {
                throw new ArgumentNullException("Did not find any VMDK Extents. " + rant);
            }

            parent = new SequenceStream(extents);
            AfterConstruction();
        }

        internal override DNA.BinaryEndianReader GetStream()
        {
            return new DNA.BinaryEndianReader(parent);
        }


        protected override byte[] GetSector(int no)
        {
            parent.Position = no * SectorSize;
            byte[] result = new byte[512];
            parent.Read(result, 0, SectorSize);
            return result;
        }

        public override void Dispose()
        {
            parent.Close();
            parent.Dispose();
        }
    }
}
