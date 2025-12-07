# README
An addin to Solidworks for various functions.

## Commands

PDF Export

- **JPG to PDF** - Export model JPG to PDF format (Drawing)
- **Model to PDF** - Export part/assembly to PDF format (Part/Assembly)
- **DWG to PDF** - Export DWG to PDF format (Drawing)
- **Sheet to PDF** - Export a drawing sheet to PDF format (Drawing)
- **View to PDF** - Export a drawing view to PDF format (Drawing)
- **OLE Object to PDF** - Export an OLE object to PDF format (Drawing)
- **JPG Model to PDF** - Export model as PDF (Part/Assembly)
- **JPG/Model to PDF** - Export to PDF based on content (JPG if image, Model if no image) (Part/Assembly/Drawing)
- **OLE/JPG/View to PDF** - Export an OLE object, JPG, or view to PDF format (Drawing)
- **JPG to PDF (multi)** - Export JPG from multiple files to PDF format
- **Model to PDF (multi)** - Export part/assembly from multiple files to PDF format
- **DWG to PDF (multi)** - Export multiple DWG files to PDF format

Image Export

- **Model to JPG** - Export model to JPG format (Part/Assembly)
- **Sheet to JPG** - Export the active sheet to JPG (Drawing)

Drawing Sheet

- **View to Sheet** - Copy a drawing view to a new sheet (Drawing)
- **Center view** - Center a view in the sheet (Drawing)
- **Scale view** - Scale a view in the sheet (Drawing)
- **OLE Object to Sheet** - Copy OLE object to a new sheet (Drawing)
- **Sheet to SVG** - Export active sheet to SVG (Drawing)
	+ Exports all views in the first sheet, & their balloon annotations.
	+ Supports `Detailing Mode` by using the `GetPolylines` API.
- **BOM to CSV** - Export BOM table to CSV (Drawing)

Other

- **Logs** - View add-in logs

## Debugging 
To debug the addin, you just have to start debugging normally in Visual Studio (using for eg. `F5`). 

However, the addin must first be built (dll) & registered at least once.

To do so, set the following property in the project file (`.csproj`):
```
<PropertyGroup>
	<XCadRegDll>true</XCadRegDll>
</PropertyGroup>
```
Then, run Visual Studio as administrator & start debugging the project.

You must only do this once, since this step is only running the following command on build:
```
%windir%\Microsoft.NET\Framework64\v4.0.30319\RegAsm.exe "{path}\{Addin}.dll" /codebase
```

Then, you can set the property `XCadRegDll` back to `false` & run Visual Studio as a standard user.


## Deployment
- Zip the `\Bin\Debug` or `\Bin\Release` folder.
- Copy it to the target computer.
	+ If the copy is made via a shared folder, make sure the zip on the target PC isn't flagged as being from the web ([MOTW](https://en.wikipedia.org/wiki/Mark_of_the_Web)).
	+ Right-click the zip file > Properties > Unblock (if the option is displayed).
- Extract the zip file into its own folder, eg. `C:\Solidworks.ExportTools`.
- Run as administrator using cmd.exe:
```
%windir%\Microsoft.NET\Framework64\v4.0.30319\RegAsm.exe "{path}\{Addin}.dll" /codebase
```
- (Re)Launch Solidworks.
	+ The addin should be loaded automatically.
	+ If not, go to Tools > Addins & look for the addin named DUBEG Export Tools (or similar).

## Debugging during deployment
If you deploy `/Debug/` binaries, you can use [DebugView](https://learn.microsoft.com/en-us/sysinternals/downloads/debugview), or [DebugView++](https://github.com/CobaltFusion/DebugViewPP) to view traces & errors when Solidworks is loading the add-in (to catch silent errors).
