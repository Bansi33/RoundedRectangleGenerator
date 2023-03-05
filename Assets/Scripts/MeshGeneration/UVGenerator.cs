using Unity.Mathematics;
using UnityEngine;

namespace BanSee.RoundedRectangleGenerator
{
    /// <summary>
    /// Static class that provides functionality for generating the UV coordinates of the
    /// rectangle and the border of the rectangle based on the position of the vertices and
    /// the rectangle dimensions.
    /// </summary>
    public static class UVGenerator
    {
        /// <summary>
        /// Function calculates the UV coordinates of the rectangle vertices, that are provided via
        /// the <paramref name="vertices"/> parameter by comparing their position in relation to the
        /// rectangle dimensions. In case the rectangle should be generated with the border, the border
        /// size is also accumulated in the UV calculations.
        /// </summary>
        /// <param name="rectangleGenerationData">Reference to the <see cref="RectangleGenerationData"/>
        /// class that contains parameters used for generating the rectangle.</param>
        /// <param name="vertices">An array of vertex positions for which the UV coordinates
        /// will be calculated based on the rectangle properties.</param>
        /// <returns>An array of UV coordinates for the provided <paramref name="vertices"/> parameter.</returns>
        public static Vector2[] GenerateUVs(RectangleGenerationData rectangleGenerationData, Vector3[] vertices)
        {
            Vector2[] uvs = rectangleGenerationData.UvMode switch
            {
                UVGenerationMode.AspectRatioFit => GenerateAspectRatioUVs(rectangleGenerationData, vertices),
                _ => GenerateUVsBasedOnVertexPosition(rectangleGenerationData, vertices),
            };
            return uvs;
        }

        /// <summary>
        /// Function generates the UV coordinates of the vertices by taking into account the aspect ratio of
        /// the rectangle and adjusting the UV coordinates accordingly, so that the texture isn't stretched
        /// when being mapped on the surface of the rectangle.
        /// </summary>
        /// <param name="rectangleGenerationData">Reference to the <see cref="RectangleGenerationData"/>
        /// class that contains parameters used for generating the rectangle.</param>
        /// <param name="vertices">An array of vertex positions for which the UV coordinates
        /// will be calculated based on the rectangle properties.</param>
        /// <returns>An array of UV coordinates for the provided <paramref name="vertices"/> parameter.</returns>
        private static Vector2[] GenerateAspectRatioUVs(RectangleGenerationData rectangleGenerationData, Vector3[] vertices)
        {
            float aspectRatioMultiplierX = 1f;
            float aspectRatioMultiplierY = 1f;
            float width = rectangleGenerationData.Width;
            float height = rectangleGenerationData.Height;

            if (width > height)
            {
                aspectRatioMultiplierY = height / width;
            }
            else
            {
                aspectRatioMultiplierX = width / height;
            }

            return GenerateUVsBasedOnVertexPosition(rectangleGenerationData, vertices, aspectRatioMultiplierX, aspectRatioMultiplierY);
        }

        private static Vector2[] GenerateUVsBasedOnVertexPosition(RectangleGenerationData rectangleGenerationData, Vector3[] vertices,
            float aspectRatioMultiplierX = 1f, float aspectRatioMultiplierY = 1f)
        {
            float width = rectangleGenerationData.Width;
            float height = rectangleGenerationData.Height;

            int totalNumberOfVertices = vertices.Length;
            Vector2[] uvs = new Vector2[totalNumberOfVertices];

            // Generate UV coordinates.
            for (int i = 0; i < totalNumberOfVertices; i++)
            {
                // UV coordinates start from the bottom left corner (0,0)
                // and go to (1,1) in the top right corner.
                Vector3 vertex = vertices[i];
                float uvX = aspectRatioMultiplierX * (vertex.x / width) + 0.5f;
                float uvY = aspectRatioMultiplierY * (vertex.y / height) + 0.5f;
                //float uvX = Mathf.InverseLerp(-width * 0.5f, width * 0.5f, vertex.x) * aspectRatioMultiplierX + 0.5f * (1.0f - aspectRatioMultiplierX);
                //float uvY = Mathf.InverseLerp(-height * 0.5f, height * 0.5f, vertex.y) * aspectRatioMultiplierY + 0.5f * (1.0f - aspectRatioMultiplierY);
                uvs[i] = new Vector2(uvX, uvY);
            }

            return uvs;
        }
    }
}
