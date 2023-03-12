using System;
using UnityEngine;

namespace BanSee.RoundedRectangleGenerator
{
    /// Part of the <see cref="MeshSolidify"/> partial class that implements rectangle
    /// and border solidification methods for the rounded rectangle.
    public static partial class MeshSolidify
    {
        private static void SolidifyRoundedRectangle(RectangleGenerationData rectangleGenerationData, MeshData rectangleMeshData)
        {
            int originalVerticesCount = rectangleMeshData.Vertices.Length;
            int numberOfFrontFaceInnerVertices = RectangleMeshUtils.GetNumberOfFrontFaceInnerVertices(rectangleGenerationData);
            int numberOfFrontFaceOuterVertices = RectangleMeshUtils.GetNumberOfFrontFaceOuterVertices(rectangleGenerationData);

            // Duplicating front face outer vertices so that they could have separate normals and 
            // provide correct shading of the connecting triangles.
            Vector3[] frontFaceVerticesWithDuplicatedOuterEdge = DuplicateSubsetOfFrontFaceVertices(rectangleMeshData.Vertices,
                numberOfFrontFaceInnerVertices, numberOfFrontFaceOuterVertices);
            rectangleMeshData.Vertices = frontFaceVerticesWithDuplicatedOuterEdge;

            // Move current vertices forward so that the center of the solidified rectangle is in the middle.
            // Also, duplicate front face vertices and move them back to form a back face.
            rectangleMeshData.Vertices = GenerateAndAppendBackFaceVertices(frontFaceVerticesWithDuplicatedOuterEdge,
                rectangleGenerationData.Depth);

            // Generate normals for the expanded vertices array.
            Vector3[] normals = GenerateNormals(originalVerticesCount + numberOfFrontFaceOuterVertices, rectangleMeshData.Vertices);

            // Recalculate normals for outer vertices of the front face and copy them to the normals array.
            Vector3[] frontFaceOuterVerticesRecalculatedNormals = GenerateNormalsBasedOnCornerRoundness(rectangleGenerationData,
                originalVerticesCount, numberOfFrontFaceOuterVertices, rectangleMeshData.Vertices);
            Array.Copy(frontFaceOuterVerticesRecalculatedNormals, 0, normals, originalVerticesCount, numberOfFrontFaceOuterVertices);

            // Recalculate normals for outer vertices of the back face and copy them to the normals array.
            Vector3[] backFaceOuterVerticesRecalculatedNormals = GenerateNormalsBasedOnCornerRoundness(rectangleGenerationData,
                originalVerticesCount * 2 + numberOfFrontFaceOuterVertices, numberOfFrontFaceOuterVertices, rectangleMeshData.Vertices);
            Array.Copy(backFaceOuterVerticesRecalculatedNormals, 0, normals, originalVerticesCount * 2 + numberOfFrontFaceOuterVertices, numberOfFrontFaceOuterVertices);
            rectangleMeshData.Normals = normals;

            // Update UVs for newly generated vertices.
            rectangleMeshData.Uvs = UVGenerator.GenerateUVs(rectangleGenerationData, rectangleMeshData.Vertices);

            // Update triangles to form a back face and connect the front and back face.
            // Total number of triangles is calculated by duplicating the current number of
            // triangles to form the back face with the exact same layout but inverted orientation.
            // Then generating connecting vertices between the outer vertices, meaning that
            // every outer vertex will gain 2 new triangles. So total count is current number of 
            // triangles doubled, with additional 2 new triangles per outer vertex of original rectangle.
            int[] indices2D = rectangleMeshData.Triangles;
            int originalIndicesCount = indices2D.Length;
            int numberOfConnectingTriangles = 2 * (originalVerticesCount - numberOfFrontFaceInnerVertices);
            // Back face vertices start after front face vertices, that were previously expanded with additional
            // set of outer edge vertices that will be used for generating the connecting triangles.
            int backFaceVerticesStartOffset = originalVerticesCount + numberOfFrontFaceOuterVertices;

            // Double current triangles count, then add connecting triangles.
            // (Triangle consists of 3 indices so multiplied by 3).
            int[] indices3D = new int[originalIndicesCount * 2 + 3 * numberOfConnectingTriangles];
            for (int i = 0; i < originalIndicesCount; i += 3)
            {
                int originalFirstVertexIndex = indices2D[i];
                int originalSecondVertexIndex = indices2D[i + 1];
                int originalThirdVertexIndex = indices2D[i + 2];

                // Copy the front face triangle.
                indices3D[i] = originalFirstVertexIndex;
                indices3D[i + 1] = originalSecondVertexIndex;
                indices3D[i + 2] = originalThirdVertexIndex;

                // Creating a back face triangle that has the second and third
                // index swapped to orientate the face in the other direction.
                indices3D[i + originalIndicesCount] = originalFirstVertexIndex + backFaceVerticesStartOffset;
                indices3D[i + originalIndicesCount + 1] = originalThirdVertexIndex + backFaceVerticesStartOffset;
                indices3D[i + originalIndicesCount + 2] = originalSecondVertexIndex + backFaceVerticesStartOffset;
            }

            // Generation of outer connecting triangles.
            int[] outerConnectingTriangleIndices = GenerateConnectingTriangles(
                frontFaceFirstVertexIndex: originalVerticesCount,
                frontFaceLastVertexIndex: backFaceVerticesStartOffset - 1,
                backFaceFirstVertexIndex: backFaceVerticesStartOffset + originalVerticesCount,
                backFaceLastVertexIndex: backFaceVerticesStartOffset * 2 - 1,
                triangleClockwiseGeneration: false);

            int currentTriangleIndex = originalIndicesCount * 2;
            for (int i = currentTriangleIndex; i < (currentTriangleIndex + outerConnectingTriangleIndices.Length); i++)
            {
                indices3D[i] = outerConnectingTriangleIndices[i - currentTriangleIndex];
            }
            rectangleMeshData.Triangles = indices3D;
        }

