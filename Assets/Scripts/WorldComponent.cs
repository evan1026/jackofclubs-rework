using System;
using System.Collections;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using UnityEngine;

public class WorldComponent : MonoBehaviour {

    public GameObject ChunkPrefab;
    public Vector3Int WorldSize;

    public GameObject Player;
    public ChunkGenerator.GeneratorType GeneratorType;

    public World world;

    private ChunkGenerator chunkGenerator;

    private Dictionary<Vector3Int, ChunkComponent> chunks;
    private FrameTimer frameTimer;

    private List<Vector3Int> updateDirections;

    private int chunksNeeded;
    private int chunksFinished;

    // Start is called before the first frame update
    void Start() {
        Time.timeScale = 0;

        chunkGenerator = ChunkGenerator.Get(GeneratorType);

        chunks = new Dictionary<Vector3Int, ChunkComponent>();
        world = new World(WorldSize);
        frameTimer = FindObjectOfType<FrameTimer>();
        updateDirections = new List<Vector3Int>();

        updateDirections.Add(new Vector3Int( 1,  1,  1));
        updateDirections.Add(new Vector3Int( 0,  1,  1));
        updateDirections.Add(new Vector3Int(-1,  1,  1));
        updateDirections.Add(new Vector3Int( 1,  0,  1));
        updateDirections.Add(new Vector3Int( 0,  0,  1));
        updateDirections.Add(new Vector3Int(-1,  0,  1));
        updateDirections.Add(new Vector3Int( 1, -1,  1));
        updateDirections.Add(new Vector3Int( 0, -1,  1));
        updateDirections.Add(new Vector3Int(-1, -1,  1));
        updateDirections.Add(new Vector3Int( 1,  1,  0));
        updateDirections.Add(new Vector3Int( 0,  1,  0));
        updateDirections.Add(new Vector3Int(-1,  1,  0));
        updateDirections.Add(new Vector3Int( 1,  0,  0));
        updateDirections.Add(new Vector3Int(-1,  0,  0));
        updateDirections.Add(new Vector3Int( 1, -1,  0));
        updateDirections.Add(new Vector3Int( 0, -1,  0));
        updateDirections.Add(new Vector3Int(-1, -1,  0));
        updateDirections.Add(new Vector3Int( 1,  1, -1));
        updateDirections.Add(new Vector3Int( 0,  1, -1));
        updateDirections.Add(new Vector3Int(-1,  1, -1));
        updateDirections.Add(new Vector3Int( 1,  0, -1));
        updateDirections.Add(new Vector3Int( 0,  0, -1));
        updateDirections.Add(new Vector3Int(-1,  0, -1));
        updateDirections.Add(new Vector3Int( 1, -1, -1));
        updateDirections.Add(new Vector3Int( 0, -1, -1));
        updateDirections.Add(new Vector3Int(-1, -1, -1));

        chunksNeeded = world.chunks.Count;

        foreach (var entry in world.chunks) {
            Vector3Int pos = entry.Key;
            Chunk chunk = entry.Value;

            Vector3 worldPos = new Vector3(pos.x * Chunk.ChunkSize, pos.y * Chunk.ChunkSize, pos.z * Chunk.ChunkSize);
            GameObject chunkComponentObject = Instantiate(ChunkPrefab, worldPos, Quaternion.identity, transform);
            ChunkComponent chunkComponent = chunkComponentObject.GetComponent<ChunkComponent>();
            chunks.Add(pos, chunkComponent);

            chunkGenerator.GenerateChunk(chunk, pos, (chunk) => ChunkGenerated(chunk, chunkComponent));
        }
    }

    // Update is called once per frame
    void Update() {
        if (chunksFinished == chunksNeeded) {
            Time.timeScale = 1;

            Queue<Vector3Int> chunksToUpdate = new Queue<Vector3Int>();
            HashSet<Vector3Int> updatedChunks = new HashSet<Vector3Int>();

            Vector3 playerPos = Player.transform.position;
            Vector3Int integerPlayerPos = new Vector3Int((int)playerPos.x, (int)playerPos.y, (int)playerPos.z);

            Vector3Int playerChunkPos = world.GetNearestChunkPos(integerPlayerPos);

            chunksToUpdate.Enqueue(playerChunkPos);

            while (chunksToUpdate.Count > 0 && frameTimer.FrameHasTime) {
                Vector3Int chunkPos = chunksToUpdate.Dequeue();
                ChunkComponent chunkComp = chunks[chunkPos];
                chunkComp.UpdateMesh();

                foreach (Vector3Int direction in updateDirections) {
                    Vector3Int newChunkPos = chunkPos + direction;
                    if (chunks.ContainsKey(newChunkPos) && !updatedChunks.Contains(newChunkPos)) {
                        chunksToUpdate.Enqueue(newChunkPos);
                        updatedChunks.Add(newChunkPos);
                    }
                }
            }
        }
    }

