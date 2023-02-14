using UnityEditor;
using UnityEngine;

namespace BanSee.RoundedRectangleGenerator
{
    /// <summary>
    /// Custom editor class for generating inspector look of the 
    /// <see cref="RoundedRectangleGenerator"/> component.
    /// </summary>
    [CustomEditor(typeof(RoundedRectangleGenerator))]
    public class RoundedRectangleGeneratorEditor : Editor
    {
        private const float SPACE_BETWEEN_CATEGORIES = 20f;

        private RoundedRectangleGenerator _roundedRectangleGenerator = null;

        private void OnEnable()
        {
            _roundedRectangleGenerator = (RoundedRectangleGenerator)target;
        }

        /// <inheritdoc/>
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();            

            // Drawing validation errors to the user, so he knows what needs to be corrected
            // in order for successful rectangle generation.
            RectangleGenerationData rectangleGenerationData = _roundedRectangleGenerator.RectangleGenerationData;
            bool isDataValidForGeneration = rectangleGenerationData.IsDataValid();
            if (!isDataValidForGeneration)
            {
                // If the data isn't valid for generation of the rounded rectangle, write out the
                // message informing the user why the generation isn't currently possible.
                EditorGUILayout.Space(SPACE_BETWEEN_CATEGORIES);
                EditorGUILayout.HelpBox(rectangleGenerationData.ValidationErrorMessage, MessageType.Error);
            }

            // Drawing area with buttons for generation of rounded rectangle.
            EditorGUILayout.Space(SPACE_BETWEEN_CATEGORIES);
            EditorGUI.BeginDisabledGroup(!isDataValidForGeneration);

            if (GUILayout.Button(nameof(_roundedRectangleGenerator.GenerateRoundedRectangle)))
            {
                _roundedRectangleGenerator.GenerateRoundedRectangle();
            }

            EditorGUI.EndDisabledGroup();
        }
    }
}