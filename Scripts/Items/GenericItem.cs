using System.Collections;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class GenericItem : NetworkBehaviour
{
    [SerializeField] protected int itemID;
    public int GetID => itemID;

    [SerializeField] protected float cooldown;

    public NetworkVariable<bool> primaryReady = new(true);

    public string GetName => gameObject.name;

    //This method is called whenever the player receives the item in their inventory
    protected virtual void OnReceive() { }

    [ServerRpc]
    public virtual void OnEquipServerRpc() 
    {
        //print(primaryReady.Value);
        gameObject.SetActive(true);
        //StartCoroutine(StartCooldown());
    }

    public virtual void OnUnequip() 
    {
        gameObject.SetActive(false);
    }
 
    IEnumerator StartCooldown()
    {
        primaryReady.Value = false;
        yield return new WaitForSeconds(cooldown);
        primaryReady.Value = true;
    }

    public virtual void OnPrimary(ulong clientID) { StartCoroutine(StartCooldown()); }

    public virtual void OnSecondaryDown(ulong clientID) 
    {
        print("Working");
        GetComponentInParent<PlayerComponentsManager>().cameraController.ZoomCameraRpc(20);
    }

    public virtual void OnSecondaryHold(ulong clientID) { }

    public virtual void OnSecondaryUp(ulong clientID)
    {
        GetComponentInParent<PlayerComponentsManager>().cameraController.ZoomCameraRpc(-20);
    }
}
