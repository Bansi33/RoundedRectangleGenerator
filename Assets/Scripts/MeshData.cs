using System;
using UnityEngine;

namespace BanSee.RoundedRectangleGenerator
{
    /// <summary>
    /// Class containing data required for generating a <see cref="Mesh"/>
    /// that can be visualized. Contains <see cref="ApplyToMesh(ref Mesh)"/>
    /// function that applies the data to the mesh so it could be created.
    /// </summary>
    [Serializable]
    public class MeshData
    {
        /// <summary>
        /// An array of <see cref="Vector3"/> containing the positions of vertices in local object space.
        /// </summary>
        public Vector3[] Vertices;
        /// <summary>
        /// An array of <see cref="Vector3"/> specifying normals directions for every vertex.
        /// </summary>
        public Vector3[] Normals;
        /// <summary>
        /// An array of <see cref="Vector2"/> specifying UV coordinates for every vertex.
        /// </summary>
        public Vector2[] Uvs;
        /// <summary>
        /// An array containing indices of vertices that specify which vertices 
        /// form a certain triangle in the  mesh.
        /// </summary>
        public int[] Triangles;
        
        public MeshData()
        {
            Vertices = null;
            Normals = null;
            Uvs = null;
            Triangles = null;
        }

        /// <summary>
        /// Function applies the data to the provided <paramref name="mesh"/>
        /// object in order to correctly initialize it. It provides information
        /// about vertex positions, UV coordinates, normals and triangle indices.
        /// </summary>
        /// <param name="mesh">Reference to the <see cref="Mesh"/> component
        /// that will receive the values for vertex positions, UV coordinates, 
        /// normals and triangle indices.</param>
        public virtual void ApplyToMesh(ref Mesh mesh)
        {
            mesh.vertices = Vertices;
            mesh.triangles = Triangles;
            mesh.normals = Normals;
            mesh.uv = Uvs;
        }
    }
}