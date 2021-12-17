using System;
using System.Threading.Tasks;
using UnityEngine;

public abstract class ChunkGenerator {

    public enum GeneratorType {
        Solid,
        Alternating,
        Empty,
        Perlin
    }

    public delegate void ChunkGenerationCallback(Chunk chunk);

    public void GenerateChunkAsync(Chunk chunk, Vector3Int chunkPos, ChunkGenerationCallback callback) {
        Task.Run(() => {
            try {
                GenerateChunk(chunk, chunkPos, callback);
            } catch (Exception e) {
                Debug.LogError(e);
            }
        });
    }

    private void GenerateChunk(Chunk chunk, Vector3Int chunkPos, ChunkGenerationCallback callback) {

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
        return type switch {
            GeneratorType.Solid => new SolidChunkGenerator(),
            GeneratorType.Alternating => new AlternatingChunkGenerator(),
            GeneratorType.Perlin => new PerlinNoiseGenerator(),
            _ => new EmptyChunkGenerator(),
        };
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
    private static readonly float yScale = 20;
    private static readonly float yOffset = 16;
    private static readonly float coordScalar = 0.02f;
    private static readonly float xOffset = UnityEngine.Random.Range(-10000, 10000);
    private static readonly float zOffset = UnityEngine.Random.Range(-10000, 10000);

    protected override Block GetBlock(Vector3Int pos) {
        float perlinHeight = GetPerlinValue(pos);
        if (perlinHeight >= pos.y) {
            Color color;

            if (perlinHeight - pos.y > 1) {
                color = new Color(.545f, .271f, .075f);
            } else {
                color = new Color(.2f, .804f, .2f);
            }

            return new Block(color);
        } else {
            return new Block();
        }
    }

    private float GetPerlinValue(Vector3Int pos) {
        return Mathf.PerlinNoise(pos.x * coordScalar + xOffset, pos.z * coordScalar + zOffset) * yScale + yOffset;
    }
}
