using CommandLine;
using System.Collections.Generic;

namespace Obj2Ifc
{
    public class Options
    {
        [Option('o', "objfiles", Required =true, HelpText ="The Wavefront OBJ input files", Min = 1)]
        public IEnumerable<string> ObjFiles { get; set; }

        [Option('i', "ifcFile", HelpText ="The IFC File to output")]
        public string IfcFile { get; set; }

        [Option('t', "loadTextures", HelpText = "Load textures from Obj files", Default = false)]
        public bool LoadTextures { get; set; }

        [Option('p', "project", HelpText = "The project name for the IFC file", Default = null)]
        public string ProjectName { get; set; }

        [Option('b', "building", HelpText = "The building or facility name for the IFC file", Default = null)]
        public string BuildingName { get; set; }

        [Option('g', "geometry", HelpText = "The IFC geometry representation to use", Default = GeometryMode.TriangulatedFaceSet)]
        public GeometryMode GeometryMode { get; set; }
    }

    public enum GeometryMode
    {
        TriangulatedFaceSet
    }
}
