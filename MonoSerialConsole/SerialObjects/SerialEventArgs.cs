using System;
using System.Xml;
using NodaTime; 	// http://nodatime.org/1.3.x/userguide/mono.html
					// https://github.com/nodatime/nodatime/issues/97
					// https://github.com/nodatime/nodatime/issues/98

namespace MonoSerialConsole
{
	// https://wiki.archlinux.org/index.php/Huawei_E1550_3G_modem
	// https://en.wikipedia.org/wiki/Hayes_command_set
	// TODO check specific GSM standard my dongle uses e.g. 
	//      http://www.etsi.org/deliver/etsi_gts/07/0707/05.00.00_60/gsmts_0707v050000p.pdf

	public class SerialEventArgs : EventArgs
	{
		// e.g. SMS is more specific than DATA so use SMS
		// e.g. LLAP is more specific than DATA so use LLAP
		// using unsolicited RSSI as heartbeat, and enum RSSI for change/diff
		public enum SerialEventBestType {sysERROR, atERROR, OK, PLUS, HAT, HEARTBEAT, RSSI, DATA, SMS, LLAP}; 

		public SerialEventBestType SerialEvent { get; }
		public Instant TimeStamp { get; }
		public string Buffer { get; }

		public SerialEventArgs (SerialEventBestType specificEventType, Instant timeref, string _buffer)
		{
			// maybe solicited (response) e.g. OK, ERROR, or detailed
			// unsolicited e.g. RSSI, SMS notification etc.

			SerialEvent  = specificEventType;
			TimeStamp = timeref;
			Buffer = _buffer;                 // NULL for HEARTBEAT, atERROR, OK

			
		}
	}
}

// DAVE ---- REMEMBER --- CAN USE SENDER OBJECT TOO!  (TO KNOW WHO RAISED THE EVENT ETC. SO CAN HAVE MORE COMMON ENUMS?)
