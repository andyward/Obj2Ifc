# Obj2Ifc

A basic converter to transform one or more Wavefront OBJ sources to an IFC model.

## Usage

`Obj2Ifc.exe -objfiles inputmodel.obj -ifcFile output.ifc`

Multiple obj files can be provided as inputs to a single IFC.

Run `Obj2Ifc.exe --help` for more options

### Units

Values implemented for the Units option are:

- [ ] Meters
- [ ] MilliMeters

```
Obj2Ifc.exe -objfiles inputmodel.obj -ifcFile output.ifc -u MilliMeters
```


## To do

To sort out:

- [x] Correct coordinate systems
- [ ] World coordinate placement
- [x] Units
	- [x] Meters and millimeters
- [ ] Honour textures where applicable
- [ ] OBJ Groups
- [ ] Ifc Geometries other than TriangulatedFaceSet (for Ifc2x3)