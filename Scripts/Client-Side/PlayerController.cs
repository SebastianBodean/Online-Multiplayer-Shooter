using Unity.Netcode;
using UnityEngine;

/// <summary>
/// This stores variables about how the player shoud move and sends all movement-related inputs to the server
/// </summary>
public class PlayerController : NetworkBehaviour
{
    [SerializeField] public NetworkVariable<float> maxSpeed { get; private set; } = new(5);
    [SerializeField] public NetworkVariable<float> acceleration { get; private set; } = new(50);
    [SerializeField] public NetworkVariable<float> deceleration { get; private set; } = new(20);

    // Update is called once per frame
    void FixedUpdate()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        SetMovementServerRpc(horizontal, vertical);

        //NetworkInputManager.Singleton.MovePlayerServerRpc(horizontal,vertical);
    }

    /// <summary>
    /// Requests to change the stored movement input on the server - this allows the server to access the value when needed
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void SetMovementServerRpc(float horizontal, float vertical, ServerRpcParams serverRpcParams = default)
    {
        ulong clientID = serverRpcParams.Receive.SenderClientId;
        Player pcm = NetworkManager.ConnectedClients[clientID].PlayerObject.GetComponent<Player>();

        //If the player does not have control, do not send the inputs
        if (!pcm.focused)
            return;

        pcm.movementInput.Value = new Vector2(horizontal, vertical);
    }

    //This was used before implementing the network movement system
    //I'll leave it in just in case I need it in the future
    /*private void MovePlayer()
    {
        Vector3 camForward = new Vector3(mainCamera.forward.x, 0, mainCamera.forward.z).normalized;
        Vector3 forward = Input.GetAxis("Vertical") * camForward;
        Vector3 right = Input.GetAxis("Horizontal") * mainCamera.right;

        Vector3 dir = forward + right;

        //This prevents the player from moving faster if walking diagonally
        if (dir.magnitude > 1)
            dir = dir.normalized;

        Vector3 targetVel = dir * maxSpeed;

        //We want to ignore the y axis
        Vector3 currentVel = rb.linearVelocity;
        currentVel.y = 0;

        Vector3 neededVel = targetVel - currentVel;

        //If the player needs to slow down, use the deceleration, otherwise use acceleration
        float multiplier = neededVel.magnitude > targetVel.magnitude ? deceleration : acceleration;

        rb.AddForce(neededVel * multiplier);
    }*/
}
