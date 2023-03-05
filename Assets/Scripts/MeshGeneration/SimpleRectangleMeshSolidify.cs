using System;
using UnityEngine;

namespace BanSee.RoundedRectangleGenerator
{
    /// Part of the <see cref="MeshSolidify"/> partial class that implements rectangle
    /// and border solidification methods for the simple rectangle.
    public static partial class MeshSolidify
    {
        /// <summary>
        /// Function expands the simple rectangle mesh data with additional data for the back face and connecting vertices.
        /// The rectangle needs to be without rounded corners and with one center connecting vertex.
        /// </summary>
        /// <param name="rectangleGenerationData">Reference to the <see cref="RectangleGenerationData"/>
        /// class that contains the generation options that were set for generating the rectangle.</param>
        /// <param name="rectangleMeshData">Reference to the <see cref="MeshData"/> class containing
        /// information about the generated 2D rectangle that needs to be expanded
        /// to the third dimension.</param>
        private static void SolidifySimpleRectangle(MeshData rectangleMeshData,
            RectangleGenerationData rectangleGenerationData)
        {
            // Simple rectangle contains one inner vertex and 4 outer vertices.
            int originalFrontFaceVerticesCount = rectangleMeshData.Vertices.Length;
            int numberOfFrontFaceInnerVertices = Utils.GetNumberOfFrontFaceInnerVertices(rectangleGenerationData);
            int numberOfFrontFaceOuterVertices = Utils.GetNumberOfFrontFaceOuterVertices(rectangleGenerationData);

            // Every original outer vertex needs to be duplicated in order to hold 2 normals, one for each connecting side.
            int numberOfConnectingOuterVertices = numberOfFrontFaceOuterVertices * 2;
            int extendedFrontFaceVerticesCount = originalFrontFaceVerticesCount + numberOfConnectingOuterVertices;

            // Updating front face vertices to contain duplicated outer vertices.
            Vector3[] updatedFrontFaceVertices = new Vector3[extendedFrontFaceVerticesCount];
            int numberOfOuterCompletedOuterEdges = 0;
            for (int i = 0; i < originalFrontFaceVerticesCount; i++)
            {
                Vector3 vertexPosition = rectangleMeshData.Vertices[i];
                updatedFrontFaceVertices[i] = vertexPosition;
                if (i >= numberOfFrontFaceInnerVertices)
                {
                    // Duplicating outer vertices two times, since they need to hold different normal vectors.
                    updatedFrontFaceVertices[i + numberOfFrontFaceOuterVertices + numberOfOuterCompletedOuterEdges] = vertexPosition;
                    updatedFrontFaceVertices[i + 1 + numberOfFrontFaceOuterVertices + numberOfOuterCompletedOuterEdges] = vertexPosition;
                    numberOfOuterCompletedOuterEdges++;
                }
            }

            // Move current vertices forward so that the center of the solidified rectangle is in the middle.
            // Also, duplicate front face vertices and move them back to form a back face.
            rectangleMeshData.Vertices = GenerateAndAppendBackFaceVertices(
                updatedFrontFaceVertices, rectangleGenerationData.Depth);

            // Generating normal vectors for the solidified simple rectangle.
            rectangleMeshData.Normals = GenerateSimpleRectangleNormals(rectangleMeshData.Vertices,
                numberOfFrontFaceInnerVertices, numberOfFrontFaceOuterVertices);

            // Update UVs for newly generated vertices.
            rectangleMeshData.Uvs = UVGenerator.GenerateUVs(rectangleGenerationData, rectangleMeshData.Vertices);

            // Generate triangles by duplicating front face triangles and inverting them for the back face,
            // and adding additional triangles for the connecting faces.
            rectangleMeshData.Triangles = GenerateIndicesForSimple3DRectangle(rectangleMeshData.Triangles, extendedFrontFaceVerticesCount);
        }

