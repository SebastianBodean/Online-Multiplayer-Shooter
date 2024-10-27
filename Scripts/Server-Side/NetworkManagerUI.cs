using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManagerUI : MonoBehaviour
{
    private Button hostButton;
    private Button joinButton;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
       // NetworkManager.Singleton.StartHost();

        FindButtons();

        hostButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartHost();
        });

        joinButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.Shutdown();
            print("Joining...");
            NetworkManager.Singleton.StartClient();
        });

        NetworkManager.Singleton.OnClientStarted += (() =>
        {
            joinButton.onClick.RemoveAllListeners();
            joinButton.onClick.AddListener(() =>
            {
                NetworkManager.Singleton.Shutdown();
            });
        });

        NetworkManager.Singleton.OnClientStopped += ((_) =>
        {
            joinButton.onClick.RemoveAllListeners();
            joinButton.onClick.AddListener(() =>
            {
                NetworkManager.Singleton.StartClient();
            });
        });
    }

    private void FindButtons()
    {
        Transform button = transform.Find("Host Button");
        if (button == null)
            Debug.LogError("The Network Manager UI needs to have a child button called 'Host Button'", this);
        hostButton = button.GetComponent<Button>();
        if (hostButton == null)
            Debug.LogError("Could not find Button component inside the object", button);

        button = transform.Find("Connect Button");
        if (button == null)
            Debug.LogError("The Network Manager UI needs to have a child button called 'Connect Button'", this);
        joinButton = button.GetComponent<Button>();
        if (joinButton == null)
            Debug.LogError("Could not find Button component inside the object", button);
    }
}
