using System;
using System.Collections;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using UnityEngine;

public class WorldComponent : MonoBehaviour {

    public GameObject ChunkPrefab;
    public Vector3Int WorldSize;

    public int DeadBlocksPerFrame = 0;

    private World world;
    private ChunkGenerator chunkGenerator;

    // Start is called before the first frame update
    void Start() {
        Time.timeScale = 0;

        chunkGenerator = new ChunkGenerator();
        world = new World(WorldSize);

        foreach (var entry in world.chunks) {
            Vector3Int pos = entry.Key;
            Chunk chunk = entry.Value;

            Vector3 worldPos = new Vector3(pos.x * Chunk.ChunkSize, pos.y * Chunk.ChunkSize, pos.z * Chunk.ChunkSize);
            GameObject chunkComponentObject = Instantiate(ChunkPrefab, worldPos, Quaternion.identity, transform);
            ChunkComponent chunkComponent = chunkComponentObject.GetComponent<ChunkComponent>();

            chunkGenerator.GenerateChunk(chunk, chunkComponent.SetChunk);
        }

        //Time.timeScale = 1;
    }

    // Update is called once per frame
    void Update() {

    }
}

public class World {
    public Dictionary<Vector3Int, Chunk> chunks;

    public World(Vector3Int size) {
        chunks = new Dictionary<Vector3Int, Chunk>();

        for (int x = -size.x / 2; x < size.x / 2; ++x) {
            for (int y = 0; y < size.y; ++y) {
                for (int z = -size.z / 2; z < size.z / 2; ++z) {
                    chunks.Add(new Vector3Int(x, y, z), new Chunk(new Vector3Int(x, y, z), this));
                }
            }
        }
    }

    private Vector3Int GetPosInChunk(Vector3Int worldPos) {
        Vector3Int inChunkPos = new Vector3Int(worldPos.x % 16, worldPos.y % 16, worldPos.z % 16);

        // Have to correct because mod can give negative values
        if (inChunkPos.x < 0) {
            inChunkPos.x += 16;
        }
        if (inChunkPos.y < 0) {
            inChunkPos.y += 16;
        }
        if (inChunkPos.z < 0) {
            inChunkPos.z += 16;
        }

        return inChunkPos;
    }

    public Block GetBlock(Vector3Int pos) {
        Vector3Int inChunkPos = GetPosInChunk(pos);

        Vector3Int chunkPos = pos - inChunkPos;
        chunkPos /= 16;

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

public class ChunkGenerator {

    public delegate void ChunkGenerationCallback(Chunk chunk);

    public void GenerateChunk(Chunk chunk, ChunkGenerationCallback callback) {
        Task.Run(() => {
            try {
                GenerateChunkAsync(chunk, callback);
            } catch (Exception e) {
                Debug.LogError(e);
            }
        });
    }

    private void GenerateChunkAsync(Chunk chunk, ChunkGenerationCallback callback) {

        for (int x = 0; x < Chunk.ChunkSize; ++x) {
            for (int y = 0; y < Chunk.ChunkSize; ++y) {
                for (int z = 0; z < Chunk.ChunkSize; ++z) {
                    chunk.SetBlock(new Vector3Int(x, y, z), new Block(new Color(x / 16f, y / 16f, z / 16f)));
                }
            }
        }

        callback(chunk);
    }
}