        private static void SolidifySimpleRectangleBorder(MeshData borderMeshData, RectangleGenerationData rectangleGenerationData,
            RectangleBorderGenerationData rectangleBorderGenerationData)
        {
            // Cache the original vertices and their count.
            Vector3[] originalBorderVertices = borderMeshData.Vertices;
            int originalFrontFaceVerticesCount = originalBorderVertices.Length;

            // Border consists of the same number of inner and outer vertices, meaning that the
            // half of the current vertices count is taken by outer/inner vertices.
            int numberOfFrontFaceInnerVertices = Mathf.RoundToInt(originalFrontFaceVerticesCount * 0.5f);

            // Expanding the front face of the border with duplicated outer and inner vertices, since they will have 
            // different normal vectors in order for connecting triangles to be correctly shaded. So the total number
            // of front face vertices will now be 3x original front face vertices, since every vertex now contains three
            // normals, one for the front face and two for connecting faces that connect the front with the back face.
            // Two additional vertices for the connecting faces are required since they need to have different normals.
            borderMeshData.Vertices = DuplicateSubsetOfFrontFaceVertices(borderMeshData.Vertices,
                0, originalFrontFaceVerticesCount, 2);

            // Move current vertices forward so that the center of the solidified border is in the middle.
            // Also, duplicate original vertices and move them back to form a back face.
            borderMeshData.Vertices = GenerateAndAppendBackFaceVertices(borderMeshData.Vertices,
                rectangleGenerationData.Depth + rectangleBorderGenerationData.BorderAdditionalDepth);

            // Generating normal vectors for the solidified simple rectangle border.
            borderMeshData.Normals = GenerateSimpleRectangleBorderNormals(borderMeshData.Vertices,
                numberOfFrontFaceInnerVertices);

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
            // additional two duplicates of inner and outer vertices. 
            int backFaceVerticesOffset = originalFrontFaceVerticesCount * 3;

            // Copy the front face triangles and assign them to the back face in the reverse order, with 
            // updated vertex indices that point to the back face vertices.
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

            int indicesOffset = originalIndicesCount * 2;
            int extendedFrontFaceVerticesCount = 3 * originalFrontFaceVerticesCount;
            // Filling in inner edge connecting indices.
            FillInConnectingTriangleIndices(ref indices3D, indicesOffset, originalFrontFaceVerticesCount, 
                extendedFrontFaceVerticesCount + originalFrontFaceVerticesCount, false);  

            // Filling in outer edge connecting indices.
            // There are 4 connecting faces, each consisting of 2 triangles. Each triangle
            // consists of 3 vertex indices, resulting in total of 4 * 2 * 3 indices. 
            indicesOffset += (4 * 2 * 3);
            FillInConnectingTriangleIndices(ref indices3D, indicesOffset, originalFrontFaceVerticesCount * 2, 
                extendedFrontFaceVerticesCount + originalFrontFaceVerticesCount * 2);
            borderMeshData.Triangles = indices3D;
        }

