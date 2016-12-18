using System;
using System.IO.Ports;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Reflection;

// ADD AT-COMMAND SUPPORT

namespace MonoSerialConsole
{
	public class GsmSerialPort : BaseSerialPort
	{
		

		public event Action AT_OK;
		public event Action AT_ERROR;
		public event Action PLUS;
		public event Action HAT;

		public event Action SMS;





		private const int MAXRSSI = 255; //www.paoli.cz.out/media/HUAWEI_ME909u-521_LTE_LGA_Module_AT_Command_Interface_Specification-V100R001_02.pdf
		private const int MINRSSI = 0;   // ... assuming common range among these devices
		private int _rssi = MINRSSI;

		public int RSSI 
		{
			get 
			{
				return _rssi;
			}
			protected set
			{
				if (value >= MINRSSI && value <= MAXRSSI) 
				{
					_rssi = value;

					//SerialEvent(this, ErssiChange);
				}
				else
					throw new ArgumentOutOfRangeException ("value", $"value should be between {MINRSSI} and {MAXRSSI}");
			}
		}
			
		// PATTERNS TO LOOK FOR - THAT I SEE ON MY HUAWEI E1150 3G GSM DONGLE
		protected char[] CharCRLF { get; } =   {'\r', '\n'};
		protected byte[] ByteCRLF { get; }

		protected char[] CharCR { get; } = {'\r'};
		protected byte[] ByteCR { get; }

		protected char[] CharRSSI { get; } = {'R', 'S', 'S', 'I', ':'};
		protected byte[] ByteRSSI { get; }
		 
		protected char[] CharOK { get; } =   {'O', 'K'};
		protected byte[] ByteOK { get; }

		protected char[] CharERROR { get; } = {'E', 'R', 'R', 'O', 'R'};
		protected byte[] ByteERROR { get; }

		protected char[] CharBOOT { get; } = {'B', 'O', 'O', 'T'};
		protected byte[] ByteBOOT { get; }

		protected char[] CharHat { get; } = {'^'};
		protected byte[] ByteHat { get; }

		protected char[] CharPlus { get; } = {'+'};
		protected byte[] BytePlus { get; }

		protected int MatchMaxLength { get; }


		protected GsmSerialPort (int blockLimit, int queueLimit) : base(blockLimit, queueLimit) 
		{
			// WHAT TO DO LOOK FOR IN SERIAL DATA (FAST/FREQUENT MATCH)
			this.ByteRSSI = this.Encoding.GetBytes (CharRSSI);
			this.ByteOK = this.Encoding.GetBytes (CharOK);
			this.ByteBOOT = this.Encoding.GetBytes (CharBOOT);
			this.ByteCRLF = this.Encoding.GetBytes (CharCRLF);
			this.ByteCR = this.Encoding.GetBytes (CharCR);
			this.ByteHat = this.Encoding.GetBytes (CharHat);
			this.BytePlus = this.Encoding.GetBytes (CharPlus);
			this.ByteERROR = this.Encoding.GetBytes (CharERROR);

			List<int> tempMaxFinder = new List<int> () 
			{ 
				ByteRSSI.Length, 
				ByteOK.Length, 
				ByteBOOT.Length, 
				ByteCRLF.Length,
				ByteERROR.Length
			};

			tempMaxFinder.Sort ();
			tempMaxFinder.Reverse();
			this.MatchMaxLength = tempMaxFinder [0];




		}

		public override void WriteAT(string s)
		{
			byte[] bytes = this.Encoding.GetBytes (s);

			WriteAT (bytes);
		}






