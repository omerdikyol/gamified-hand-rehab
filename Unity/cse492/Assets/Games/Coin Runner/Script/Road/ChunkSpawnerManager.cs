using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkSpawnerManager : MonoBehaviour
{
    [SerializeField] GameObject[] chunks;
    [SerializeField] private float chunkLength = 54f;
    [SerializeField] private Transform playerTransform;
    [Header("Pool Config")]
    [SerializeField] private int size = 10;

    private List<Queue<GameObject>> chunksQueueList = new List<Queue<GameObject>>();
    private List<GameObject> activeChunks = new List<GameObject>();
    [SerializeField] private float spawnZ = 10f;
    [SerializeField] private float chunkSpeed = 0f;
    public Vector3 _spawnPos = new Vector3(0f, 0f, 50f);
    private void Start()
    {
        PoolChunks();
        for (int i = 0; i < 7; i++)
        {
            SpawnRandomChunkStart();
        }
    }
    private void Update()
    {
        for (int i = 0; i < activeChunks.Count; i++)
        {
            if (activeChunks[i].transform.position.z + chunkLength < 0f)
            {
                SpawnRandomChunk();
                ReturnToPool(activeChunks[i]);
                activeChunks.RemoveAt(i);
                i--;
            }
        }
    }

    private void FixedUpdate()
    {
        foreach (var chunk in activeChunks)
        {
            Rigidbody rb = chunk.GetComponent<Rigidbody>();
            rb.MovePosition(chunk.transform.position + Vector3.back * chunkSpeed * Time.fixedDeltaTime);
        }
    }

    private void PoolChunks()
    {
        for (int i = 0; i < chunks.Length; i++)
        {
            Queue<GameObject> newPool = new Queue<GameObject>();
            for (int j = 0; j < size; j++)
            {
                GameObject newObj = Instantiate(chunks[i].gameObject);
                newObj.transform.SetParent(transform);
                newObj.gameObject.SetActive(false);
                newPool.Enqueue(newObj);
            }
            chunksQueueList.Add(newPool);
        }
    }
    private void SpawnRandomChunkStart()
    {
        GameObject newChunk = chunksQueueList[Random.Range(0, chunksQueueList.Count)].Dequeue();
        newChunk.transform.position = Vector3.forward * spawnZ;
        newChunk.gameObject.SetActive(true);
        spawnZ += chunkLength;
        activeChunks.Add(newChunk);
    }
    private void SpawnRandomChunk()
    {
        GameObject newChunk = chunksQueueList[Random.Range(0, chunksQueueList.Count)].Dequeue();
        newChunk.transform.position = activeChunks[6].transform.position + _spawnPos;
        newChunk.gameObject.SetActive(true);
        activeChunks.Add(newChunk);
    }

    private void ReturnToPool(GameObject chunk)
    {
        chunk.SetActive(false);
        chunksQueueList[Random.Range(0, chunksQueueList.Count)].Enqueue(chunk);
    }

    public void ChangeRunSpeed(float newSpeed)
    {
        chunkSpeed = newSpeed;
    }
}
