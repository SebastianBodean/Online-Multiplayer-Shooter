using Unity.Netcode;
using UnityEngine;

/// <summary>
/// This component handles the client-side camera controls 
/// </summary>
public class CameraController : NetworkBehaviour
{
    [SerializeField] private int controlType; //0 - Independent cam; 1 - Locked cam; 2 - First person cam
    /// <summary>
    /// The distance that it will try to maintain between the camera and the player
    /// </summary>
    [SerializeField] private float targetDist;

    private Transform player;

    //get the player component manager of this player
    public void Start()
    {
        PlayerComponentsManager pcm = GetComponentInParent<PlayerComponentsManager>();
        if (!pcm.networkObject.IsOwner)
        {
            gameObject.SetActive(false);
            return;
        }

        player = pcm.playerController.transform;
    }

    // Update is called once per frame
    void Update()
    {
        // TODO:    Change this so the server asks for the player's inputs rather than the player sending their inputs whenever
        //          This should give more control to the server and prevent any issues caused by a modified client
        float vertical = Input.GetAxis("Mouse X");
        float horizontal = Input.GetAxis("Mouse Y");
        NetworkPlayerManager.Singleton.RotateCameraServerRpc(vertical, horizontal, 2);

        PushCamera();

        //TestRotation();
    }

    /// <summary>
    /// Allows you to move the camera without it affecting the rotation of the caracter
    /// 
    /// -Currently not implemented
    /// </summary>
    private void IndependentCam()
    {
        float x = Input.GetAxis("Mouse X");
        float y = Input.GetAxis("Mouse Y");

        transform.RotateAround(player.position, Vector3.up, x);
        transform.RotateAround(player.position, transform.right, y);
    }

    /// <summary>
    /// The player's character will rotate to face the same direction as the camera
    /// Inspired by roblox's shift lock control
    /// -Currently not implemented
    /// </summary>
    private void LockedCam()
    {
        float x = Input.GetAxis("Mouse X");
        float y = Input.GetAxis("Mouse Y");
        
        player.RotateAround(player.position, Vector3.up, x);
        transform.RotateAround(player.position, transform.right, y);
    }

    /// <summary>
    /// This function prevents the camera from rotating past 90 degrees on the z axis
    /// 
    /// -Currently not working
    /// </summary>
    private void FixRotation()
    {
        if (Mathf.Abs(transform.rotation.eulerAngles.y) < 90)
            return;

        Vector3 rotation = transform.rotation.eulerAngles;
        rotation.y = Mathf.Sign(rotation.y) * 90;
        transform.rotation = Quaternion.Euler(rotation);
    }

    /// <summary>
    /// This function pushes the camera forward if there are any obstacles that obscure the player
    /// </summary>
    private void PushCamera()
    {
        int layermask = 1 << 7;
        //layermask = ~layermask;

        //This value prevents the camera from clipping through the object it is hitting
        float radius = GetComponent<Camera>().nearClipPlane;

        //We cast a thick line between the player and the desired position. If it hits any terrain, we move the camera in front of the position of the hit
        if (Physics.SphereCast(player.position, radius, (transform.position - player.position).normalized, out RaycastHit hit, targetDist, layermask)) 
        {
            transform.position = hit.point + hit.normal * radius;
            return;
        }

        //Move the camera back to its desired position if it hasn't encountered any obstacles
        transform.position = player.position + (transform.position - player.position).normalized * targetDist;
    }

    /// <summary>
    /// Adjusts the field of view to create a zoom effect
    /// </summary>
    /// <param name="amount">Positive values will zoom in, where as negative values will zoom out</param>
    [Rpc(SendTo.Owner)]
    public void ZoomCameraRpc(float amount)
    {
        GetComponent<Camera>().fieldOfView -= amount;
    }


    private void TestRotation()
    {
        print(transform.rotation);
        print(transform.rotation.eulerAngles);
        //This does not work and I don't know how to make it work
        if (Mathf.Abs(transform.rotation.eulerAngles.x) > 90)
            print("Error: Rotation exceedes bounds");
    }
}
