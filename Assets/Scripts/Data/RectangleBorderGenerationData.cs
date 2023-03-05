using System;

namespace BanSee.RoundedRectangleGenerator
{
    /// <summary>
    /// Class representing data required for the generation of the rectangle border.
    /// </summary>
    [Serializable]
    public class RectangleBorderGenerationData : ValidatableData
    {
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
        protected override void ValidateData()
        {           
            if (BorderThickness <= 0f)
            {
                AppendErrorMessage($"When generating border, the border needs to have thickness larger than 0.");
            }
        }
    }
}