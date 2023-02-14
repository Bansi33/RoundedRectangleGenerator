using UnityEngine;

namespace BanSee.RoundedRectangleGenerator
{
    /// <summary>
    /// Component used to display the <see cref="RectangleGenerationData"/> properties
    /// in the inspector and provide an example of rounded rectangle generation.
    /// </summary>
    public class RoundedRectangleGenerator : MonoBehaviour
    {
        [Header("References:")]
        [SerializeField] private Material _roundedRectangleMaterial = null;
        [SerializeField] private Material _roundedRectangleBorderMaterial = null;

        [Header("Options:")]
        [SerializeField] private RectangleGenerationData _rectangleGenerationData = default;

        /// <summary>
        /// Reference to the <see cref="RoundedRectangleGenerator.RectangleGenerationData"/> class
        /// containing properties for generating the rounded rectangle mesh.
        /// </summary>
        public RectangleGenerationData RectangleGenerationData { get { return _rectangleGenerationData; } }

        /// <summary>
        /// Function triggers the generation of the rounded rectangle and creates game object that will
        /// display the generated mesh.
        /// </summary>
        public void GenerateRoundedRectangle()
        {
            Mesh rectangleMesh = GenerateRoundedRectangleMesh();
            if(rectangleMesh == null)
            {
                Debug.LogError("Couldn't generate the rectangle mesh!");
                return;
            }

            CreateMeshVisualizer(rectangleMesh, "RoundedRectangle", _roundedRectangleMaterial);
        }

        private Mesh GenerateRoundedRectangleMesh()
        {
            if (!_rectangleGenerationData.IsDataValid())
            {                
                return null;
            }

            RectangleMeshData rectangleMeshData;
            if (Mathf.Approximately(_rectangleGenerationData.CornerRoundnessRadius, 0f))
            {
                // In case there is no corner roundness required, rounded rectangle
                // is actually an ordinary rectangle. Regular rectangle generation logic
                // is separated for clarity.
                rectangleMeshData = RectangleMeshGenerator.GenerateRectangleMeshData(_rectangleGenerationData);
            }
            else
            {
                rectangleMeshData = RoundedRectangleMeshGenerator.GenerateRoundedRectangleMeshData(_rectangleGenerationData);
            }

            if(_rectangleGenerationData.HasBorder && _rectangleGenerationData.BorderThickness > 0f)
            {
                MeshData borderMeshData = RectangleBorderGenerator.GenerateBorder(_rectangleGenerationData, rectangleMeshData);
                if (_rectangleGenerationData.Is3D && _rectangleGenerationData.Depth > 0f) {
                    MeshSolidify.SolidifyRectangleBorder(borderMeshData, _rectangleGenerationData);
                }
                Mesh borderMesh = CreateMeshFromMeshData(borderMeshData);
                CreateMeshVisualizer(borderMesh, "Border", _roundedRectangleBorderMaterial);
            }

            // In case the request is for a 3D rectangle, solidify the mesh with third dimension.
            if (_rectangleGenerationData.Is3D && _rectangleGenerationData.Depth > 0f)
            {
                MeshSolidify.SolidifyRectangle(rectangleMeshData, _rectangleGenerationData);
            }

            return CreateMeshFromMeshData(rectangleMeshData);            
        }

        private Mesh CreateMeshFromMeshData(MeshData meshData)
        {
            Mesh generatedMesh = new Mesh();
            meshData.ApplyToMesh(ref generatedMesh);
            generatedMesh.RecalculateBounds();
            return generatedMesh;
        }

        private void CreateMeshVisualizer(Mesh mesh, string name, Material material)
        {
            GameObject meshHolderGameObject = new GameObject(name);
            MeshFilter meshFilter = meshHolderGameObject.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = mesh;

            MeshRenderer meshRenderer = meshHolderGameObject.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = material;
        }
    }
}