        /// <summary>
        /// Function generates simple rectangle normals for the simple rectangle vertices
        /// that were already expanded with duplicated outer vertices for the front and the 
        /// back face of the solidified simple rectangle.
        /// </summary>
        /// <param name="vertices">An array of vertices containing front and back face vertices
        /// that were already expanded with the duplicated outer edge vertices.</param>
        /// <param name="numberOfFrontFaceInnerVertices">Number of vertices on the front face
        /// that are not located on the outer edge of the front face.</param>
        /// <param name="numberOfFrontFaceOuterVertices">Number of vertices on the front face
        /// that are located on the outer edge.</param>
        /// <returns>An array containing generated normal vectors, specifically created for the
        /// simple rectangle with solidified connection between front and back face.</returns>
        private static Vector3[] GenerateSimpleRectangleNormals(Vector3[] vertices,
            int numberOfFrontFaceInnerVertices, int numberOfFrontFaceOuterVertices)
        {
            int originalFrontFaceVerticesCount = numberOfFrontFaceInnerVertices + numberOfFrontFaceOuterVertices;
            int extendedFrontFaceVerticesCount = numberOfFrontFaceInnerVertices + 3 * numberOfFrontFaceOuterVertices;

            Vector3[] normals = new Vector3[vertices.Length];
            int numberOfOuterCompletedOuterEdges = 0;

            // Each vertex has two normal vectors, one for each face it constructs of the outer 
            // connecting faces between the front and the back face. For example, the vertex 
            // that constructs the top and the left face of the simple rectangle will have
            // the one normal vector pointing up and the other pointing to the left.
            Vector3 vertexFirstNormal = Vector3.left;
            Vector3 vertexSecondNormal = Vector3.up;
            int backFaceVertexIndexOffset = extendedFrontFaceVerticesCount; 
            for (int i = 0; i < originalFrontFaceVerticesCount; i++)
            {
                // Front and back face normals.
                normals[i] = Utils.RECTANGLE_NORMAL;
                normals[i + extendedFrontFaceVerticesCount] = -Utils.RECTANGLE_NORMAL;

                // Outer vertices duplication and normal direction.
                if (i >= numberOfFrontFaceInnerVertices)
                {
                    int frontFaceVertexIndexOffset = numberOfFrontFaceOuterVertices + numberOfOuterCompletedOuterEdges;
                    // Front face vertex normals assignment.
                    normals[i + frontFaceVertexIndexOffset] = vertexFirstNormal;
                    normals[i + frontFaceVertexIndexOffset + 1] = vertexSecondNormal;
                    
                    // Back face vertex normals assignment.
                    normals[i + frontFaceVertexIndexOffset + backFaceVertexIndexOffset] = vertexFirstNormal;
                    normals[i + frontFaceVertexIndexOffset + backFaceVertexIndexOffset + 1] = vertexSecondNormal;
                    
                    // Updating normals for the next vertex and updating number of completed outer edges.
                    numberOfOuterCompletedOuterEdges++;
                    vertexFirstNormal = GetNextRectangleNormal(vertexFirstNormal);
                    vertexSecondNormal = GetNextRectangleNormal(vertexSecondNormal);
                }
            }
            return normals;
        }

        private static Vector3[] GenerateSimpleRectangleBorderNormals(Vector3[] vertices,
            int numberOfFrontFaceInnerVertices)
        {
            // Borders contain the same amount of outer and inner face vertices.
            int numberOfFrontFaceOuterVertices = numberOfFrontFaceInnerVertices;
            int originalFrontFaceVerticesCount = numberOfFrontFaceInnerVertices + numberOfFrontFaceOuterVertices;

            // The front face of the border is expanded with the duplicated outer and inner vertices, since they will have 
            // different normal vectors in order for connecting triangles to be correctly shaded. So the total number
            // of front face vertices will now be 3x original front face vertices, since every vertex now contains three
            // normals, one for the front face and two for connecting faces that connect the front with the back face.
            // Two additional vertices for the connecting faces are required since they need to have different normals.
            int extendedFrontFaceVerticesCount = 3 * numberOfFrontFaceInnerVertices + 3 * numberOfFrontFaceOuterVertices;

            Vector3[] normals = new Vector3[vertices.Length];
            int numberOfCompletedBorderEdges = 0;

            // Each connecting vertex has two normal vectors, one for each face it constructs of the outer 
            // connecting faces between the front and the back face. For example, the vertex 
            // that constructs the top and the left face of the simple rectangle will have
            // the one normal vector pointing up and the other pointing to the left.
            Vector3 vertexFirstNormal = Vector3.left;
            Vector3 vertexSecondNormal = Vector3.up;
            int backFaceVertexIndexOffset = extendedFrontFaceVerticesCount;
            for (int i = 0; i < originalFrontFaceVerticesCount; i++)
            {
                // Inner connecting triangles need to have normals inverted.
                float normalDirectionMultiplier = i < numberOfFrontFaceInnerVertices ? -1.0f : 1.0f;

                // Copy the front face normals to the back face and invert them.
                normals[i] = Utils.RECTANGLE_NORMAL;
                normals[i + extendedFrontFaceVerticesCount] = -Utils.RECTANGLE_NORMAL;

                int frontFaceVertexIndexOffset = originalFrontFaceVerticesCount + numberOfCompletedBorderEdges;
                // Front face vertex normals assignment.
                normals[i + frontFaceVertexIndexOffset] = vertexFirstNormal * normalDirectionMultiplier;
                normals[i + frontFaceVertexIndexOffset + 1] = vertexSecondNormal * normalDirectionMultiplier;

                // Back face vertex normals assignment.
                normals[i + frontFaceVertexIndexOffset + backFaceVertexIndexOffset] = vertexFirstNormal * normalDirectionMultiplier;
                normals[i + frontFaceVertexIndexOffset + backFaceVertexIndexOffset + 1] = vertexSecondNormal * normalDirectionMultiplier;

                // Updating normals for the next vertex and updating number of completed outer edges.
                numberOfCompletedBorderEdges++;
                vertexFirstNormal = GetNextRectangleNormal(vertexFirstNormal);
                vertexSecondNormal = GetNextRectangleNormal(vertexSecondNormal);
            }
            return normals;
        }

