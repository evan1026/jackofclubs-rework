using UnityEngine;

public class ChunkComponent : MonoBehaviour {

    private Chunk renderedChunk;

    private MeshFilter meshFilter;
    private MeshCollider meshCollider;

    private bool dirty = false;

    // Start is called before the first frame update
    public void Start() {
        meshFilter = GetComponent<MeshFilter>();
        meshCollider = GetComponent<MeshCollider>();

        meshCollider.sharedMesh = meshFilter.mesh;
    }

    public void UpdateMesh() {
        if (renderedChunk != null && renderedChunk.GenerateMesh()) {
            dirty = true;
        }

        if (dirty) {
            PushMeshData(renderedChunk?.meshData);
            dirty = false;
        }
    }

    private void PushMeshData(ChunkMeshData meshData) {
        meshFilter.mesh.Clear();

        if (meshData != null) {
            
            meshFilter.mesh.SetVertices(meshData.vertices);
            meshFilter.mesh.SetColors(meshData.colors);
            meshFilter.mesh.SetNormals(meshData.normals);
            meshFilter.mesh.SetUVs(0, meshData.textureCoords);
            meshFilter.mesh.SetTangents(meshData.tangents);
            meshFilter.mesh.SetTriangles(meshData.triangles, 0);
        }

        meshCollider.sharedMesh = meshFilter.mesh; // forces a recalculation of collision geometry
    }

    public void SetChunk(Chunk chunk) {
        renderedChunk = chunk;
        dirty = true;
    }
}