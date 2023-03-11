using System;
using UnityEngine;
using UnityEditor;
using static BanSee.RoundedRectangleGenerator.RectangleGeneratorGUI;

namespace BanSee.RoundedRectangleGenerator
{
    /// <summary>
    /// Static class containing useful functions for constructing editor windows.
    /// </summary>
    public static class EditorUtils
    {
        /// <summary>
        /// Function displays a float field along with the specified label in a horizontal layout.
        /// </summary>
        /// <param name="label">Name of the float field that will be displayed alongside the float field.</param>
        /// <param name="value">The name that the float field should have.</param>
        /// <returns>Updated float value that the user changed through the inspector.</returns>
        public static float DisplayLabeledFloat(string label, float value)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(ObjectNames.NicifyVariableName(label), EditorStyles.boldLabel);
            float updatedValue = EditorGUILayout.FloatField(value);
            GUILayout.EndHorizontal();
            return updatedValue;
        }

        /// <summary>
        /// Function displays a float field along with the specified label in a horizontal layout.
        /// The float value is restricted by the <paramref name="minValue"/>.
        /// </summary>
        /// <param name="label">Name of the float field that will be displayed alongside the float field.</param>
        /// <param name="value">The name that the float field should have.</param>
        /// <param name="minValue">Minimum value that the user can select.</param>
        /// <returns>Updated float value that the user changed through the inspector, constricted
        /// to be at least the <paramref name="minValue"/>.</returns>
        public static float DisplayLabeledFloat(string label, float value, float minValue)
        {
            return Mathf.Max(DisplayLabeledFloat(label, value), minValue);
        }

        /// <summary>
        /// Function displays a float slider along with the specified label in a horizontal layout.
        /// </summary>
        /// <param name="label">Name of the float slider that will be displayed alongside the float slider.</param>
        /// <param name="value">The name that the float slider should have.</param>
        /// <param name="minValue">Minimum value that the float slider can have.</param>
        /// <param name="maxValue">Maximum value that the float slider can have.</param>
        /// <returns>Updated float value that the user changed through the inspector.</returns>
        public static float DisplayLabeledFloatSlider(string label, float value, float minValue, float maxValue)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(ObjectNames.NicifyVariableName(label), EditorStyles.boldLabel);
            float updatedValue = EditorGUILayout.Slider(value, minValue, maxValue);
            GUILayout.EndHorizontal();
            return updatedValue;
        }

        /// <summary>
        /// Function displays an integer slider along with the specified label in a horizontal layout.
        /// </summary>
        /// <param name="label">Name of the integer slider that will be displayed alongside the integer slider.</param>
        /// <param name="value">The name that the integer slider should have.</param>
        /// <param name="minValue">Minimum value that the integer slider can have.</param>
        /// <param name="maxValue">Maximum value that the integer slider can have.</param>
        /// <returns>Updated integer value that the user changed through the inspector.</returns>
        public static int DisplayIntSlider(string label, int value, int minValue, int maxValue)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(ObjectNames.NicifyVariableName(label), EditorStyles.boldLabel);
            int updatedValue = EditorGUILayout.IntSlider(value, minValue, maxValue);
            GUILayout.EndHorizontal();
            return updatedValue;
        }

        /// <summary>
        /// Function displays an enumeration pop-up along with the specified label in a horizontal layout.
        /// </summary>
        /// <param name="label">Name of the enumeration that will be displayed alongside the enumeration pop-up.</param>
        /// <param name="enumForDisplay">Enumeration whose options will be displayed and the value selected.</param>
        /// <returns>Updated enumeration value that the user changed through the inspector.</returns>
        public static Enum DisplayEnumPopup(string label, Enum enumForDisplay)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(ObjectNames.NicifyVariableName(label), EditorStyles.boldLabel);
            Enum updatedValue = EditorGUILayout.EnumPopup(enumForDisplay);
            GUILayout.EndHorizontal();
            return updatedValue;
        }

        /// <summary>
        /// Function displays a labeled toggle check box.
        /// </summary>
        /// <param name="label">Label of the toggle check box that will appear right of the toggle.</param>
        /// <param name="toggleValue">Current value of the toggle check box.</param>
        /// <returns>An updated value of the toggle check box that the user selected.</returns>
        public static bool DisplayLabeledToggle(string label, bool toggleValue)
        {
            GUILayout.BeginHorizontal();
            bool updatedValue = EditorGUILayout.Toggle(toggleValue, ToggleSizeOptions);
            GUILayout.Label(label, ToggleLabelGUIStyle);
            GUILayout.EndHorizontal();

            return updatedValue;
        }
    }
}
