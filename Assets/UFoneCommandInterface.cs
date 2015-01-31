using System.Collections;
using System.Collections.Generic;

public abstract class UFoneCommandInterface {

	public void AddData(byte[] ReceivedData)
	{
		dataReceived.AddRange(ReceivedData);
	}

	public abstract void Update();

	//Retrived Data
	protected List<byte> dataReceived = new List<byte>();
	//Data to Send
	protected List<byte> dataToSend = new List<byte> ();

	public byte[] GetRawCommand()
	{
		byte[] rawCommand = dataToSend.ToArray ();
		dataToSend.Clear ();
		return rawCommand;
	}

	//DebugTool
	public delegate void DebugMessageHandler(string log);
	public DebugMessageHandler DebugMessageEvent;

	//Listening

	//please register handler before Reuest
	public abstract void RequestIMU();
	public delegate void IMUChangedHandler(int accx,int accy,int accz,int gyrx,int gyry,int gyrz,int magx,int magy,int magz);
	public IMUChangedHandler IMUResultEvent;

	//please register handler before Request
	public abstract void RequestRC();
	public delegate void RCChangedHandler(int Roll,int Pitch,int Yaw,int Throttle,int AUX1,int AUX2,int AUX3,int AUX4);
	public RCChangedHandler RCResultEvent;
	
	public delegate void AckHandler(int command);
	public AckHandler AckEvent;

	public delegate void UnhandledCommandHandler(byte[] command);
	public UnhandledCommandHandler UnhandledCommandEvent;

	
	//Command
	public abstract void SetRawRC (int Roll, int Pitch, int Yaw, int Throttle, int AUX1, int AUX2, int AUX3, int AUX4);




}
