# RoundedRectangleGenerator
![Rounded Rectangle Example](ExampleMedia/ExampleShapes-1.png?raw=true "Rounded Rectangle Example")

Tool for creating rectangles with rounded corners inside of Unity Editor. Adjustable properties:
- rectangle dimensions (width, height) 
- optional depth (both 2D and 3D supported)
- corner roundness percentage (percentage of smaller rectangle dimension that determines the roundness of the corners)  
- corners resolution (amount of vertices that will be used to construct the rounded corners)
- multiple UV generation modes
- multiple mesh topology modes
- optional border generation with adjustable size and depth
- automatic/manual rectangle update mode

Generated rectangle and border meshes can be saved as assets to the project folder.

## How To Use
Rectangle generation can be triggered either through the editor window manually or via the C# scripts. 

For manual creation, open the editor window by selecting "BanSee/Rectangle Generator" at the main toolbar, then adjust the desired properties and click on the "Create/Update Rectangle" button at the footer of the tool. Additionally, you can select the "Automatic Update" so that every change of properties triggers an automatic update of the rectangle mesh.

If you want to create the rectangles via C# scripts, check the example scene (Assets/Scenes/Example-RectangleGenerationViaScript) and the example script (Assets/Scripts/Example/RectangleGenerationExample). 

## Editor Window
Tool exposes an editor window that contains all adjustable properties and provides an intuitive and easy-to-use workflow for generation of the rectangles and borders. It supports automatic and manual update modes along with options for saving rectangle and border meshes as assets to the project folder. Additionally, it provides explanations on why certain properties can't be changed in relation to other settings, and restricts the values input to supported ranges.

<img src="https://github.com/Bansi33/RoundedRectangleGenerator/blob/examples/ExampleMedia/EditorWindow.png" width=75% height=75%>

## UV Generation Modes
Tool supports two UV generation modes:
- "Stretch" UV mode
- "Aspect Ratio Fit" mode
 
The "Stretch" mode maps the UV coordinates to the [0, 1] range without taking into account the dimensions and the aspect ratio of the rectangle.
The "Aspect Ratio Fit" mode scales the UV coordinates according to the aspect ratio of the rectangle and centers the smaller dimension around the middle of the UV range.

Comparison of the UV modes ("Stretch" left, "Aspect Ratio Fit" right):

<img src="https://github.com/Bansi33/RoundedRectangleGenerator/blob/examples/ExampleMedia/StretchVsAspectRatioFit.png" width=50% height=50%>

## Mesh Topology
Tool supports two different ways of connecting the mesh vertices into triangles:
- "Center Vertex Connection"
- "Corner Vertices Connection"

"Center Vertex Connection" topology mode generates the rectangle mesh by connecting all outer vertices with one center vertex located at the local origin of the mesh. This type of topology is suited for circular shapes, but should be avoided for rectangles with large difference between dimensions and with high corner vertex resolution since it will result in creation of long thing triangles which can produce unwanted rendering issues.
<p float="left">
  <img src="https://github.com/Bansi33/RoundedRectangleGenerator/blob/examples/ExampleMedia/CenterVertexConnection-Correct.png" width=50% height=50%>
  <img src="https://github.com/Bansi33/RoundedRectangleGenerator/blob/examples/ExampleMedia/CenterVertexConnection-Wrong.png" width=50% height=50%>
</p>

"Corner Vertex Connection" topology mode generates the rectangle mesh by connecting all outer corner vertices with additional vertex that is placed at the center of the circle that was used to construct them. This type of topology produces more vertices than the "Center Vertex Connection" but removes rendering issues caused by the thin long triangles. This type of topology should be used for generating non-circular rectangles with rounded corners.

 <img src="https://github.com/Bansi33/RoundedRectangleGenerator/blob/examples/ExampleMedia/CornerVertexConnection.png" width=50% height=50%>
