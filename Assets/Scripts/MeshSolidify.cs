using UnityEngine;

namespace BanSee.RoundedRectangleGenerator
{
    /// <summary>
    /// Static class providing functionality for expanding the 2D rectangle 
    /// meshes into third dimension by first duplicating the front face, reversing
    /// normals and then joining the front and back face by generating the connecting
    /// triangles between the outer vertices.
    /// </summary>
    public static class MeshSolidify
    {
        /// <summary>
        /// Function extends the 2D generated <paramref name="rectangleMeshData"/>
        /// into third dimension by first duplicating the front face and reversing
        /// normals and then connecting the front and back face with connecting side geometry.
        /// </summary>
        /// <param name="rectangleMeshData">Reference to the <see cref="RectangleMeshData"/>
        /// class that contains the data for generating a 2D rectangle (X-Y plane) and whose data will be expanded
        /// with the data required for generating a 3D rectangle.</param>
        /// <param name="rectangleGenerationData">Reference to the <see cref="RectangleGenerationData"/>
        /// class that contains information about the desired depth of the rectangle and the type
        /// of the mesh topology that was used during the generation of the 2D rectangle.</param>
        public static void SolidifyRectangle(RectangleMeshData rectangleMeshData, 
            RectangleGenerationData rectangleGenerationData)
        {
            int originalVerticesCount = rectangleMeshData.Vertices.Length;
            int numberOfInnerVertices = rectangleMeshData.NumberOfInnerVertices;

            // Move current vertices forward so that the center of the solidified rectangle is in the middle.
            // Also, duplicate original vertices and move them back to form a back face.
            rectangleMeshData.Vertices = GenerateBackFaceVertices(rectangleMeshData.Vertices,
                rectangleGenerationData.Depth);

            // Update normals for newly generated vertices.
            rectangleMeshData.Normals = DuplicateAndInvertNormals(rectangleMeshData.Normals);

            // Update UVs for newly generated vertices.
            rectangleMeshData.Uvs = DuplicateCurrentUVs(rectangleMeshData.Uvs);

            // Update triangles to form a back face and connect the front and back face.
            // Total number of triangles is calculated by duplicating the current number of
            // triangles to form the back face with the exact same layout but inverted orientation.
            // Then generating connecting vertices between the outer vertices, meaning that
            // every outer vertex will gain 2 new triangles. So total count is current number of 
            // triangles doubled, with additional 2 new triangles per outer vertex of original rectangle.
            int[] indices2D = rectangleMeshData.Triangles;
            int originalIndicesCount = indices2D.Length;
            int numberOfConnectingTriangles = 2 * (originalVerticesCount - numberOfInnerVertices);

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
                indices3D[i + originalIndicesCount] = originalFirstVertexIndex + originalVerticesCount;
                indices3D[i + originalIndicesCount + 1] = originalThirdVertexIndex + originalVerticesCount;
                indices3D[i + originalIndicesCount + 2] = originalSecondVertexIndex + originalVerticesCount;
            }

            // Generation of outer connecting triangles.
            int[] outerConnectingTriangleIndices = GenerateConnectingTriangles(
                frontFaceFirstVertexIndex: numberOfInnerVertices,
                frontFaceLastVertexIndex: originalVerticesCount - 1,
                backFaceFirstVertexIndex: originalVerticesCount + numberOfInnerVertices,
                backFaceLastVertexIndex: originalVerticesCount * 2 - 1,
                triangleClockwiseGeneration: false);

            int currentTriangleIndex = originalIndicesCount * 2;
            for (int i = currentTriangleIndex; i < (currentTriangleIndex + outerConnectingTriangleIndices.Length); i++)
            {
                indices3D[i] = outerConnectingTriangleIndices[i - currentTriangleIndex];
            }
            rectangleMeshData.Triangles = indices3D;
        }

