using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using static Controls;

/// <summary>
/// Stores copies of all items and manages the inventories of all connected clients
/// </summary>
public class ItemDatabase : NetworkBehaviour
{
    public static ItemDatabase Singleton { get; private set; }

    /// <summary>
    /// This allows you to assign items in the inspector through drag and drop
    /// </summary>
    [SerializeField] private GenericItem[] itemsList;

    /// <summary>
    /// A dictionary containing a reference to all items
    /// 
    /// You can access these by passing in their corresponding ID value
    /// </summary>
    private Dictionary<int,GenericItem> itemDictionary = new Dictionary<int, GenericItem>();

    /// <summary>
    /// The number of inventory slots players should have
    /// </summary>
    private NetworkVariable<int> slotsCount = new(3);

    private void Awake()
    {
        if (Singleton != null)
        {
            Destroy(this);
            return;
        }

        Singleton = this;

        for (int i = 0; itemsList.Length > i; i++)
            AddItem(itemsList[i].GetID, itemsList[i]);
    }

    /// <summary>
    /// Spawns an 'Empty' item for each inventory slot
    /// </summary>
    /// <param name="clientID"></param>
    [ServerRpc]
    public void InitialiseInventoryServerRpc(ulong clientID)
    {
        Transform inventory = NetworkManager.ConnectedClients[clientID].PlayerObject.GetComponent<PlayerComponentsManager>().inventoryManager.transform;

        NetworkObject prefab = itemDictionary[1].GetComponent<NetworkObject>();

        for (int i = 0; i < slotsCount.Value; i++)
        {
            NetworkObject copy = NetworkManager.SpawnManager.InstantiateAndSpawn(prefab, clientID);

            copy.TrySetParent(inventory);
            copy.transform.localPosition = Vector3.right * 0.6f;

            //We disable all items except the first one
            if (i == 0)
                continue;

            copy.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Adds a given item to the dictionary
    /// </summary>
    private void AddItem(int ID, GenericItem item)
    {
        if (itemDictionary.ContainsKey(ID))
        {
            Debug.LogError(string.Format("Couldn't assign {0} to ID {1} in the dictionary because item {2} is already assigned to it", item.name, ID, itemDictionary[ID]));
            return;
        }
        itemDictionary.Add(ID, item);
    }

    /// <summary>
    /// Requests access to an item from the server
    /// The server may then decide whether it will assign it to the client or not
    /// -Not currently implemented
    /// </summary>
    /// <param name="itemID">The ID of the requested GenericItem component</param>
    /// <param name="slot">The slot in which the requested item should be stored</param>
    [ServerRpc(RequireOwnership = false)]
    public void RequestItemServerRpc(int itemID, int slot, ServerRpcParams serverRpcParams = default)
    {

    }

    /// <summary>
    /// Sets the item in the given slot of a player
    /// </summary>
    /// <param name="itemID">The ID of the GenericItem component</param>
    /// <param name="slot">The slot in which the item will be stored</param>
    [ServerRpc]
    private void SetItemServerRpc(int itemID, int slot, ulong clientID)
    {
        PlayerComponentsManager pcm = NetworkManager.ConnectedClients[clientID].PlayerObject.GetComponent<PlayerComponentsManager>();
        pcm.inventoryManager.inventory[slot] = itemID;

        //GameObject copy = NetworkManager.SpawnManager.InstantiateAndSpawn(itemDictionary[itemID], serverRpcParams.Receive.SenderClientId).gameObject;
        //item = copy.GetComponent<GenericItem>();
    }

    /// <summary>
    /// Outputs the database
    /// Useful for debugging
    /// </summary>
    /// <returns>A string containing a list of the name of all items in the database</returns>
    private string GetDatabase()
    {
        string output = "";

        for(int i = 0;i < itemDictionary.Count;i++)
            output += itemDictionary[i].gameObject.name + "\n";

        return output;
    }

    /// <summary>
    /// Swaps the active slot of the player
    /// </summary>
    /// <param name="slot">The number of the slot to swap to</param>
    /// <param name="clientID">The ID of the client</param>

    private void SwapSlot(int slot, ulong clientID)
    {
        print("Swapping to slot " + slot);
        NetworkManager.ConnectedClients[clientID].PlayerObject.GetComponent<PlayerComponentsManager>().inventoryManager.selectedSlot.Value = slot;
    }

    /// <summary>
    /// Processes all item-related player inputs
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void RequestActionServerRpc(ControlType control, ServerRpcParams serverRpcParams = default)
    {
        ulong clientID = serverRpcParams.Receive.SenderClientId;
        PlayerComponentsManager pcm = NetworkManager.ConnectedClients[clientID].PlayerObject.GetComponent<PlayerComponentsManager>();
        GenericItem activeItem = pcm.inventoryManager.GetItem(pcm.inventoryManager.selectedSlot.Value);
        switch (control)
        {
            #region Mouse Controls
            case ControlType.PrimaryDown:
                if (!activeItem.primaryReady.Value)
                    return;
                activeItem.OnPrimary(clientID);
                break;
            case ControlType.SecondaryDown:
                activeItem.OnSecondaryDown(clientID);
                break;
            case ControlType.SecondaryUp:
                activeItem.OnSecondaryUp(clientID);
                break;
            #endregion

            #region Slot Controls
            case ControlType.Slot1:
                SwapSlot(0, clientID);
                break;
            case ControlType.Slot2:
                SwapSlot(1, clientID);
                break;
            case ControlType.Slot3:
                SwapSlot(2, clientID);
                break;
            case ControlType.Slot4:
                SwapSlot(3, clientID);
                    break;
            case ControlType.Slot5:
                SwapSlot(4, clientID);
                break;
            case ControlType.Slot6:
                SwapSlot(5, clientID);
                break;
            case ControlType.Slot7:
                SwapSlot(6, clientID);
                break;
            case ControlType.Slot8:
                SwapSlot(7, clientID);
                break;
            case ControlType.Slot9:
                SwapSlot(8, clientID);
                break;
            case ControlType.Slot10:
                SwapSlot(9, clientID);
                break;
                #endregion
        }
    }
}
