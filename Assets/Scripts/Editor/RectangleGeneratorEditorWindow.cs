using UnityEditor;
using UnityEngine;
using static BanSee.RoundedRectangleGenerator.EditorUtils;
using static BanSee.RoundedRectangleGenerator.RectangleGeneratorGUI;

namespace BanSee.RoundedRectangleGenerator
{
    /// <summary>
    /// Custom editor window for generating the rectangles.
    /// </summary>
    public class RectangleGeneratorEditorWindow : EditorWindow
    {
        private const string RECTANGLE_GENERATOR_TITLE = "Rectangle Generator";
        private const float SPACE_BETWEEN_CATEGORIES = 10f;
        private const float SPACE_INSIDE_CATEGORY = 5f;

        private static RectangleGeneratorEditorWindow _window = null;
        private static RectangleGenerationData _rectangleGenerationData = null;
        private static RectangleBorderGenerationData _rectangleBorderGenerationData = null;

        private Texture _dimensionsExplainTexture = null;
        private Vector2 _scrollPosition = Vector2.zero;
        private bool _automaticUpdate = false;
        private bool _createBorder = true;
        private Material _rectangleMaterial = null;
        private Material _rectangleBorderMaterial = null;

        private GameObject _rectangleInstance = null;
        private GameObject _rectangleBorderInstance = null;
        private RectangleGenerationData _instancedRectangleGenerationData = null;
        private RectangleBorderGenerationData _instancedBorderGenerationData = null;
        private Material _instancedRectangleMaterial = null;
        private Material _instancedBorderMaterial = null;

        private RectangleGenerationData RectangleGenerationData
        {
            get
            {
                if (_rectangleGenerationData == null)
                {
                    _rectangleGenerationData = new RectangleGenerationData();
                }
                return _rectangleGenerationData;
            }
        }
        private RectangleBorderGenerationData RectangleBorderGenerationData
        {
            get
            {
                if (_rectangleBorderGenerationData == null)
                {
                    _rectangleBorderGenerationData = new RectangleBorderGenerationData();
                }
                return _rectangleBorderGenerationData;
            }
        }
        private Texture DimensionsExplainTexture
        {
            get
            {
                if (_dimensionsExplainTexture == null)
                {
                    _dimensionsExplainTexture = Resources.Load<Texture>("DimensionsExplain");
                }
                return _dimensionsExplainTexture;
            }
        }

        /// <summary>
        /// Function shows the already existing instance of the editor window or creates a new one
        /// if one doesn't exist already.
        /// </summary>
        [MenuItem("BanSee/" + RECTANGLE_GENERATOR_TITLE)]
        public static void ShowWindow()
        {
            // Show existing window instance or create one if one doesn't exist.
            _window = EditorWindow.GetWindow<RectangleGeneratorEditorWindow>(RECTANGLE_GENERATOR_TITLE);
        }

        private void OnEnable()
        {
            _scrollPosition = Vector2.zero;
            _rectangleMaterial = Resources.Load<Material>("RectangleMaterial");
            _rectangleBorderMaterial = Resources.Load<Material>("RectangleBorderMaterial");
        }

        private void OnDestroy()
        {
            // Clearing references once the window closes.
            _window = null;
            _dimensionsExplainTexture = null;
        }

        private void OnGUI()
        {
            if (_window == null)
            {
                ShowWindow();
            }

            GUILayout.Label(RECTANGLE_GENERATOR_TITLE, TitleGUIStyle);
            GUILayout.Space(SPACE_BETWEEN_CATEGORIES);

            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, false, false);

            // Displaying image that explains the dimensions properties of the rounded rectangle.
            DisplayDimensionsExplainImage();
            GUILayout.Space(SPACE_BETWEEN_CATEGORIES);

            // Rectangle size properties (width, height, corner roundness, ...).
            DisplaySizeProperties();
            GUILayout.Space(SPACE_BETWEEN_CATEGORIES);

            // Rectangle rounded corners generation options (roundness percentage, resolution,..)
            DisplayCornerRoundnessProperties();
            GUILayout.Space(SPACE_BETWEEN_CATEGORIES);

            // Mesh topology, UV generation, etc.
            DisplayMeshGenerationProperties();
            GUILayout.Space(SPACE_BETWEEN_CATEGORIES);

            // Border generation options (size, additional depth, etc.)
            DisplayBorderGenerationProperties();
            GUILayout.Space(SPACE_BETWEEN_CATEGORIES);            

            GUILayout.EndScrollView();
            GUILayout.Space(SPACE_BETWEEN_CATEGORIES);