		public override void SerialDataEvent(byte[] received)
		{
			$"\nINPUT = ".CW(); received.CW(this,"",ByteCRLF.Length,received.Length-ByteCRLF.Length); ", ".CW();

			try
			{
				/*
				The set-up of the Huawei E1150 3G modem dongles I have is such that on the port
				with the SMS notifications, it also produces a lot of RSSI reports - which is
				useful to me, and "BOOT" notifications - I don't know what they are yet.
				I'm not using data.

				It makes sense to me that for something being called all the time with 
				byte arrays, and with known delimiters <CR><LF>, and only a few items of
				interest, then I should avoid garbage as much as possible and work with
				byte array references and matching byte arrays as much as possible, 
				avoiding costs such as conversions or copying arrays etc. if not necessary.
				Remember - using on a Raspberry Pi and Compute Sticks etc. as well as PC
				Will try to take advantage of conditional-shortcuts.
				*/
				// IGNORE ANYTHING EXCEPT WHAT WE EXPECT, AND DISCARD BUFFER IF GROWS TOO LARGE
						


				int receivedEnd = received.Length;

				if(LittleBufferRemainCount>0)
				{
					$"LITTLEBUFFERREMAINCOUNT>0".CW();
					if((receivedEnd + LittleBufferRemainCount) > LittleBuffer.Length)
					{
						if( (receivedEnd + LittleBufferRemainCount) < LITTLEBUFFERMAX)
						{
							// TODO TRACE THIS AS SHOULDN'T HAPPEN
							// Some insurance
							byte[] tempByteArray = new byte[receivedEnd + LittleBufferRemainCount];
							Buffer.BlockCopy(LittleBuffer, LittleBufferIndex, tempByteArray, 0, LittleBufferRemainCount);
							this.LittleBuffer = tempByteArray;
							LittleBufferIndex = LittleBufferRemainCount + 1; // where to put new received
						}
						else
						{
							// TODO TRACE SOMETHING WRONG ... TOO MUCH BUFFERING (maybe currupted data)
							//      or can't keep-up - so ignore ... if critical, sender will try again
							LittleBufferIndex = 0; // discard any carry-over data, just deal with new
							LittleBufferRemainCount = 0;
						}
					}
					else
					{
						// shift remaining data to front of byte[]
						Buffer.BlockCopy(LittleBuffer, LittleBufferIndex, LittleBuffer, 0,LittleBufferRemainCount);
						LittleBufferIndex = LittleBufferRemainCount + 1; // where to put new received
					}

				}
				else if(receivedEnd > LittleBuffer.Length)
				{
					if(receivedEnd < LITTLEBUFFERMAX)
					{
						byte[] tempByteArray = new byte[receivedEnd];
						LittleBuffer = tempByteArray;
					}
					LittleBufferIndex = 0;
				}
				else
				{
					LittleBufferIndex = 0; // nothing remaining from last time, put new received at start of byte[]
					// TODO Experiment with assign received to LittleBuffer				
				}


				Buffer.BlockCopy(received, 0, LittleBuffer, LittleBufferIndex, receivedEnd);
				LittleBufferIndex = 0; // start iterating over byte[] from beginning
				receivedEnd += LittleBufferRemainCount; // length of bytes to iterate over
				LittleBufferRemainCount = 0; // reset



				int startMatchBytes = 0; int endMatchBytes = 0; 
				int matchBytesLength = 0;

				for (int i = LittleBufferIndex; i < receivedEnd; i++) 
				{
					// <CR><LF>--data--<CR><LF>
					startMatchBytes = StartAfterCRLF(i, LittleBuffer);   // returns i if <CR><LF> not found

					endMatchBytes = EndBeforeCRLF(startMatchBytes, LittleBuffer);  // returns start if <CR><LF> not found
					// returns -1 if no data between


					matchBytesLength = endMatchBytes - startMatchBytes;
					$"matchBytesLength = {matchBytesLength}, ".CW();
					if(matchBytesLength > 0)                            // some data to test
					{
						// data present
						$"OUTPUT = ".CW(); LittleBuffer.CW(this,n ,startMatchBytes, endMatchBytes); 




						if(Match(ByteHat,startMatchBytes))
						{
							startMatchBytes += ByteHat.Length;

							if(Match(ByteRSSI, startMatchBytes)) 
							{
								int valRSSI;

								if(Int32.TryParse(
									LittleBuffer.GetSubString(this,startMatchBytes + ByteRSSI.Length, endMatchBytes), 
									out valRSSI))
								{
									if(this.RSSI != valRSSI)
									{
										try
										{
											this.RSSI = valRSSI;
											// TODO check what happens at the TryParse stage if numbers outside of int range?
										}
										catch(ArgumentOutOfRangeException ex)
										{
											ex.Message.CW(n);
										}
									}
								}
							}
							//HAT(LittleBuffer.GetSubString(this, startMatchBytes, endMatchBytes));

						}
						else if(Match(BytePlus, startMatchBytes))
						{
							startMatchBytes += BytePlus.Length;

							// e.g. AT responses (solicited / non-solicited)


							// TODO: Trace unknown AT command
						}
						else if(Match(ByteOK, startMatchBytes))
						{
							// OK response
							//AT_OK(); // raise public event for anybody subscribed
						}
						else if(Match(ByteERROR, startMatchBytes))
						{
							// ERROR response
							//AT_ERROR(); // raise public event for anybody subscribed
						}
						else
						{	
							// no match currently available
						}

						i = endMatchBytes + ByteCRLF.Length -1;
						// now Continue with For loop - look for more
					}
					else
					{
						if(startMatchBytes==i)
						{
							// TODO Special check for <CR> only at receivedEnd
							// - in which case will need to carry-over <CR> to next round

							// no valid data in remaining of accumuldated LittleBuffer - discard
							LittleBufferIndex = 0;
							LittleBufferRemainCount = 0;
							break;
						}
						else
						{
							if(endMatchBytes == -1)
							{
								// no data between <CR><LF><CR><LF>
								i = startMatchBytes + ByteCRLF.Length;
								continue;
							}
							else
							{
								// no closing <CR><LF>
								// carry-over remaining from opening <CR><LF>
								LittleBufferIndex = startMatchBytes - ByteCRLF.Length;
								LittleBufferRemainCount = receivedEnd - LittleBufferIndex;
								break;
							}


						}



					}

				} // END FOR



				//BigBuffer.Enqueue(this.Encoding.GetString(received));
			}
			catch (Exception ex) 
			{
				// TODO meaningful exceptions (or change Try/Catch structure even)
				ex.Message.CW ();
			}
		}

