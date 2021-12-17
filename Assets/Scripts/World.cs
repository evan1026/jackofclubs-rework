using System.Collections.Generic;
using UnityEngine;
using System;

public class World {
    public Dictionary<Vector3Int, Chunk> chunks;

    private Vector3Int worldMin;
    private Vector3Int worldMax;

    public World(Vector3Int size) {
        chunks = new Dictionary<Vector3Int, Chunk>();
        worldMin = new Vector3Int(-size.x / 2 * Chunk.ChunkSize, 0, -size.z / 2 * Chunk.ChunkSize);
        worldMax = new Vector3Int(size.x / 2 * Chunk.ChunkSize + 15, size.y * Chunk.ChunkSize - 1, size.z / 2 * Chunk.ChunkSize + 15);

        for (int x = -size.x / 2; x < size.x / 2; ++x) {
            for (int y = 0; y < size.y; ++y) {
                for (int z = -size.z / 2; z < size.z / 2; ++z) {
                    chunks.Add(new Vector3Int(x, y, z), new Chunk(new Vector3Int(x, y, z), this));
                }
            }
        }
    }

    public Vector3Int GetPosInChunk(Vector3Int worldPos) {
        Vector3Int inChunkPos = new Vector3Int(worldPos.x % Chunk.ChunkSize, worldPos.y % Chunk.ChunkSize, worldPos.z % Chunk.ChunkSize);

        // Have to correct because mod can give negative values
        if (inChunkPos.x < 0) {
            inChunkPos.x += Chunk.ChunkSize;
        }
        if (inChunkPos.y < 0) {
            inChunkPos.y += Chunk.ChunkSize;
        }
        if (inChunkPos.z < 0) {
            inChunkPos.z += Chunk.ChunkSize;
        }

        return inChunkPos;
    }

    public Vector3Int GetChunkPos(Vector3Int worldPos) {
        Vector3Int inChunkPos = GetPosInChunk(worldPos);

        Vector3Int chunkPos = worldPos - inChunkPos;
        chunkPos /= Chunk.ChunkSize;

        return chunkPos;
    }

    public Vector3Int GetNearestBlockPos(Vector3Int worldPos) {
        worldPos.Clamp(worldMin, worldMax);
        return worldPos;
    }

    public Vector3Int GetNearestChunkPos(Vector3Int worldPos) {
        Vector3Int blockPos = GetNearestBlockPos(worldPos);
        return GetChunkPos(blockPos);
    }

    public Block GetBlock(Vector3Int pos) {
        Vector3Int inChunkPos = GetPosInChunk(pos);
        Vector3Int chunkPos = GetChunkPos(pos);

        if (chunks.ContainsKey(chunkPos)) {
            Chunk chunk = chunks[chunkPos];
            return chunk?.GetBlock(inChunkPos);
        } else {
            return null;
        }
    }

    public void SetBlock(Vector3Int pos, Block block) {
        Vector3Int inChunkPos = GetPosInChunk(pos);

        Vector3Int chunkPos = pos - inChunkPos;
        chunkPos /= 16;

        if (chunks.ContainsKey(chunkPos)) {
            Chunk chunk = chunks[chunkPos];
            chunk?.SetBlock(inChunkPos, block);
        }
    }

    public bool IsFree(Vector3Int pos) {
        Block requestedBlock = GetBlock(pos);
        return requestedBlock == null || requestedBlock.type == Block.Type.Air;
    }
}

public class Chunk {
    public static int ChunkSize = 16;

    public ChunkMeshData meshData;

    private Block[,,] blocks;
    private bool dirty = false;

    private Vector3Int pos;
    private readonly World world;

    public Chunk(Vector3Int pos, World world) {
        blocks = new Block[ChunkSize, ChunkSize, ChunkSize];
        meshData = new ChunkMeshData();

        this.pos = pos;
        this.world = world;
    }

    public Block GetBlock(Vector3Int pos) {
        if (pos.x < 0 || pos.x >= Chunk.ChunkSize ||
            pos.y < 0 || pos.y >= Chunk.ChunkSize ||
            pos.z < 0 || pos.z >= Chunk.ChunkSize) {
            return null;
        }
        return blocks[pos.x, pos.y, pos.z];
    }

    public void SetBlock(Vector3Int pos, Block block) {
        if (pos.x < 0 || pos.x >= Chunk.ChunkSize ||
            pos.y < 0 || pos.y >= Chunk.ChunkSize ||
            pos.z < 0 || pos.z >= Chunk.ChunkSize) {
            throw new IndexOutOfRangeException(pos + " is not a valid block index");
        }

        if (block == null) {
            block = new Block();
        }

        blocks[pos.x, pos.y, pos.z] = block;
        dirty = true;
    }

    private bool IsFree(Vector3Int pos) {
        Block requestedBlock = GetBlock(pos);

        if (requestedBlock == null) {
            Vector3Int worldPos = pos + (this.pos * ChunkSize);
            requestedBlock = world.GetBlock(worldPos);
        }

        return requestedBlock == null || requestedBlock.type == Block.Type.Air;
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

    public Block() : this(Type.Air, new Color()) { }

    public Block(Type type) : this(type, new Color()) { }

    public Block(Color color) : this(Type.Solid, color) { }

    public Block(Type type, Color color) {
        this.type = type;
        this.color = color;
    }
}