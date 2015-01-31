using UnityEngine;
using System.Collections;

public class BlueToothManager : MonoBehaviour {

	//public string TargetDeviceName;

	public GameObject ButtonConnect;
	public GameObject ButtonDisconnect;
	public GameObject ArmButton;
	public GameObject DisArmButton;

	public bool Armed = false;

	private UFoneCommandInterface UFOneAPI;

	public void SelectDevices()
	{
		BtConnector.showDevices();
	}


	public void Connect()
	{
		if(BtConnector.isDevicePicked)
		{
			if (!BtConnector.isBluetoothEnabled ())
			{
				BtConnector.askEnableBluetooth();
			} 
			else 
			{
				string name = BtConnector.getPickedDeviceName();
				BtConnector.moduleName(name);
				BtConnector.connect();

				Debug.Log("try to connect "+name);
			}
		}
	}

	public void Disconnect()
	{
		if(BtConnector.isConnected())
		{
			BtConnector.close();
		}
	}

	public bool isConnected
	{
		get
		{
			return BtConnector.isConnected();
		}
	}

	public void ARM()
	{
		UFOneAPI.SetRawRC (1500, 1500, 1500, 1000, 2000, 1500, 1500, 1500);
		Armed = true;
	}

	public void DISARM()
	{
		UFOneAPI.SetRawRC (1500, 1500, 1500, 1000, 1000, 1500, 1500, 1500);
		Armed = false;
	}

	// Use this for initialization
	void Start () {

		UFOneAPI = new MultiWiiProtocol();
		BtConnector.askEnableBluetooth();

		UFOneAPI.DebugMessageEvent += (string log)=>
		{
			Debug.Log(log);
		};
	
	}
	
	// Update is called once per frame
	void Update () {


		HandleBlueTooth();

		UpdateView ();

	}

	void HandleBlueTooth()
	{
		//input
		UFOneAPI.AddData(BtConnector.readBuffer());

		//output
		BtConnector.sendBytes(UFOneAPI.GetRawCommand());

		UFOneAPI.Update();
		
	}

	void UpdateView()
	{
		ArmButton.SetActive(!Armed && BtConnector.isConnected ());
		DisArmButton.SetActive(Armed && BtConnector.isConnected ());


		if (BtConnector.isConnected ()) 
		{
			ButtonConnect.SetActive(false);
			ButtonDisconnect.SetActive(true);
		}
		else
		{
			if(BtConnector.isDevicePicked)
			{
				ButtonConnect.SetActive(true);
			}
			else
			{
				ButtonConnect.SetActive(false);
			}


			ButtonDisconnect.SetActive(false);
		}
	}

}
