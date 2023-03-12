using System;
using UnityEngine;

namespace BanSee.RoundedRectangleGenerator
{
    /// <summary>
    /// Class representing data required for the generation of the rectangle border.
    /// </summary>
    [Serializable]
    public class RectangleBorderGenerationData : ValidatableData
    {
        /// <summary>
        /// Minimum border thickness allowed for the border to be visible and 
        /// correctly generated.
        /// </summary>
        public const float MIN_BORDER_THICKNESS = 0.001f;

        public float BorderThickness;
        public float BorderAdditionalDepth;

        public RectangleBorderGenerationData()
        {
            BorderThickness = 0.1f;
            BorderAdditionalDepth = 0f;
        }

        public RectangleBorderGenerationData(RectangleBorderGenerationData source)
        {
            BorderThickness = source.BorderThickness;
            BorderAdditionalDepth = source.BorderAdditionalDepth;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (obj is not RectangleBorderGenerationData)
            {
                return false;
            }

            RectangleBorderGenerationData other = (RectangleBorderGenerationData)obj;
            return Mathf.Approximately(BorderThickness, other.BorderThickness) &&
                Mathf.Approximately(BorderAdditionalDepth, other.BorderAdditionalDepth);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <inheritdoc/>
        protected override void ValidateData()
        {           
            if (BorderThickness <= 0f)
            {
                AppendErrorMessage($"When generating border, the border needs to have thickness larger than 0.");
            }

            if (BorderAdditionalDepth < 0f)
            {
                AppendErrorMessage($"When generating border, the additional depth of the border " +
                    $"needs to be larger or equal than 0.");
            }
        }
    }
}