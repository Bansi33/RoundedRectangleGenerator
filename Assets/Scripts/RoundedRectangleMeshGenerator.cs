using UnityEngine;

namespace BanSee.RoundedRectangleGenerator
{
    /// <summary>
    /// Static class providing functionality for generating the <see cref="RectangleMeshData"/>
    /// class that will be used for visually representing the generated rounded rectangle. It contains
    /// algorithms for correctly positioning vertices and connecting them into triangulated mesh that
    /// is optimized for rendering and texture mapping.
    /// </summary>
    public static class RoundedRectangleMeshGenerator
    {
        /// <summary>
        /// Enumeration specifying all possible edge types in a rectangle, 
        /// going from top edge clockwise.
        /// </summary>
        private enum RectangleEdgeType
        {
            Top = 0,
            Right = 1,
            Bottom = 2,
            Left = 3
        }

        /// <summary>
        /// Enumeration specifying all possible corner types in a rectangle,
        /// going from the top-right corner clockwise.
        /// </summary>
        private enum RectangleCornerType
        {
            TopRight = 0,
            BottomRight = 1,
            BottomLeft = 2,
            TopLeft = 3
        }

        /// <summary>
        /// Function generates the <see cref="RectangleMeshData"/> class and fills it with information 
        /// about vertex positions, UV coordinates, normals and triangles indices required for visually
        /// representing the rounded rectangles in the 3D space. It generates the triangulated mesh
        /// based on the <see cref="RectangleGenerationData.TopologyType"/>.
        /// </summary>
        /// <param name="rectangleGenerationData">Reference to the <see cref="RectangleGenerationData"/>
        /// class that contains properties for generating the rounded rectangle. It's topology type, 
        /// size, corner roundness radius, etc.</param>
        /// <returns>Reference to the instance of the <see cref="RectangleMeshData"/> containing properties
        /// required for visually representing the rounded rectangle mesh in 3D.</returns>
        public static RectangleMeshData GenerateRoundedRectangleMeshData(RectangleGenerationData rectangleGenerationData)
        {
            // Generating rectangle mesh data based on the desired topology type. Generated rectangle
            // is in the X-Y plane and doesn't contain information about the third dimension.
            RectangleMeshData rectangleMeshData;
            switch (rectangleGenerationData.TopologyType)
            {
                case RectangleTopologyType.CenterVertexConnection:
                    rectangleMeshData = GenerateRoundedRectangleMesh(rectangleGenerationData);
                    break;
                case RectangleTopologyType.CornerConnections:
                    rectangleMeshData = GenerateRoundedRectangleMeshWithConnectingVertices(rectangleGenerationData);
                    break;
                default:
                    Debug.LogError($"No implementation available for provided {nameof(RectangleTopologyType)}." +
                        $"Please implement mesh generation for the {rectangleGenerationData.TopologyType}.");
                    return null;
            }            

            return rectangleMeshData;
        }

