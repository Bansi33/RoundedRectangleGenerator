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
    public class RectangleMeshData : MeshData
    {
        /// <summary>
        /// Number of vertices that are not contained on the outer edge of the rectangle, and are
        /// always stored first in the <see cref="Vertices"/> array.
        /// </summary>
        public int NumberOfInnerVertices
        {
            get
            {
                return _topologyType switch
                {
                    // In topology with center vertex connection, there is only one inner vertex
                    // that connects to all outer vertices and is stored first in the array of vertices.
                    RectangleTopologyType.CenterVertexConnection => 1,

                    // In topology with corner connections, there are 4 vertices forming an inner
                    // rectangle, and serving as center vertices for generating corner triangles.
                    // In case of a regular rectangle (one without rounded corners), there are
                    // no inner vertices, since inner vertices are also an outer ones.
                    RectangleTopologyType.CornerConnections => Vertices != null && Vertices.Length > 4 ? 4 : 0,
                    _ => 0
                };
            }
        }

        protected RectangleTopologyType _topologyType;

        public RectangleMeshData(RectangleTopologyType rectangleTopologyType) : base()
        {
            _topologyType = rectangleTopologyType;
        }
    }
}