            // Options for automatic or manual creating/updating of the rectangle and border.
            DisplayVisualizationProperties();
        }

        private void DisplayDimensionsExplainImage()
        {
            Rect windowCurrentRect = _window.position;
            GUILayout.BeginVertical(BackgroundGUIStyle);
            float textureAspectRatio = (DimensionsExplainTexture.height / (float)DimensionsExplainTexture.width);
            Vector2 maxImageSize = new Vector2(windowCurrentRect.width, windowCurrentRect.height) - new Vector2(CenteredImageGUIStyle.padding.horizontal, CenteredImageGUIStyle.padding.vertical);
            maxImageSize.y = maxImageSize.x * textureAspectRatio;
            Vector2 desiredImageSize = Vector3.Scale(maxImageSize, Vector3.one * 0.75f);
            desiredImageSize.x += (maxImageSize.x - desiredImageSize.x) + CenteredImageGUIStyle.padding.left;
            GUILayout.Label(DimensionsExplainTexture, CenteredImageGUIStyle, GUILayout.MaxWidth(desiredImageSize.x), GUILayout.MaxHeight(desiredImageSize.y));
            GUILayout.EndVertical();
        }

        private void DisplaySizeProperties()
        {
            GUILayout.BeginVertical(BackgroundGUIStyle);
            GUILayout.Label("SIZE", HeaderGUIStyle);
            RectangleGenerationData.Width = DisplayLabeledFloat(nameof(RectangleGenerationData.Width), RectangleGenerationData.Width, RectangleGenerationData.MIN_RECTANGLE_SIZE);
            RectangleGenerationData.Height = DisplayLabeledFloat(nameof(RectangleGenerationData.Height), RectangleGenerationData.Height, RectangleGenerationData.MIN_RECTANGLE_SIZE);

            GUILayout.Space(SPACE_BETWEEN_CATEGORIES);
            GUILayout.BeginHorizontal();
            RectangleGenerationData.Is3D = EditorGUILayout.BeginToggleGroup("Is 3D Rectangle", RectangleGenerationData.Is3D);
            if (RectangleGenerationData.Is3D)
            {
                RectangleGenerationData.Depth = DisplayLabeledFloat(nameof(RectangleGenerationData.Depth), RectangleGenerationData.Depth, RectangleGenerationData.MIN_RECTANGLE_SIZE);
            }
            EditorGUILayout.EndToggleGroup();
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }

        private void DisplayCornerRoundnessProperties()
        {
            GUILayout.BeginVertical(BackgroundGUIStyle);
            GUILayout.Label("ROUNDED CORNERS", HeaderGUIStyle);

            EditorGUILayout.HelpBox("Corner roundness percentage is expressed as the percentage of the smaller dimension of the rectangle.", MessageType.Info);
            RectangleGenerationData.CornerRoundnessPercentage = DisplayLabeledFloatSlider(nameof(RectangleGenerationData.CornerRoundnessPercentage),
                RectangleGenerationData.CornerRoundnessPercentage,
                RectangleGenerationData.MIN_CORNER_ROUNDNESS_PERCENTAGE,
                RectangleGenerationData.MAX_CORNER_ROUNDNESS_PERCENTAGE);

            EditorGUI.BeginDisabledGroup(!RectangleGenerationData.IsRoundedRectangle);
            if (!RectangleGenerationData.IsRoundedRectangle)
            {
                EditorGUILayout.HelpBox("If roundness is set to 0%, the rectangle will be a regular one and won't have rounded corners.", MessageType.Warning);
                RectangleGenerationData.CornerVertexCount = 0;
            }

            RectangleGenerationData.CornerVertexCount = DisplayIntSlider(nameof(RectangleGenerationData.CornerVertexCount),
                RectangleGenerationData.CornerVertexCount,
                RectangleGenerationData.MIN_ROUNDED_CORNER_VERTEX_COUNT,
                RectangleGenerationData.MAX_ROUNDED_CORNER_VERTEX_COUNT);
            EditorGUI.EndDisabledGroup();

            GUILayout.EndVertical();
        }

