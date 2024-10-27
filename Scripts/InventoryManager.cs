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

    private void Awake()
    {
        //We need to initialise network lists here, otherwise it will lead to memory leaks
        inventory = new NetworkList<int>();

        //Whenever this value is changed, make the newly selected item active
        /*selectedSlot.OnValueChanged += ((previousValue, newValue) =>
        {
            GetItem(previousValue).gameObject.SetActive(false);
            GetItem(newValue).gameObject.SetActive(true);
        });

        inventory.OnListChanged += ((changeEvent) =>
        {
            //This might not be needed but I'll keep it just in case
        });*/
    }

    public override void OnNetworkSpawn()
    {
        if (!GetComponent<NetworkObject>().IsOwner)
            return;

        StartCoroutine(StartImputCheck());
    }

    /// <summary>
    /// After this coroutine is called, the component will start listening for the player's inputs and send them to the server 
    /// </summary>
    private IEnumerator StartImputCheck()
    {
        //wait until the player's items are spawned
        while (true)
        {
            if (transform.childCount > 2)
                break;
            yield return null;
        }

        ProcessInputs += (() =>
        {
            ManageMouse();

            SwitchItem();
        });
    }

    private event System.Action ProcessInputs = null;

    private void Update()
    {
        //The question mark is the equivalent of "if (ProcessInputs != null)"
        ProcessInputs?.Invoke();
    }

    private void ManageMouse()
    {
        GenericItem selectedItem = GetItem(selectedSlot.Value);

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

    public GenericItem GetItem(int slot)
    {
        if (transform.childCount <= 2 + slot)
        { 
            Debug.LogError("Couldn't find an item at slot " + slot);
            return null;
        }

        GenericItem component = transform.GetChild(2 + slot).GetComponent<GenericItem>();
        if (component == null)
        {
            Debug.LogError("Object does not contain a GenericItem component", transform.GetChild(2 + slot));
            return null;
        }
        return component;
    }
}
