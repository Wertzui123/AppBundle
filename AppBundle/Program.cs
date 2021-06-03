using System;
using System.IO;
using System.Reflection;

namespace AppBundle
{
    internal static class Program
    {

        public static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: file resource=string|assembly");
                Console.WriteLine("Example: program.exe test=\"Hello_World!\" example=Assembly.dll");
                Environment.Exit(1);
            }

            if (!File.Exists(args[0]))
            {
                Error("No file called " + args[0] + " found!");
            }

            var bundle = new AppBundle(args[0]);

            for (var i = 0; i < args.Length; i++)
            {
                if (i == 0) continue;
                var resource = args[i].Split('=');

                if (resource.Length != 2)
                {
                    Error("Invalid resource syntax; you should use: name=value");
                }

                if (File.Exists(resource[1]))
                {
                    bundle.AddResource(resource[0], Assembly.LoadFile(resource[1]));
                }
                else
                {
                    bundle.AddResource(resource[0], resource[1].Substring(1, resource[1].Length - 1));
                }
            }

            bundle.Generate();
        }

        private static void Error(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ResetColor();
            Environment.Exit(1);
        }

    }
}