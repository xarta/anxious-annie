using System;
using System.IO.Ports;
using System.Collections.Generic;

namespace MonoSerialConsole
{
	// TODO Dispose pattern - and flush commands
	sealed public class LlapSerialPort : BaseSerialPort
	{
		private LlapSerialPort (int blockLimit, int queueLimit) : base(blockLimit, queueLimit) {}


		public override void SerialDataEvent(byte[] received)
		{
			Console.Write ("LLAP-RECEIVED-START   ");
			received.CW(this);
			Console.Write ("    LLAP-RECEIVED-END\n");
		}

		public override void SerialError (IndexOutOfRangeException ex)
		{
			ex.Message.CW (n);
		}

	}
}