        /// <summary>
        /// Function returns the next normal vector for the simple rectangle vertex in clockwise
        /// order --> up, right, down, left. Used to construct outer connection faces in simple rectangle.
        /// </summary>
        /// <param name="currentNormal">Current normal vector of the simple rectangle vertex.</param>
        /// <returns>Next normal vector the simple rectangle vertex should obtain in order to 
        /// construct the normals correctly for the outer connecting faces.</returns>
        private static Vector3 GetNextRectangleNormal(Vector3 currentNormal)
        {
            if(Approximately(currentNormal, Vector3.up))
            {
                return Vector3.right;
            }

            if (Approximately(currentNormal, Vector3.right))
            {
                return Vector3.down;
            }

            if (Approximately(currentNormal, Vector3.down))
            {
                return Vector3.left;
            }

            if (Approximately(currentNormal, Vector3.left))
            {
                return Vector3.up;
            }

            // This should never occur when dealing with simple rectangle generation.
            return Vector3.up;
        }

        /// <summary>
        /// Function compares the two vectors and returns true if all of their coordinates
        /// are approximately the same.
        /// </summary>
        /// <param name="v1">The first vector for comparison.</param>
        /// <param name="v2">The second vector for comparison.</param>
        /// <returns>True if both vectors have approximately the same coordinates.</returns>
        private static bool Approximately(Vector3 v1, Vector3 v2)
        {
            return Mathf.Approximately(v1.x, v2.x) &&
                Mathf.Approximately(v1.y, v2.y) &&
                Mathf.Approximately(v1.z, v2.z);
        }

