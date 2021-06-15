using System;
using System.Linq;

namespace Thundershock.ThunderPak
{
    class Program
    {
        public static string Source { get; private set; }
        public static string Destination { get; private set; }
        
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: thunderpak <source> <destination> [thundershockArgs]");
                return;
            }

            var source = args[0];
            var dest = args[1];

            Source = source;
            Destination = dest; 

            EntryPoint.Run<ThunderPakApp>(args.Skip(2).ToArray());
        }
    }
}
