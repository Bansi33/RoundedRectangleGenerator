using UnityEngine;

namespace BanSee.RoundedRectangleGenerator
{
    /// <summary>
    /// Static class containing utility functions useful when creating rectangle meshes
    /// and borders surrounding them.
    /// </summary>
    public static class RectangleMeshUtils
    {
        /// <summary>
        /// Predefined normal orientation that all generated rectangles share.
        /// </summary>
        public static readonly Vector3 RECTANGLE_NORMAL = new Vector3(0f, 0f, -1f);

        /// <summary>
        /// Function calculates the total number of vertices that construct the front face of the rectangle
        /// based on the total number of inner and outer vertices. The number of inner and outer vertices 
        /// depends on the type of the mesh topology used when constructing the rectangle and number of 
        /// vertices constructing the rounded corners of the rectangle (optional).
        /// </summary>
        /// <param name="rectangleGenerationData">Reference to the instance of the <see cref="RectangleGenerationData"/>
        /// class, used to generate a rectangle, containing information about the topology of the rectangle
        /// and the rectangle type.</param>
        /// <returns>Total number of vertices contained on the front face of the rectangle mesh.</returns>
        public static int GetTotalNumberOfFrontFaceVertices(RectangleGenerationData rectangleGenerationData)
        {
            return GetNumberOfFrontFaceInnerVertices(rectangleGenerationData) + 
                   GetNumberOfFrontFaceOuterVertices(rectangleGenerationData);
        }

        /// <summary>
        /// Function calculates the number of inner vertices on the front face of the rectangle.
        /// The inner vertices are ones that are not present on the outer edge of the face, and are not
        /// being used during the connecting phase of the mesh solidification.
        /// </summary>
        /// <param name="rectangleGenerationData">Reference to the instance of the <see cref="RectangleGenerationData"/>
        /// class, used to generate a rectangle, containing information about the topology of the rectangle
        /// and the rectangle type.</param>
        /// <returns>Number of inner vertices on the front face of the rectangle.</returns>
        public static int GetNumberOfFrontFaceInnerVertices(RectangleGenerationData rectangleGenerationData)
        {
            RectangleTopologyType rectangleTopologyType = rectangleGenerationData.TopologyType;
            return rectangleTopologyType switch
            {
                // In topology with center vertex connection, there is only one inner vertex
                // that connects to all outer vertices and is stored first in the array of vertices.
                RectangleTopologyType.CenterVertexConnection => 1,

                // In topology with corner connections, there are 4 vertices forming an inner
                // rectangle, and serving as center vertices for generating corner triangles.
                // In case of a regular rectangle (one without rounded corners), there are
                // no inner vertices, since inner vertices are also an outer ones.
                RectangleTopologyType.CornerConnections => rectangleGenerationData.IsRoundedRectangle ? 4 : 0,

                // In all other unsupported cases, just return 0.
                _ => 0
            };
        }

        /// <summary>
        /// Function returns the total number of outer vertices for the front face of the rectangle.
        /// Outer vertices are the one that construct the outer edge of the rectangle.
        /// </summary>
        /// <param name="rectangleGenerationData">Reference to the instance of the <see cref="RectangleGenerationData"/>
        /// class, used to generate a rectangle, containing information about the topology of the rectangle
        /// and the rectangle type.</param>
        /// <returns>Total number of the vertices constructing the outer edge of the rectangle.</returns>
        public static int GetNumberOfFrontFaceOuterVertices(RectangleGenerationData rectangleGenerationData)
        {
            if (!rectangleGenerationData.IsRoundedRectangle)
            {
                // In case of a regular rectangle (the one without the rounded corners),
                // there is always 4 vertices constructing the outer edge of the rectangle.
                return 4;
            }

            // One corner contains a certain number of vertices specified by the user,
            // resulting in total of 4x that amount, since there are 4 corners of the rectangle.
            int totalNumberOfCornerVertices = 4 * rectangleGenerationData.CornerVertexCount;

            // Edges connect the corners, with each edge having one vertex at the start and
            // one vertex at the end of the edge. In total, 4x2 vertices.
            int numberOfEdgeVertices = 2;
            int totalNumberOfEdgeVertices = 4 * numberOfEdgeVertices;

            // Total number of front face outer vertices is a combination of edge and corner
            // vertices, no matter of the topology type that connects these outer vertices
            // with the inner ones.
            return totalNumberOfCornerVertices + totalNumberOfEdgeVertices;
        }

        /// <summary>
        /// Function generates an array of normal vectors for the provided <paramref name="normalsCount"/>.
        /// </summary>
        /// <param name="normalsCount">Number of normals, specifying the size of the array.</param>
        /// <returns>An array of normal vectors, all pointing in the 
        /// <see cref="RECTANGLE_NORMAL"/> direction.</returns>
        public static Vector3[] GenerateNormals(int normalsCount)
        {
            Vector3[] normals = new Vector3[normalsCount];
            for (int i = 0; i < normalsCount; i++)
            {
                // All normals point in the same direction since the mesh is in the X-Y plane.
                normals[i] = RECTANGLE_NORMAL;
            }
            return normals;
        }
    }
}