        private void DisplayMeshGenerationProperties()
        {
            GUILayout.BeginVertical(BackgroundGUIStyle);
            GUILayout.Label("MESH TOPOLGY", HeaderGUIStyle);

            string uvModeInfoMessage = string.Empty;
            switch (RectangleGenerationData.UvMode)
            {
                case UVGenerationMode.AspectRatioFit:
                    uvModeInfoMessage = "Aspect Ratio Fit UV mode scales the UV coordinates so that the larger " +
                        "rectangle dimension fills the whole range (0, 1) while the smaller one fills the range " +
                        "based on rectangle's aspect ratio. The smaller dimension is also centered.";
                    break;
                case UVGenerationMode.Stretch:
                    uvModeInfoMessage = "Stretch UV mode doesn't scale the UV coordinates based on the rectangle size, " +
                        "both dimensions will be stretched inside the (0, 1) range.";
                    break;
                default:
                    Debug.LogError($"No info message implemented for selected UV generation mode: {RectangleGenerationData.UvMode}");
                    break;
            }

            if (!string.IsNullOrEmpty(uvModeInfoMessage))
            {
                EditorGUILayout.HelpBox(uvModeInfoMessage, MessageType.Info);
            }
            RectangleGenerationData.UvMode = (UVGenerationMode)DisplayEnumPopup(nameof(RectangleGenerationData.UvMode), RectangleGenerationData.UvMode);

            GUILayout.Space(SPACE_INSIDE_CATEGORY);

            EditorGUI.BeginDisabledGroup(RectangleGenerationData.MustRectangleUseCenterVertexConnection);
            string topologyInfoMessage = string.Empty;
            switch (RectangleGenerationData.TopologyType)
            {
                case RectangleTopologyType.CenterVertexConnection:
                    topologyInfoMessage = "Rectangle mesh will be generated by connecting all outer vertices with " +
                        "one center inner vertex in order to generate mesh triangles. Suitable for simple rectangle and circular shapes.";
                    break;
                case RectangleTopologyType.CornerConnections:
                    topologyInfoMessage = "Rectangle mesh will be generated by first generating simple inner rectangle and then connecting " +
                        "each corner vertex of the inner rectangle with the outer corner vertices that construct the rounded corners. " +
                        "Suitable for rounded rectangles.";
                    break;
                default:
                    Debug.LogError($"No info message implemented for selected rectangle topology type: {RectangleGenerationData.TopologyType}");
                    break;
            }

            if (!string.IsNullOrEmpty(topologyInfoMessage))
            {
                EditorGUILayout.HelpBox(topologyInfoMessage, MessageType.Info);
            }

            if (RectangleGenerationData.MustRectangleUseCenterVertexConnection)
            {
                EditorGUILayout.HelpBox("When the rectangle doesn't have rounded corners or when both width and height are the same with " +
                    $"max corner roundness percentage (NGon), the rectangle topology must be {RectangleTopologyType.CenterVertexConnection}.", MessageType.Warning);
            }
            RectangleTopologyType currentTopologyType = RectangleGenerationData.TopologyType;
            RectangleGenerationData.TopologyType = (RectangleTopologyType)DisplayEnumPopup(nameof(RectangleGenerationData.TopologyType), currentTopologyType);

            EditorGUI.EndDisabledGroup();
            GUILayout.EndVertical();
        }

        private void DisplayBorderGenerationProperties()
        {
            GUILayout.BeginVertical(BackgroundGUIStyle);
            GUILayout.Label("BORDER", HeaderGUIStyle);

            _createBorder = EditorGUILayout.BeginToggleGroup("Create Border", _createBorder);
            if (_createBorder)
            {
                RectangleBorderGenerationData.BorderThickness = DisplayLabeledFloat(nameof(RectangleBorderGenerationData.BorderThickness), RectangleBorderGenerationData.BorderThickness, RectangleBorderGenerationData.MIN_BORDER_THICKNESS);

                EditorGUI.BeginDisabledGroup(!RectangleGenerationData.Is3D);
                EditorGUILayout.HelpBox("Additional depth that the border will have compared to the rectangle it surrounds.", MessageType.Info);
                if (!RectangleGenerationData.Is3D)
                {
                    EditorGUILayout.HelpBox("For 2D rectangles, the border should also be 2D.", MessageType.Warning);
                    RectangleBorderGenerationData.BorderAdditionalDepth = 0;
                }

                RectangleBorderGenerationData.BorderAdditionalDepth = DisplayLabeledFloat(nameof(RectangleBorderGenerationData.BorderAdditionalDepth), RectangleBorderGenerationData.BorderAdditionalDepth, 0f);
                EditorGUI.EndDisabledGroup();
            }

            EditorGUILayout.EndToggleGroup();
            GUILayout.EndVertical();
        }

