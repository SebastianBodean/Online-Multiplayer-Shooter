using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    [SerializeField] public NetworkVariable<float> maxSpeed { get; private set; } = new(5);
    [SerializeField] public NetworkVariable<float> acceleration { get; private set; } = new(3);
    [SerializeField] public NetworkVariable<float> deceleration { get; private set; } = new(1);

    [SerializeField] private float gravity;

    private Rigidbody rb;
    private Transform mainCamera;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        PlayerComponentsManager pcm = GetComponentInParent<PlayerComponentsManager>();
        rb = pcm.rb;
        mainCamera = pcm.cameraController.transform;
    }

    // Update is called once per frame
    void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        
        NetworkPlayerManager.Singleton.MovePlayerServerRpc(horizontal,vertical);

        //MovePlayer();

        //ApplyGravityServerRpc();
    }

    private void ApplyGravity()
    {
        rb.AddForce(Vector3.down * Mathf.Pow(gravity, 2));
    }

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
