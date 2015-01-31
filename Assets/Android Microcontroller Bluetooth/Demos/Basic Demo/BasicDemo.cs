using UnityEngine;
using System.Collections;


public class BasicDemo : MonoBehaviour {

	string fromArduino ="";
	string stringToEdit = "RandomBot";

	public UFoneCommandInterface UFoneInterface;


	void Start () {

		BtConnector.moduleName (stringToEdit);



		//BtConnector.sho

		UFoneInterface = new MultiWiiProtocol();
		UFoneInterface.DebugMessageEvent += (string log) =>
		{
				Debug.Log (log);
		};

		UFoneInterface.IMUResultEvent += (short accx,short accy,short accz,short gyrx,short gyry,short gyrz,short magx,short magy,short magz) =>
		{
				Debug.Log ("IMU: accX=" + accx + " accY=" + accy + " accZ=" + accz + " gyrx=" + gyrx + " gyry=" + gyry + " gyrz=" + gyrz + " magx=" + magx + " magy=" + magy + " magz=" + magz);
		};

		UFoneInterface.RCResultEvent += (ushort Roll,ushort Pitch,ushort Yaw,ushort Throttle,ushort AUX1,ushort AUX2,ushort AUX3,ushort AUX4) => 
		{
				Debug.Log ("RC: Roll=" + Roll + " PITCH=" + Pitch + " Yaw=" + Yaw + " Throttle=" + Throttle + " AUX1=" + AUX1 + " AUX2=" + AUX2 + " AUX3=" + AUX3 + " AUX4=" + AUX4);
		};

	}

	void Update()
	{
		UFoneInterface.AddData (BtConnector.readBuffer());
		UFoneInterface.Update();


		Debug.Log ("Picked:"+BtConnector.isDevicePicked+":"+BtConnector.getPickedDeviceName());
		//if(BtConnector.isConnected())
		//UFoneInterface.SetRawRC(1400,1410,1420,1000,1950,1500,1500,1501);

		//SendData
		byte[] dataToSend = UFoneInterface.GetRawCommand();

		if (dataToSend.Length > 0) 
		{
			BtConnector.sendBytes (dataToSend);
			Debug.Log("sendBytes:"+byteToIntStr(dataToSend));
		}
		//GetData
	}

	void OnGUI(){
		GUI.Label(new Rect(0, 0, Screen.width*0.15f, Screen.height*0.1f),"Module Name ");


		if(BtConnector.isConnected())
		{
			if(GUI.Button(new Rect(0,Screen.height*0.5f,Screen.width,Screen.height*0.1f), "RequestIMU")) 
			{
				UFoneInterface.RequestIMU();
			}
			if(GUI.Button(new Rect(0,Screen.height*0.6f,Screen.width,Screen.height*0.1f), "RequestRC")) 
			{
				UFoneInterface.RequestRC();
			}

			if(GUI.Button(new Rect(0,Screen.height*0.7f,Screen.width,Screen.height*0.1f), "ARM")) 
			{
				UFoneInterface.SetRawRC(1400,1410,1420,1000,1950,1500,1500,1501);
			}

			if(GUI.Button(new Rect(0,Screen.height*0.8f,Screen.width,Screen.height*0.1f), "DisArm")) 
			{
				UFoneInterface.SetRawRC(1400,1410,1420,1000,1050,1500,1500,1500);
			}



		}
		else
		{
			if(GUI.Button(new Rect(0,Screen.height*0.4f,Screen.width,Screen.height*0.1f), "Try Connect")) 
			{
				if (!BtConnector.isBluetoothEnabled ())
				{
					BtConnector.askEnableBluetooth();
				} 
				else 
				{
					BtConnector.connect();
				}


			}

			if(GUI.Button(new Rect(0,Screen.height*0.9f,Screen.width,Screen.height*0.1f), "Show")) 
			{
				BtConnector.showDevices();
			}

		}
	}

	public string byteToIntStr(byte[] byteArray)
	{
		if(byteArray==null)return "no data";

		string data = "";

		foreach(byte b in byteArray)
		{
			data+= ((int)b)+",";
		}

		return data;
	}
}