        private void DisplayVisualizationProperties()
        {
            GUILayout.BeginVertical(BackgroundGUIStyle);
            GUILayout.Label("VISUALIZATION", HeaderGUIStyle);

            _rectangleMaterial = (Material)EditorGUILayout.ObjectField("Rectangle Material", _rectangleMaterial, typeof(Material), false);
            if (_createBorder)
            {
                _rectangleBorderMaterial = (Material)EditorGUILayout.ObjectField("Border Material", _rectangleBorderMaterial, typeof(Material), false);
            }

            _automaticUpdate = DisplayLabeledToggle("Automatic Update", _automaticUpdate);

            bool shouldRegenerateRectangle = ShouldCreateOrUpdateRectangle();
            bool shouldRegenerateBorder = ShouldCreateOrUpdateRectangleBorder();
            bool rectangleUpdateRequested = false;
            bool rectangleBorderUpdateRequested = false;

            if (_automaticUpdate)
            {
                rectangleUpdateRequested = true;
                rectangleBorderUpdateRequested = true;
            }
            else
            {
                if (GUILayout.Button("Create/Update Rectangle"))
                {
                    // Forcing rectangle update when user manually clicks.
                    shouldRegenerateRectangle = true;
                    rectangleUpdateRequested = true;
                }

                if (_createBorder && GUILayout.Button("Create/Update Border"))
                {
                    // Forcing rectangle border update when user manually clicks.
                    shouldRegenerateBorder = true;
                    rectangleBorderUpdateRequested = true;
                }
            }

            if(shouldRegenerateRectangle && rectangleUpdateRequested)
            {
                CreateOrUpdateRectangle();
            }

            if(_createBorder && shouldRegenerateBorder && rectangleBorderUpdateRequested)
            {
                CreateOrUpdateBorder();
            }

            // Border cleanup in case it was previously created but not wanted anymore.
            if(!_createBorder && _rectangleBorderInstance != null)
            {
                DestroyImmediate(_rectangleBorderInstance);
                _instancedBorderGenerationData = null;
                _instancedBorderMaterial = null;
            }

            GUILayout.EndVertical();
        }

        private bool ShouldCreateOrUpdateRectangle()
        {
            if(_instancedRectangleGenerationData == null)
            {
                // If we haven't generated the rectangle/border yet, it means
                // that we need to create it.
                return true;
            }

            if (!_instancedRectangleGenerationData.Equals(RectangleGenerationData))
            {
                return true;
            }

            if(_instancedRectangleMaterial != null &&
                !_instancedRectangleMaterial.Equals(_rectangleMaterial))
            {
                // Rectangle properties haven't changed, but the rectangle material changed.
                return true;
            }

            return false;
        }

        private bool ShouldCreateOrUpdateRectangleBorder()
        {
            if (_instancedBorderGenerationData == null)
            {
                // If we haven't generated the rectangle/border yet, it means
                // that we need to create it.
                return true;
            }

            if (!_instancedBorderGenerationData.Equals(RectangleBorderGenerationData))
            {
                return true;
            }

            if (_instancedBorderMaterial != null &&
                !_instancedBorderMaterial.Equals(_rectangleBorderMaterial))
            {
                // Rectangle properties haven't changed, but the rectangle material changed.
                return true;
            }

            if (ShouldCreateOrUpdateRectangle())
            {
                // In case there were some changes on the rectangle itself, we need to regenerate the border as well.
                return true;
            }

            return false;
        }

        private void CreateOrUpdateRectangle()
        {
            if(_rectangleInstance != null)
            {
                DestroyImmediate(_rectangleInstance);
            }

            MeshData rectangleMeshData = RectangleMeshGenerator.GenerateRectangleMeshData(RectangleGenerationData);
            Mesh rectangleMesh = Utils.CreateMeshFromMeshData(rectangleMeshData);
            _rectangleInstance = Utils.CreateMeshVisualizer(rectangleMesh, "Rectangle", _rectangleMaterial);
            _instancedRectangleGenerationData = new RectangleGenerationData(RectangleGenerationData);
            _instancedRectangleMaterial = _rectangleMaterial;
        }

        private void CreateOrUpdateBorder()
        {
            if (_rectangleBorderInstance != null)
            {
                DestroyImmediate(_rectangleBorderInstance);
            }

            MeshData borderMeshData = RectangleBorderGenerator.GenerateBorder(RectangleGenerationData, RectangleBorderGenerationData);
            Mesh rectangleBorderMesh = Utils.CreateMeshFromMeshData(borderMeshData);
            _rectangleBorderInstance = Utils.CreateMeshVisualizer(rectangleBorderMesh, "Rectangle Border", _rectangleBorderMaterial);
            _instancedBorderGenerationData = new RectangleBorderGenerationData(RectangleBorderGenerationData);
            _instancedBorderMaterial = _rectangleBorderMaterial;
        }
    }
}