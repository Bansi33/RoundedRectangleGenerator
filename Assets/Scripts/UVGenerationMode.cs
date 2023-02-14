namespace BanSee
{
    /// <summary>
    /// Enumeration specifying the type of the UV generation.
    /// </summary>
    public enum UVGenerationMode
    {
        /// <summary>
        /// Type of UV generation where the UVs are stretched so that the
        /// vertex with the local position (-width/2, -height/2) has UVs of (0,0) and
        /// vertex with the local position (width/2, height/2) has UVs of (1,1).
        /// This type of UV generation doesn't take into account the aspect ratio
        /// of the rectangle and will produce texture stretching if not taken into account.
        /// </summary>
        Stretch = 0,

        /// <summary>
        /// Type of UV generation where the aspect ratio of the rectangle 
        /// is taken into account so that the UVs gets fully distributed along
        /// the larger dimension of the rectangle, and proportionally distributed
        /// along the other dimension, with having the aspect ratio in mind.
        /// </summary>
        AspectRatioFit = 1
    }
}
