using Photon.Pun;
using UnityEngine;
using TMPro;
using Photon.Realtime;

public class ConnectManager : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private GameObject ConnectPanel;

    [SerializeField]
    private TextMeshProUGUI StatusText;

    private bool isConnecting = false;
    private const string gameVersion = "v1";

    // Start is called before the first frame update
    void Awake()
    {
        if (ConnectPanel == null)
        {
            Debug.LogError("Please set the ConnectPanel");
        }
        if (StatusText == null)
        {
            Debug.LogError("Please set the StatusText");
        }

        // #Critical
        // this makes sure we can use PhotonNetwork.LoadLevel() on the master client and all clients in the same room sync their level automatically
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    public void Connect()
    {
        isConnecting = true;
        ConnectPanel.SetActive(false);
        ShowStatus("Connecting...");

        if (PhotonNetwork.IsConnected)
        {
            ShowStatus("Joining Random Room...");
            PhotonNetwork.JoinRandomRoom();
        }
        else
        {
            ShowStatus("Connecting...");
            PhotonNetwork.ConnectUsingSettings();
            PhotonNetwork.GameVersion = gameVersion;
        }
    }

    public override void OnConnectedToMaster()
    {
        if (isConnecting)
        {
            ShowStatus("Connected, joining room...");
            PhotonNetwork.JoinRandomRoom();
        }
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        ShowStatus("Creating new room...");
        PhotonNetwork.CreateRoom(null, new Photon.Realtime.RoomOptions { MaxPlayers = 2 });
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        isConnecting = false;
        ConnectPanel.SetActive(true);
    }

    public override void OnJoinedRoom()
    {
        ShowStatus("Joined room with " + PhotonNetwork.CurrentRoom.PlayerCount + " player(s).");
        if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
        {
            PhotonNetwork.LoadLevel("Game");
        }
    }

    private void ShowStatus(string text)
    {
        if (StatusText == null)
        {
            // do nothing
            return;
        }

        StatusText.gameObject.SetActive(true);
        StatusText.text = text;
    }
}
