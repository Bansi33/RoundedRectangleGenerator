using UnityEngine;

namespace BanSee.RoundedRectangleGenerator
{
    /// <summary>
    /// Static class that provides a functionality for generating a
    /// rectangle mesh. It applies specific UVs layout based on the
    /// <see cref="UVGenerator"/> class.
    /// </summary>
    public static class SimpleRectangleMeshGenerator
    {
        /// <summary>
        /// Function generates <see cref="RectangleMeshData"/> and fills it with the
        /// correct data that can be applied to the <see cref="Mesh"/> in order to 
        /// generate a 2D or 3D rectangle.
        /// </summary>
        /// <param name="rectangleGenerationData">Reference to the class containing the desired
        /// specifications for generating the rectangle - it's width, height, etc.</param>
        /// <returns>Populated <see cref="RectangleMeshData"/> class with values that can be
        /// applied to the <see cref="Mesh"/> in order to generate a rectangle.</returns>
        public static MeshData GenerateRectangleMeshData(RectangleGenerationData rectangleGenerationData)
        {
            if(rectangleGenerationData.TopologyType != RectangleTopologyType.CenterVertexConnection)
            {
                Debug.LogError("Rectangles that don't have rounded corners should be generated with " +
                    $"the {RectangleTopologyType.CenterVertexConnection} topology type. Can't generate a " +
                    $"simple rectangle with {rectangleGenerationData.TopologyType} topology. Returning NULL!");
                return null;
            }

            // Generating rectangle mesh data based on the desired topology type. Generated rectangle
            // is in the X-Y plane and doesn't contain information about the third dimension.
            return GenerateRectangleWithConnectingCenterVertex(rectangleGenerationData);
        }

        /// <summary>
        /// Function generates <see cref="RectangleMeshData"/> and fills it with the
        /// correct data that can be applied to the <see cref="Mesh"/> in order to 
        /// generate a 2D rectangle (X-Y plane). Rectangle mesh consist of 4 corner
        /// vertices and one connecting center vertex that is a part of every triangle.
        /// </summary>
        /// <param name="rectangleGenerationData">Reference to the class containing the desired
        /// specifications for generating the rectangle - it's width, height, etc.</param>
        /// <returns>Populated <see cref="RectangleMeshData"/> class with values that can be
        /// applied to the <see cref="Mesh"/> in order to generate a rectangle.</returns>
        private static MeshData GenerateRectangleWithConnectingCenterVertex(RectangleGenerationData rectangleGenerationData)
        {
            float width = rectangleGenerationData.Width;
            float height = rectangleGenerationData.Height;

            // Every regular rectangle has only 4 vertices. Add one to the center of the mesh
            // that will serve as connector to all others.
            int totalNumberOfVertices = 4 + 1;
            MeshData rectangleMeshData = new MeshData();

            // Vertices sorted from top left to bottom left, with connecting center vertex
            // being the first vertex. Center, Top Left, Top Right, Bottom Right, Bottom Left.
            Vector3[] vertices = new Vector3[totalNumberOfVertices];
            vertices[0] = Vector3.zero;
            vertices[1] = new Vector3(-0.5f * width, 0.5f * height, 0f);
            vertices[2] = new Vector3(0.5f * width, 0.5f * height, 0f);
            vertices[3] = new Vector3(0.5f * width, -0.5f * height, 0f);
            vertices[4] = new Vector3(-0.5f * width, -0.5f * height, 0f);
            rectangleMeshData.Vertices = vertices;

            // Generating UV coordinates based on the selected generation mode and size 
            // of the requested rectangle.
            rectangleMeshData.Uvs = UVGenerator.GenerateUVs(rectangleGenerationData, vertices);

            // All normals of the rectangle pointing in the same direction since it's 
            // a 2D mesh representation (X-Y plane).
            rectangleMeshData.Normals = RectangleMeshUtils.GenerateNormals(totalNumberOfVertices);

            // 4 triangles are generated to form the rectangle mesh. All triangles
            // consist of two corner vertices and the center vertex.
            int totalNumberOfTriangles = totalNumberOfVertices - 1;
            int[] triangles = new int[totalNumberOfTriangles * 3];

            // Upper triangle.
            triangles[0] = 1;
            triangles[1] = 2;
            triangles[2] = 0;

            // Right triangle.
            triangles[3] = 2;
            triangles[4] = 3;
            triangles[5] = 0;

            // Bottom triangle.
            triangles[6] = 3;
            triangles[7] = 4;
            triangles[8] = 0;

            // Left triangle.
            triangles[9] = 4;
            triangles[10] = 1;
            triangles[11] = 0;
            rectangleMeshData.Triangles = triangles;

            return rectangleMeshData;
        }
    }
}
