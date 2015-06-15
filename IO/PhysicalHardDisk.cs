using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;

namespace LgeFs
{
    public class PhysicalHardDisk : HardDisk
    {
        public PhysicalHardDisk(FileInfo fi)
            : base()
        {
            fs = fi.OpenRead();
        }

        public PhysicalHardDisk(string s)
            : base()
        {
            DeviceHandler deviceHandler = new DeviceHandler();
            deviceHandler.DeviceName = s;
            fs = new FileStream(deviceHandler.GetDeviceHandle, FileAccess.Read);

            SectorSize = 512;

            AfterConstruction();
        }

        FileStream fs;

        internal override DNA.BinaryEndianReader GetStream()
        {
            return new DNA.BinaryEndianReader(fs);
        }

        protected override byte[] GetSector(int no)
        {
            fs.Position = no * SectorSize;
            byte[] result = new byte[512];
            int amountRead = fs.Read(result, 0, 512);
            return result;
        }

        public override void Dispose()
        {
            fs.Close();
            fs.Dispose();
        }

        //This subclass was taken from http://www.sharpcoding.net/2010/03/25/physicaldrive0-mit-net/ 
        //It is written by Mr. Freitag, all credits for the following lines go to him!
        #region DeviceHandler Class
        
        private class DeviceHandler
        {
        string _deviceName = String.Empty;

        ///
        /// Import the CreateFile() from kernel32.dll to open a handle to the Disk.
        /// WebLink:    http://msdn.microsoft.com/en-us/library/aa363858%28VS.85%29.aspx
        ///
        ///          ///          ///          ///          ///          ///          ///          ///
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern Microsoft.Win32.SafeHandles.SafeFileHandle CreateFile(
        String lpFileName,
        FileAccess dwDesiredAccess,
        FileShare dwShareMode,
        IntPtr lpSecurityAttributes,
        FileMode dwCreationDisposition,
        UInt32 dwFlagsAndAttributes,
        IntPtr hTemplateFile
        );

        ///
        /// Get or Set the DeviceName of the targetdevice e.g. \\.\PHYSICALDRIVE0 or \\.\C:
        ///
        public string DeviceName
        {
        get { return _deviceName; }
        set { _deviceName = value; }
        }

        ///
        /// Get the Handle to the device set in DeviceName
        ///
        public Microsoft.Win32.SafeHandles.SafeFileHandle GetDeviceHandle
        {
        get
        {
        if (string.IsNullOrEmpty(_deviceName))
        throw new ArgumentNullException("The DeviceName can not be left blank!");

        Microsoft.Win32.SafeHandles.SafeFileHandle handleValue = null;
        handleValue = CreateFile(_deviceName, FileAccess.Read, FileShare.None, IntPtr.Zero, FileMode.Open, 0, IntPtr.Zero);

        if (handleValue.IsInvalid)
        {
        Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
        }

        return handleValue;
        }
        }

        }
        #endregion
    }
}
