using Unity.Netcode;
using UnityEngine;

public class CameraController : NetworkBehaviour
{
    [SerializeField] private int controlType; //0 - Independent cam; 1 - Locked cam; 2 - First person cam
    [SerializeField] private float targetDist;

    private Transform player;

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
        float vertical = Input.GetAxis("Mouse X");
        float horizontal = Input.GetAxis("Mouse Y");
        NetworkPlayerManager.Singleton.RotateCameraServerRpc(vertical, horizontal, 2);

        PushCamera();

        //TestRotation();
    }

    private void IndependentCam()
    {
        float x = Input.GetAxis("Mouse X");
        float y = Input.GetAxis("Mouse Y");

        transform.RotateAround(player.position, Vector3.up, x);
        transform.RotateAround(player.position, transform.right, y);
    }

    private void LockedCam()
    {
        float x = Input.GetAxis("Mouse X");
        float y = Input.GetAxis("Mouse Y");
        
        player.RotateAround(player.position, Vector3.up, x);
        transform.RotateAround(player.position, transform.right, y);
    }

    //This function prevents the camera from rotating past 90 degrees on the z axis
    private void FixRotation()
    {
        if (Mathf.Abs(transform.rotation.eulerAngles.y) < 90)
            return;

        Vector3 rotation = transform.rotation.eulerAngles;
        rotation.y = Mathf.Sign(rotation.y) * 90;
        transform.rotation = Quaternion.Euler(rotation);
    }

    //This function pushes the camera forward if there are objects obstructing the player
    private void PushCamera()
    {
        int layermask = 1 << 7;
        //layermask = ~layermask;

        //This value prevents the camera from clipping through the object it is hitting
        float radius = GetComponent<Camera>().nearClipPlane;

        

        //We cast a line between the player and the desired position. If it hits any terrain, we move the camera to the position of the hit
        if (Physics.SphereCast(player.position, radius, (transform.position - player.position).normalized, out RaycastHit hit, targetDist, layermask)) 
        {
            transform.position = hit.point + hit.normal * radius;
            return;
        }

        //Move the camera back to its desired position
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