        private static void SolidifyRoundedRectangleBorder(MeshData borderMeshData, RectangleGenerationData rectangleGenerationData,
            RectangleBorderGenerationData rectangleBorderGenerationData)
        {
            // Cache the original vertices and their count.
            Vector3[] originalBorderVertices = borderMeshData.Vertices;
            int originalFrontFaceVerticesCount = originalBorderVertices.Length;

            // Border consists of the same number of inner and outer vertices, meaning that the
            // half of the current vertices count is taken by outer/inner vertices.
            int numberOfFrontFaceInnerVertices = Mathf.RoundToInt(originalFrontFaceVerticesCount * 0.5f);
            // Border has the same number of inner and outer vertices.
            int numberOfFrontFaceOuterVertices = numberOfFrontFaceInnerVertices;

            // Expanding the front face of the border with duplicated outer and inner vertices, since they will have 
            // different normal vectors in order for connecting triangles to be correctly shaded.
            borderMeshData.Vertices = DuplicateSubsetOfFrontFaceVertices(borderMeshData.Vertices,
                0, originalFrontFaceVerticesCount);

            // Move current vertices forward so that the center of the solidified border is in the middle.
            // Also, duplicate original vertices and move them back to form a back face.
            borderMeshData.Vertices = GenerateAndAppendBackFaceVertices(borderMeshData.Vertices,
                rectangleGenerationData.Depth + rectangleBorderGenerationData.BorderAdditionalDepth);

            // Generate normals for the front and back faces.
            Vector3[] normals = GenerateNormals(originalFrontFaceVerticesCount * 2, borderMeshData.Vertices);

            // START: --- FRONT FACE DUPLICATED OUTER AND INNER VERTICES NORMAL GENERATION ---
            // Recalculate normals for inner vertices of the front face and copy them to the normals array.
            Vector3[] frontFaceInnerVerticesRecalculatedNormals = GenerateNormalsBasedOnCornerRoundness(rectangleGenerationData,
                originalFrontFaceVerticesCount, numberOfFrontFaceInnerVertices, borderMeshData.Vertices);
            CopyAndInvertNormals(ref frontFaceInnerVerticesRecalculatedNormals, 0, ref normals, originalFrontFaceVerticesCount, numberOfFrontFaceInnerVertices);

            // Recalculate normals for outer vertices of the front face and copy them to the normals array.
            Vector3[] frontFaceOuterVerticesRecalculatedNormals = GenerateNormalsBasedOnCornerRoundness(rectangleGenerationData,
                originalFrontFaceVerticesCount + numberOfFrontFaceInnerVertices, numberOfFrontFaceOuterVertices, borderMeshData.Vertices);
            Array.Copy(frontFaceOuterVerticesRecalculatedNormals, 0, normals, originalFrontFaceVerticesCount + numberOfFrontFaceInnerVertices, numberOfFrontFaceOuterVertices);
            // END: --- FRONT FACE DUPLICATED OUTER AND INNER VERTICES NORMAL GENERATION ---

            // START: --- BACK FACE DUPLICATED OUTER AND INNER VERTICES NORMAL GENERATION ---
            // Recalculate normals for inner vertices of the back face and copy them to the normals array.
            Vector3[] backFaceInnerVerticesRecalculatedNormals = GenerateNormalsBasedOnCornerRoundness(rectangleGenerationData,
                originalFrontFaceVerticesCount * 3, numberOfFrontFaceInnerVertices, borderMeshData.Vertices);
            CopyAndInvertNormals(ref backFaceInnerVerticesRecalculatedNormals, 0, ref normals, originalFrontFaceVerticesCount * 3, numberOfFrontFaceInnerVertices);

            // Recalculate normals for outer vertices of the back face and copy them to the normals array.
            Vector3[] backFaceOuterVerticesRecalculatedNormals = GenerateNormalsBasedOnCornerRoundness(rectangleGenerationData,
                originalFrontFaceVerticesCount * 3 + numberOfFrontFaceInnerVertices, numberOfFrontFaceOuterVertices, borderMeshData.Vertices);
            Array.Copy(backFaceOuterVerticesRecalculatedNormals, 0, normals, originalFrontFaceVerticesCount * 3 + numberOfFrontFaceInnerVertices, numberOfFrontFaceOuterVertices);
            // END: --- BACK FACE DUPLICATED OUTER AND INNER VERTICES NORMAL GENERATION ---

            // Assigning generated normals to the mesh data.
            borderMeshData.Normals = normals;

            // Update UVs for newly generated vertices.
            borderMeshData.Uvs = UVGenerator.GenerateUVs(rectangleGenerationData, borderMeshData.Vertices);

            // Update triangles to form a back face and connect the front and back face.
            // Total number of triangles is calculated by duplicating the current number of
            // triangles to form the back face with the exact same layout but inverted orientation.
            // Then generating connecting vertices between the outer vertices, meaning that
            // every outer vertex will gain 2 new triangles. The same thing needs to be done for the
            // connecting faces of the inside vertices of the border. So total count is current number of 
            // triangles doubled, with additional 2 new triangles per outer and inner vertex of original rectangle.
            int[] indices2D = borderMeshData.Triangles;
            int originalIndicesCount = indices2D.Length;
            int numberOfConnectingTriangles = 2 * originalFrontFaceVerticesCount;
            // Back face vertices start after the front face vertices, but front face vertices now contain
            // additional duplicates of inner and outer vertices. Meaning that the back face vertices start
            // after duplicate number of vertices.
            int backFaceVerticesOffset = originalFrontFaceVerticesCount * 2;

            // Double current triangles count, then add connecting triangles.
            // (Triangle consists of 3 indices so multiplied by 3).
            int[] indices3D = new int[originalIndicesCount * 2 + 3 * numberOfConnectingTriangles];
            for (int i = 0; i < originalIndicesCount; i += 3)
            {
                int originalFirstVertexIndex = indices2D[i];
                int originalSecondVertexIndex = indices2D[i + 1];
                int originalThirdVertexIndex = indices2D[i + 2];

                // Copy the front face triangle.
                indices3D[i] = originalFirstVertexIndex;
                indices3D[i + 1] = originalSecondVertexIndex;
                indices3D[i + 2] = originalThirdVertexIndex;

                // Creating a back face triangle that has the second and third
                // index swapped to orientate the face in the other direction.
                indices3D[i + originalIndicesCount] = originalFirstVertexIndex + backFaceVerticesOffset;
                indices3D[i + originalIndicesCount + 1] = originalThirdVertexIndex + backFaceVerticesOffset;
                indices3D[i + originalIndicesCount + 2] = originalSecondVertexIndex + backFaceVerticesOffset;
            }

            int currentTriangleIndex = originalIndicesCount * 2;

            // Generation of outer connecting triangles.
            int[] outerConnectingTriangleIndices = GenerateConnectingTriangles(
                frontFaceFirstVertexIndex: originalFrontFaceVerticesCount + numberOfFrontFaceInnerVertices,
                frontFaceLastVertexIndex: originalFrontFaceVerticesCount * 2 - 1,
                backFaceFirstVertexIndex: originalFrontFaceVerticesCount * 3 + numberOfFrontFaceInnerVertices,
                backFaceLastVertexIndex: originalFrontFaceVerticesCount * 4 - 1,
                triangleClockwiseGeneration: false);

            for (int i = currentTriangleIndex; i < (currentTriangleIndex + outerConnectingTriangleIndices.Length); i++)
            {
                indices3D[i] = outerConnectingTriangleIndices[i - currentTriangleIndex];
            }
            currentTriangleIndex += outerConnectingTriangleIndices.Length;

            // Generation of inner connecting triangles.
            int[] innerConnectingTriangleIndices = GenerateConnectingTriangles(
                frontFaceFirstVertexIndex: originalFrontFaceVerticesCount,
                frontFaceLastVertexIndex: originalFrontFaceVerticesCount + numberOfFrontFaceInnerVertices - 1,
                backFaceFirstVertexIndex: originalFrontFaceVerticesCount * 3,
                backFaceLastVertexIndex: originalFrontFaceVerticesCount * 3 + numberOfFrontFaceInnerVertices - 1,
                triangleClockwiseGeneration: true);

            for (int i = currentTriangleIndex; i < (currentTriangleIndex + innerConnectingTriangleIndices.Length); i++)
            {
                indices3D[i] = innerConnectingTriangleIndices[i - currentTriangleIndex];
            }

            borderMeshData.Triangles = indices3D;
        }

