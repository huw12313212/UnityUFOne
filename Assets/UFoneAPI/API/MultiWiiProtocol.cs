using System.Collections;
using System.Collections.Generic;

public class MultiWiiProtocol : UFoneCommandInterface{

	//HEADER '$' 'M'
	private byte[] PREAMBLE_HEADER = new byte[]{36,77};

	//DIRECTION
	private const char DIRECTION_TO_DRONE = '<';
	private const char DIRECTION_FROM_DRONE = '>';

	//Values
	private const int SIZE_ZERO = 0;
	private const int SIZE_SIXTEEN_BYTE = 16;

	//Index
	private const int CHECK_SUM_START_INDEX = 3;
	private const int COMMAND_INDEX = 4;

	//STATE_MACHINE
	private enum MultiWiiState
	{
		WAITING_PRENABLE_HEADER,
		WAITING_DIRECTION,
		WAITING_SIZE,
		WAITING_COMMAND_TYPE,
		WAITING_DATA,
		WAITING_CRC,
	};
	MultiWiiState ReceivingState = MultiWiiState.WAITING_PRENABLE_HEADER;
	private int HEADER_INDEX = 0;
	private int DEMANDING_SIZE = 0;
	private int DATA_INDEX = 0;
	private int COMMAND_TYPE = 0;
	private List<byte> CommandBytes = new List<byte>();
	//STATE_MACHINE

	//receive
	private const int MSP_IDENT = 100;
	private const int MSP_STATUS = 101;
	private const int MSP_RAW_IMU = 102;
	private const int MSP_SERVO = 103;
	private const int MSP_MSP_MOTOR = 104;
	private const int MSP_RC = 105;
	private const int MSP_RAW_GPS = 106;
	private const int MSP_COMP_GPS = 107;
	private const int MSP_ATTITUDE = 108;
	private const int MSP_ALTITUDE = 109;
	private const int MSP_ANALOG = 110;
	private const int MSP_RC_TUNING = 111;
	private const int MSP_PID = 112;
	private const int MSP_BOX = 113;
	private const int MSP_MISC = 114;
	private const int MSP_MOTOR_PINS = 115;
	private const int MSP_BOXNAMES = 116;
	private const int MSP_PIDNAMES = 117;
	private const int MSP_WP = 118;
	private const int MSP_BOXIDS = 119;
	private const int MSP_SERVO_CONF = 120;
	
	//send
	private const int MSP_SET_RAW_RC = 200;
	private const int MSP_SET_RAW_FPS = 201;
	private const int MSP_SET_RC_TUNING = 204;
	private const int MSP_SET_MOTOR = 214;
	private const int MSP_SET_PID = 202;
	private const int MSP_SET_BOX = 203;
	private const int MSP_SET_MISC = 207;
	private const int MSP_SET_WP = 209;
	private const int MSP_SET_SERVO_CONF = 212;
	private const int MSP_ACC_CALIBRATION = 205;
	private const int MSP_MAG_CALIBRATION = 206;
	private const int MSP_RESET_CONF = 208;
	private const int MSP_SELECT_SETTING = 210;
	private const int MSP_SET_HEAD = 211;
	private const int MSP_BIND = 240;
	private const int MSP_EEPROM_WRITE = 250;

	override public void Update()
	{
		for (int i = 0; i < dataReceived.Count; i++) 
		{
			ByteParsing(dataReceived[i]);
		}
		dataReceived.Clear();
	}