        /// <summary>
        /// Function expands the provided <paramref name="borderMeshData"/> with additional
        /// geometry information for the back face of the border mesh and by generating additional
        /// connecting triangles between the inner vertices of the front and back face, as well as
        /// outer vertices between the front and the back face.
        /// </summary>
        /// <param name="borderMeshData">Reference to the <see cref="MeshData"/> class containing
        /// information about the generated 2D border of the rectangle that needs to be expanded
        /// to the third dimension.</param>
        /// <param name="rectangleGenerationData">Reference to the <see cref="RectangleGenerationData"/>
        /// class that contains the generation options that were set for generating the rectangle and
        /// the border of the rectangle.</param>
        public static void SolidifyRectangleBorder(MeshData borderMeshData, RectangleGenerationData rectangleGenerationData)
        {
            // Cache the original vertices and their count.
            Vector3[] originalBorderVertices = borderMeshData.Vertices;
            int originalVerticesCount = originalBorderVertices.Length;

            // Border consists of the same number of inner and outer vertices, meaning that the
            // half of the current vertices count is taken by outer/inner vertices.
            int numberOfInnerVertices = Mathf.RoundToInt(originalVerticesCount * 0.5f);

            // Move current vertices forward so that the center of the solidified border is in the middle.
            // Also, duplicate original vertices and move them back to form a back face.
            borderMeshData.Vertices = GenerateBackFaceVertices(borderMeshData.Vertices, 
                rectangleGenerationData.Depth + rectangleGenerationData.BorderAdditionalDepth);

            // Update normals for newly generated vertices. The back face normal needs to point
            // in a different direction in order to show the back face correctly.
            borderMeshData.Normals = DuplicateAndInvertNormals(borderMeshData.Normals);

            // Update UVs for newly generated vertices. The back face vertices share the same
            // UV coordinates as the front face ones.
            borderMeshData.Uvs = DuplicateCurrentUVs(borderMeshData.Uvs);

            // Update triangles to form a back face and connect the front and back face.
            // Total number of triangles is calculated by duplicating the current number of
            // triangles to form the back face with the exact same layout but inverted orientation.
            // Then generating connecting vertices between the outer vertices, meaning that
            // every outer vertex will gain 2 new triangles. The same thing needs to be done for the
            // connecting faces of the inside vertices of the border. So total count is current number of 
            // triangles doubled, with additional 2 new triangles per outer and inner vertex of original rectangle.
            int[] indices2D = borderMeshData.Triangles;
            int originalIndicesCount = indices2D.Length;
            int numberOfConnectingTriangles = 2 * originalVerticesCount;

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
                indices3D[i + originalIndicesCount] = originalFirstVertexIndex + originalVerticesCount;
                indices3D[i + originalIndicesCount + 1] = originalThirdVertexIndex + originalVerticesCount;
                indices3D[i + originalIndicesCount + 2] = originalSecondVertexIndex + originalVerticesCount;
            }

            int currentTriangleIndex = originalIndicesCount * 2;

            // Generation of outer connecting triangles.
            int[] outerConnectingTriangleIndices = GenerateConnectingTriangles(
                frontFaceFirstVertexIndex: numberOfInnerVertices,
                frontFaceLastVertexIndex: originalVerticesCount - 1,
                backFaceFirstVertexIndex: originalVerticesCount + numberOfInnerVertices,
                backFaceLastVertexIndex: originalVerticesCount * 2 - 1,
                triangleClockwiseGeneration: false);

            for (int i = currentTriangleIndex; i < (currentTriangleIndex + outerConnectingTriangleIndices.Length); i++)
            {
                indices3D[i] = outerConnectingTriangleIndices[i - currentTriangleIndex];
            }
            currentTriangleIndex += outerConnectingTriangleIndices.Length;

