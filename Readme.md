# Obj2Ifc

A very basic converter to transform Wavefront OBJ format to an IFC model.

## Usage

`Obj2Ifc.exe -objfiles inputmodel.obj -ifcFile output.ifc`

Multiple obj files can be provided as inputs to a single IFC.

Run `Obj2Ifc.exe --help` for more options

## To do

To sort out:

- [ ] Correct coordinate systems
- [ ] World coordinate placement
- [ ] Units
- [ ] Honour textures where applicable
- [ ] OBJ Groups
- [ ] Ifc Geometries other than TriangulatedFaceSet (for Ifc2x3)