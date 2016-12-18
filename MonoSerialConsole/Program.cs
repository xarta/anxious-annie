using System;
using System.IO.Ports;
using System.IO; // IOException
using System.Threading;
using System.Collections.Generic;
using System.Text;
using NodaTime;

#if (LINUX)
' do something (eventually)
#endif

namespace MonoSerialConsole
{
	
	class MainClass
	{
		// TODO implement IComparable to sort - maybe by buffer size or other priority?
		private static List<ISerialPort> iSerialPorts { get; set; }

		private static string n { get; } = BaseSerialPort.n; // Environment.NewLine

		public static void Main (string[] args)
		{
			IClock iNodaTime = SystemClock.Instance; // nodatime (singleton) ... think about log applications and time interpreting later
			                                         // use iNodaTime interface for testability

		
				Action start = () => Console.WriteLine("Start"); // null or do something to indicate starting
				Action nullifyStart = () => start = null;

				// TO DO: Platform specific - obtain right Port names
				// e.g. use reflection / interface etc. load specific assemblies @ runtime
				// e.g. File Exists check / Linux UDev and so on and entity management query in Windows for Com ports by instance name

				// Either Raspberry Pi UART for SliceOfRadio Or USB stick with SRF onboard
				// LLAP = Lightweight Local Automation Protocol (Ciseco/Wirelessthings.net)
				// WILL EVENTUALLY BE SUPPLYING THE COM PORT / SERIAL-RESOURCE-FILE-PATH AT RUNTTIME
				ISerialPort ispLLP = BaseSerialPort.Factory<LlapSerialPort>("/dev/ttyAMA0", iNodaTime, ref start, 9600, 512, 128);
				// Huawei E1150 3G USB Modem (only one modem connected - 3 ports)
				// NOT USING 3G data on GSM port, and pretty quiet - good for minimising AT command parsing and AT command writing (WITH NO DATA)
				// SMS port is main AT command port (noisy with data) but includes SMS notifications
				ISerialPort ispSMS = BaseSerialPort.Factory<SmsSerialPort>("/dev/ttyUSB2", iNodaTime, ref start, 9600, 512, 128);
				ISerialPort ispGSM = BaseSerialPort.Factory<GsmSerialPort>("/dev/ttyUSB0", iNodaTime, ref start,  115200, 512, 128);


				iSerialPorts = new List<ISerialPort> () {ispLLP, ispSMS, ispGSM};
				

				
				start = (start != null) ? start + nullifyStart : nullifyStart;

				if (start != null)
					start ();

				if (start != null)
					start ();

				Thread.Sleep (2000); // allow ports to settle - look at Task Wait and test for serial ports in some way?
				
				Console.WriteLine ("blaaaaaaaaaaaaaahhhhhhhhhhhhhhhhhhhhhhhhhhhhhhh");

				ispGSM.WriteAT("AT+CMGL=\"ALL\"\r");
				Thread.Sleep(1000);
				ispGSM.WriteAT("AT+CNMI=?\r");
				Thread.Sleep(100);
				ispGSM.WriteAT ("AT+CNMI?\r");
				ispGSM.WriteAT ("AT+CNMI=1,2,2,1\r");
				Thread.Sleep(100);

				ispLLP.WriteAT("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");


			//start ();



			/*
			// https://msdn.microsoft.com/en-us/library/system.io.ports.serialport.open(v=vs.110).aspx
			catch (UnauthorizedAccessException ex){ex.Message.CW (n);} // TODO (Inner/outer and "when" and alert etc.)
			catch (ArgumentOutOfRangeException ex){ex.Message.CW (n);} // TODO
			catch (ArgumentException ex){ex.Message.CW (n);} // TODO
			catch (IOException ex){ex.Message.CW (n);} // TODO
			catch (Exception ex){ex.Message.CW(n);} // TODO
			*/










			//spSMS.Write("AT+CMGL=\"ALL\"\r");

			while (true) 
			{

				// WAIT FOR EVENTS
				// Circular List? Time slicing?

				// TODO:
				// Make "heartbeat" on RESTful endpoint for external alarm
				// And possibly for triggering of external watchdog for reboot
				// (Maybe send "AT" and look for "OK" within set time)

				// TODO more processing in classes (temp string) - so just llap stuff gets to this point?
				/*
				foreach (var iSP in iSerialPorts) 
				{
					if (iSP.BigBuffer.Count > 0) 
					{
						//Console.Write (	iSP.GetType ());
						//(iSP.BigBuffer.Dequeue ()).CW (n);
						string temp = iSP.BigBuffer.Dequeue();
					}
				}
				*/


			}

		}

	






	}



}



	/*
	 * 
	 * 
			spSliceRadio.DataReceived += new SerialDataReceivedEventHandler (DataReceivedHandler);
			spSMS.DataReceived += new SerialDataReceivedEventHandler (DataReceivedHandler);

			string[] ports = SerialPort.GetPortNames ();
			Console.WriteLine ("The following serial ports were found:");
			foreach(string port in ports)
				Console.WriteLine (port);
			
			Console.WriteLine ("\n\n");



			try
			{
				spSliceRadio.Open();
				Console.WriteLine( $"Serial Port {spSliceRadio.PortName} Opened: {spSliceRadio.IsOpen}");
				//spSliceRadio.Write("+++");
				//Thread.Sleep(2000);
				//spSliceRadio.Write("ATBD\r\n");
				//Thread.Sleep(2000);

			}
			catch {
				Console.WriteLine ("Error in Opening Raspberry Pi UART / Slice Of Radio");
			}

			try
			{
				spSMS.Open();
				Console.WriteLine( $"Serial Port {spSMS.PortName} Opened: {spSMS.IsOpen}");
				Thread.Sleep(100);
				spSMS.Write("AT+CMGL=\"ALL\"\r");
				Thread.Sleep(1000);
				spSMS.Write ("AT+CNMI=?\r");
				Thread.Sleep(100);
				spSMS.Write ("AT+CNMI?\r");
				spSMS.Write ("AT+CNMI=1,2,2,1\r");
				Thread.Sleep(100);


			}
			catch {
				Console.WriteLine ("Error in Opening USB SMS Modem Serial Port");
			}

			try
			{
				bool spSMS_DSR = spSMS.DsrHolding; Console.WriteLine ($"spSMS.DsrHolding = {spSMS.DsrHolding}\n");
				Thread.Sleep(100);
				while(true){
					//if(spSMS.BytesToRead > 0)
						//Console.WriteLine ($"{spSMS.PortName} -> {spSMS.ReadExisting()}");

					if(spSMS.DsrHolding != spSMS_DSR)
						Console.Write ($"spSMS.DsrHolding = {spSMS.DsrHolding}\n");
				} // WAIT FOR EVENTS
			}
			catch(Exception e) {
				Console.WriteLine (e.Message);
			}
			finally {
				if (spSliceRadio.IsOpen)
					spSliceRadio.Close ();
				if (spSMS.IsOpen)
					spSMS.Close ();
			}


		}



		private static void DataReceivedHandler(
			object sender,
			SerialDataReceivedEventArgs e)
		{
			//Console.WriteLine (e.EventType.ToString());
			SerialPort sp = (SerialPort)sender;

			//Console.Write ($"{sp.PortName} -> {sp.ReadExisting()}");
		}





*/






