using UnityEngine;

namespace BanSee.RoundedRectangleGenerator
{
    /// <summary>
    /// Static class that provides functionality for creating a border around the 
    /// generated 2D rectangle or rounded rectangle meshes. It creates border by duplicating
    /// outer edge vertices of the rectangle and expanding them in correct directions based
    /// on the structure of the rectangle.
    /// </summary>
    public static class RectangleBorderMeshGenerator
    {
        /// <summary>
        /// Function generates a 2D rectangle border around the rectangle or rounded rectangle
        /// by duplicating the outer edge vertices of the rectangle and expanding them in the 
        /// correct direction based on the structure of the rectangle. 
        /// <br>For the regular rectangles, the vertices are being scaled by using the appropriate
        /// aspect ratio multipliers to preserve the width of the border along all edges. </br>
        /// <br>For the rounded rectangles, the vertices are being displaced along vector that 
        /// connects the vertex on the outer edge and the center of the roundness circle, in order
        /// to preserve the correct corner roundness.</br>
        /// </summary>
        /// <param name="rectangleGenerationData">Reference to the class containing options for 
        /// generation of the rectangle mesh.</param>
        /// <param name="rectangleBorderGenerationData">Reference to the class containing options
        /// for generation of the rectangle border mesh.</param>
        /// <returns>Reference to the <see cref="MeshData"/> class containing properties for 
        /// generating the mesh of the border that surrounds the rectangle with perfect width
        /// and corner radius preservation.</returns>
        public static MeshData GenerateBorder(RectangleGenerationData rectangleGenerationData,
            RectangleBorderGenerationData rectangleBorderGenerationData)
        {
            if(rectangleGenerationData == null || rectangleBorderGenerationData == null)
            {
                Debug.LogError($"Both {nameof(RectangleGenerationData)} and {nameof(RectangleBorderGenerationData)} " +
                    $"references must be provided in order to create border!");
                return null;
            }

            if (!rectangleGenerationData.IsDataValid())
            {
                Debug.LogError($"{rectangleGenerationData.ValidationErrorMessage}");
                return null;
            }

            if (!rectangleBorderGenerationData.IsDataValid())
            {
                Debug.LogError($"{rectangleBorderGenerationData.ValidationErrorMessage}");
                return null;
            }

            int numberOfOuterVertices = RectangleMeshUtils.GetNumberOfFrontFaceOuterVertices(rectangleGenerationData);
            int totalNumberOfBorderVertices = numberOfOuterVertices * 2;

            // Generating vertices based on the outer edge of the rectangle and adding
            // additional outer layer of vertices that are offset by the border thickness.
            Vector3[] borderVertices = GenerateOuterVerticesPositions(rectangleGenerationData, rectangleBorderGenerationData);

            // All normals should be the same as for the original rectangle.
            Vector3[] borderNormals = new Vector3[totalNumberOfBorderVertices];
            for(int i = 0; i < totalNumberOfBorderVertices; i++)
            {
                borderNormals[i] = RectangleMeshUtils.RECTANGLE_NORMAL;
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

            MeshData borderMeshData =  new MeshData
            {
                Vertices = borderVertices,
                Normals = borderNormals,
                Uvs = borderUvs,
                Triangles = borderIndices
            };

            // In case of 3D rectangle, generate 3D rectangle border.
            if (rectangleGenerationData.Is3D && rectangleGenerationData.Depth > 0f)
            {
                MeshSolidify.SolidifyRectangleBorder(borderMeshData, rectangleGenerationData,
                    rectangleBorderGenerationData);
            }

            return borderMeshData;
        }

        private static Vector3[] GenerateOuterVerticesPositions(RectangleGenerationData rectangleGenerationData,
            RectangleBorderGenerationData rectangleBorderGenerationData)
        {
            // Generating rectangle data that will be used to generate the border.
            MeshData rectangleMeshData = RectangleMeshGenerator.GenerateRectangleMeshData(rectangleGenerationData);
            Vector3[] rectangleVertices = rectangleMeshData.Vertices;

            int numberOfInnerVertices = RectangleMeshUtils.GetNumberOfFrontFaceInnerVertices(rectangleGenerationData);
            int numberOfOuterVertices = RectangleMeshUtils.GetNumberOfFrontFaceOuterVertices(rectangleGenerationData);
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
                    // Making sure that were getting positions without the depth offset which
                    // would happen if the rectangle is 3D.
                    Vector3 originalOuterVertex = rectangleVertices[i + numberOfInnerVertices];
                    originalOuterVertex.z = 0f;

                    Vector3 roundnessCenter = new Vector3(
                        Mathf.Sign(originalOuterVertex.x) * a, 
                        Mathf.Sign(originalOuterVertex.y) * b, 
                        0f);

                    Vector3 centerToVertexDirection = (originalOuterVertex - roundnessCenter).normalized;
                    borderVertices[i] = originalOuterVertex;
                    borderVertices[i + numberOfOuterVertices] = roundnessCenter + centerToVertexDirection * (cornerRoundnessRadius + rectangleBorderGenerationData.BorderThickness);
                }
            }
            else
            {
                // Regular rectangles can create outer border vertices just by multiplying
                // the current vertex positions with border thickness.
                for (int i = 0; i < numberOfOuterVertices; i++)
                {
                    // Making sure that were getting positions without the depth offset which
                    // would happen if the rectangle is 3D.
                    Vector3 originalOuterVertex = rectangleVertices[i + numberOfInnerVertices];
                    originalOuterVertex.z = 0f;

                    borderVertices[i] = originalOuterVertex;
                    borderVertices[i + numberOfOuterVertices] = new Vector3(
                        originalOuterVertex.x + Mathf.Sign(originalOuterVertex.x) * rectangleBorderGenerationData.BorderThickness,
                        originalOuterVertex.y + Mathf.Sign(originalOuterVertex.y) * rectangleBorderGenerationData.BorderThickness,
                        originalOuterVertex.z);
                }
            }

            return borderVertices;
        }
    }
}
