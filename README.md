# RoundedRectangleGenerator
![Rounded Rectangle Example](ExampleMedia/ExampleShapes-1.png?raw=true "Rounded Rectangle Example")

Tool for creating rectangles with rounded corners inside of Unity Editor. Adjustable properties:
- rectangle dimensions (width, height, depth) 
- corner roundness percentage (percentage of smaller rectangle dimension that determines the roundness of the corners)  
- corners resolution (amount of vertices that will be used to construct the rounded corners)
- multiple UV generation modes
- multiple mesh topology modes
- optional border generation with adjustable size and depth
- automatic/manual rectangle update mode

Generated rectangle and border meshes can be saved as assets to the project folder.

## Editor Window
Tool exposes an editor window that contains all adjustable properties and provides an intuitive and easy-to-use workflow for generation of the rectangles and borders. It supports automatic and manual update modes along with options for saving rectangle and border meshes as assets to the project folder. Additionally, it provides explanations on why certain properties can't be changed in relation to other settings, and restricts the values input to supported ranges.

![Editor Window Overview](ExampleMedia/EditorWindow.png?raw=true "Editor Window Overview")

## Multiple UV Generation Modes
Tool supports two UV generation modes - "Stretch" and "Aspect Ratio Fit". 
The "Stretch" mode maps the UV coordinates to the [0, 1] range without taking into account the dimensions and the aspect ratio of the rectangle.
The "Aspect Ratio Fit" mode scales the UV coordinates according to the aspect ratio of the rectangle and centers the smaller dimension around the middle of the UV range.

Comparison of the UV modes ("Stretch" left, "Aspect Ratio Fit" right):
![UV Modes Overview](ExampleMedia/EditorWindow.png?raw=true "UV Modes Overview")
