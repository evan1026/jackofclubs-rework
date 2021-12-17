using System.Threading;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class WorldComponent : MonoBehaviour {

    public Vector3Int WorldSize;
    public ChunkGenerator.GeneratorType GeneratorType;

    public GameObject ChunkPrefab;
    public GameObject Player;
    public World world;

    public UnityEvent<float> GenerationProgressEvent;
    public UnityEvent GenerationFinishedEvent;

    private ChunkGenerator chunkGenerator;

    private Dictionary<Vector3Int, ChunkComponent> chunks;
    private FrameTimer frameTimer;

    private List<Vector3Int> updateDirections;

    private int chunksNeeded;
    private int chunksFinished;
    private bool finishedEventFired = false;

    // Start is called before the first frame update
    public void Start() {
        Time.timeScale = 0;

        chunkGenerator = ChunkGenerator.Get(GeneratorType);

        chunks = new Dictionary<Vector3Int, ChunkComponent>();
        world = new World(WorldSize);
        frameTimer = FindObjectOfType<FrameTimer>();
        updateDirections = new List<Vector3Int> {
            new Vector3Int(1, 1, 1),
            new Vector3Int(0, 1, 1),
            new Vector3Int(-1, 1, 1),
            new Vector3Int(1, 0, 1),
            new Vector3Int(0, 0, 1),
            new Vector3Int(-1, 0, 1),
            new Vector3Int(1, -1, 1),
            new Vector3Int(0, -1, 1),
            new Vector3Int(-1, -1, 1),
            new Vector3Int(1, 1, 0),
            new Vector3Int(0, 1, 0),
            new Vector3Int(-1, 1, 0),
            new Vector3Int(1, 0, 0),
            new Vector3Int(-1, 0, 0),
            new Vector3Int(1, -1, 0),
            new Vector3Int(0, -1, 0),
            new Vector3Int(-1, -1, 0),
            new Vector3Int(1, 1, -1),
            new Vector3Int(0, 1, -1),
            new Vector3Int(-1, 1, -1),
            new Vector3Int(1, 0, -1),
            new Vector3Int(0, 0, -1),
            new Vector3Int(-1, 0, -1),
            new Vector3Int(1, -1, -1),
            new Vector3Int(0, -1, -1),
            new Vector3Int(-1, -1, -1)
        };

        chunksNeeded = world.chunks.Count;

        foreach (var entry in world.chunks) {
            Vector3Int pos = entry.Key;
            Chunk chunk = entry.Value;

            Vector3 worldPos = new Vector3(pos.x * Chunk.ChunkSize, pos.y * Chunk.ChunkSize, pos.z * Chunk.ChunkSize);
            GameObject chunkComponentObject = Instantiate(ChunkPrefab, worldPos, Quaternion.identity, transform);
            ChunkComponent chunkComponent = chunkComponentObject.GetComponent<ChunkComponent>();
            chunks.Add(pos, chunkComponent);

            chunkGenerator.GenerateChunkAsync(chunk, pos, (chunk) => ChunkGenerated(chunk, chunkComponent));
        }
    }

    // Update is called once per frame
    public void Update() {
        if (chunksFinished == chunksNeeded) {
            if (!finishedEventFired) {
                GenerationFinishedEvent.Invoke();
                finishedEventFired = true;
            }

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
        } else {
            GenerationProgressEvent.Invoke(GetPercentGenerated());
        }
    }

    private void ChunkGenerated(Chunk chunk, ChunkComponent chunkComponent) {
        chunkComponent.SetChunk(chunk);
        Interlocked.Increment(ref chunksFinished);
    }

    public float GetPercentGenerated() {
        return (float)chunksFinished / chunksNeeded;
    }
}