	private void ByteParsing(byte newByte)
	{
		switch (ReceivingState) 
		{
			//step 0
			case MultiWiiState.WAITING_PRENABLE_HEADER:
			if((newByte)== PREAMBLE_HEADER[HEADER_INDEX])
			{
				HEADER_INDEX++;
				CommandBytes.Add(newByte);

				if(HEADER_INDEX==PREAMBLE_HEADER.Length)
				{
					ReceivingState = MultiWiiState.WAITING_DIRECTION;
				}
			}
			else
			{
				CommandBytes.Add(newByte);
				ClearState(false,"INVALIDATE_HEADER");
			}
			break;

			//step 1
			case MultiWiiState.WAITING_DIRECTION:

			if(((char)newByte)==DIRECTION_FROM_DRONE)
			{
				CommandBytes.Add(newByte);
				ReceivingState = MultiWiiState.WAITING_SIZE;
			}
			else
			{
				CommandBytes.Add(newByte);
				ClearState(false,"DIRECTION_FLAG_ERROR");
			}
			break;

			//step 2
			case MultiWiiState.WAITING_SIZE:

			CommandBytes.Add(newByte);
			DEMANDING_SIZE = (int)newByte;
			ReceivingState = MultiWiiState.WAITING_COMMAND_TYPE;

			break;

			//step 3
			case MultiWiiState.WAITING_COMMAND_TYPE:
			
			CommandBytes.Add(newByte);
			COMMAND_TYPE = (int)newByte;

			if(DEMANDING_SIZE==0)
			{
				ReceivingState = MultiWiiState.WAITING_CRC;
			}
			else
			{
				ReceivingState = MultiWiiState.WAITING_DATA;
			}
			break;

			//step 4
			case MultiWiiState.WAITING_DATA:


			CommandBytes.Add(newByte);
			DATA_INDEX ++;

			if(DATA_INDEX == DEMANDING_SIZE)
			{
				ReceivingState = MultiWiiState.WAITING_CRC;
			}

			break;

			case MultiWiiState.WAITING_CRC:

			byte sum = 0;

			//int len = CommandBytes.to
			//check sum for size(3),commandType(4),data(5~)
			for(int i = CHECK_SUM_START_INDEX;i < CommandBytes.Count; i ++)
			{
				sum = (byte)(sum^CommandBytes[i]);
			}

			//validate
			if(sum == newByte)
			{
				CommandBytes.Add(newByte);
				RunCommand(CommandBytes);
				ClearState(true);
			}
			else
			{
				CommandBytes.Add(newByte);

				ClearState(false,"CRC_NOT_VALIDATE");
			}

			break;
		}
	}

	private void RunCommand(List<byte> data)
	{
		int commandNum = data[COMMAND_INDEX];
		List<byte> copied = new List<byte> ();
		copied.AddRange(data);

		RemoveHeadBytes(copied,5);

		switch(commandNum)
		{
			case MSP_RAW_IMU:
			//Reference to the MultiWii-EZ GUI.
			short accX = GetInt16(copied);
			short accY = GetInt16(copied);
			short accZ = GetInt16(copied);
			short gyrX = (short)(GetInt16(copied)/8);
			short gyrY = (short)(GetInt16(copied)/8);
			short gyrZ = (short)(GetInt16(copied)/8);
			short magX = (short)(GetInt16(copied)/3);
			short magY = (short)(GetInt16(copied)/3);
			short magZ = (short)(GetInt16(copied)/3);
				
			if(IMUResultEvent !=null)
			{
				IMUResultEvent.Invoke(accX,accY,accZ,gyrX,gyrY,gyrZ,magX,magY,magZ);
			}
			else
			{
				Debug("[ERROR]IMUResultEventHandler Not Defined");
			}
			break;

			case MSP_RC:
			ushort roll =  GetUNInt16(copied);
			ushort pitch =  GetUNInt16(copied);
			ushort yaw =  GetUNInt16(copied);
			ushort throttle =  GetUNInt16(copied);
			ushort AUX1 =  GetUNInt16(copied);
			ushort AUX2 =  GetUNInt16(copied);
			ushort AUX3 =  GetUNInt16(copied);
			ushort AUX4 =  GetUNInt16(copied);

			if(RCResultEvent != null)
			{
				RCResultEvent.Invoke(roll,pitch,yaw,throttle,AUX1,AUX2,AUX3,AUX4);
			}
			else
			{
				Debug("[ERROR]RCResultEvent Not Defined");
			}
			break;


			default:
				Debug("[VALIDATE_UNHANDLED_COMMAND]"+byteToIntStr(CommandBytes.ToArray()));
			break;
		}
	}

	public void RemoveHeadBytes(List<byte> target,int num)
	{
		target.RemoveRange (0, num);
	}

