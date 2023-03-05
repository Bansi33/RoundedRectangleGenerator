namespace BanSee.RoundedRectangleGenerator
{
    /// <summary>
    /// Enumeration specifying possible mesh topologies for
    /// generating rectangles and rounded rectangles.
    /// </summary>
    public enum RectangleTopologyType
    {
        /// <summary>
        /// Type of topology where there is an additional center
        /// vertex added to the mesh that connects to all corner vertices.
        /// All triangles of the mesh are connecting a center vertex and 
        /// two outer vertices.
        /// </summary>
        CenterVertexConnection = 0,

        /// <summary>
        /// Type of topology where rounded rectangle is constructed by creating an
        /// inner rectangle and using it's corner vertices as center vertices for 
        /// connecting to the vertices on the rounded corners. Other vertices on the
        /// edges of the rectangle are connected to the outer rim of the inner rectangle.
        /// There is no central vertex that connects all others.
        /// </summary>
        CornerConnections = 1
    }
}
