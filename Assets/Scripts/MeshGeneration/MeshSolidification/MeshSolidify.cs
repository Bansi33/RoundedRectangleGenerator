using UnityEngine;

namespace BanSee.RoundedRectangleGenerator
{
    /// <summary>
    /// Static class providing functionality for expanding the 2D rectangle 
    /// meshes into third dimension by first duplicating the front face, reversing
    /// normals and then joining the front and back face by generating the connecting
    /// triangles between the outer vertices. It implements the same logic for solidifying
    /// the border of the rectangle.
    /// </summary>
    public static partial class MeshSolidify
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
        public static void SolidifyRectangle(MeshData rectangleMeshData,
            RectangleGenerationData rectangleGenerationData)
        {
            if (rectangleGenerationData.IsRoundedRectangle)
            {
                SolidifyRoundedRectangle(rectangleGenerationData, rectangleMeshData);
            }
            else
            {
                SolidifySimpleRectangle(rectangleMeshData, rectangleGenerationData);
            }
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
        public static void SolidifyRectangleBorder(MeshData borderMeshData, RectangleGenerationData rectangleGenerationData,
            RectangleBorderGenerationData rectangleBorderGenerationData)
        {
            if (rectangleGenerationData.IsRoundedRectangle)
            {
                SolidifyRoundedRectangleBorder(borderMeshData, rectangleGenerationData, rectangleBorderGenerationData);
            }
            else
            {
                SolidifySimpleRectangleBorder(borderMeshData, rectangleGenerationData, rectangleBorderGenerationData);
            }
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
        private static Vector3[] GenerateAndAppendBackFaceVertices(Vector3[] frontFaceVertices, float distanceBetweenFaces)
        {
            if (frontFaceVertices == null || frontFaceVertices.Length == 0)
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
    }
}
