using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Component which is responsible of storing and managing all other components used by the player character
/// </summary>
public class PlayerComponentsManager : NetworkBehaviour
{
    public PlayerController playerController { get; private set; }
    public CameraController cameraController { get; private set; }
    public Rigidbody rb { get; private set; }
    public InventoryManager inventoryManager { get; private set; }

    public PlayerHealth healthManager { get; private set; }
    public NetworkObject networkObject { get; private set; }

    /// <summary>
    /// The collider which is responsible for the player's collision with other objects
    /// </summary>
    public Collider terrainCollider { get; private set; }
    /// <summary>
    /// The collider which allows the player to take damage
    /// </summary>
    public Collider damageCollider { get; private set; }

    public Renderer rend { get; private set; }
    
    void Awake()
    {
        playerController = GetComponent<PlayerController>();
        if (playerController == null) Debug.LogError("Could not find a PlayerController component inside the player character");

        cameraController = transform.Find("Main Camera").GetComponent<CameraController>();
        if (cameraController == null) Debug.LogError("Could not find a CameraController component inside the player character");

        rb = GetComponent<Rigidbody>();
        if (rb == null) Debug.LogError("Could not find a Rigidbody component inside the player character");

        inventoryManager = GetComponentInChildren<InventoryManager>();
        if (inventoryManager == null) Debug.LogError("Could not find a InventoryManager component inside the player character");

        healthManager = GetComponent<PlayerHealth>();
        if (healthManager == null) Debug.LogError("Could not find a PlayerHealth component inside the player character");

        networkObject = GetComponent<NetworkObject>();
        if (networkObject == null) Debug.LogError("Could not find a NetworkObject component inside the player character. The component must be present inside the main parent of the player character");

        //Temporary solution. In the final product, the player model will contain multiple colliders for each part of the body.
        damageCollider = GetComponent<Collider>();
        if (damageCollider == null) Debug.LogError("Could not find a Collider component inside the player character.");

        rend = GetComponent<Renderer>();
        if (rend == null) Debug.LogError("Could not find a Renderer component inside the player character.");
    }

    public override void OnNetworkSpawn()
    {
        if (!GetComponent<NetworkObject>().IsOwner)
            return;
        
        
    }
}
