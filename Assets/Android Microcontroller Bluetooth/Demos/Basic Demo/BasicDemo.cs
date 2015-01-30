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

		UFoneInterface.IMUResultEvent += (int accx,int accy,int accz,int gyrx,int gyry,int gyrz,int magx,int magy,int magz) =>
		{
				Debug.Log ("IMU: accX=" + accx + " accY=" + accy + " accZ=" + accz + " gyrx=" + gyrx + " gyry=" + gyry + " gyrz=" + gyrz + " magx=" + magx + " magy=" + magy + " magz=" + magz);
		};

	}

	void Update()
	{
		UFoneInterface.AddData (BtConnector.readBuffer());
		UFoneInterface.Loop();


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

		stringToEdit = GUI.TextField(new Rect(Screen.width*0.15f, 0, Screen.width*0.8f, Screen.height*0.1f), stringToEdit);
		GUI.Label(new Rect(0,Screen.height*0.2f,Screen.width,Screen.height*0.1f),"Arduino Says : " + fromArduino);
		GUI.Label(new Rect(0,Screen.height*0.3f,Screen.width,Screen.height*0.1f),"from PlugIn : " + BtConnector.readControlData ());

		if(GUI.Button(new Rect(0,Screen.height*0.4f,Screen.width,Screen.height*0.1f), "Try Connect")) 
		{
				if (!BtConnector.isBluetoothEnabled ()){
					BtConnector.askEnableBluetooth();
				} else BtConnector.connect();
		}


		if(BtConnector.isConnected())
		{
		
			if(GUI.Button(new Rect(0,Screen.height*0.6f,Screen.width,Screen.height*0.1f), "RequestIMU")) {

				UFoneInterface.RequestIMU();
			}
			if(GUI.Button(new Rect(0,Screen.height*0.5f,Screen.width,Screen.height*0.1f), "SetRawRC")) {
				
		
			}

			if(GUI.Button(new Rect(0,Screen.height*0.7f,Screen.width,Screen.height*0.1f), "Close")) {
				BtConnector.close();
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