	public ushort GetUNInt16(List<byte> target)
	{
		ushort result = System.BitConverter.ToUInt16 (target.ToArray(),0);
		target.RemoveRange (0, 2);
		
		return result;
	}


	public short GetInt16(List<byte> target)
	{
		short result = System.BitConverter.ToInt16 (target.ToArray(),0);
		target.RemoveRange (0, 2);

		return result;
	}

	private void Debug(string log)
	{
		if (DebugMessageEvent != null) 
		{
			DebugMessageEvent.Invoke(log);
		}
	}

 	override public void RequestIMU()
	{
		List<byte> currentCommand = MakeRequestCommand (MSP_RAW_IMU);
		dataToSend.AddRange(currentCommand);

	}

	override public void RequestRC()
	{
		List<byte> currentCommand = MakeRequestCommand (MSP_RC);
		dataToSend.AddRange(currentCommand);
	}

	private List<byte> MakeRequestCommand(byte commandFlag)
	{
		List<byte> currentCommand = new List<byte>();
		
		currentCommand.AddRange (PREAMBLE_HEADER);
		currentCommand.Add((byte)DIRECTION_TO_DRONE);
		currentCommand.Add(SIZE_ZERO);
		currentCommand.Add(commandFlag);
		AddCheckSum(currentCommand,CHECK_SUM_START_INDEX);

		return currentCommand;
	}

	override public void SetRawRC (ushort Roll, ushort Pitch, ushort Yaw, ushort Throttle, ushort AUX1, ushort AUX2, ushort AUX3, ushort AUX4)
	{
		List<byte> currentCommand = new List<byte>();
		
		currentCommand.AddRange (PREAMBLE_HEADER);
		currentCommand.Add((byte)DIRECTION_TO_DRONE);
		currentCommand.Add(SIZE_SIXTEEN_BYTE);
		currentCommand.Add(MSP_SET_RAW_RC);

		byte[] RollByte = System.BitConverter.GetBytes (Roll);
		currentCommand.AddRange (RollByte);

		byte[] PitchByte = System.BitConverter.GetBytes (Pitch);
		currentCommand.AddRange (PitchByte);

		byte[] YawByte = System.BitConverter.GetBytes (Yaw);
		currentCommand.AddRange (YawByte);

		byte[] ThrottleByte = System.BitConverter.GetBytes (Throttle);
		currentCommand.AddRange (ThrottleByte);

		byte[] AUX1Byte = System.BitConverter.GetBytes (AUX1);
		currentCommand.AddRange (AUX1Byte);

		byte[] AUX2Byte = System.BitConverter.GetBytes (AUX2);
		currentCommand.AddRange (AUX2Byte);

		byte[] AUX3Byte = System.BitConverter.GetBytes (AUX3);
		currentCommand.AddRange (AUX3Byte);

		byte[] AUX4Byte = System.BitConverter.GetBytes (AUX4);
		currentCommand.AddRange (AUX4Byte);

		AddCheckSum(currentCommand,CHECK_SUM_START_INDEX);

		dataToSend.AddRange(currentCommand);
	}

	private void AddCheckSum(List<byte> bytes, int StartIndex)
	{
		byte sum = 0;

		for(int i = StartIndex;i < bytes.Count; i ++)
		{
			sum = (byte)(sum^bytes[i]);
		}

		bytes.Add(sum);
	}

	private string byteToIntStr(byte[] byteArray)
	{
		if(byteArray==null)return "no data";
		
		string data = "";
		
		foreach(byte b in byteArray)
		{
			data+= ((int)b)+",";
		}
		
		return data;
	}
	
	private void ClearState (bool validate,string reason="")
	{
		if (!validate) 
		{
				Debug ("[DropByte:" + reason + "]" + byteToIntStr (CommandBytes.ToArray ()));
		} else 
		{
				//Debug("[Validate]"+byteToIntStr (CommandBytes.ToArray ()));
		}

		HEADER_INDEX = 0;
		DEMANDING_SIZE = 0;
		DATA_INDEX = 0;
		COMMAND_TYPE = 0;
		CommandBytes.Clear();
		ReceivingState = MultiWiiState.WAITING_PRENABLE_HEADER;
	}
}
