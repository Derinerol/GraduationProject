
using UnityEngine;
using UnityEngine.AI;

public class SceneSpawnPoint : MonoBehaviour
{
    [SerializeField] private string spawnID;
    [SerializeField] private GameObject player;

    private void Start()
    {
        if (SceneSpawnManager.PendingSpawnID != spawnID)
            return;

        if (player == null)
        {
            Debug.LogError("SceneSpawnPoint: Player is not assigned.");
            return;
        }

        NavMeshAgent agent = player.GetComponent<NavMeshAgent>();

        if (agent != null)
        {
            agent.Warp(transform.position);
        }
        else
        {
            player.transform.position = transform.position;
        }

        player.transform.rotation = transform.rotation;

        SceneSpawnManager.ClearPendingSpawn();
    }
}
