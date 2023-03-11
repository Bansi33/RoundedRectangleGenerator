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
        /// <summary>
        /// Minimal percentage available for the roundness of the rectangle corners.
        /// If the roundness is set to this value, the rectangle will be a regular one.
        /// </summary>
        public const float MIN_CORNER_ROUNDNESS_PERCENTAGE = 0f;
        /// <summary>
        /// Maximum available percentage for the roundness of the rectangle corners.
        /// If roundness is set to this value, the whole smaller dimension of the rectangle
        /// will be constructing a rounded corner.
        /// </summary>
        public const float MAX_CORNER_ROUNDNESS_PERCENTAGE = 0.5f;
        /// <summary>
        /// Minimal number of vertices for constructing a rounded rectangle. If set to this value,
        /// the rounded corner will just be a line connection between side edges.
        /// </summary>
        public const int MIN_ROUNDED_CORNER_VERTEX_COUNT = 0;
        /// <summary>
        /// Maximum number of vertices for constructing a rounded rectangle. Increase this value
        /// if you need a crazy smooth rounded corners.
        /// </summary>
        public const int MAX_ROUNDED_CORNER_VERTEX_COUNT = 64;
        /// <summary>
        /// Minimum size of a certain rectangle dimension.
        /// </summary>
        public const float MIN_RECTANGLE_SIZE = 0.001f;
        
        public float Width;
        public float Height;

        public bool Is3D;
        public float Depth;

        [Range(MIN_CORNER_ROUNDNESS_PERCENTAGE, MAX_CORNER_ROUNDNESS_PERCENTAGE)] 
        public float CornerRoundnessPercentage;
        public int CornerVertexCount;

        public UVGenerationMode UvMode;
        [SerializeField] private RectangleTopologyType _topologyType = RectangleTopologyType.CenterVertexConnection;

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
        /// Property checks if the rectangle is a simple rectangle (no rounded corners)
        /// or if the rectangle is an NGon (rectangle with max corner roundness and both
        /// dimensions of the same value) and returns true if any of these conditions are
        /// satisfied.
        /// </summary>
        public bool MustRectangleUseCenterVertexConnection
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

                return isNGon || isSimpleRectangle;
            }
        }
        /// <summary>
        /// Property specifying the topology type of the rectangle.
        /// </summary>
        public RectangleTopologyType TopologyType 
        { 
            get 
            {
                if (MustRectangleUseCenterVertexConnection)
                {
                    _topologyType = RectangleTopologyType.CenterVertexConnection;
                }

                return _topologyType;
            }
            set
            {
                if (MustRectangleUseCenterVertexConnection)
                {
                    _topologyType = RectangleTopologyType.CenterVertexConnection;
                }
                else
                {
                    _topologyType = value;
                }                
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
            TopologyType = RectangleTopologyType.CenterVertexConnection;
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
            TopologyType = source.TopologyType;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if(obj == null)
            {
                return false;
            }

            if(obj is not RectangleGenerationData)
            {
                return false;
            }

            RectangleGenerationData other = (RectangleGenerationData)obj;
            return Width == other.Width && Height == other.Height && 
                Is3D == other.Is3D && Depth == other.Depth && 
                CornerRoundnessPercentage == other.CornerRoundnessPercentage &&
                CornerVertexCount == other.CornerVertexCount &&
                UvMode == other.UvMode &&
                TopologyType == other.TopologyType;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return base.GetHashCode();
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