        /// <summary>
        /// Function generates indices of the vertices that form the triangles of the solidified 
        /// simple rectangle mesh by duplicating the original front face indices and reversing them
        /// for the back face and adding additional triangles for the connecting faces.
        /// </summary>
        /// <param name="originalFrontFaceIndices">Original indices that were generated 
        /// for the front face of the simple rectangle.</param>
        /// <param name="numberOfFrontFaceVertices">Number of vertices that form the expanded
        /// front face of the simple rectangle (expanded front face is the one with inner vertices
        /// and three sets of outer vertices).</param>
        /// <returns></returns>
        private static int[] GenerateIndicesForSimple3DRectangle(int[] originalFrontFaceIndices,
            int numberOfFrontFaceVertices)
        {
            // Total number of triangles is 4 for the front face, 4 for the back face and 2 for each connecting
            // side, resulting in 8 for the connecting sides. In total 4 + 4 + 4*2 = 16 triangles. 
            // Each triangle contains 3 vertex indices, in total 16 * 3 indices.
            int[] indices3D = new int[16 * 3];
            int originalIndicesCount = originalFrontFaceIndices.Length;
            int backFaceVerticesStartOffset = numberOfFrontFaceVertices;
            for (int i = 0; i < originalIndicesCount; i += 3)
            {
                int originalFirstVertexIndex = originalFrontFaceIndices[i];
                int originalSecondVertexIndex = originalFrontFaceIndices[i + 1];
                int originalThirdVertexIndex = originalFrontFaceIndices[i + 2];

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

            // Generating connecting triangles, 8 in total.
            int indicesOffset = originalIndicesCount * 2;
            int frontFaceTopLeftVertexIndex = 5;
            int backFaceTopLeftVertexIndex = 18;
            FillInConnectingTriangleIndices(ref indices3D, indicesOffset, frontFaceTopLeftVertexIndex, backFaceTopLeftVertexIndex);

            return indices3D;
        }

        private static void FillInConnectingTriangleIndices(ref int[] indices3D, int indicesOffsetInArray,
            int frontFaceTopLeftVertexIndex, int backFaceTopLeftVertexIndex, bool clockwiseOrientation = true)
        {
            // There are 4 sides connecting the front and the back face, each side having 2 triangles,
            // resulting in total of 8 triangles. Each triangle consists of 3 vertex indices, thus
            // resulting in total of 24 indices for the connecting triangles.
            int numberOfConnectingIndices = 24;

            // Starting from the top face, and the top left vertex index has the first normal set
            // for the left face, thus we need to offset by one in order to get the top left vertex with
            // the normal for the top face.
            int frontFaceVertexIndex = frontFaceTopLeftVertexIndex + 1;
            int backFaceVertexIndex = backFaceTopLeftVertexIndex + 1;

            // Each outer edge of the rectangle has 4 vertices, with each vertex duplicated to contain
            // two normal vectors, one for each connecting side it constructs, resulting in 8 total vertices.
            int frontFaceVertexRange = frontFaceTopLeftVertexIndex + 8;
            int backFaceVertexRange = backFaceTopLeftVertexIndex + 8;

            // Generating connecting triangle indices.
            for(int i = 0; i < numberOfConnectingIndices; i++)
            {
                int frontFaceNextVertexIndex = IncreaseWithWrap(frontFaceVertexIndex, frontFaceVertexRange, frontFaceTopLeftVertexIndex);
                int backFaceNextVertexIndex = IncreaseWithWrap(backFaceVertexIndex, backFaceVertexRange, backFaceTopLeftVertexIndex);

                // First triangle of the connecting face.
                indices3D[indicesOffsetInArray + i] = frontFaceVertexIndex;
                indices3D[indicesOffsetInArray + i  + 1] = clockwiseOrientation ? backFaceVertexIndex : backFaceNextVertexIndex;
                indices3D[indicesOffsetInArray + i  + 2] = clockwiseOrientation ? backFaceNextVertexIndex : backFaceVertexIndex;

                // Second triangle of the connecting face.
                indices3D[indicesOffsetInArray + i + 3] = frontFaceVertexIndex;
                indices3D[indicesOffsetInArray + i + 4] = clockwiseOrientation ? backFaceNextVertexIndex : frontFaceNextVertexIndex;
                indices3D[indicesOffsetInArray + i + 5] = clockwiseOrientation ? frontFaceNextVertexIndex : backFaceNextVertexIndex;

                // Updating vertex indices for the next connecting face.
                frontFaceVertexIndex = IncreaseWithWrap(frontFaceNextVertexIndex, frontFaceVertexRange, frontFaceTopLeftVertexIndex);
                backFaceVertexIndex = IncreaseWithWrap(backFaceNextVertexIndex, backFaceVertexRange, backFaceTopLeftVertexIndex);
                i += 5;
            }
        }

        /// <summary>
        /// Function increases the <paramref name="originalValue"/> by one and wraps it in the
        /// range specified with the <paramref name="bottomRange"/> and <paramref name="topRange"/> 
        /// parameters.
        /// </summary>
        /// <param name="originalValue">The original value that will be increased by one.</param>
        /// <param name="topRange">Top range value that the original value should never reach.</param>
        /// <param name="bottomRange">Bottom range value that the original value will be assigned if it reaches
        /// the <paramref name="topRange"/> value.</param>
        /// <returns>Increased <paramref name="originalValue"/> by one, wrapped into specified range [bottomRange, topRange).</returns>
        private static int IncreaseWithWrap(int originalValue, int topRange, int bottomRange)
        {
            int newValue = originalValue + 1;
            if(newValue >= topRange)
            {
                newValue = (newValue % topRange) + bottomRange;
            }
            return newValue;
        }
    }
}
