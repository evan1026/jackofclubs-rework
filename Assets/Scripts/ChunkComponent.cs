using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class ChunkComponent : MonoBehaviour {

    private Chunk renderedChunk;

    private MeshFilter meshFilter;
    private MeshCollider meshCollider;

    private bool dirty = false;

    private FrameTimer frameTimer;

    // Start is called before the first frame update
    internal void Start() {
        meshFilter = GetComponent<MeshFilter>();
        meshCollider = GetComponent<MeshCollider>();

        meshCollider.sharedMesh = meshFilter.mesh;

        frameTimer = FindObjectOfType<FrameTimer>();
    }

    internal void Update() {
        if (renderedChunk != null && renderedChunk.GenerateMesh()) {
            Debug.Log("Chunk regened");
            dirty = true;
        }

        if (dirty && frameTimer.FrameHasTime) {
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

public class Chunk {
    public static int ChunkSize = 16;

    public ChunkMeshData meshData;

    private Block[,,] blocks;
    private bool dirty = false;

    public Chunk() {
        blocks = new Block[ChunkSize, ChunkSize, ChunkSize];
        meshData = new ChunkMeshData();

        for (int x = 0; x < ChunkSize; ++x) {
            for (int y = 0; y < ChunkSize; ++y) {
                for (int z = 0; z < ChunkSize; ++z) {
                    if ((x + y + z) % 2 == 0) {
                        blocks[x, y, z] = new Block(new Color(x / 16f, y / 16f, z / 16f));
                    } else {
                        blocks[x, y, z] = new Block();
                    }
                }
            }
        }

        dirty = true;
    }

    private bool IsFree(Vector3Int pos) {
        if (pos.x < 0 || pos.x >= Chunk.ChunkSize ||
            pos.y < 0 || pos.y >= Chunk.ChunkSize ||
            pos.z < 0 || pos.z >= Chunk.ChunkSize ||
            blocks[pos.x, pos.y, pos.z].type == Block.Type.Air) {
            return true;
        }
        return false;
    }

    public bool GenerateMesh() {

        if (!dirty) {
            return false;
        }

        meshData.vertices.Clear();
        meshData.colors.Clear();
        meshData.normals.Clear();
        meshData.textureCoords.Clear();
        meshData.tangents.Clear();
        meshData.triangles.Clear();

        for (int x = 0; x < Chunk.ChunkSize; ++x) {
            for (int y = 0; y < Chunk.ChunkSize; ++y) {
                for (int z = 0; z < Chunk.ChunkSize; ++z) {
                    if (blocks[x, y, z].type == Block.Type.Solid) {

                        if (IsFree(new Vector3Int(x, y, z + 1))) {
                            int startIndex = meshData.vertices.Count;

                            meshData.vertices.Add(new Vector3(x, y, z + 1));
                            meshData.vertices.Add(new Vector3(x + 1, y, z + 1));
                            meshData.vertices.Add(new Vector3(x, y + 1, z + 1));
                            meshData.vertices.Add(new Vector3(x + 1, y + 1, z + 1));

                            meshData.textureCoords.Add(new Vector2(0.0f, 1.0f));
                            meshData.textureCoords.Add(new Vector2(1.0f, 1.0f));
                            meshData.textureCoords.Add(new Vector2(0.0f, 0.0f));
                            meshData.textureCoords.Add(new Vector2(1.0f, 0.0f));

                            meshData.triangles.Add(startIndex);
                            meshData.triangles.Add(startIndex + 1);
                            meshData.triangles.Add(startIndex + 3);
                            meshData.triangles.Add(startIndex);
                            meshData.triangles.Add(startIndex + 3);
                            meshData.triangles.Add(startIndex + 2);

                            for (int i = 0; i < 4; ++i) {
                                meshData.colors.Add(blocks[x, y, z].color);
                                meshData.normals.Add(new Vector3(0, 0, 1));
                                meshData.tangents.Add(new Vector4(1.0f, 0.0f, 0.0f, 1.0f));
                            }
                        }

                        if (IsFree(new Vector3Int(x, y, z - 1))) {
                            int startIndex = meshData.vertices.Count;

                            meshData.vertices.Add(new Vector3(x, y, z));
                            meshData.vertices.Add(new Vector3(x + 1, y, z));
                            meshData.vertices.Add(new Vector3(x, y + 1, z));
                            meshData.vertices.Add(new Vector3(x + 1, y + 1, z));

                            meshData.textureCoords.Add(new Vector2(1.0f, 0.0f));
                            meshData.textureCoords.Add(new Vector2(1.0f, 1.0f));
                            meshData.textureCoords.Add(new Vector2(0.0f, 0.0f));
                            meshData.textureCoords.Add(new Vector2(0.0f, 1.0f));

                            meshData.triangles.Add(startIndex + 1);
                            meshData.triangles.Add(startIndex);
                            meshData.triangles.Add(startIndex + 2);
                            meshData.triangles.Add(startIndex + 1);
                            meshData.triangles.Add(startIndex + 2);
                            meshData.triangles.Add(startIndex + 3);

                            for (int i = 0; i < 4; ++i) {
                                meshData.colors.Add(blocks[x, y, z].color);
                                meshData.normals.Add(new Vector3(0, 0, -1));
                                meshData.tangents.Add(new Vector4(0.0f, 1.0f, 0.0f, -1.0f));
                            }
                        }

                        if (IsFree(new Vector3Int(x + 1, y, z))) {
                            int startIndex = meshData.vertices.Count;

                            meshData.vertices.Add(new Vector3(x + 1, y, z));
                            meshData.vertices.Add(new Vector3(x + 1, y + 1, z));
                            meshData.vertices.Add(new Vector3(x + 1, y, z + 1));
                            meshData.vertices.Add(new Vector3(x + 1, y + 1, z + 1));

                            meshData.textureCoords.Add(new Vector2(0.0f, 1.0f));
                            meshData.textureCoords.Add(new Vector2(1.0f, 1.0f));
                            meshData.textureCoords.Add(new Vector2(0.0f, 0.0f));
                            meshData.textureCoords.Add(new Vector2(1.0f, 0.0f));

                            meshData.triangles.Add(startIndex);
                            meshData.triangles.Add(startIndex + 3);
                            meshData.triangles.Add(startIndex + 2);
                            meshData.triangles.Add(startIndex);
                            meshData.triangles.Add(startIndex + 1);
                            meshData.triangles.Add(startIndex + 3);

                            for (int i = 0; i < 4; ++i) {
                                meshData.tangents.Add(new Vector4(0.0f, 1.0f, 0.0f, 1.0f));
                                meshData.normals.Add(new Vector3(1, 0, 0));
                                meshData.colors.Add(blocks[x, y, z].color);
                            }
                        }

                        if (IsFree(new Vector3Int(x - 1, y, z))) {
                            int startIndex = meshData.vertices.Count;

                            meshData.vertices.Add(new Vector3(x, y, z));
                            meshData.vertices.Add(new Vector3(x, y + 1, z));
                            meshData.vertices.Add(new Vector3(x, y, z + 1));
                            meshData.vertices.Add(new Vector3(x, y + 1, z + 1));

                            meshData.textureCoords.Add(new Vector2(0.0f, 0.0f));
                            meshData.textureCoords.Add(new Vector2(1.0f, 0.0f));
                            meshData.textureCoords.Add(new Vector2(0.0f, 1.0f));
                            meshData.textureCoords.Add(new Vector2(1.0f, 1.0f));

                            meshData.triangles.Add(startIndex + 2);
                            meshData.triangles.Add(startIndex + 1);
                            meshData.triangles.Add(startIndex);
                            meshData.triangles.Add(startIndex + 2);
                            meshData.triangles.Add(startIndex + 3);
                            meshData.triangles.Add(startIndex + 1);

                            for (int i = 0; i < 4; ++i) {
                                meshData.tangents.Add(new Vector4(0.0f, 1.0f, 0.0f, 1.0f));
                                meshData.normals.Add(new Vector3(-1, 0, 0));
                                meshData.colors.Add(blocks[x, y, z].color);
                            }
                        }

                        if (IsFree(new Vector3Int(x, y + 1, z))) {
                            int startIndex = meshData.vertices.Count;

                            meshData.vertices.Add(new Vector3(x, y + 1, z));
                            meshData.vertices.Add(new Vector3(x + 1, y + 1, z));
                            meshData.vertices.Add(new Vector3(x, y + 1, z + 1));
                            meshData.vertices.Add(new Vector3(x + 1, y + 1, z + 1));

                            meshData.textureCoords.Add(new Vector2(0.0f, 0.0f));
                            meshData.textureCoords.Add(new Vector2(1.0f, 0.0f));
                            meshData.textureCoords.Add(new Vector2(0.0f, 1.0f));
                            meshData.textureCoords.Add(new Vector2(1.0f, 1.0f));

                            meshData.triangles.Add(startIndex + 2);
                            meshData.triangles.Add(startIndex + 1);
                            meshData.triangles.Add(startIndex);
                            meshData.triangles.Add(startIndex + 2);
                            meshData.triangles.Add(startIndex + 3);
                            meshData.triangles.Add(startIndex + 1);

                            for (int i = 0; i < 4; ++i) {
                                meshData.tangents.Add(new Vector4(1.0f, 0.0f, 0.0f, 1.0f));
                                meshData.normals.Add(new Vector3(0, 1, 0));
                                meshData.colors.Add(blocks[x, y, z].color);
                            }
                        }

                        if (IsFree(new Vector3Int(x, y - 1, z))) {
                            int startIndex = meshData.vertices.Count;

                            meshData.vertices.Add(new Vector3(x, y, z));
                            meshData.vertices.Add(new Vector3(x + 1, y, z));
                            meshData.vertices.Add(new Vector3(x, y, z + 1));
                            meshData.vertices.Add(new Vector3(x + 1, y, z + 1));

                            meshData.textureCoords.Add(new Vector2(0.0f, 1.0f));
                            meshData.textureCoords.Add(new Vector2(1.0f, 1.0f));
                            meshData.textureCoords.Add(new Vector2(0.0f, 0.0f));
                            meshData.textureCoords.Add(new Vector2(1.0f, 0.0f));

                            meshData.triangles.Add(startIndex + 1);
                            meshData.triangles.Add(startIndex + 2);
                            meshData.triangles.Add(startIndex);
                            meshData.triangles.Add(startIndex + 1);
                            meshData.triangles.Add(startIndex + 3);
                            meshData.triangles.Add(startIndex + 2);

                            for (int i = 0; i < 4; ++i) {
                                meshData.tangents.Add(new Vector4(1.0f, 0.0f, 0.0f, 1.0f));
                                meshData.normals.Add(new Vector3(0, -1, 0));
                                meshData.colors.Add(blocks[x, y, z].color);
                            }
                        }
                    }
                }
            }
        }

        dirty = false;

        return true;
    }
}

public class ChunkMeshData {
    public List<Vector3> vertices = new List<Vector3>();
    public List<Vector3> normals = new List<Vector3>();
    public List<Vector2> textureCoords = new List<Vector2>();
    public List<Vector4> tangents = new List<Vector4>();
    public List<Color> colors = new List<Color>();
    public List<int> triangles = new List<int>();
}

public class Block {
    public enum Type {
        Air = 0,
        Solid = 1
    }

    public Type type;
    public Color color;

    public Block() : this(Type.Air, new Color()) {}

    public Block(Type type) : this(type, new Color()) {}

    public Block(Color color) : this(Type.Solid, color) {}

    public Block(Type type, Color color) {
        this.type = type;
        this.color = color;
    }
}