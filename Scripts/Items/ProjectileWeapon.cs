using Unity.Netcode;
using UnityEngine;

public class ProjectileWeapon : GenericItem
{
    [SerializeField] private NetworkObject projectile;

    public override void OnPrimary(ulong clientID)
    {
        base.OnPrimary(clientID);
        NetworkManager.SpawnManager.InstantiateAndSpawn(projectile, clientID, position: transform.position, rotation: transform.rotation);
    }
}
