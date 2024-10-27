using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// This class is responsible for taking the clients' movement inputs and moving them based on their inputs
/// </summary>
public class NetworkPlayerManager : NetworkBehaviour
{
    public static NetworkPlayerManager Singleton {  get; private set; }

    private void Awake()
    {
        if (Singleton != null)
        {
            Destroy(this);
            return;
        }

        Singleton = this;
    }

    // Update is called once per frame
    void Update()
    {
        if (!NetworkManager.IsServer)
            return;

        ApplyGravityServerRpc();
    }

    /// <summary>
    /// Returns the PlayerComponentsManager of the client that invoked this method
    /// </summary>
    /// <param name="serverRpcParams"></param>
    /// <returns></returns>
    private PlayerComponentsManager GetPlayerComponents(ServerRpcParams serverRpcParams)
    {
        ulong senderID = serverRpcParams.Receive.SenderClientId;
        return NetworkManager.ConnectedClients[senderID].PlayerObject.GetComponent<PlayerComponentsManager>();
    }

    /// <summary>
    /// Goes through each client and applies a downward force to their player character
    /// </summary>
    [ServerRpc]
    private void ApplyGravityServerRpc()
    {
        IReadOnlyList<ulong> clients = NetworkManager.ConnectedClientsIds;
        for (int i = 0; i < clients.Count; i++)
        {
            NetworkObject player = NetworkManager.ConnectedClients[clients[i]].PlayerObject;
            player.GetComponent<PlayerComponentsManager>().rb.AddForce(Vector3.down * Mathf.Pow(3f, 2));
        }
    }

    /// <summary>
    /// Tells the server to move the player according to their inputs
    /// </summary>
    /// <param name="horizontal">The horizontal input axis</param>
    /// <param name="vertical">The vertical input axis</param>
    /// <param name="serverRpcParams"></param>
    [ServerRpc(RequireOwnership = false)] 
    public void MovePlayerServerRpc(float horizontal, float vertical, ServerRpcParams serverRpcParams = default)
    {
        //Validate the client's outputs
        if (Mathf.Abs(horizontal) > 1f || Mathf.Abs(vertical) > 1f)
        {
            Debug.LogError("One of the clients is sending invalid inputs");
            return;
        }

        PlayerComponentsManager pcm = GetPlayerComponents(serverRpcParams);
        Transform mainCamera = pcm.cameraController.transform;
        PlayerController pc = pcm.playerController;
        Rigidbody rb = pcm.rb;

        Vector3 camForward = new Vector3(mainCamera.forward.x, 0, mainCamera.forward.z).normalized;
        Vector3 forward = vertical * camForward;
        Vector3 right = horizontal * mainCamera.right;

        Vector3 dir = forward + right;

        //This prevents the player from moving faster if walking diagonally
        if (dir.magnitude > 1)
            dir = dir.normalized;

        Vector3 targetVel = dir * pc.maxSpeed.Value;

        //We want to ignore the y axis
        Vector3 currentVel = rb.linearVelocity;
        currentVel.y = 0;

        Vector3 neededVel = targetVel - currentVel;

        //If the player needs to slow down, use the deceleration, otherwise use acceleration
        float multiplier = neededVel.magnitude > targetVel.magnitude ? pc.deceleration.Value : pc.acceleration.Value;

        rb.AddForce(neededVel * multiplier);
    }

    /// <summary>
    /// Tells the server to rotate the camera using the player's inputs
    /// </summary>
    /// <param name="vertical">The Mouse X input axis</param>
    /// <param name="horizontal">The Mouse Y input axis</param>
    /// <param name="camType">0- First Person; 1- Locked Third Person; 2- Independent Third Person</param>
    /// <param name="serverRpcParams"></param>
    [ServerRpc(RequireOwnership = false)]
    public void RotateCameraServerRpc(float vertical, float horizontal, int camType, ServerRpcParams serverRpcParams = default)
    {
        ulong senderID = serverRpcParams.Receive.SenderClientId;
        PlayerComponentsManager pcm = NetworkManager.ConnectedClients[senderID].PlayerObject.GetComponent<PlayerComponentsManager>();
        Transform player = pcm.playerController.transform;
        Transform camera = pcm.cameraController.transform;

        camera.RotateAround(player.position, Vector3.up, vertical);
        camera.RotateAround(player.position, camera.right, horizontal);
    }
}
