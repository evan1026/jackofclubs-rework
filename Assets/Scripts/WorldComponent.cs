using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldComponent : MonoBehaviour {

    public GameObject ChunkPrefab;
    public Vector3Int WorldSize;

    private World world;

    // Start is called before the first frame update
    void Start() {
        Time.timeScale = 0;
        world = new World(WorldSize);

        foreach (var entry in world.chunks) {
            Vector3Int pos = entry.Key;
            Chunk chunk = entry.Value;

            Vector3 worldPos = new Vector3(pos.x * Chunk.ChunkSize, pos.y * Chunk.ChunkSize, pos.z * Chunk.ChunkSize);
            GameObject chunkComponentObject = Instantiate(ChunkPrefab, worldPos, Quaternion.identity, transform);
            ChunkComponent chunkComponent = chunkComponentObject.GetComponent<ChunkComponent>();
            chunkComponent.SetChunk(chunk);
        }
        Time.timeScale = 1;
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
                    Chunk newChunk = new Chunk();
                    chunks.Add(new Vector3Int(x, y, z), newChunk);
                }
            }
        }
    }
}