using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace rsct
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                PrintUsage();
                return;
            }
                
            string ext = Path.GetExtension(args[0]);
            RSCT rsct = new RSCT();
            if (ext == ".rsct")
            {
                Console.WriteLine("Mode: RSCT to TXT");
                rsct.Extract(args[0]);
            }
            else if (ext == ".txt")
            {
                Console.WriteLine("Mode: TXT to RSCT");
                rsct.ConvertToRSCT(args[0]);
            }
            else
            {
                PrintUsage();
                return;
            }  
        }

        static void PrintUsage()
        {
            Console.WriteLine("Danganronpa RSCT Tool by LinkOFF v.1.0");
            Console.WriteLine("");
            Console.WriteLine("Usage:");
            Console.WriteLine("\tDrag rsct file to extract it");
            Console.WriteLine("\tDrag txt file to convert it to RSCT");
            Console.WriteLine("");
        }
    }
}
