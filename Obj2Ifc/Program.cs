using CommandLine;
using FileFormatWavefront;
using FileFormatWavefront.Model;
using System;
using System.Collections.Generic;
using System.IO;
using static Obj2Ifc.Obj2IfcBuilder;

namespace Obj2Ifc
{
    class Program
    {
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(Run)
                .WithNotParsed(HandleParseError);
        }

        static void Run(Options opts)
        {
            var builder = new Obj2IfcBuilder();
            foreach (var objfile in opts.ObjFiles)
            {
                if (File.Exists(objfile))
                {
                    FileInfo f = new FileInfo(objfile);
                    Console.WriteLine($"Opening Obj File: ${objfile}");
                    var objFile = OpenObJFile(objfile, opts.LoadTextures);

                    Source s = new Source(objFile.Model, f);
                    builder.AddObjScene(s);
                    
                }
                else
                {
                    Console.Error.WriteLine($"Failed to open file {objfile}");
                }
            }
            Console.WriteLine($"{builder.SceneCount} obj files added. Creating IFC...");
            var file = builder.CreateIfcModel(opts);
            Console.WriteLine($"Created IFC File {file}");

        }

        private static FileLoadResult<Scene> OpenObJFile(string objFile, bool loadTextures)
        {
            if(File.Exists(objFile))
            {
                return FileFormatObj.Load(objFile, loadTextures);
            }
            else
            {
                Console.Error.WriteLine($"Failed to load ${objFile}");
                throw new FileNotFoundException(objFile);
            }
        }

        static void HandleParseError(IEnumerable<Error> errs)
        {
            //handle errors etc
            foreach (var err in errs)
            {
      
                switch (err)
                {
                    case VersionRequestedError _:
                    case HelpRequestedError _:
                        break;

                    case MissingRequiredOptionError missing:
                        Console.WriteLine($"Missing required input: --{missing.NameInfo.LongName}");
                        break;

                    case MissingValueOptionError missing:
                        Console.WriteLine($"Missing value for input: --{missing.NameInfo.LongName}");
                        break;

                    
                    default:
                        Console.Error.WriteLine(err);

                        break;

                }
                if (err.StopsProcessing)
                {
                    break;
                }
            }
        }
    }
}
