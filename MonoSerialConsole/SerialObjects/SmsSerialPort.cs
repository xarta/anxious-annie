using System;
using System.IO.Ports;
using System.Text;
using System.Collections.Generic;

namespace MonoSerialConsole
{
	/// <summary>
	/// Sms serial port.
	/// Not using PDU ... just text, and looking for known short sequences, and RSSI values.
	/// Discard everything else (can update later).
	/// </summary>
	sealed public class SmsSerialPort : GsmSerialPort
	{



		private SmsSerialPort (int blockLimit, int queueLimit) : base(blockLimit, queueLimit){}


		public override void SerialDataEvent(byte[] received)
		{
			base.SerialDataEvent(received);

			// TODO ... well ... maybe do extra stuff here - not sure yet
		}

		public override void SerialError (IndexOutOfRangeException ex)
		{
			// TODO better exception stuff - and include SMS related exceptions here (customer error class?)
			ex.Message.CW (n);
		}





	}
}

