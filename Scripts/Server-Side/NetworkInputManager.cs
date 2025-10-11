using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;


/// <summary>
/// This class is responsible with processing all inputs of a client
/// </summary>
public class NetworkInputManager : NetworkBehaviour
{
    public static NetworkInputManager Singleton {  get; private set; }

    private void Awake()
    {
        if (Singleton != null)
        {
            Destroy(this);
            return;
        }

        Singleton = this;
    }

    void FixedUpdate()
    {
        //Only run this code if this client is the host
        if (!NetworkManager.IsServer)
            return;

        //Update the clients over the network
        UpdatePlayersServerRpc();
    }

    [ServerRpc]
    private void UpdatePlayersServerRpc()
    {
        //Get a list of all the connected clients and iterate through them
        IReadOnlyList<ulong> clients = NetworkManager.ConnectedClientsIds;
        for (int i = 0; i < clients.Count; i++)
        {
            //Get the player character of the client
            NetworkObject player = NetworkManager.ConnectedClients[clients[i]].PlayerObject;

            ApplyGravity(player);
            ProcessInputs(player);
        }
    }

    /// <summary>
    /// Applies downward force on a specified player character, mimicking gravity
    /// </summary>
    /// <param name="player">The target player character</param>
    private void ApplyGravity(NetworkObject player)
    {
        player.GetComponent<Player>().rb.AddForce(Vector3.down * Mathf.Pow(3f, 2));
    }


    #region Inputs
    /// <summary>
    /// Retreives and processes the player's inputs
    /// </summary>
    /// <param name="player"></param>
    /// <param name="inputs"></param>
    private void ProcessInputs(NetworkObject player)
    {
        NetworkVariable<Vector2> movement = player.GetComponent<Player>().movementInput;
        NetworkList<bool> inputs = player.GetComponent<Player>().inputs;

        //Process the movement inputs and reset the value
        MovePlayer(movement.Value.x, movement.Value.y, player);
        //movement.Value = Vector2.zero;

        //Process the rest of the inputs

    }

    /// <summary>
    /// Tells the server to move the player according to their inputs
    /// </summary>
    /// <param name="horizontal">The horizontal input axis</param>
    /// <param name="vertical">The vertical input axis</param>
    private void MovePlayer(float horizontal, float vertical, NetworkObject player)
    {
        //Validate the client's outputs
        if (Mathf.Abs(horizontal) > 1f || Mathf.Abs(vertical) > 1f)
            Debug.LogError("One of the clients is sending invalid inputs");

        Player pcm = player.GetComponent<Player>();
        Transform mainCamera = pcm.cameraController.transform;
        PlayerController pc = pcm.playerController;
        Rigidbody rb = pcm.rb;

        Vector3 camForward = new Vector3(mainCamera.forward.x, 0, mainCamera.forward.z).normalized;
        Vector3 forward = vertical * camForward;
        Vector3 right = horizontal * mainCamera.right;

        Vector3 dir = forward + right;

        //Prevent the player from moving faster than possible, either through cheating or by moving diagonally
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
    #endregion


    /// <summary>
    /// Returns the PlayerComponentsManager of the client that invoked this method
    /// </summary>
    /// <param name="serverRpcParams"></param>
    /// <returns></returns>
    private Player GetPlayerComponents(ServerRpcParams serverRpcParams)
    {
        ulong senderID = serverRpcParams.Receive.SenderClientId;
        return NetworkManager.ConnectedClients[senderID].PlayerObject.GetComponent<Player>();
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
        Player pcm = NetworkManager.ConnectedClients[senderID].PlayerObject.GetComponent<Player>();
        Transform player = pcm.playerController.transform;
        Transform camera = pcm.cameraController.transform;

        camera.RotateAround(player.position, Vector3.up, vertical);
        camera.RotateAround(player.position, camera.right, horizontal);
    }
}
