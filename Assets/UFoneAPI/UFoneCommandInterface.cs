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
	public delegate void IMUChangedHandler(short accx,short accy,short accz,short gyrx,short gyry,short gyrz,short magx,short magy,short magz);
	public IMUChangedHandler IMUResultEvent;

	//please register handler before Request
	public abstract void RequestRC();
	public delegate void RCChangedHandler(ushort Roll,ushort Pitch,ushort Yaw,ushort Throttle,ushort AUX1,ushort AUX2,ushort AUX3,ushort AUX4);
	public RCChangedHandler RCResultEvent;
	
	public delegate void AckHandler(byte command);
	public AckHandler AckEvent;

	public delegate void UnhandledCommandHandler(byte[] command);
	public UnhandledCommandHandler UnhandledCommandEvent;

	
	//Command
	public abstract void SetRawRC (ushort Roll, ushort Pitch, ushort Yaw, ushort Throttle, ushort AUX1, ushort AUX2, ushort AUX3, ushort AUX4);




}
