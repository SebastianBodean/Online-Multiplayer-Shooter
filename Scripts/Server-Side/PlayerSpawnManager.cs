using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.Netcode;
using UnityEditor.PackageManager;
using UnityEngine;

/// <summary>
/// This component handles the initialisation of the player
/// </summary>
public class PlayerSpawnManager : NetworkBehaviour
{
    public static PlayerSpawnManager Singleton { get; private set; }

    /// <summary>
    /// The base max health that all players will have
    /// </summary>
    [SerializeField] private int playerMaxHealth;

    [SerializeField] private float respawnTime;

    private List<Vector3> spawnPoints = new();

    private void Awake()
    {
        if (Singleton != null)
        {
            Destroy(this);
            return;
        }
        Singleton = this;

        //If no spawn points were defined, spawn the players at the origin
        if (spawnPoints.Count == 0)
            spawnPoints.Add(Vector3.zero);
    }

    //Use this to determine the order of execution when a player connects to the server

    /*NetworkManager.Singleton.OnClientStarted += () => { print("OnClientStarted"); };
    NetworkManager.Singleton.OnClientConnectedCallback += (ulong clientID) => { print("OnClientConnected"); };
    NetworkManager.Singleton.OnServerStarted += () => { print("OnServerStarted"); };*/


    private void Start()
    {
        NetworkManager.OnServerStarted += (() => OnConnect());
    }

    private void OnConnect()
    {
        print("Starting server");
        NetworkManager.OnClientConnectedCallback += ((ulong clientID) =>
        {
            print("A client has connected to the server");
            PlayerComponentsManager pcm = NetworkManager.ConnectedClients[clientID].PlayerObject.GetComponent<PlayerComponentsManager>();
            

            //Initialising the player's health
            pcm.healthManager.maxHealth.Value = playerMaxHealth;

            //Initialising the player's inventory
            pcm.inventoryManager.selectedSlot.OnValueChanged += ((previousValue, newValue) =>
            {
                pcm.inventoryManager.GetItem(previousValue).gameObject.SetActive(false);
                pcm.inventoryManager.GetItem(newValue).gameObject.SetActive(true);
            });

            ItemDatabase.Singleton.InitialiseInventoryServerRpc(clientID);

            //Spawn the player
            Respawn(clientID);
        });
    }

    /// <summary>
    /// Request to respawn the player
    /// </summary>
    public void RequestRespawn(PlayerComponentsManager pcm)
    {
        ulong clientID = pcm.networkObject.OwnerClientId;
        StartCoroutine(QueueRespawn(clientID));
    }

    /// <summary>
    /// Respawns the player at a random location and resets their stats
    /// </summary>
    /// <param name="clientID"></param>
    private void Respawn(ulong clientID)
    {
        PlayerComponentsManager pcm = NetworkManager.ConnectedClients[clientID].PlayerObject.GetComponent<PlayerComponentsManager>();

        Debug.Log(clientID,pcm); 
        pcm.transform.position = spawnPoints[Random.Range(0, spawnPoints.Count)];
        pcm.transform.gameObject.SetActive(true);

        pcm.healthManager.health.Value = pcm.healthManager.maxHealth.Value;

        //  TODO: Inventory is assigned here
    }

    private IEnumerator QueueRespawn(ulong clientID)
    {
        yield return new WaitForSeconds(respawnTime);
        Respawn(clientID);
    }

    /// <summary>
    /// 
    /// </summary>
    public void SoftDespawn()
    {

    }
}
