using UnityEngine;
using System.Collections;

public class BlueToothManager : MonoBehaviour {

	//public string TargetDeviceName;

	public GameObject ButtonConnect;
	public GameObject ButtonDisconnect;
	public GameObject ArmButton;
	public GameObject DisArmButton;
	public GameObject JoySticks;
	public CNJoystick leftStick;
	public CNJoystick rightStick;


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
		AUX1 = RANGE_MAX;
		Armed = true;
	}

	public void DISARM()
	{
		AUX1 = RANGE_MIN;
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

		leftStick.ControllerMovedEvent+= (position, stick) => 
		{
			int dif = (RANGE_MAX - RANGE_MIN) / 2;
			int mean = (RANGE_MAX + RANGE_MIN) / 2;

			ROLL = (ushort) (position.x * dif + mean);
			PITCH = (ushort)(position.y * dif + mean);

			Debug.Log("Left:"+position.x+":"+position.y);
		};

		leftStick.ControllerMovedEvent+= (position, stick) => 
		{
			int dif = (RANGE_MAX - RANGE_MIN) / 2;
			int mean = (RANGE_MAX + RANGE_MIN) / 2;
			
			YAW = (ushort)(position.x * dif + mean);
			THROTTLE = (ushort)(position.y * dif + mean);

			Debug.Log("Right:"+position.x+":"+position.y);
		};
	}

	public const int RANGE_MAX = 2000;
	public const int RANGE_MIN = 1000;

	private ushort YAW = 1500;
	private ushort PITCH = 1500;
	private ushort ROLL = 1500;
	private ushort THROTTLE = 1000;
	private ushort AUX1 = 1000;
	private ushort AUX2 = 1000;
	private ushort AUX3 = 1000;
	private ushort AUX4 = 1000;

	
	// Update is called once per frame
	void Update () {

		if(UpdateRCCounter>UpdateRCPeriod)
		{
			UpdateRCCounter = 0;
			if (isConnected) 
			{
				UFOneAPI.SetRawRC(ROLL,PITCH,YAW,THROTTLE,AUX1,AUX2,AUX3,AUX4);
			}
		}
		else
		{
			UpdateRCCounter+= Time.deltaTime;
		}

		HandleBlueTooth();

		UpdateView ();

	}

	public float UpdateRCPeriod;
	private float UpdateRCCounter =0;

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
		//JoySticks.SetActive (BtConnector.isConnected ());

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