        /// <summary>
        /// Function generates triangle indices that construct the mesh between front and the back
        /// face of the solidified rectangle. This can be also looked as mesh generation between
        /// the top and bottom face of the cylinder. NOTE: Function makes assumption that the
        /// back face indices are larger than front face ones, meaning that in the vertices array
        /// the front face vertices need to be specified before the back face ones.
        /// </summary>
        /// <param name="frontFaceFirstVertexIndex">Index of the first vertex on the front face
        /// that needs to be connected to the back face.</param>
        /// <param name="frontFaceLastVertexIndex">Index of the last vertex on the front face
        /// that needs to be connected to the back face.</param>
        /// <param name="backFaceFirstVertexIndex">Index of the first vertex on the back face
        /// that needs to be connected to the front face.</param>
        /// <param name="backFaceLastVertexIndex">Index of the last vertex on the back face
        /// that needs to be connected to the front face.</param>
        /// <param name="triangleClockwiseGeneration">Boolean indicating the direction of 
        /// the triangle vertices ordering (clockwise/counter clockwise).</param>
        /// <returns>Array of vertex indices that form the triangles that connect the 
        /// front and the back face. Null if the number of front face indices doesn't match
        /// the back face ones.</returns>
        private static int[] GenerateConnectingTriangles(int frontFaceFirstVertexIndex, int frontFaceLastVertexIndex,
        int backFaceFirstVertexIndex, int backFaceLastVertexIndex, bool triangleClockwiseGeneration)
        {
            // Checking if the number of outer vertices of the front face matches the number of outer vertices
            // of the back face, which is a requirement for the correct connecting triangles generation.
            int numberOfFrontFaceOuterVertices = (frontFaceLastVertexIndex - frontFaceFirstVertexIndex) + 1;
            int numberOfBackFaceOuterVertices = (backFaceLastVertexIndex - backFaceFirstVertexIndex) + 1;
            if (numberOfFrontFaceOuterVertices != numberOfBackFaceOuterVertices)
            {
                Debug.LogError("In order to create connecting triangles, the number of front face " +
                    "outer vertices and back face outer vertices must be the same!");
                return null;
            }

            int numberOfConnectingTriangles = 2 * (numberOfFrontFaceOuterVertices);
            int[] connectingTrianglesIndices = new int[3 * numberOfConnectingTriangles];

            // Setting up helper variables that will help track vertex indices and
            // currently active triangle index.
            int firstVertexIndex = frontFaceFirstVertexIndex;
            int secondVertexIndex = backFaceFirstVertexIndex;
            int currentTriangleIndex = 0;

            // Setting helper variables that determine the orientation of the triangle generation.
            int secondVertexIndexOffset = triangleClockwiseGeneration ? 2 : 1;
            int thirdVertexIndexOffset = triangleClockwiseGeneration ? 1 : 2;

            for (int i = 0; i < numberOfConnectingTriangles; i++)
            {
                connectingTrianglesIndices[currentTriangleIndex] = firstVertexIndex;
                connectingTrianglesIndices[currentTriangleIndex + secondVertexIndexOffset] = secondVertexIndex;

                // Third index is once on the front face and other iteration on the back face.
                // But, of course it needs to be kept within the vertex indices range.
                int thirdVertexIndex;
                if (i % 2 == 0)
                {
                    thirdVertexIndex = firstVertexIndex + 1;
                    if (thirdVertexIndex > frontFaceLastVertexIndex)
                    {
                        thirdVertexIndex = (thirdVertexIndex % (frontFaceLastVertexIndex + 1)) + frontFaceFirstVertexIndex;
                    }
                }
                else
                {
                    thirdVertexIndex = secondVertexIndex + 1;
                    if (thirdVertexIndex > backFaceLastVertexIndex)
                    {
                        thirdVertexIndex = (thirdVertexIndex % (backFaceLastVertexIndex + 1)) + backFaceFirstVertexIndex;
                    }
                }
                connectingTrianglesIndices[currentTriangleIndex + thirdVertexIndexOffset] = thirdVertexIndex;

                // For every vertex on the front face, we need to generate 2 connecting triangles with
                // the back face. Meaning that we need to update the vertex index every second iteration.
                if (i % 2 == 0)
                {
                    firstVertexIndex++;
                    if (firstVertexIndex > frontFaceLastVertexIndex)
                    {
                        firstVertexIndex = (firstVertexIndex % (frontFaceLastVertexIndex + 1)) + frontFaceFirstVertexIndex;
                    }
                }

                // Updating vertex index on the back face every second iteration, with offset to the front face
                // index update by one, in order to correctly update the index one iteration after the front face one was updated.
                if (i % 2 == 1)
                {
                    secondVertexIndex++;
                }

                currentTriangleIndex += 3;
            }

            return connectingTrianglesIndices;
        }

