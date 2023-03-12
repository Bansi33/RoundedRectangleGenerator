using UnityEngine;

namespace BanSee.RoundedRectangleGenerator
{
    /// <summary>
    /// Component used to display the <see cref="RectangleGenerationData"/> properties
    /// in the inspector and provide an example of rounded rectangle generation via code.
    /// </summary>
    public class RectangleGenerationExample : MonoBehaviour
    {
        [Header("References:")]
        [SerializeField] private Material _roundedRectangleMaterial = null;
        [SerializeField] private Material _rectangleBorderMaterial = null;

        [Header("Options:")]
        [SerializeField] private RectangleGenerationData _rectangleGenerationData = default;
        [SerializeField] private RectangleBorderGenerationData _rectangleBorderGenerationData = default;

        private GameObject _rectangleInstance = null;
        private GameObject _borderInstance = null;

        /// <summary>
        /// Reference to the <see cref="RoundedRectangleGenerator.RectangleGenerationData"/> class
        /// containing properties for generating the rounded rectangle mesh.
        /// </summary>
        public RectangleGenerationData RectangleGenerationData { get { return _rectangleGenerationData; } }
        /// <summary>
        /// Reference to the <see cref="RoundedRectangleGenerator.RectangleBorderGenerationData"/> class
        /// containing properties for generating the rectangle border mesh.
        /// </summary>
        public RectangleBorderGenerationData RectangleBorderGenerationData { get { return _rectangleBorderGenerationData; } }

        /// <summary>
        /// Function triggers the generation of the rounded rectangle and creates game object that will
        /// display the generated mesh.
        /// </summary>
        public void GenerateRectangle()
        {
            Mesh rectangleMesh = GenerateRoundedRectangleMesh();
            if(rectangleMesh == null)
            {
                Debug.LogError("Couldn't generate the rectangle mesh!");
                return;
            }

            DestroyInstance(_rectangleInstance);
            _rectangleInstance = RectangleCreationUtility.CreateMeshVisualizer(rectangleMesh, "Rectangle", _roundedRectangleMaterial);
        }

        /// <summary>
        /// Function triggers the generation of the rectangle border and creates game object that will
        /// display the generated border mesh.
        /// </summary>
        public void GenerateRectangleBorder()
        {
            Mesh rectangleBorderMesh = GenerateRectangleBorderMesh();
            if (rectangleBorderMesh == null)
            {
                Debug.LogError("Couldn't generate the rectangle border mesh!");
                return;
            }

            DestroyInstance(_borderInstance);
            _borderInstance = RectangleCreationUtility.CreateMeshVisualizer(rectangleBorderMesh, "Rectangle Border", _rectangleBorderMaterial);
        }

        private Mesh GenerateRoundedRectangleMesh()
        {
            MeshData rectangleMeshData = RectangleMeshGenerator.GenerateRectangleMeshData(_rectangleGenerationData);
            return RectangleCreationUtility.CreateMeshFromMeshData(rectangleMeshData);            
        }

        private Mesh GenerateRectangleBorderMesh()
        {
            MeshData borderMeshData = RectangleBorderMeshGenerator.GenerateBorder(_rectangleGenerationData, _rectangleBorderGenerationData);            
            return RectangleCreationUtility.CreateMeshFromMeshData(borderMeshData);
        }

        private void DestroyInstance(GameObject instance)
        {
            if(instance != null)
            {
                DestroyImmediate(instance);
                instance = null;
            }
        }
    }
}