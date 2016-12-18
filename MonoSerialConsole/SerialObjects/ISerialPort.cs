using System;
using System.Collections.Generic;

//  Restrict how serial port is used - minimise chance of corrupting serial stream etc.

namespace MonoSerialConsole
{
	public interface ISerialPort
	{

		void WriteAT (string s);
		// string ReadAT();        // maybe use yield?


		// void WriteData (string s);
		// string ReadData ();


		event EventHandler<SerialEventArgs> SerialEvent;


	}
}