        /// <summary>
        /// Function creates additional duplicates of vertices on front face that will be 
        /// used for connecting the front and back face. Duplicate vertices are required for correct 
        /// shading since they will have normals pointed in the direction perpendicular to the surface.
        /// </summary>
        /// <param name="frontFaceVertices">An array containing positions of front face vertices.</param>
        /// <param name="duplicatedVerticesStartIndex">Start index of the vertex that will first be duplicated
        /// in the vertices array.</param>
        /// <param name="duplicatedVerticesCount">Number of vertices that need to be duplicated.</param>
        /// <param name="duplicationCount"> How many times will the vertices be duplicated.</param>
        /// <returns>An array of vertex positions containing front face vertices, expanded with the
        /// duplicated vertices of the specified range.</returns>
        private static Vector3[] DuplicateSubsetOfFrontFaceVertices(Vector3[] frontFaceVertices,
            int duplicatedVerticesStartIndex, int duplicatedVerticesCount, int duplicationCount = 1)
        {
            int originalFrontFaceVerticesCount = frontFaceVertices.Length;
            int duplicatedVerticesOffset = originalFrontFaceVerticesCount - duplicatedVerticesStartIndex;

            // Final number of vertices equals to the previous number of front face vertices 
            // expanded with the duplicated outer vertices.
            Vector3[] expandedVertices = new Vector3[originalFrontFaceVerticesCount + duplicatedVerticesCount * duplicationCount];
            for (int i = 0; i < originalFrontFaceVerticesCount; i++)
            {
                // Copying all previous vertex positions to the expanded array.
                Vector3 originalVertexPosition = frontFaceVertices[i];
                expandedVertices[i] = originalVertexPosition;

                // Duplicating all outer front face vertices.
                if (i >= duplicatedVerticesStartIndex && i < (duplicatedVerticesStartIndex + duplicatedVerticesCount))
                {
                    for(int duplicationIndex = 0; duplicationIndex < duplicationCount; duplicationIndex++)
                    {
                        int vertexIndex = i + duplicatedVerticesOffset + i * (duplicationCount - 1) + duplicationIndex;
                        expandedVertices[vertexIndex] = originalVertexPosition;
                    }
                }
            }

            return expandedVertices;
        }

