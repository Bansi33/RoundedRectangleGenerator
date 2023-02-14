using System;

namespace BanSee.RoundedRectangleGenerator
{
    /// <summary>
    /// Class representing the generation properties for the rounded rectangle
    /// generation. It also provides insightful messages on why the rectangle
    /// can't be generated - in cases of wrong data entry.
    /// </summary>
    [Serializable]
    public class RectangleGenerationData
    {
        public float Width;
        public float Height;

        public bool Is3D;
        public float Depth;

        public float CornerRoundnessRadius;
        public int CornerVertexCount;

        public UVGenerationMode UvMode;
        public RectangleTopologyType TopologyType;

        public bool HasBorder;
        public float BorderThickness;
        public float BorderAdditionalDepth;

        private string _validationErrorMessage = string.Empty;

        /// <summary>
        /// Rectangle is a rounded rectangle if it has a corner roundness radius 
        /// greater than zero. Number of corner vertices doesn't matter, it only
        /// represents the roundness more clearer.
        /// </summary>
        public bool IsRoundedRectangle { get { return CornerRoundnessRadius > 0f; } }
        /// <summary>
        /// String representing the error message explaining why the user isn't able to 
        /// generate the rounded rectangle.
        /// </summary>
        public string ValidationErrorMessage 
        { 
            get
            {
                return _validationErrorMessage;
            }
        }

        public RectangleGenerationData()
        {
            Width = 1f;
            Height = 1f;

            Is3D = false;
            Depth = 0.2f;

            CornerRoundnessRadius = 0.1f;
            CornerVertexCount = 0;

            UvMode = UVGenerationMode.AspectRatioFit;
            TopologyType = RectangleTopologyType.CenterVertexConnection;

            HasBorder = false;
            BorderThickness = 0f;
            BorderAdditionalDepth = 0f;
        }

        public RectangleGenerationData(RectangleGenerationData source)
        {
            Width = source.Width;
            Height = source.Height;

            Is3D = source.Is3D;
            Depth = source.Depth;

            CornerRoundnessRadius = source.CornerRoundnessRadius;
            CornerVertexCount = source.CornerVertexCount;

            UvMode = source.UvMode;
            TopologyType = source.TopologyType;

            HasBorder = source.HasBorder;
            BorderThickness = source.BorderThickness;
            BorderAdditionalDepth = source.BorderAdditionalDepth;
        }

        /// <summary>
        /// Function checks if the provided data is valid for generating a rectangle or a 
        /// rounded rectangle. It also caches the reason for the failure of validation, if 
        /// there is one.
        /// </summary>
        /// <returns>True if the data is valid and the rounded rectangle can be generated, false
        /// otherwise.</returns>
        public bool IsDataValid()
        {
            _validationErrorMessage = string.Empty;

            if (Width <= 0f)
            {
                _validationErrorMessage = $"Width of the rectangle must be greater than 0.";
                return false;
            }

            if (Height <= 0f)
            {
                _validationErrorMessage = $"Height of the rectangle must be greater than 0.";
                return false;
            }

            if (CornerRoundnessRadius < 0f)
            {
                // Corner roundness radius can be null, but not negative.
                _validationErrorMessage = $"Corner roundness must be greater or equal than 0! It represents the radius of the " +
                    $"circle used for displacing the corner vertices of the rounded rectangle.";
                return false;
            }

            if (Is3D && Depth <= 0f)
            {
                _validationErrorMessage = $"When generating 3D rectangles, the depth must be greater than 0. Either " +
                    $"remove the check box next to the 3D option, or increase the desired depth of the rectangle.";
                return false;
            }

            float cornerRoundnessDiameter = 2f * CornerRoundnessRadius;
            if(Width < cornerRoundnessDiameter)
            {
                _validationErrorMessage = $"Width of the rectangle is too small. The minimum value a width can have " +
                    $"is double the value of the {nameof(CornerRoundnessRadius)}.";
                return false;
            }

            if (Height < cornerRoundnessDiameter)
            {
                _validationErrorMessage = $"Height of the rectangle is too small. The minimum value a height can have " +
                    $"is double the value of the {nameof(CornerRoundnessRadius)}.";
                return false;
            }

            if (HasBorder && BorderThickness <= 0f)
            {
                _validationErrorMessage = $"When generating border, the border needs to have thickness larger than 0.";
                return false;
            }

            return true;
        }
    }
}
