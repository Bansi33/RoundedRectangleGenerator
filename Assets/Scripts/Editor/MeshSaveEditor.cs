using UnityEditor;
using UnityEngine;

namespace BanSee.RoundedRectangleGenerator
{
    /// <summary>
    /// Custom editor class for generating inspector look of the 
    /// <see cref="MeshSave"/> component.
    /// </summary>
    [CustomEditor(typeof(MeshSave))]
    public class MeshSaveEditor : Editor
    {
        private const float SPACE_BETWEEN_CATEGORIES = 20f;

        private MeshSave _meshSave = null;

        private void OnEnable()
        {
            _meshSave = (MeshSave)target;
        }

        /// <inheritdoc/>
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            bool canSaveMesh = _meshSave.CanSaveMesh();
            if (!canSaveMesh)
            {
                // If the data isn't valid for generation of the rounded rectangle, write out the
                // message informing the user why the generation isn't currently possible.                
                EditorGUILayout.HelpBox($"Please assign a mesh to the {nameof(MeshFilter)} component " +
                    $"in order to save it as an asset. No mesh found.", MessageType.Error);
                EditorGUILayout.Space(SPACE_BETWEEN_CATEGORIES);
            }

            EditorGUI.BeginDisabledGroup(!canSaveMesh);

            if (GUILayout.Button(nameof(_meshSave.SaveMeshAsAsset)))
            {
                _meshSave.SaveMeshAsAsset();
            }

            EditorGUI.EndDisabledGroup();
        }
    }
}