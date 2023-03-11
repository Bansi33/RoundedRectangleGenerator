using UnityEngine;
using UnityEditor;

namespace BanSee.RoundedRectangleGenerator
{
    /// <summary>
    /// Static class containing GUI styles required for rendering the 
    /// <see cref="RectangleGeneratorEditorWindow"/> properly.
    /// </summary>
    public static class RectangleGeneratorGUI
    {
        private const int PADDING = 10;

        private static GUIStyle _titleGUIStyle = null;
        private static GUIStyle _headerGUIStyle = null;
        private static GUIStyle _toggleLabelGUIStyle = null;
        private static GUIStyle _backgroundGUIStyle = null;
        private static GUIStyle _centeredImageGUIStyle = null;

        /// <summary>
        /// Property defining GUI style for displaying titles in the editor window.
        /// </summary>
        public static GUIStyle TitleGUIStyle
        {
            get
            {
                if (_titleGUIStyle == null)
                {
                    _titleGUIStyle = new GUIStyle("HeaderLabel");
                    _titleGUIStyle.fontSize = 30;
                    _titleGUIStyle.fontStyle = FontStyle.Bold;
                    _titleGUIStyle.margin.top = -50;
                    _titleGUIStyle.alignment = TextAnchor.UpperCenter;
                }

                return _titleGUIStyle;
            }
        }

        /// <summary>
        /// Property defining GUI style for displaying headers in the editor window.
        /// </summary>
        public static GUIStyle HeaderGUIStyle
        {
            get
            {
                if (_headerGUIStyle == null)
                {
                    _headerGUIStyle = new GUIStyle("HeaderLabel");
                    _headerGUIStyle.fontSize = 20;
                    _headerGUIStyle.fontStyle = FontStyle.Bold;
                    _headerGUIStyle.margin.top = -50;
                    _headerGUIStyle.alignment = TextAnchor.MiddleLeft;
                }

                return _headerGUIStyle;
            }
        }

        /// <summary>
        /// Property defining GUI style for displaying labels of the toggle check boxes.
        /// </summary>
        public static GUIStyle ToggleLabelGUIStyle
        {
            get
            {
                if (_toggleLabelGUIStyle == null)
                {
                    _toggleLabelGUIStyle = new GUIStyle("HeaderLabel");
                    _toggleLabelGUIStyle.fontStyle = FontStyle.Bold;
                    _toggleLabelGUIStyle.alignment = TextAnchor.UpperLeft;
                }

                return _toggleLabelGUIStyle;
            }
        }

        /// <summary>
        /// Property defining GUI style for rendering background areas of editor options.
        /// </summary>
        public static GUIStyle BackgroundGUIStyle
        {
            get
            {
                if (_backgroundGUIStyle == null)
                {
                    _backgroundGUIStyle = new GUIStyle(EditorStyles.textArea);
                    _backgroundGUIStyle.fixedHeight = 0;
                    _backgroundGUIStyle.overflow.left = 50;
                    _backgroundGUIStyle.overflow.right = 50;
                    _backgroundGUIStyle.overflow.top = -5;
                    _backgroundGUIStyle.overflow.bottom = 0;
                    _backgroundGUIStyle.padding = new RectOffset(PADDING, PADDING, PADDING, PADDING);
                }

                return _backgroundGUIStyle;
            }
        }

        /// <summary>
        /// GUI style for images that need to be displayed with the center alignment.
        /// </summary>
        public static GUIStyle CenteredImageGUIStyle
        {
            get
            {
                if (_centeredImageGUIStyle == null)
                {
                    _centeredImageGUIStyle = new GUIStyle();
                    _centeredImageGUIStyle.alignment = TextAnchor.MiddleCenter;
                    _centeredImageGUIStyle.padding = new RectOffset(PADDING, PADDING, PADDING, PADDING);
                }
                return _centeredImageGUIStyle;
            }
        }

        /// <summary>
        /// Property providing size options for creating toggle check boxes in the editor window.
        /// </summary>
        public static GUILayoutOption[] ToggleSizeOptions
        {
            get
            {
                return new GUILayoutOption[]
                {
                    GUILayout.MaxWidth(20),
                    GUILayout.MaxHeight(30)
                };
            }
        }
    }
}
