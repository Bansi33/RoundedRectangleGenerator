using UnityEngine;

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
        /// class, containing information required for creation of the <see cref="Mesh"/>
        /// asset for visually representing a rectangle. Null if the provided <paramref name="rectangleGenerationData"/>
        /// is not valid.</returns>
        public static MeshData GenerateRectangleMeshData(RectangleGenerationData rectangleGenerationData)
        {
            if(rectangleGenerationData == null)
            {
                Debug.LogError($"Provided {nameof(RectangleGenerationData)} instance is null! Can't generate rectangle mesh data.");
                return null;
            }

            if (!rectangleGenerationData.IsDataValid())
            {
                Debug.LogError($"{rectangleGenerationData.ValidationErrorMessage}");
                return null;
            }

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
