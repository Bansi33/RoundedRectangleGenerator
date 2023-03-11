namespace BanSee.RoundedRectangleGenerator
{
    /// <summary>
    /// Static class providing functionality of generating the rectangle mesh data.
    /// It uses the appropriate mesh data generation method based on the provided
    /// <see cref="RectangleGenerationData"/>.
    /// </summary>
    public static class RectangleMeshGenerator
    {
        /// <summary>
        /// Function creates the mesh data required for creating a <see cref="UnityEngine.Mesh"/>
        /// asset based on the provided <paramref name="rectangleGenerationData"/>.
        /// </summary>
        /// <param name="rectangleGenerationData">Instance of the <see cref="RectangleGenerationData"/>
        /// class containing properties for creation of the rectangle mesh data.</param>
        /// <returns>Reference to the instance of the created <see cref="MeshData"/>
        /// class, containing information required for creation of the <see cref="UnityEngine.Mesh"/>
        /// asset for visually representing a rectangle.</returns>
        public static MeshData GenerateRectangleMeshData(RectangleGenerationData rectangleGenerationData)
        {
            MeshData rectangleMeshData;
            if (rectangleGenerationData.IsRoundedRectangle)
            {
                rectangleMeshData = RoundedRectangleMeshGenerator.GenerateRoundedRectangleMeshData(rectangleGenerationData);
            }
            else
            {
                // In case there is no corner roundness required, rounded rectangle
                // is actually an ordinary rectangle. Regular rectangle generation logic
                // is separated for clarity.
                rectangleMeshData = SimpleRectangleMeshGenerator.GenerateRectangleMeshData(rectangleGenerationData);
            }

            // In case the request is for a 3D rectangle, solidify the mesh with third dimension.
            if (rectangleGenerationData.Is3D && rectangleGenerationData.Depth > 0f)
            {
                MeshSolidify.SolidifyRectangle(rectangleMeshData, rectangleGenerationData);
            }

            return rectangleMeshData;
        }
    }
}
