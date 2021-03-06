using CommandLine;
using System.Collections.Generic;

namespace Obj2Ifc
{
    public class Options
    {
        [Option('o', "objfiles", Required =true, HelpText ="The Wavefront OBJ input files", Min = 1)]
        public IEnumerable<string> ObjFiles { get; set; }

        [Option('i', "ifcFile", HelpText ="The IFC File to output, the extension chosen determines the format (e.g. ifczip).")]
        public string IfcFile { get; set; }

        [Option('t', "loadTextures", HelpText = "Load textures from Obj files", Default = false)]
        public bool LoadTextures { get; set; }

        [Option('s', "simplifySpatial", HelpText = "Simplify spatial structure (building only)", Default = false)]
        public bool SimplifySpatial { get; set; }

        [Option('p', "project", HelpText = "The project name for the IFC file", Default = null)]
        public string ProjectName { get; set; }

        [Option('t', "site", HelpText = "The site for the IFC file", Default = null)]
        public string SiteName { get; set; }

        [Option('b', "building", HelpText = "The building or facility name for the IFC file", Default = null)]
        public string BuildingName { get; set; }

        [Option('r', "storey", HelpText = "The storey for the IFC file", Default = null)]
        public string StoreyName { get; set; }

        [Option('f', "useObjFileName", HelpText = "Uses obj file name for object, if name is not defined in the file", Default = false)]
        public bool UseObjFileName { get; set; }

        [Option('g', "geometry", HelpText = "The IFC geometry representation to use", Default = GeometryMode.TriangulatedFaceSet)]
        public GeometryMode GeometryMode { get; set; }

        [Option('u', "units", HelpText = "The lenght unit represented by coordinates in the obj file", Default = LenghtUnits.Meters)]
        public LenghtUnits LenghtUnit { get; set; }
    }

    public enum LenghtUnits
    {
        MilliMeters,
        Meters
    }

    public enum GeometryMode
    {
        TriangulatedFaceSet
    }
}