        /// <summary>
        /// Function generates normal vectors for front face and inverts them for the back face.
        /// </summary>
        /// <param name="totalNumberOfFrontFaceVertices">Total number of vertices that form the front face
        /// of the solidified rectangle.</param>
        /// <param name="vertices">Array of vertex positions, containing both front face and back face vertices.</param>
        /// <returns>An array of normal vectors for the provided <paramref name="vertices"/>.</returns>
        private static Vector3[] GenerateNormals(int totalNumberOfFrontFaceVertices, Vector3[] vertices)
        {
            int totalNumberOfVertices = vertices.Length;
            Vector3[] normals = new Vector3[totalNumberOfVertices];
            for (int i = 0; i < totalNumberOfFrontFaceVertices; i++)
            {
                // Front face original vertices, use predefined normals and invert them for back faces.
                normals[i] = RectangleMeshUtils.RECTANGLE_NORMAL;
                normals[i + totalNumberOfFrontFaceVertices] = -RectangleMeshUtils.RECTANGLE_NORMAL;
            }
            return normals;
        }

        /// <summary>
        /// Function generates an array of normal vectors that were calculated for vertices whose indices lay between
        /// specified indices range. The normal vectors are calculated by getting direction of the vector going from
        /// the corner roundness center to the vertex position.
        /// </summary>
        /// <param name="rectangleGenerationData">Reference to the <see cref="RectangleGenerationData"/> instance
        /// containing the properties describing the rectangle (width, height, ...)</param>
        /// <param name="startVertexIndex">Index of the first vertex for whom the normal vector will be calculated.</param>
        /// <param name="totalNormalsCount">Total amount of normal vectors that will be calculated</param>
        /// <param name="vertices">Array of vertex positions.</param>
        /// <returns>An array of normal vectors that were calculated for the specified vertices.</returns>
        private static Vector3[] GenerateNormalsBasedOnCornerRoundness(RectangleGenerationData rectangleGenerationData,
            int startVertexIndex, int totalNormalsCount, Vector3[] vertices)
        {
            float width = rectangleGenerationData.Width;
            float height = rectangleGenerationData.Height;
            float cornerRoundnessRadius = rectangleGenerationData.CornerRoundnessRadius;
            float a = width * 0.5f - cornerRoundnessRadius;
            float b = height * 0.5f - cornerRoundnessRadius;

            Vector3[] normals = new Vector3[totalNormalsCount];
            for (int i = 0; i < totalNormalsCount; i++)
            {
                int vertexIndex = startVertexIndex + i;
                Vector3 vertexPosition = vertices[vertexIndex];
                Vector3 roundnessCenter = new Vector3(
                        Mathf.Sign(vertexPosition.x) * a,
                        Mathf.Sign(vertexPosition.y) * b,
                        vertexPosition.z);
                Vector3 centerToVertexDirection = (vertexPosition - roundnessCenter).normalized;

                normals[i] = centerToVertexDirection;
            }
            return normals;
        }

