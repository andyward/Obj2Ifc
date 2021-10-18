using FileFormatWavefront.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xbim.Common;
using Xbim.Common.Step21;
using Xbim.Ifc;
using Xbim.Ifc4.GeometricConstraintResource;
using Xbim.Ifc4.GeometricModelResource;
using Xbim.Ifc4.GeometryResource;
using Xbim.Ifc4.Interfaces;
using Xbim.Ifc4.Kernel;
using Xbim.Ifc4.MeasureResource;
using Xbim.Ifc4.ProductExtension;
using Xbim.Ifc4.RepresentationResource;
using Xbim.Ifc4.SharedBldgElements;
using Xbim.IO;

namespace Obj2Ifc
{
    public class Obj2IfcBuilder
    {
        private IList<Scene> _scenes = new List<Scene>();

        public int SceneCount { get => _scenes.Count;  }

        public void AddObjScene(Scene scene)
        {
            _scenes.Add(scene);
        }

        public string CreateIfcModel(Options opts)
        {

            using (var model = InitialiseIfcModel(opts))
            {
                if (model != null)
                {
                    IfcBuilding building = CreateBuilding(model, opts);
                    IList<IfcProduct> products = new List<IfcProduct>();
                    foreach (var scene in _scenes)
                    {
                        var sceneProducts = CreateProducts(model, scene, opts);
                        foreach(var product in sceneProducts)
                        {
                            products.Add(product);
                        }
                    }

                    using (var txn = model.BeginTransaction("Add products"))
                    {
                        foreach(var product in products)
                        {
                            building.AddElement(product);
                        }
                        txn.Commit();
                    }

                    var ifcFile = string.IsNullOrEmpty(opts.IfcFile) ? Path.ChangeExtension(opts.ObjFiles.First(), "ifczip") : opts.IfcFile;
                    //write the Ifc File
                    model.SaveAs(ifcFile, StorageType.IfcZip);

                    return ifcFile;

                }
                else
                {
                    Console.WriteLine("Failed to initialise the model");
                }
                return "";
            }

        }

        private IEnumerable<IfcProduct> CreateProducts(IfcStore model, Scene scene, Options opts)
        {
            using (var txn = model.BeginTransaction("Create Product"))
            {
                var product = model.Instances.New<IfcBuildingElementProxy>();
                product.Name = scene.ObjectName ?? "Obj Object";
                IfcGeometricRepresentationItem geometry;
                string representationType;

                switch (opts.GeometryMode)
                {
                    case GeometryMode.TriangulatedFaceSet:
                        geometry = CreateTriangulatedFaceSet(model, scene);
                        representationType = "Tessellation";
                        break;
                        


                    default:
                        throw new NotImplementedException($"Geometry mode not implemented {opts.GeometryMode}");
                        
                }


                //Create a Definition shape to hold the geometry
                var shape = model.Instances.New<IfcShapeRepresentation>();
                var modelContext = model.Instances.OfType<IfcGeometricRepresentationContext>().FirstOrDefault(c => c.ContextType == "Model");
                shape.ContextOfItems = modelContext;
                shape.RepresentationType = representationType;
                shape.RepresentationIdentifier = "Body";
                shape.Items.Add(geometry);

                //Create a Product Definition and add the model geometry to the wall
                var rep = model.Instances.New<IfcProductDefinitionShape>();
                rep.Representations.Add(shape);
                product.Representation = rep;

                //parameters to insert the geometry in the model
                var origin = model.Instances.New<IfcCartesianPoint>();
                origin.SetXYZ(0, 0, 0);

                //now place the element into the model
                var lp = model.Instances.New<IfcLocalPlacement>();
                var ax3D = model.Instances.New<IfcAxis2Placement3D>();
                ax3D.Location = origin;
                ax3D.RefDirection = model.Instances.New<IfcDirection>();
                ax3D.RefDirection.SetXYZ(0, 1, 0);
                ax3D.Axis = model.Instances.New<IfcDirection>();
                ax3D.Axis.SetXYZ(0, 0, 1);
                lp.RelativePlacement = ax3D;
                product.ObjectPlacement = lp;

                // AddPropertiesToProduct(model, product);

                txn.Commit();
                return new IfcProduct[] { product };
            }

        }

        private static IfcTriangulatedFaceSet CreateTriangulatedFaceSet(IfcStore model, Scene scene)
        {
            var coords = model.Instances.New<IfcCartesianPointList3D>();
            var faceSet = model.Instances.New<IfcTriangulatedFaceSet>(fs =>
            {
                fs.Closed = false;
                fs.Coordinates = coords;
            });

            int i = 0;
            foreach (var vertex in scene.Vertices)
            {
                var ifcVertex = new[] { new IfcLengthMeasure(vertex.x), new IfcLengthMeasure(vertex.z), new IfcLengthMeasure(vertex.y) };
                coords.CoordList.GetAt(i).AddRange(ifcVertex);
                i++;
            }

            i = 0;
            foreach (var face in scene.UngroupedFaces)
            {
                var indices = face.Indices.Select(v => new IfcPositiveInteger(v.vertex + 1));
                faceSet.CoordIndex.GetAt(i).AddRange(indices);
                i++;
            }

            return faceSet;
        }

        private IfcStore InitialiseIfcModel(Options opts)
        {
            

            //first we need to set up some credentials for ownership of data in the new model
            var credentials = new XbimEditorCredentials
            {
                ApplicationDevelopersName = "xbim Ltd",
                ApplicationFullName = "Xbim Obj2Ifc",
                ApplicationIdentifier = "Xbim.Obj2Ifc.exe",
                ApplicationVersion = "1.0",
                EditorsFamilyName = "Team",
                EditorsGivenName = "xbim",
                EditorsOrganisationName = "xbim Ltd"
            };
            //now we can create an IfcStore, it is in Ifc4 format and will be held in memory rather than in a database
      

            var model = IfcStore.Create(credentials, XbimSchemaVersion.Ifc4, XbimStoreType.InMemoryModel);

            //Begin a transaction as all changes to a model are ACID
            using (var txn = model.BeginTransaction("Initialise Model"))
            {

                //create a project
                var project = model.Instances.New<IfcProject>();
                //set the units to SI (mm and metres)
                project.Initialize(ProjectUnits.SIUnitsUK);
                project.Name = opts.ProjectName ?? "Default Project";
                //now commit the changes, else they will be rolled back at the end of the scope of the using statement
                txn.Commit();
            }
            return model;
        }

        private IfcBuilding CreateBuilding(IfcStore model, Options opts)
        {
            using (var txn = model.BeginTransaction("Create Building"))
            {
                var building = model.Instances.New<IfcBuilding>();
                building.Name = opts.BuildingName ?? "Default Building";

                building.CompositionType = IfcElementCompositionEnum.ELEMENT;
                var localPlacement = model.Instances.New<IfcLocalPlacement>();
                building.ObjectPlacement = localPlacement;
                var placement = model.Instances.New<IfcAxis2Placement3D>();
                localPlacement.RelativePlacement = placement;
                placement.Location = model.Instances.New<IfcCartesianPoint>(p => p.SetXYZ(0, 0, 0));
                //get the project there should only be one and it should exist
                var project = model.Instances.OfType<IfcProject>().FirstOrDefault();
                project?.AddBuilding(building);
                txn.Commit();
                return building;
            }
        }
    }
}
