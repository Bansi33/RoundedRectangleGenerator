using UnityEngine;

namespace BanSee.RoundedRectangleGenerator
{
    /// <summary>
    /// Static class that provides functionality for creating a border around the 
    /// generated 2D rectangle or rounded rectangle meshes. It creates border by duplicating
    /// outer edge vertices of the rectangle and expanding them in correct directions based
    /// on the structure of the rectangle.
    /// </summary>
    public static class RectangleBorderGenerator
    {
        /// <summary>
        /// Function generates a 2D rectangle border around the rectangle or rounded rectangle
        /// provided by the <paramref name="rectangleMeshData"/> by duplicating the outer edge
        /// vertices of the rectangle and expanding them in the correct direction based on the
        /// structure of the rectangle. 
        /// <br>For the regular rectangles, the vertices are being scaled by using the appropriate
        /// aspect ratio multipliers to preserve the width of the border along all edges. </br>
        /// <br>For the rounded rectangles, the vertices are being displaced along vector that 
        /// connects the vertex on the outer edge and the center of the roundness circle, in order
        /// to preserve the correct corner roundness.</br>
        /// </summary>
        /// <param name="rectangleGenerationData">Reference to the class containing options for 
        /// generation of the rectangle mesh.</param>
        /// <param name="rectangleMeshData">Reference to the class containing generated 2D rectangle
        /// mesh properties that will be used to generate the correct border.</param>
        /// <returns>Reference to the <see cref="MeshData"/> class containing properties for 
        /// generating the mesh of the border that surrounds the rectangle with perfect width
        /// and corner radius preservation.</returns>
        public static MeshData GenerateBorder(RectangleGenerationData rectangleGenerationData, 
            RectangleMeshData rectangleMeshData)
        {
            Vector3[] rectangleVertices = rectangleMeshData.Vertices;
            int totalNumberOfVertices = rectangleVertices.Length;
            int numberOfInnerVertices = rectangleMeshData.NumberOfInnerVertices;
            int numberOfOuterVertices = totalNumberOfVertices - numberOfInnerVertices;
            int totalNumberOfBorderVertices = numberOfOuterVertices * 2;

            // Generating vertices based on the outer edge of the rectangle and adding
            // additional outer layer of vertices that are offset by the border thickness.
            Vector3[] borderVertices = GenerateOuterVerticesPositions(rectangleMeshData, rectangleGenerationData);

            // All normals should be the same as for the original rectangle.
            Vector3[] rectangleNormals = rectangleMeshData.Normals;
            Vector3[] borderNormals = new Vector3[totalNumberOfBorderVertices];
            for(int i = 0; i < numberOfOuterVertices; i++)
            {
                Vector3 originalNormal = rectangleNormals[i + numberOfInnerVertices];
                borderNormals[i] = originalNormal;
                borderNormals[i + numberOfOuterVertices] = originalNormal;
            }

            /// Generating UV coordinates.
            Vector2[] borderUvs = UVGenerator.GenerateUVs(rectangleGenerationData, borderVertices);

            // Creating triangles for the border.             
            int[] borderIndices = new int[totalNumberOfBorderVertices * 3];
            for (int i = 0; i < numberOfOuterVertices; i++)
            {
                int nextInnerVertexIndex = (i + 1) % numberOfOuterVertices;
                int currentOuterVertexIndex = i + numberOfOuterVertices;
                int nextOuterVertexIndex = Mathf.Max((i + numberOfOuterVertices + 1) % totalNumberOfBorderVertices, numberOfOuterVertices);

                borderIndices[i * 6] = i;
                borderIndices[i * 6 + 1] = currentOuterVertexIndex;
                borderIndices[i * 6 + 2] = nextInnerVertexIndex;

                borderIndices[i * 6 + 3] = nextInnerVertexIndex;
                borderIndices[i * 6 + 4] = currentOuterVertexIndex;
                borderIndices[i * 6 + 5] = nextOuterVertexIndex;
            }

            return new MeshData
            {
                Vertices = borderVertices,
                Normals = borderNormals,
                Uvs = borderUvs,
                Triangles = borderIndices
            };
        }

        private static Vector3[] GenerateOuterVerticesPositions(RectangleMeshData rectangleMeshData, 
            RectangleGenerationData rectangleGenerationData)
        {
            Vector3[] rectangleVertices = rectangleMeshData.Vertices;
            int totalNumberOfVertices = rectangleVertices.Length;
            int numberOfInnerVertices = rectangleMeshData.NumberOfInnerVertices;
            int numberOfOuterVertices = totalNumberOfVertices - numberOfInnerVertices;
            int totalNumberOfBorderVertices = numberOfOuterVertices * 2;
            Vector3[] borderVertices = new Vector3[totalNumberOfBorderVertices];

            float width = rectangleGenerationData.Width;
            float height = rectangleGenerationData.Height;

            if (rectangleGenerationData.IsRoundedRectangle)
            {
                float cornerRoundnessRadius = rectangleGenerationData.CornerRoundnessRadius; 
                float a = width * 0.5f - cornerRoundnessRadius;
                float b = height * 0.5f - cornerRoundnessRadius;

                // Rounded rectangles need to extend the outer border of vertices by
                // moving them in the direction created by pulling the vector from the
                // center of the corner roundness to the inner vertex and expanding it
                // by the border thickness.
                for (int i = 0; i < numberOfOuterVertices; i++)
                {
                    Vector3 originalOuterVertex = rectangleVertices[i + numberOfInnerVertices];
                    Vector3 roundnessCenter = new Vector3(
                        Mathf.Sign(originalOuterVertex.x) * a, 
                        Mathf.Sign(originalOuterVertex.y) * b, 
                        0f);

                    Vector3 centerToVertexDirection = (originalOuterVertex - roundnessCenter).normalized;
                    borderVertices[i] = originalOuterVertex;
                    borderVertices[i + numberOfOuterVertices] = roundnessCenter + centerToVertexDirection * (cornerRoundnessRadius + rectangleGenerationData.BorderThickness);
                }
            }
            else
            {
                // Calculating aspect ratio of the rectangle, that will be used in offsetting 
                // outer border vertices to maintain a correct border thickness.
                float aspectRatioMultiplierX = 1f;
                float aspectRatioMultiplierY = 1f;

                if (width > height)
                {
                    aspectRatioMultiplierX = height / width;
                }
                else
                {
                    aspectRatioMultiplierY = width / height;
                }

                // Regular rectangles can create outer border vertices just by multiplying
                // the current vertex positions with border thickness.
                for (int i = 0; i < numberOfOuterVertices; i++)
                {
                    Vector3 originalOuterVertex = rectangleVertices[i + numberOfInnerVertices];
                    borderVertices[i] = originalOuterVertex;
                    borderVertices[i + numberOfOuterVertices] = new Vector3(
                        originalOuterVertex.x * (1.0f + rectangleGenerationData.BorderThickness * aspectRatioMultiplierX),
                        originalOuterVertex.y * (1.0f + rectangleGenerationData.BorderThickness * aspectRatioMultiplierY),
                        originalOuterVertex.z);
                }
            }

            return borderVertices;
        }
    }
}
