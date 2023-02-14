using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BanSee.RoundedRectangleGenerator
{
    /// <summary>
    /// Class used for saving the generated mesh as an asset
    /// in the assets database of the project.
    /// </summary>
    [RequireComponent(typeof(MeshFilter))]
    public class MeshSave : MonoBehaviour
    {
        private MeshFilter _meshFilter = null;

        private MeshFilter MeshFilter
        {
            get
            {
                if (_meshFilter == null)
                {
                    _meshFilter = GetComponent<MeshFilter>();
                }

                return _meshFilter;
            }
        }

        /// <summary>
        /// Function returns true if the mesh is contained within
        /// the <see cref="MeshFilter"/> component. False otherwise.
        /// </summary>
        /// <returns>True if the mesh is contained within
        /// the <see cref="MeshFilter"/> component. False otherwise.</returns>
        public bool CanSaveMesh()
        {
            return MeshFilter.sharedMesh != null;
        }

        /// <summary>
        /// Function saves the <see cref="MeshFilter.sharedMesh"/> as an asset
        /// to the asset database of the project, to a desired location specified 
        /// by the user, with the name also specified by the user. Location and name
        /// are being defined by using the EditorUtility methods.
        /// method.
        /// </summary>
        public void SaveMeshAsAsset()
        {
            Mesh mesh = MeshFilter.sharedMesh;
            if (mesh == null)
            {
                Debug.LogError($"There is no mesh assigned to the {nameof(MeshFilter)} component.");
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