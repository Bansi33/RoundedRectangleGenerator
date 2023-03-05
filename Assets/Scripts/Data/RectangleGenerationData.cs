using System;
using UnityEngine;

namespace BanSee.RoundedRectangleGenerator
{
    /// <summary>
    /// Class representing the generation properties for the rounded rectangle
    /// generation. It also provides insightful messages on why the rectangle
    /// can't be generated - in cases of wrong data entry.
    /// </summary>
    [Serializable]
    public class RectangleGenerationData : ValidatableData
    {
        private const float MIN_CORNER_ROUNDNESS_PERCENTAGE = 0f;
        private const float MAX_CORNER_ROUNDNESS_PERCENTAGE = 0.5f;
        
        public float Width;
        public float Height;

        public bool Is3D;
        public float Depth;

        [Range(MIN_CORNER_ROUNDNESS_PERCENTAGE, MAX_CORNER_ROUNDNESS_PERCENTAGE)] 
        public float CornerRoundnessPercentage;
        public int CornerVertexCount;

        public UVGenerationMode UvMode;
        [SerializeField] private RectangleTopologyType _topologyType;

        /// <summary>
        /// Property returning the smaller dimension of the rectangle.
        /// </summary>
        private float SmallerDimension { get { return Width < Height ? Width : Height; } }

        /// <summary>
        /// Property returning the radius of the corner circle used for generating the
        /// rounded corner vertices.
        /// </summary>
        public float CornerRoundnessRadius
        {
            get
            {
                return SmallerDimension * CornerRoundnessPercentage;
            }
        }
        /// <summary>
        /// Property specifying the topology type of the rectangle.
        /// </summary>
        public RectangleTopologyType TopologyType 
        { 
            get 
            {
                // In case the rectangle is actually a square and the roundness is set to maximum,
                // we're actually talking about a circle, or a polygon (hexagon, octagon, ...) which
                // should use the center vertex connection topology since it doesn't have a rectangle
                // on the inside of the front face as the base.
                bool isNGon = Mathf.Approximately(CornerRoundnessPercentage, MAX_CORNER_ROUNDNESS_PERCENTAGE) &&
                   Mathf.Approximately(Width, Height);

                // Simple rectangles don't have corner roundness, they don't need additional corner vertex connections.
                bool isSimpleRectangle = !IsRoundedRectangle;

                if (isNGon || isSimpleRectangle)
                {                    
                    _topologyType = RectangleTopologyType.CenterVertexConnection;
                }

                return _topologyType; 
            } 
        }
        /// <summary>
        /// Rectangle is a rounded rectangle if it has a corner roundness radius 
        /// greater than zero. Number of corner vertices doesn't matter, it only
        /// represents the roundness more clearer.
        /// </summary>
        public bool IsRoundedRectangle { get { return CornerRoundnessRadius > 0f; } }

        public RectangleGenerationData()
        {
            Width = 1f;
            Height = 1f;

            Is3D = false;
            Depth = 0.2f;

            CornerRoundnessPercentage = 0.1f;
            CornerVertexCount = 0;

            UvMode = UVGenerationMode.AspectRatioFit;
            _topologyType = RectangleTopologyType.CenterVertexConnection;
        }

        public RectangleGenerationData(RectangleGenerationData source)
        {
            Width = source.Width;
            Height = source.Height;

            Is3D = source.Is3D;
            Depth = source.Depth;

            CornerRoundnessPercentage = source.CornerRoundnessPercentage;
            CornerVertexCount = source.CornerVertexCount;

            UvMode = source.UvMode;
            _topologyType = source.TopologyType;
        }

        /// <inheritdoc/>
        protected override void ValidateData()
        {
            if (Width <= 0f)
            {
                AppendErrorMessage($"Width of the rectangle must be greater than 0.");
            }

            if (Height <= 0f)
            {
                AppendErrorMessage($"Height of the rectangle must be greater than 0.");
            }

            if (CornerRoundnessPercentage < MIN_CORNER_ROUNDNESS_PERCENTAGE || 
                CornerRoundnessPercentage > MAX_CORNER_ROUNDNESS_PERCENTAGE)
            {
                // Corner roundness radius can be null, but not negative.
                AppendErrorMessage($"Corner roundness percentage must be greater or equal than " +
                    $"{MAX_CORNER_ROUNDNESS_PERCENTAGE} and less or equal to {MAX_CORNER_ROUNDNESS_PERCENTAGE}! " +
                    $"It represents the percentage of the smaller edge of the rectangle that will result in the " +
                    $"radius of the circle used for displacing the corner vertices of the rounded rectangle.");
            }

            if (Is3D && Depth <= 0f)
            {
                AppendErrorMessage($"When generating 3D rectangles, the depth must be greater than 0. Either " +
                    $"remove the check box next to the 3D option, or increase the desired depth of the rectangle.");
            }

            float cornerRoundnessDiameter = 2f * CornerRoundnessRadius;
            if(Width < cornerRoundnessDiameter)
            {
                AppendErrorMessage($"Width of the rectangle is too small. The minimum value a width can have " +
                    $"is double the value of the {nameof(CornerRoundnessRadius)}.");
            }

            if (Height < cornerRoundnessDiameter)
            {
                AppendErrorMessage($"Height of the rectangle is too small. The minimum value a height can have " +
                    $"is double the value of the {nameof(CornerRoundnessRadius)}.");
            }
        }
    }
}