        /// <summary>
        /// Function copies the normal vectors from the <paramref name="sourceArray"/> to the <paramref name="destinationArray"/>,
        /// starting from the <paramref name="sourceArrayStartIndex"/> in the <paramref name="sourceArray"/> and from the 
        /// <paramref name="destinationArrayStartIndex"/> in the <paramref name="destinationArray"/>. The actual value pasted will be
        /// inverted copied value.
        /// </summary>
        /// <param name="sourceArray">An array of normal vectors whole values will be copied.</param>
        /// <param name="sourceArrayStartIndex">First index of the normal vector in the <paramref name="sourceArray"/> whose
        /// value will be copied.</param>
        /// <param name="destinationArray">An array of normal vectors whose values will be overridden with the inverted values
        /// from the <paramref name="sourceArray"/>.</param>
        /// <param name="destinationArrayStartIndex">First index of the normal vector in the <paramref name="destinationArray"/> whose
        /// value will be overridden.</param>
        /// <param name="count">Total number of normal vectors that will be copied in the process.</param>
        private static void CopyAndInvertNormals(ref Vector3[] sourceArray, int sourceArrayStartIndex, ref Vector3[] destinationArray,
            int destinationArrayStartIndex, int count)
        {
            for (int i = 0; i < count; i++)
            {
                int sourceArrayIndex = sourceArrayStartIndex + i;
                int destinationArrayIndex = destinationArrayStartIndex + i;
                destinationArray[destinationArrayIndex] = -sourceArray[sourceArrayIndex];
            }
        }
    }
}