        /// <summary>
        /// Function generates the mesh properties for the rounded rectangle, with triangle topology being
        /// created by first creating the inside rectangle of that connects 4 vertices that are also the 
        /// centers of the corner roundness circles, and then generating the outer vertices based on the
        /// <see cref="RectangleGenerationData.CornerRoundnessRadius"/> and the size of the rectangle.
        /// </summary>
        /// <param name="rectangleGenerationData">Reference to the <see cref="RectangleGenerationData"/>
        /// class that contains properties for generating the rounded rectangle. It's topology type, 
        /// size, corner roundness radius, etc.</param>
        /// <returns>Reference to the instance of the <see cref="RectangleMeshData"/> containing properties
        /// required for visually representing the rounded rectangle mesh in 3D.</returns>
        private static RectangleMeshData GenerateRoundedRectangleMeshWithConnectingVertices(RectangleGenerationData rectangleGenerationData)
        {
            float cornerRoundnessRadius = rectangleGenerationData.CornerRoundnessRadius;
            if (cornerRoundnessRadius <= 0f)
            {
                // Corner roundness radius can be null, but not negative.
                // In case it's null, a regular rectangle is actually requested. 
                // Generation of regular rectangles isn't the job for this class.
                return null;
            }

            float cornerRoundnessDiameter = 2f * cornerRoundnessRadius;
            float width = rectangleGenerationData.Width;
            float height = rectangleGenerationData.Height;
            if (width <= 0f || height <= 0f ||
                width < cornerRoundnessDiameter ||
                height < cornerRoundnessDiameter)
            {
                // Rounded rectangle size too small for wanted roundness
                // of the corners. This should be prevented during the data
                // setting phase.
                return null;
            }

            // Regular rectangle has 4 vertices, simple math.
            int numberOfVerticesInRectangle = 4;
            // Every regular rectangle corner vertex splits into two (first number 4).
            // These vertices will serve as connecting vertices for the corner ones.
            int numberOfAdditionalCornerVertices = 4;
            int cornerVertexCount = rectangleGenerationData.CornerVertexCount;
            if (cornerVertexCount > 0)
            {
                // If there are additional vertices requested (to smooth out the roundness),
                // then add them for every corner.
                numberOfAdditionalCornerVertices += 4 * cornerVertexCount;
            }
            int numberOfOuterRectangleVertices = numberOfVerticesInRectangle + numberOfAdditionalCornerVertices;

            // Inner rectangle contains 4 vertices, and these vertices serve as corner vertices
            // connecting to the outer rectangle vertices.
            int numberOfInnerRectangleVertices = 4;


            // Summarizing all required vertices, inner rectangle and outer edge ones.
            int totalNumberOfVertices = numberOfInnerRectangleVertices + numberOfOuterRectangleVertices;
            // Total number of triangles equals a total number of vertices + 2 triangles 
            // for the inner rectangle.
            int totalNumberOfTriangles = (totalNumberOfVertices + 2);
            int totalNumberOfIndices = 3 * totalNumberOfTriangles;

            // Generating vertex positions in the X-Y plane.
            float a = width - cornerRoundnessDiameter;
            float b = height - cornerRoundnessDiameter;
            float halfA = a * 0.5f;
            float halfB = b * 0.5f;
            int currentVertexIndex = 0;

            // First 4 vertices are for the inner rectangle that has a size of:
            // Width = a, Height = b
            Vector3[] vertices = new Vector3[totalNumberOfVertices];
            vertices[0] = new Vector3(-halfA,  halfB, 0);
            vertices[1] = new Vector3( halfA,  halfB, 0);
            vertices[2] = new Vector3( halfA, -halfB, 0);
            vertices[3] = new Vector3(-halfA, -halfB, 0);
            currentVertexIndex += 4;

            // Every rectangle has 4 edges, explicitly stated for clarity.
            int numberOfRectangleEdges = 4;

            // Rectangle edge generation starts from top edge, proceeds to right, then bottom
            // and then finally generates the left edge.
            for (int rectangleEdgeIndex = 0; rectangleEdgeIndex < numberOfRectangleEdges; rectangleEdgeIndex++)
            {
                // Edge vertices generation.
                InsertRectangleEdge((RectangleEdgeType)rectangleEdgeIndex, currentVertexIndex,
                    halfA, halfB, cornerRoundnessRadius, ref vertices);
                currentVertexIndex += 2;

                // Generate corner vertices that connect this edge with the next one.
                InsertCornerVertices((RectangleCornerType)rectangleEdgeIndex, currentVertexIndex,
                    halfA, halfB, cornerRoundnessRadius, cornerVertexCount, ref vertices);
                currentVertexIndex += cornerVertexCount;
            }

            // Generate UV coordinates.
            Vector2[] uvs = UVGenerator.GenerateUVs(rectangleGenerationData, vertices);

            // Generate normals.
            Vector3[] normals = new Vector3[totalNumberOfVertices];
            for (int i = 0; i < totalNumberOfVertices; i++)
            {
                // All normals point in the same direction since the mesh is in the X-Y plane.
                normals[i] = new Vector3(0f, 0f, -1f);
            }

            // Generate indices.
            int[] indices = new int[totalNumberOfIndices];

            // First two triangles are for the predefined inner rectangle.
            indices[0] = 0;
            indices[1] = 1;
            indices[2] = 2;
            
            indices[3] = 0;
            indices[4] = 2;
            indices[5] = 3;

            // Starting after two already created triangles (or 6 indices).
            int indicesArrayIndex = 6;
            for (int rectangleEdgeIndex = 0; rectangleEdgeIndex < numberOfRectangleEdges; rectangleEdgeIndex++)
            {
                int nextRectangleEdgeVertex = (rectangleEdgeIndex + 1) % numberOfRectangleEdges;
                int outerEdgeVertex = 4 + rectangleEdgeIndex * (2 + cornerVertexCount);

                // Edge indices generation. Every rectangle edge generates two triangles,
                // so 6 indices in total.
                indices[indicesArrayIndex] = rectangleEdgeIndex; 
                indices[indicesArrayIndex + 1] = outerEdgeVertex; 
                indices[indicesArrayIndex + 2] = nextRectangleEdgeVertex;
                
                indices[indicesArrayIndex + 3] = nextRectangleEdgeVertex; 
                indices[indicesArrayIndex + 4] = outerEdgeVertex; 
                indices[indicesArrayIndex + 5] = outerEdgeVertex + 1;
                indicesArrayIndex += 6;

                // Generating corner triangles.
                int cornerConnectingIndex = nextRectangleEdgeVertex;
                int currentOuterVertex = outerEdgeVertex + 1;
                for(int i = 0; i < cornerVertexCount; i++)
                {
                    indices[indicesArrayIndex] = cornerConnectingIndex;
                    indices[indicesArrayIndex + 1] = currentOuterVertex;
                    indices[indicesArrayIndex + 2] = currentOuterVertex + 1;

                    indicesArrayIndex += 3;
                    currentOuterVertex++;
                }

                // Connect last triangle of the corner with the next outer edge vertex.
                indices[indicesArrayIndex] = cornerConnectingIndex;
                indices[indicesArrayIndex + 1] = currentOuterVertex;
                indices[indicesArrayIndex + 2] = 4 + nextRectangleEdgeVertex * (2 + cornerVertexCount);
                indicesArrayIndex += 3;
            }

            // Data class creation.
            RectangleMeshData rectangleMeshData = new RectangleMeshData(rectangleGenerationData.TopologyType)
            {
                Vertices = vertices,
                Uvs = uvs,
                Normals = normals,
                Triangles = indices
            };

            return rectangleMeshData;
        }