		public override void SerialError (IndexOutOfRangeException ex)
		{
			ex.Message.CW (n);
		}


		private int StartAfterCRLF(int cursor, byte[] buffer)
		{
			int buffLength = buffer.Length;
			int patternLength = ByteCRLF.Length;
			int matchingBytes;

			for (int i = cursor; i < buffLength; i++) 
			{
				matchingBytes = 0;
				while (matchingBytes < patternLength && (matchingBytes + i) < buffLength) 
				{
					if (buffer [matchingBytes + i] == ByteCRLF [matchingBytes])
						matchingBytes++;
					else
						break;
				}
				if (matchingBytes == patternLength)
					return matchingBytes + i;
			}
			return cursor; // nothing found

		}

		private int EndBeforeCRLF(int cursor, byte[] buffer)
		{
			int buffLength = buffer.Length;
			int patternLength = ByteCRLF.Length;
			int matchingBytes;

			for (int i = cursor; i < buffLength; i++) 
			{
				matchingBytes = 0;

				while (matchingBytes < patternLength && (matchingBytes + i) < buffLength) 
				{
					if (buffer [matchingBytes + i] == ByteCRLF [matchingBytes]) {
						matchingBytes++;
					}
					else
					{
						//Console.WriteLine (	Convert.ToString(buffer[matchingBytes + i]));
						break;
					}
				}
				if (matchingBytes == patternLength)
				{
					if (i == cursor)
						return -1; // no data between <CR><LF><CR><LF>
					else
						return i; // index before end <CR><LF>
				}
			}
			return cursor; // nothing found
		}


		private bool Match(byte[] testBytes, int startMatchBytes)
		{
			int i = 0;

			while(i < testBytes.Length)
			{
				if (LittleBuffer [startMatchBytes + i] != testBytes [i])
					return false;
				i++;
			}
			return true;
		}






























	}

}

