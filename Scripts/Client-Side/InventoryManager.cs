using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using static Controls;

public class InventoryManager : NetworkBehaviour
{
    //This might not be needed
    public NetworkList<int> inventory;
    public NetworkVariable<int> selectedSlot = new(0);

    private Player pcm;

    private void Awake()
    {
        //We need to initialise network lists here, otherwise it will lead to memory leaks
        inventory = new NetworkList<int>();

        pcm = GetComponent<Player>();

        //Whenever this value is changed, make the newly selected item active
        /*selectedSlot.OnValueChanged += ((previousValue, newValue) =>
        {
            GetItem(previousValue).gameObject.SetActive(false);
            GetItem(newValue).gameObject.SetActive(true);
        });*/

        //When changing the value of an item in the inventory
        /*inventory.OnListChanged += ((changeEvent) =>
        {
           
        });*/
    }

    public override void OnNetworkSpawn()
    {
        if (!GetComponent<NetworkObject>().IsOwner)
            return;

        StartCoroutine(StartInputCheck());
    }

    /// <summary>
    /// After this coroutine is called, the component will start listening for the player's inputs and send them to the server 
    /// </summary>
    private IEnumerator StartInputCheck()
    {
        //wait until the player's items are spawned
        while (true)
        {
            if (transform.childCount > 2)
                break;
            yield return null;
        }

        ProcessInputs += ManageMouse;
        ProcessInputs += SwitchItem;
    }

    private event System.Action ProcessInputs = null;

    private void Update()
    {
        //If the player does not have control, do not send the inputs
        if (!pcm.focused)
            return;

        //The question mark is the equivalent of "if (ProcessInputs != null)"
        ProcessInputs?.Invoke();
    }

    private void ManageMouse()
    {
        GenericItem selectedItem = GetItem();

        if (Input.GetKeyDown(KeyCode.Mouse0))
            ItemDatabase.Singleton.RequestActionServerRpc(ControlType.PrimaryDown);

        if (Input.GetKeyDown(KeyCode.Mouse1))
            ItemDatabase.Singleton.RequestActionServerRpc(ControlType.SecondaryDown);
        else if (Input.GetKey(KeyCode.Mouse1))
            ItemDatabase.Singleton.RequestActionServerRpc(ControlType.SecondaryHold);
        else if (Input.GetKeyUp(KeyCode.Mouse1))
            ItemDatabase.Singleton.RequestActionServerRpc(ControlType.SecondaryUp);
    }

    private void SwitchItem()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1) && selectedSlot.Value != 0)
            ItemDatabase.Singleton.RequestActionServerRpc(ControlType.Slot1);
        else if (Input.GetKeyDown(KeyCode.Alpha2) && selectedSlot.Value != 1)
            ItemDatabase.Singleton.RequestActionServerRpc(ControlType.Slot2);
        else if (Input.GetKeyDown(KeyCode.Alpha3) && selectedSlot.Value != 2)
            ItemDatabase.Singleton.RequestActionServerRpc(ControlType.Slot3);
    }

    /// <summary>
    /// Returns the item's component
    /// </summary>
    /// <param name="slot"></param>
    /// <returns></returns>
    public GenericItem GetItem()
    {
        return transform.GetChild(0).GetComponent<GenericItem>();
    }
}
