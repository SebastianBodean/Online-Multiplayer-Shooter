using Unity.Netcode;
using UnityEngine;

public class HitscanWeapon : GenericItem
{
    [SerializeField] private float damage;

    [SerializeField] private float range;

    public override void OnPrimary(ulong clientID)
    {
        base.OnPrimary(clientID);

        PlayerComponentsManager pcm = NetworkManager.ConnectedClients[clientID].PlayerObject.GetComponent<PlayerComponentsManager>();
        Vector3 start = pcm.transform.position;
        Vector3 dir = pcm.cameraController.transform.forward;
        Vector3 end = start + dir * range;
        Debug.DrawLine(start, end, Color.red, 5);
        if (Physics.Linecast(start, end, out RaycastHit hit)) 
        {
            if (!hit.transform.TryGetComponent(out SimpleHealth target))
                return;

            target.Damage(damage, hit.point);

            print("Remaining health: "+target.health.Value);
        }
    }
}
