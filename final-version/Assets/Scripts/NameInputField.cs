using Photon.Pun;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(TMP_InputField))]
public class NameInputField : MonoBehaviour
{

    public void SetPlayerName(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            Debug.LogError("Player name is empty");
            return;
        }
        PhotonNetwork.NickName = name;
    }

}
