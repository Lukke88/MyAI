using Mirror;
using TMPro;
using UnityEngine;

public class ChatManager : NetworkBehaviour
{
	public TMP_InputField ChatMessageInput;
    public TMP_Text ChatText;
	/*
	funguje pouze pokud je objekt součástí síťové identity.

Potřebuji vědět:

Je ChatManager na Canvasu?
Má objekt ChatManager komponentu NetworkIdentity?
Máš už ve scéně NetworkManagerCustom?
Máš vytvořený Player prefab s NetworkIdentity?

Pokud ne, Mirror vyhodí něco jako:

Command CmdSendMessage called on ChatManager without authority

Musíš v Inspectoru přetáhnout objekt Text ze Scroll View.
	*/
	public void ConnectToServer()
	{
    NetworkManagerCustom.singleton.networkAddress = "192.168.0.1";
    NetworkManagerCustom.singleton.StartClient();
	}
    [Command(requiresAuthority = false)]
    public void CmdSendMessage(string msg)
    {
        RpcReceiveMessage(msg);
    }

    [ClientRpc]
    void RpcReceiveMessage(string msg)
    {
        ChatText.text += "\n" + msg;
    }

	 public void SendMessageToAll()
    {
        if (string.IsNullOrWhiteSpace(ChatMessageInput.text))
            return;

        CmdSendMessage(ChatMessageInput.text);

        ChatMessageInput.text = "";
    }
}