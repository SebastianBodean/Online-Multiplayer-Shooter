using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Adding this component to an object will cause it to face the camera of the local player
/// </summary>
public class LookAtCamera : NetworkBehaviour
{
    // Update is called once per frame
    void Update()
    {
        if (!NetworkManager.Singleton.IsConnectedClient)
            return;
        PlayerComponentsManager pcm = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerComponentsManager>();
        
        Vector3 forward = pcm.cameraController.transform.forward;
        transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
    }
}
