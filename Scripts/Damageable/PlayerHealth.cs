using Unity.Netcode;
using UnityEngine;

public class PlayerHealth : SimpleHealth
{
    protected override void OnDeath()
    {
        DeathEffect?.Invoke();
        PlayerSpawnManager.Singleton.RequestRespawn(GetComponent<Player>());
        gameObject.SetActive(false);
    }
}
