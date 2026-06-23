using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;

public class NetworkManagerCustom : NetworkManager
{
	public bool IsCheckedServer, IsCheckedClient;
	public string ip_address;
	public TMP_InputField ipInputText;
	/*public void Start()
	{
		if(IsCheckedServer)
		{
			NetworkManager.singleton.StartServer();
		}
		else if(IsCheckedClient)
		{
			NetworkManager.singleton.networkAddress = ipInputText.text;
			NetworkManager.singleton.StartClient();
		}
	}*/
	
	 public void Connect()
    {
      if (IsCheckedServer && !IsCheckedClient)
        {
            StartServer();
        }
        else if (IsCheckedClient && !IsCheckedServer)
        {
            networkAddress = ipInputText.text;
            StartClient();
        }
		else
		{
			if(IsCheckedClient)IsCheckedServer=false;
			else
			{
				IsCheckedServer=true;
				IsCheckedClient = false;
			}
		}
    }
}