        /// <summary>
        /// Function generates the rounded rectangle that has one center vertex that all outer vertices
        /// connect to. This is a simpler topology type, but creates a lot of thin long vertices that
        /// are bad for rendering pipeline. This type of simple topology is better
        /// when the rectangle's size is equal to the double corner roundness, thus resulting in a circular shape.
        /// <br>When possible, use the 
        /// <see cref="GenerateRoundedRectangleMeshWithConnectingVertices(RectangleGenerationData)"/>
        /// method instead, since it will generate a better topology.</br>
        /// </summary>
        /// <param name="rectangleGenerationData">Reference to the <see cref="RectangleGenerationData"/>
        /// class that contains properties for generating the rounded rectangle. It's topology type, 
        /// size, corner roundness radius, etc.</param>
        /// <returns>Reference to the instance of the <see cref="RectangleMeshData"/> containing properties
        /// required for visually representing the rounded rectangle mesh in 3D.</returns>
        private static RectangleMeshData GenerateRoundedRectangleMesh(RectangleGenerationData rectangleGenerationData)
        {
            float cornerRoundnessRadius = rectangleGenerationData.CornerRoundnessRadius;
            if (cornerRoundnessRadius <= 0f)
            {
                // Corner roundness radius can be null, but not negative.
                // In case it's null, a regular rectangle is actually requested. 
                // Generation of regular rectangles isn't the job for this class.
                return null;
            }

            float cornerRoundnessDiameter = 2f * cornerRoundnessRadius;
            float width = rectangleGenerationData.Width;
            float height = rectangleGenerationData.Height;
            if (width <= 0f || height <= 0f ||
                width < cornerRoundnessDiameter ||
                height < cornerRoundnessDiameter)
            {
                // Rounded rectangle size too small for wanted roundness
                // of the corners. This should be prevented during the data
                // setting phase.
                return null;
            }

            // Regular rectangle has 4 vertices, simple math.
            int numberOfVerticesInRectangle = 4;
            // Every regular rectangle corner vertex splits into two (first number 4).
            // These vertices will serve as connecting vertices for the corner ones.
            int numberOfAdditionalCornerVertices = 4;
            int cornerVertexCount = rectangleGenerationData.CornerVertexCount;
            if (cornerVertexCount > 0)
            {
                // If there are additional vertices requested (to smooth out the roundness),
                // then add them for every corner.
                numberOfAdditionalCornerVertices += 4 * cornerVertexCount;
            }

            // Summarizing all required vertices, plus adding one additional for the center.
            // All triangles will connect to the center one.
            int totalNumberOfVertices = numberOfVerticesInRectangle + numberOfAdditionalCornerVertices + 1;
            int totalNumberOfTriangles = (totalNumberOfVertices - 1);
            int totalNumberOfIndices = 3 * totalNumberOfTriangles;

            // Generating vertex positions in the X-Y plane.
            float a = width - cornerRoundnessDiameter;
            float b = height - cornerRoundnessDiameter;
            float halfA = a * 0.5f;
            float halfB = b * 0.5f;
            int currentVertexIndex = 0;

            // First vertex is the central one, serving as connector of all others.
            Vector3[] vertices = new Vector3[totalNumberOfVertices];
            vertices[currentVertexIndex] = Vector3.zero;
            currentVertexIndex++;

            // Every rectangle has 4 edges, explicitly stated for clarity.
            int numberOfRectangleEdges = 4;

            // Rectangle edge generation starts from top edge, proceeds to right, then bottom
            // and then finally generates the left edge.
            for(int rectangleEdgeIndex = 0; rectangleEdgeIndex < numberOfRectangleEdges; rectangleEdgeIndex++)
            {
                // Edge vertices generation.
                InsertRectangleEdge((RectangleEdgeType)rectangleEdgeIndex, currentVertexIndex,
                    halfA, halfB, cornerRoundnessRadius, ref vertices);
                currentVertexIndex += 2;

                // Generate corner vertices that connect this edge with the next one.
                InsertCornerVertices((RectangleCornerType)rectangleEdgeIndex, currentVertexIndex, 
                    halfA, halfB, cornerRoundnessRadius, cornerVertexCount, ref vertices);
                currentVertexIndex += cornerVertexCount;
            }

            // Generate UV coordinates.
            Vector2[] uvs = UVGenerator.GenerateUVs(rectangleGenerationData, vertices);

            // Generate normals.
            Vector3[] normals = new Vector3[totalNumberOfVertices];
            for (int i = 0; i < totalNumberOfVertices; i++)
            {
                // All normals point in the same direction since the mesh is in the X-Y plane.
                normals[i] = new Vector3(0f, 0f, -1f);
            }

            // Generate indices.
            int[] indices = new int[totalNumberOfIndices];
            for (int i = 0; i < totalNumberOfIndices; i++)
            {
                int desiredVertexIndex = (i % 3) switch
                {
                    0 => Mathf.RoundToInt(i / 3) + 1,
                    1 => Mathf.RoundToInt(i / 3) + 2,
                    _ => 0,
                };

                // Account overflow when joining last couple of triangles into circular strip.
                if (desiredVertexIndex >= totalNumberOfVertices)
                {
                    desiredVertexIndex = (desiredVertexIndex % totalNumberOfVertices) + 1;
                }

                indices[i] = desiredVertexIndex;
            }

            // Data class creation.
            RectangleMeshData rectangleMeshData = new RectangleMeshData(rectangleGenerationData.TopologyType)
            {
                Vertices = vertices,
                Uvs = uvs,
                Normals = normals,
                Triangles = indices
            };

            return rectangleMeshData;
        }

