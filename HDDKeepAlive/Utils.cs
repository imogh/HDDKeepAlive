using System;
using System.Management;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace HDDKeepAlive
{
    class Utils
    {
        #region "API CALLS"

        public enum EMoveMethod : uint
        {
            Begin = 0,
            Current = 1,
            End = 2
        }

        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern uint SetFilePointer(
            [In] SafeFileHandle hFile,
            [In] long lDistanceToMove,
            [Out] out int lpDistanceToMoveHigh,
            [In] EMoveMethod dwMoveMethod);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern SafeFileHandle CreateFile(string lpFileName, uint dwDesiredAccess,
          uint dwShareMode, IntPtr lpSecurityAttributes, uint dwCreationDisposition,
          uint dwFlagsAndAttributes, IntPtr hTemplateFile);

        [DllImport("kernel32", SetLastError = true)]
        internal extern static int ReadFile(SafeFileHandle handle, byte[] bytes,
           int numBytesToRead, out int numBytesRead, IntPtr overlapped_MustBeZero);

        #endregion

        public static string GetPhysicalDriveFromDriveLetter(string driveLetter)
        {
            var deviceId = string.Empty;

            var query = "ASSOCIATORS OF {Win32_LogicalDisk.DeviceID='" + driveLetter + "'} WHERE AssocClass = Win32_LogicalDiskToPartition";
            var queryResults = new ManagementObjectSearcher(query);
            var partitions = queryResults.Get();

            foreach (var partition in partitions)
            {
                query = "ASSOCIATORS OF {Win32_DiskPartition.DeviceID='" + partition["DeviceID"] + "'} WHERE AssocClass = Win32_DiskDriveToDiskPartition";
                queryResults = new ManagementObjectSearcher(query);
                var drives = queryResults.Get();

                foreach (var drive in drives)
                    deviceId = drive["DeviceID"].ToString();
            }
            return deviceId;
        }

        public static int BytesPerSector(string drive)
        {
            try
            {

                ManagementObjectSearcher searcher =
                    new ManagementObjectSearcher("root\\CIMV2",
                    "SELECT * FROM Win32_DiskDrive WHERE DeviceID='" + drive.Replace(@"\", @"\\") + "'");

                foreach (ManagementObject queryObj in searcher.Get())
                {
                    var t = queryObj["BytesPerSector"];
                    return int.Parse(t.ToString());

                }
            }
            catch (ManagementException)
            {
                return -1;
            }
            return -1;
        }

        public static int GetTotalSectors(string drive)
        {
            try
            {

                ManagementObjectSearcher searcher =
                    new ManagementObjectSearcher("root\\CIMV2",
                    "SELECT * FROM Win32_DiskDrive WHERE DeviceID='" + drive.Replace(@"\", @"\\") + "'");

                foreach (ManagementObject queryObj in searcher.Get())
                {
                    var t = queryObj["TotalSectors"];
                    return int.Parse(t.ToString());

                }
            }
            catch (ManagementException)
            {
                return -1;
            }
            return -1;
        }

        public static byte[] DumpSector(string drive, double sector, int bytesPerSector)
        {
            uint GENERIC_READ = 0x80000000;
            uint OPEN_EXISTING = 3;

            short FILE_ATTRIBUTE_NORMAL = 0x80;
            short INVALID_HANDLE_VALUE = -1;
            uint GENERIC_WRITE = 0x40000000;
            uint CREATE_NEW = 1;
            uint CREATE_ALWAYS = 2;

            SafeFileHandle handleValue = CreateFile(drive, GENERIC_READ, 0, IntPtr.Zero, OPEN_EXISTING, 0, IntPtr.Zero);
            if (handleValue.IsInvalid)
            {
                Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
            }
            double sec = sector * bytesPerSector;

            int size = int.Parse(bytesPerSector.ToString());
            byte[] buf = new byte[size];
            int read = 0;
            int moveToHigh;
            SetFilePointer(handleValue, long.Parse(sec.ToString()), out moveToHigh, EMoveMethod.Begin);
            ReadFile(handleValue, buf, size, out read, IntPtr.Zero);
            handleValue.Close();
            return buf;
        }

    }
}
