using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CpsDbHelper.CodeGenerator.Executeable
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = DacpacExtractor.ConfigFileName;
            if (args.Length == 2)
            {
                config = args[1];
            }
            if (args.Length >= 1 && File.Exists(args[0]))
            {
                if (File.Exists(config))
                {
                    try
                    {
                        var xml = XDocument.Load(config);
                        string error;
                        var parser = DacpacExtractor.LoadFromXml(xml.Root, out error);
                        if (parser == null)
                        {
                            Console.WriteLine(error);
                            return;
                        }
                        parser.ParseDacpac(args[0]);
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Unexpected Error:");
                        throw;
                    }
                }
                else
                {
                    Console.WriteLine("Config file not exists");
                    ShowUsage();
                }
            }
            else
            {
                ShowUsage();
            }
        }

        static void ShowUsage()
        {
            Console.WriteLine("Usage: GenDacpac <DacpackFile> [<ConfigFile>]");
            Console.WriteLine("\t If ConfigFile was not supplied, the command will try to find '{0}' in the working directory", DacpacExtractor.ConfigFileName);
        }
    }
}