        private static void InsertRectangleEdge(RectangleEdgeType edgeType, int currentVertexIndex, 
            float halfA, float halfB, float cornerRoundnessRadius, ref Vector3[] vertices)
        {
            switch (edgeType)
            {
                case RectangleEdgeType.Top:
                    vertices[currentVertexIndex] = new Vector3(-halfA, halfB + cornerRoundnessRadius, 0);
                    vertices[currentVertexIndex + 1] = new Vector3(halfA, halfB + cornerRoundnessRadius, 0);
                    break;
                case RectangleEdgeType.Right:
                    vertices[currentVertexIndex] = new Vector3(halfA + cornerRoundnessRadius, halfB, 0);
                    vertices[currentVertexIndex + 1] = new Vector3(halfA + cornerRoundnessRadius, -halfB, 0);
                    break;
                case RectangleEdgeType.Bottom:
                    vertices[currentVertexIndex] = new Vector3(halfA, -halfB - cornerRoundnessRadius, 0);
                    vertices[currentVertexIndex + 1] = new Vector3(-halfA, -halfB - cornerRoundnessRadius, 0);
                    break;
                case RectangleEdgeType.Left:
                    vertices[currentVertexIndex] = new Vector3(-halfA - cornerRoundnessRadius, -halfB, 0);
                    vertices[currentVertexIndex + 1] = new Vector3(-halfA - cornerRoundnessRadius, halfB, 0);
                    break;
                default:
                    Debug.LogError("No rectangle edge specified!");
                    break;
            }
        }

