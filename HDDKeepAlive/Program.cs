using System;

namespace HDDKeepAlive
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                PrintUsage();
                return;
            }

            int sl = 0;
            string drive;
            int totalSectors = 0;
            int bytesPerSector = 0;
            int targetSector = 0;
            try
            {
                sl = int.Parse(args[1]);

                drive = args[0].Contains(":") ? Utils.GetPhysicalDriveFromDriveLetter(args[0]) : args[0];

                totalSectors = Utils.GetTotalSectors(drive);
                bytesPerSector = Utils.BytesPerSector(drive);

                int c = 0;

                Console.WriteLine("Press Ctrl-C to stop.");

                while (true)
                {
                    c++;
                    targetSector = new Random().Next(0, totalSectors - 1);
                    byte[] b = Utils.DumpSector(drive, targetSector, bytesPerSector);
                    Console.SetCursorPosition(0, Console.CursorTop);
                    Console.Write("                                     ");
                    Console.SetCursorPosition(0, Console.CursorTop);
                    Console.Write(string.Format("Sector {0} read.", targetSector));

                    System.Threading.Thread.Sleep(sl);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        static void PrintUsage()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine();
            Console.WriteLine("Reading random sector every 1 second from drive C:");
            Console.WriteLine("HDDKeepAlive c: 1000");
            Console.WriteLine();
            Console.WriteLine(@"Reading random sector every 200ms from pysical drive \\.\PHYSICALDRIVE0");
            Console.WriteLine(@"HDDKeepAlive \\.\PHYSICALDRIVE0 200");
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Exit with Ctrl-C");
        }
    }
}