    private void ChunkGenerated(Chunk chunk, ChunkComponent chunkComponent) {
        chunkComponent.SetChunk(chunk);
        Interlocked.Increment(ref chunksFinished);
    }
}

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

public abstract class ChunkGenerator {

    public enum GeneratorType {
        Solid,
        Alternating,
        Empty,
        Perlin
    }

    public delegate void ChunkGenerationCallback(Chunk chunk);

    public void GenerateChunk(Chunk chunk, Vector3Int chunkPos, ChunkGenerationCallback callback) {
        Task.Run(() => {
            try {
                GenerateChunkAsync(chunk, chunkPos, callback);
            } catch (Exception e) {
                Debug.LogError(e);
            }
        });
    }

    private void GenerateChunkAsync(Chunk chunk, Vector3Int chunkPos, ChunkGenerationCallback callback) {

        for (int x = 0; x < Chunk.ChunkSize; ++x) {
            for (int y = 0; y < Chunk.ChunkSize; ++y) {
                for (int z = 0; z < Chunk.ChunkSize; ++z) {
                    chunk.SetBlock(new Vector3Int(x, y, z), GetBlock(chunkPos * Chunk.ChunkSize + new Vector3Int(x, y, z)));
                }
            }
        }

        callback(chunk);
    }

    protected abstract Block GetBlock(Vector3Int pos);

    public static ChunkGenerator Get(GeneratorType type) {
        switch (type) {
            case GeneratorType.Solid:
                return new SolidChunkGenerator();
            case GeneratorType.Alternating:
                return new AlternatingChunkGenerator();
            case GeneratorType.Perlin:
                return new PerlinNoiseGenerator();
            case GeneratorType.Empty:
            default:
                return new EmptyChunkGenerator();
        }
    }
}

public class SolidChunkGenerator : ChunkGenerator {
    protected override Block GetBlock(Vector3Int pos) {
        Color color = new Color(Mod(pos.x, Chunk.ChunkSize) / (float)Chunk.ChunkSize,
                                Mod(pos.y, Chunk.ChunkSize) / (float)Chunk.ChunkSize,
                                Mod(pos.z, Chunk.ChunkSize) / (float)Chunk.ChunkSize);

        return new Block(color);
    }

    private int Mod(int x, int m) {
        return (x % m + m) % m;
    }
}

public class AlternatingChunkGenerator : ChunkGenerator {
    protected override Block GetBlock(Vector3Int pos) {
        if ((pos.x + pos.y + pos.z) % 2 == 0) {
            Color color = new Color(Mod(pos.x, Chunk.ChunkSize) / (float)Chunk.ChunkSize,
                                    Mod(pos.y, Chunk.ChunkSize) / (float)Chunk.ChunkSize,
                                    Mod(pos.z, Chunk.ChunkSize) / (float)Chunk.ChunkSize);

            return new Block(color);
        } else {
            return new Block();
        }
    }

    private int Mod(int x, int m) {
        return (x % m + m) % m;
    }
}

public class EmptyChunkGenerator : ChunkGenerator {
    protected override Block GetBlock(Vector3Int pos) {
        return new Block();
    }
}

public class PerlinNoiseGenerator : ChunkGenerator {
    private static float yScale = 20;
    private static float yOffset = 16;
    private static float coordScalar = 0.02f;
    private static float xOffset = UnityEngine.Random.Range(-10000, 10000);
    private static float zOffset = UnityEngine.Random.Range(-10000, 10000);

    protected override Block GetBlock(Vector3Int pos) {
        if (Mathf.PerlinNoise(pos.x * coordScalar + xOffset, pos.z * coordScalar + zOffset) * yScale + yOffset >= pos.y) {
            return new Block(new Color(.8f, .8f, .8f));
        } else {
            return new Block();
        }
    }
}