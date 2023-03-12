using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BanSee.RoundedRectangleGenerator
{
    /// <summary>
    /// Static class containing utility functions useful during rectangle creation
    /// and assets saving process.
    /// </summary>
    public static class RectangleCreationUtility
    {
        /// <summary>
        /// Function creates a new instance of the <see cref="Mesh"/> class and
        /// populates it with the data from the provided <paramref name="meshData"/>.
        /// It also recalculates bounds of the mesh and returns the created instance.
        /// </summary>
        /// <param name="meshData">Reference to the <see cref="MeshData"/> class 
        /// containing information about vertices, triangles, normals and UV coordinates
        /// of the mesh.</param>
        /// <returns>Instance of the mesh with applied settings from the provided
        /// <paramref name="meshData"/>.</returns>
        public static Mesh CreateMeshFromMeshData(MeshData meshData)
        {
            if (meshData == null)
            {
                return null;
            }

            Mesh mesh = new Mesh();
            meshData.ApplyToMesh(ref mesh);
            mesh.RecalculateBounds();
            return mesh;
        }

        /// <summary>
        /// Function creates a new game object and assigns a <see cref="MeshFilter"/>
        /// component and a <see cref="MeshRenderer"/> component to it in order to represent
        /// the provided <paramref name="mesh"/>.
        /// </summary>
        /// <param name="mesh">Reference to the <see cref="Mesh"/> instance that will
        /// be visualized on a new game object.</param>
        /// <param name="name">Name that will be assigned to the created game object.</param>
        /// <param name="material">Material that will be assigned to visually represent
        /// the mesh on a <see cref="MeshRenderer"/>. NOTE - material won't be instanced, meaning
        /// that it will be assigned to the <see cref="MeshRenderer.sharedMaterial"/> slot.</param>
        /// <returns>Reference to the instantiated game object.</returns>
        public static GameObject CreateMeshVisualizer(Mesh mesh, string name, Material material)
        {
            GameObject meshHolderGameObject = new GameObject(name);
            MeshFilter meshFilter = meshHolderGameObject.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = mesh;

            MeshRenderer meshRenderer = meshHolderGameObject.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = material;

            return meshHolderGameObject;
        }

        /// <summary>
        /// Function saves the <paramref name="mesh"/> as an asset
        /// to the asset database of the project, to a desired location specified 
        /// by the user, with the name also specified by the user. Location and name
        /// are being defined by using the EditorUtility methods.
        /// method.
        /// </summary>
        /// <param name="mesh">Reference to the <see cref="Mesh"/> that needs to be saved
        /// to the assets folder of the project.</param>
        /// <param name="name">Name of the mesh asset that will be assigned during the saving.</param>
        public static void SaveMeshAsAsset(Mesh mesh, string name)
        {
            if (mesh == null)
            {
                Debug.LogError($"There is no mesh assigned for saving.");
                return;
            }

#if UNITY_EDITOR
            string path = EditorUtility.SaveFilePanel("Save Mesh Asset", "Assets/", name, "asset");
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            path = FileUtil.GetProjectRelativePath(path);
            AssetDatabase.CreateAsset(mesh, path);
            AssetDatabase.SaveAssets();
#endif
        }
    }
}