            // Generation of inner connecting triangles.
            int[] innerConnectingTriangleIndices = GenerateConnectingTriangles(
                frontFaceFirstVertexIndex: 0,
                frontFaceLastVertexIndex: numberOfInnerVertices - 1,
                backFaceFirstVertexIndex: originalVerticesCount,
                backFaceLastVertexIndex: originalVerticesCount + numberOfInnerVertices - 1,
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
            // Checking if the number of inner and outer vertices match, which is a requirement
            // for correct connecting triangles generation.
            int numberOfInnerVertices = (frontFaceLastVertexIndex - frontFaceFirstVertexIndex) + 1;
            int numberOfOuterVertices = (backFaceLastVertexIndex - backFaceFirstVertexIndex) + 1;
            if (numberOfInnerVertices != numberOfOuterVertices)
            {
                Debug.LogError("In order to create connecting triangles, the number of inner " +
                    "and outer vertices must be the same!");
                return null;
            }

            int numberOfConnectingTriangles = 2 * (numberOfInnerVertices);
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
        /// Function generates an array of <see cref="Vector3"/> values representing
        /// positions of the vertices, containing both front and back face vertices.
        /// The original front face vertices are offset by the half of the distance between
        /// the front and the back face, so that the origin stays in the middle. The back
        /// face vertices are offset by the half of the distance in the opposite direction.
        /// </summary>
        /// <param name="frontFaceVertices">Reference to the array of vertices that represent
        /// the front face of the mesh.</param>
        /// <param name="distanceBetweenFaces">Distance between front and the back face of the mesh.</param>
        /// <returns>An array containing positions for both front and back face of the mesh. Front face
        /// vertices are contained first in the array.</returns>
        private static Vector3[] GenerateBackFaceVertices(Vector3[] frontFaceVertices, float distanceBetweenFaces)
        {
            if(frontFaceVertices == null || frontFaceVertices.Length == 0)
            {
                return frontFaceVertices;
            }

            float halfDistanceOffset = distanceBetweenFaces * 0.5f;
            int frontFaceVerticesCount = frontFaceVertices.Length;
            Vector3[] frontAndBackFaceVertices = new Vector3[frontFaceVerticesCount * 2];
            for (int i = 0; i < frontFaceVerticesCount; i++)
            {
                Vector3 originalVertex = frontFaceVertices[i];
                frontAndBackFaceVertices[i] = originalVertex - Vector3.forward * halfDistanceOffset;
                frontAndBackFaceVertices[i + frontFaceVerticesCount] = originalVertex + Vector3.forward * halfDistanceOffset;
            }
            return frontAndBackFaceVertices;
        }

        /// <summary>
        /// Function duplicates the provided <paramref name="originalUVs"/> to a new
        /// array of <see cref="Vector2"/> UV coordinates that is double the size by
        /// concatenating the <paramref name="originalUVs"/> twice.
        /// <br>Example:</br>
        /// <br>OriginalUVs: [0, 0.1, 0.2, 0.3]</br>
        /// <br>DuplicatedUVs: [0, 0.1, 0.2, 0.3, 0, 0.1, 0.2, 0.3]</br>
        /// </summary>
        /// <param name="originalUVs">Reference to the UV coordinates array that
        /// will be duplicated.</param>
        /// <returns>An array of <see cref="Vector2"/> coordinates that is double the
        /// size of the provided <paramref name="originalUVs"/> and contains the same
        /// values.</returns>
        private static Vector2[] DuplicateCurrentUVs(Vector2[] originalUVs)
        {
            if(originalUVs == null || originalUVs.Length == 0)
            {
                return originalUVs;
            }

            int originalUVCount = originalUVs.Length;
            int doubleUVCount = originalUVCount * 2;
            Vector2[] duplicatedUVs = new Vector2[doubleUVCount];
            for (int i = 0; i < originalUVCount; i++)
            {
                Vector2 originalUv = originalUVs[i];
                duplicatedUVs[i] = originalUv;
                duplicatedUVs[i + originalUVCount] = originalUv;
            }

            return duplicatedUVs;
        }

        /// <summary>
        /// Function duplicates the provided <paramref name="originalNormals"/> and inverts the
        /// duplicated ones so that they face in the opposite direction. This is used to generate
        /// normals for the back face of the rectangle.
        /// <br>Example:</br>
        /// <br>OriginalNormals: [(0, 1.0, 0.0), (1.0, 0.0, 0.0)]</br>
        /// <br>DuplicatedNormals: [(0, 1.0, 0.0), (1.0, 0.0, 0.0), (0, -1.0, 0.0), (-1.0, 0.0, 0.0)]</br>
        /// </summary>
        /// <param name="originalNormals">Reference to the array of <see cref="Vector3"/> values
        /// containing original normal vector directions.</param>
        /// <returns>Duplicated and inverted normal vectors.</returns>
        private static Vector3[] DuplicateAndInvertNormals(Vector3[] originalNormals)
        {
            if(originalNormals == null || originalNormals.Length == 0)
            {
                return originalNormals;
            }

            int originalNormalsCount = originalNormals.Length;
            Vector3[] duplicatedNormals = new Vector3[originalNormalsCount * 2];
            for (int i = 0; i < originalNormalsCount; i++)
            {
                Vector3 originalNormal = originalNormals[i];
                duplicatedNormals[i] = originalNormal;
                duplicatedNormals[i + originalNormalsCount] = -originalNormal;
            }
            return duplicatedNormals;
        }
    }    
}
