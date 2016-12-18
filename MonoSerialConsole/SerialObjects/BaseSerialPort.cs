using System;
using System.IO.Ports;
using System.Text;
using System.Collections.Generic;
using System.Reflection;
using NodaTime;

namespace MonoSerialConsole
{
	public abstract class BaseSerialPort : SerialPort, ISerialPort
	{

		public virtual Queue<string> BigBuffer { get; }
		protected static HashSet<string> NotOpenedBefore { get;} = new HashSet<string>();
		protected IClock IClockSerial { get; set;} // NodaTime library - make sure can use dependency injection for testing
		protected virtual byte[] LittleBuffer { get; set; } // persist for lifecycle of programme
		protected int LittleBufferIndex { get; set; } = 0;
		protected int LittleBufferRemainCount { get; set; } = 0;
		protected const int LITTLEBUFFERMAX = 1048576; // massive 1MB - if bigger than this serious problem even with say 4 bytes per character!!!
		public static string n { get; } = Environment.NewLine;
		public int BlockLimit { get; private set;}



		public event EventHandler<SerialEventArgs> SerialEvent;
		/*
		protected object objectLock = new object();
		public event EventHandler<SerialEventArgs> SerialEvent {
			add {
				lock (objectLock) {
					SerialEvent += value;
				}
			}
			remove {
				lock (objectLock) {
					SerialEvent -= value;
				}
			}
		}
		*/

		protected BaseSerialPort (int blockLimit = 128, int queueLimit = 1)
		{
			this.BlockLimit = blockLimit; // TODO Throw error if outside of some reasonable range
			this.LittleBuffer = new byte[16 * blockLimit];
			this.BigBuffer = new Queue<string> (queueLimit);

			// defaults
			this.BaudRate= 9600;
			this.DataBits = 8;
			this.Parity = Parity.None;
			this.StopBits = StopBits.One;
			this.RtsEnable = true;
			this.DtrEnable = true;
			this.Encoding = Encoding.UTF8;
			this.NewLine = n; // Environment Newline
		}

		public virtual void SerialDataEvent(byte[] received)
		{
			received.CW(this);
		}

		public virtual void SerialError (IndexOutOfRangeException ex)
		{
			ex.Message.CW (n);
		}

		public virtual void WriteAT(byte[] bytes)
		{
			this.Write (bytes, 0, bytes.Length);

		
		}

		public virtual void WriteAT(string s)
		{
			byte[] bytes = this.Encoding.GetBytes (s);

			WriteAT (bytes);
		}



		private void Start()
		{
			if(!this.IsOpen) this.Open();

			Console.WriteLine ($"Starting port {this.GetType()}");

			Action kickoffRead = null;

			// www.sparxeng.com/blog/software/must-use-net-system-io-ports-serialport
			// TODO: look at using ASync / Await or Wait ... Task etc.
			// TODO: NEED TIMEOUT / ERROR-HANDLING FOR HARDWARE ERRORS ETC.
			// TODO: look into Encoding more. I only get matches when I use UTF8 everywhere ...
			//       ... but I thought default encoding on the dongle is UTF7. Check with AT C's
			// ----------------------------------------------------------------------------------

			byte[] buffer = new byte[this.BlockLimit];
			kickoffRead = delegate {
				this.BaseStream.BeginRead (buffer, 0, buffer.Length, delegate (IAsyncResult ar) {
					try {
						int actualLength = this.BaseStream.EndRead (ar);
						byte[] received = new byte[actualLength];
						Buffer.BlockCopy (buffer, 0, received, 0, actualLength);
						this.SerialDataEvent(received);
					} catch (IndexOutOfRangeException exc) {
						this.SerialError (exc);
					}
					kickoffRead (); // keep going and going
				}, null);
			};
			kickoffRead ();
			// ----------------------------------------------------------------------------------
		}



		// IS THIS AN ANTI-PATTERN?  USING <T> TO RESOLVE?
		// http://www.devtrends.co.uk/blog/how-not-to-do-dependency-injection-the-static-or-singleton-container
		// on other hand - using this sort of Factory pattern - I am making constructors protected/private so ... mitigates?
		// Common factors in all the serial ports used (known hardware)
		public static ISerialPort Factory<T>(
			string portName,
			IClock iclock,
			ref Action start,
			int baudRate = 9600,
			int blockLimit = 512,
			int queueLimit = 128) where T : BaseSerialPort
		{


			if(NotOpenedBefore.Add (portName)) // TODO consider thread-safety & maybe "singleton per portName" sort of
			{
				try
				{
					
					// this way so can use private/protected constructors
					ConstructorInfo c = typeof(T).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(int), typeof(int) },null);
					BaseSerialPort newsp = c.Invoke(new Object[] {blockLimit, queueLimit}) as BaseSerialPort;

					// nb this way would need public constructors
					//BaseSerialPort newsp = Activator.CreateInstance(type, new Object[] {512, 128}) as BaseSerialPort;



					newsp.PortName = portName;
					newsp.BaudRate = baudRate;
					newsp.IClockSerial = iclock; // do before opening port - before possible errors requiring time instance

					start = (start != null) ? start + newsp.Start : newsp.Start;


					Console.WriteLine ($"Port {newsp.PortName} open? {newsp.IsOpen}");
					return newsp as ISerialPort;
				}
				catch(Exception) 
				{
					$"Error creating serial port {portName}".CW(n);
					throw;
				}
			}
			else
			{
				// TODO more tests?  Check if open?  Criticality?
				throw new InvalidOperationException("This application only permits one serialport object per port-resource");
			}
		}

	}
}