        private static void InsertCornerVertices(RectangleCornerType cornerType, int currentVertexIndex,
            float halfA, float halfB, float cornerRoundnessRadius, int cornerVertexCount, ref Vector3[] vertices)
        {
            Vector3 cornerCircleCenter;
            float startAngleInDeg;

            switch (cornerType)
            {
                default:
                case RectangleCornerType.TopRight:
                    cornerCircleCenter = new Vector3(halfA, halfB, 0);
                    startAngleInDeg = 90;
                    break;
                case RectangleCornerType.BottomRight:
                    cornerCircleCenter = new Vector3(halfA, -halfB, 0);
                    startAngleInDeg = 0;
                    break;
                case RectangleCornerType.BottomLeft:
                    cornerCircleCenter = new Vector3(-halfA, -halfB, 0);
                    startAngleInDeg = -90;
                    break;
                case RectangleCornerType.TopLeft:
                    cornerCircleCenter = new Vector3(-halfA, halfB, 0);
                    startAngleInDeg = -180;
                    break;
            }

            float angleChangeInDeg = 90f / (cornerVertexCount + 1);
            for(int i = 0; i < cornerVertexCount; i++)
            {
                float angleInRad = Mathf.Deg2Rad * (startAngleInDeg - (i + 1) * angleChangeInDeg);
                float xPosAddition = cornerRoundnessRadius * Mathf.Cos(angleInRad);
                float yPosAddition = cornerRoundnessRadius * Mathf.Sin(angleInRad);

                vertices[currentVertexIndex] = cornerCircleCenter + new Vector3(xPosAddition, yPosAddition, 0);
                currentVertexIndex++;
            }
        }        
    }
}