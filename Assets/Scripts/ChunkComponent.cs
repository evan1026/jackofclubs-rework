using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkComponent : MonoBehaviour {

    private Chunk renderedChunk;

    private MeshFilter meshFilter;
    private MeshCollider meshCollider;

    private bool dirty = false;

    public Block[,,] blocks {
        get {
            return renderedChunk.blocks;
        }
    }

    // Start is called before the first frame update
    internal void Start() {
        meshFilter = GetComponent<MeshFilter>();
        meshCollider = GetComponent<MeshCollider>();
    }

    internal void Update() {
        if (dirty) {
            GenerateMesh();
        }
    }

    private void GenerateMesh() {
        Mesh mesh = new Mesh();

        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector2> textureCoords = new List<Vector2>();
        List<Vector4> tangents = new List<Vector4>();
        List<Color> colors = new List<Color>();
        List<int> triangles = new List<int>();

        for(int x = 0; x < Chunk.ChunkSize; ++x) {
            for (int y = 0; y < Chunk.ChunkSize; ++y) {
                for (int z = 0; z < Chunk.ChunkSize; ++z) {
                    if (renderedChunk.blocks[x, y, z].type == Block.Type.Solid) {

                        if (IsFree(new Vector3Int(x, y, z + 1))) {
                            int startIndex = vertices.Count;

                            vertices.Add(new Vector3(x,     y,     z + 1));
                            vertices.Add(new Vector3(x + 1, y,     z + 1));
                            vertices.Add(new Vector3(x,     y + 1, z + 1));
                            vertices.Add(new Vector3(x + 1, y + 1, z + 1));

                            textureCoords.Add(new Vector2(0.0f, 1.0f));
                            textureCoords.Add(new Vector2(1.0f, 1.0f));
                            textureCoords.Add(new Vector2(0.0f, 0.0f));
                            textureCoords.Add(new Vector2(1.0f, 0.0f));

                            triangles.Add(startIndex);
                            triangles.Add(startIndex + 1);
                            triangles.Add(startIndex + 3);
                            triangles.Add(startIndex);
                            triangles.Add(startIndex + 3);
                            triangles.Add(startIndex + 2);

                            for (int i = 0; i < 4; ++i) {
                                colors.Add(renderedChunk.blocks[x, y, z].color);
                                normals.Add(new Vector3(0, 0, 1));
                                tangents.Add(new Vector4(1.0f, 0.0f, 0.0f, 1.0f));
                            }
                        }

                        if (IsFree(new Vector3Int(x, y, z - 1))) {
                            int startIndex = vertices.Count;

                            vertices.Add(new Vector3(x,     y,     z));
                            vertices.Add(new Vector3(x + 1, y,     z));
                            vertices.Add(new Vector3(x,     y + 1, z));
                            vertices.Add(new Vector3(x + 1, y + 1, z));

                            textureCoords.Add(new Vector2(1.0f, 0.0f));
                            textureCoords.Add(new Vector2(1.0f, 1.0f));
                            textureCoords.Add(new Vector2(0.0f, 0.0f));
                            textureCoords.Add(new Vector2(0.0f, 1.0f));

                            triangles.Add(startIndex + 1);
                            triangles.Add(startIndex);
                            triangles.Add(startIndex + 2);
                            triangles.Add(startIndex + 1);
                            triangles.Add(startIndex + 2);
                            triangles.Add(startIndex + 3);

                            for (int i = 0; i < 4; ++i) {
                                colors.Add(renderedChunk.blocks[x, y, z].color);
                                normals.Add(new Vector3(0, 0, -1));
                                tangents.Add(new Vector4(0.0f, 1.0f, 0.0f, -1.0f));
                            }
                        }

                        if (IsFree(new Vector3Int(x + 1, y, z))) {
                            int startIndex = vertices.Count;

                            vertices.Add(new Vector3(x + 1, y,     z));
                            vertices.Add(new Vector3(x + 1, y + 1, z));
                            vertices.Add(new Vector3(x + 1, y,     z + 1));
                            vertices.Add(new Vector3(x + 1, y + 1, z + 1));       

                            textureCoords.Add(new Vector2(0.0f, 1.0f));
                            textureCoords.Add(new Vector2(1.0f, 1.0f));
                            textureCoords.Add(new Vector2(0.0f, 0.0f));
                            textureCoords.Add(new Vector2(1.0f, 0.0f));

                            triangles.Add(startIndex);
                            triangles.Add(startIndex + 3);
                            triangles.Add(startIndex + 2);
                            triangles.Add(startIndex);
                            triangles.Add(startIndex + 1);
                            triangles.Add(startIndex + 3);

                            for (int i = 0; i < 4; ++i) {
                                tangents.Add(new Vector4(0.0f, 1.0f, 0.0f, 1.0f));
                                normals.Add(new Vector3(1, 0, 0));
                                colors.Add(renderedChunk.blocks[x, y, z].color);
                            }
                        }

                        if (IsFree(new Vector3Int(x - 1, y, z))) {
                            int startIndex = vertices.Count;

                            vertices.Add(new Vector3(x, y,     z));
                            vertices.Add(new Vector3(x, y + 1, z));
                            vertices.Add(new Vector3(x, y,     z + 1));
                            vertices.Add(new Vector3(x, y + 1, z + 1));

                            textureCoords.Add(new Vector2(0.0f, 0.0f));
                            textureCoords.Add(new Vector2(1.0f, 0.0f));
                            textureCoords.Add(new Vector2(0.0f, 1.0f));
                            textureCoords.Add(new Vector2(1.0f, 1.0f));

                            triangles.Add(startIndex + 2);
                            triangles.Add(startIndex + 1);
                            triangles.Add(startIndex);
                            triangles.Add(startIndex + 2);
                            triangles.Add(startIndex + 3);
                            triangles.Add(startIndex + 1);

                            for (int i = 0; i < 4; ++i) {
                                tangents.Add(new Vector4(0.0f, 1.0f, 0.0f, 1.0f));
                                normals.Add(new Vector3(-1, 0, 0));
                                colors.Add(renderedChunk.blocks[x, y, z].color);
                            }
                        }

                        if (IsFree(new Vector3Int(x, y + 1, z))) {
                            int startIndex = vertices.Count;

                            vertices.Add(new Vector3(x,     y + 1, z));
                            vertices.Add(new Vector3(x + 1, y + 1, z));
                            vertices.Add(new Vector3(x,     y + 1, z + 1));
                            vertices.Add(new Vector3(x + 1, y + 1, z + 1));

                            textureCoords.Add(new Vector2(0.0f, 0.0f));
                            textureCoords.Add(new Vector2(1.0f, 0.0f));
                            textureCoords.Add(new Vector2(0.0f, 1.0f));
                            textureCoords.Add(new Vector2(1.0f, 1.0f));

                            triangles.Add(startIndex + 2);
                            triangles.Add(startIndex + 1);
                            triangles.Add(startIndex);
                            triangles.Add(startIndex + 2);
                            triangles.Add(startIndex + 3);
                            triangles.Add(startIndex + 1);

                            for (int i = 0; i < 4; ++i) {
                                tangents.Add(new Vector4(1.0f, 0.0f, 0.0f, 1.0f));
                                normals.Add(new Vector3(0, 1, 0));
                                colors.Add(renderedChunk.blocks[x, y, z].color);
                            }
                        }

                        if (IsFree(new Vector3Int(x, y - 1, z))) {
                            int startIndex = vertices.Count;

                            vertices.Add(new Vector3(x,     y, z));
                            vertices.Add(new Vector3(x + 1, y, z));
                            vertices.Add(new Vector3(x,     y, z + 1));
                            vertices.Add(new Vector3(x + 1, y, z + 1));

                            textureCoords.Add(new Vector2(0.0f, 1.0f));
                            textureCoords.Add(new Vector2(1.0f, 1.0f));
                            textureCoords.Add(new Vector2(0.0f, 0.0f));
                            textureCoords.Add(new Vector2(1.0f, 0.0f));

                            triangles.Add(startIndex + 1);
                            triangles.Add(startIndex + 2);
                            triangles.Add(startIndex);
                            triangles.Add(startIndex + 1);
                            triangles.Add(startIndex + 3);
                            triangles.Add(startIndex + 2);

                            for (int i = 0; i < 4; ++i) {
                                tangents.Add(new Vector4(1.0f, 0.0f, 0.0f, 1.0f));
                                normals.Add(new Vector3(0, -1, 0));
                                colors.Add(renderedChunk.blocks[x, y, z].color);
                            }
                        }
                    }
                }
            }
        }

        mesh.SetVertices(vertices);
        mesh.SetColors(colors);
        mesh.SetNormals(normals);
        mesh.SetUVs(0, textureCoords);
        mesh.SetTangents(tangents);
        mesh.SetTriangles(triangles, 0);

        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh;

        dirty = false;
    }

    private bool IsFree(Vector3Int pos) {
        if (pos.x < 0 || pos.x >= Chunk.ChunkSize ||
            pos.y < 0 || pos.y >= Chunk.ChunkSize ||
            pos.z < 0 || pos.z >= Chunk.ChunkSize ||
            renderedChunk.blocks[pos.x, pos.y, pos.z].type == Block.Type.Air) {
            return true;
        }
        return false;
    }

    public void SetChunk(Chunk chunk) {
        renderedChunk = chunk;
        dirty = true;
    }
}

public class Chunk {
    public static int ChunkSize = 16;

    public Block[,,] blocks;

    public Chunk() {
        blocks = new Block[ChunkSize, ChunkSize, ChunkSize];

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
    }
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