using Unity.Netcode;
using UnityEngine;

/// <summary>
/// This stores variables about how the player shoud move and sends all movement-related inputs to the server
/// </summary>
public class PlayerController : NetworkBehaviour
{
    [SerializeField] public NetworkVariable<float> maxSpeed { get; private set; } = new(5);
    [SerializeField] public NetworkVariable<float> acceleration { get; private set; } = new(3);
    [SerializeField] public NetworkVariable<float> deceleration { get; private set; } = new(1);

    private Rigidbody rb;
    private Transform mainCamera;
    
    //Get the character's rigid body and camera components
    void Start()
    {
        PlayerComponentsManager pcm = GetComponentInParent<PlayerComponentsManager>();
        rb = pcm.rb;
        mainCamera = pcm.cameraController.transform;
    }

    // Update is called once per frame
    void Update()
    {
        // TODO:    Change this so the server asks for the player's inputs rather than the player sending their inputs whenever
        //          This should give more control to the server and prevent any issues caused by a modified client
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        
        NetworkPlayerManager.Singleton.MovePlayerServerRpc(horizontal,vertical);
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
