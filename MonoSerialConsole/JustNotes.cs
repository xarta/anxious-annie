
// ENCODING
// byte[] newline = Encoding.ASCII.GetBytes(Environment.NewLine);
// 			string n = Environment.NewLine; // TODO - do I need this?
// https://msdn.microsoft.com/en-us/library/system.text.utf8encoding.getdecoder%28v=vs.110%29.aspx?f=255&MSPPError=-2147217396
// https://msdn.microsoft.com/en-us/library/ms404377(v=vs.110).aspx     Character Encoding in the .NET Framework
// http://stackoverflow.com/questions/32864198/can-i-use-serialport-write-to-send-byte-array
// http://stackoverflow.com/questions/2714533/how-to-apply-encoding-when-reading-from-a-serial-port
// http://aakinshin.net/en/blog/dotnet/mono-utf8-conversions/
// https://codeblog.jonskeet.uk/2014/11/07/when-is-a-string-not-a-string/





// SERIAL PORTS
// https://www.raspberrypi.org/forums/viewtopic.php?f=34&t=41840    				Advice with Mono C# and serial ports

// http://stackoverflow.com/questions/11458835/finding-information-about-all-serial-devices-connected-through-usb-in-c-sharp     
// https://msdn.microsoft.com/en-us/library/aa394353%28v=vs.85%29.aspx?f=255&MSPPError=-2147217396    Win32_PnPEntity

// http://stackoverflow.com/questions/23230375/sample-serial-port-comms-code-using-async-api-in-net-4-5

    



// MONO
// http://elinux.org/CSharp_on_RPi


// MONO ASP.NET
// http://www.amazedsaint.com/2013/04/hack-raspberry-pi-how-to-build.html
// Hack Raspberry Pi – How To Build Apps In C#, WinForms and ASP.NET Using Mono In Pi



// MONO WINFORMS
// http://zetcode.com/gui/csharpwinforms/firststeps/
// http://zetcode.com/gui/csharpwinforms/controls/


// MODEM & SMS
// https://wiki.archlinux.org/index.php/Huawei_E1550_3G_modem
// http://www.paoli.cz/out/media/HUAWEI_ME909u-521_LTE_LGA_Module_AT_Command_Interface_Specification-V100R001_02.pdf
// http://www.zeeman.de/wp-content/uploads/2007/09/ubinetics-at-command-set.pdf
// http://m2msupport.net/m2msupport/forums/topic/huawei-usb-modem-at-commands/
// https://myraspberryandme.wordpress.com/2013/09/13/short-message-texting-sms-with-huawei-e220/
// http://www.activexperts.com/sms-component/at/commands/?at=%2BCNMI
// https://en.wikipedia.org/wiki/Data_Terminal_Ready
// http://www.c-sharpcorner.com/UploadFile/ranjancse/send-receive-and-delete-sms-with-iot-devices-arduino-and-g/
// http://stackoverflow.com/questions/25093937/receive-sms-through-gsm-modem-in-c-sharp
// http://stackoverflow.com/questions/1786800/huawei-mobile-connect-e170
// http://stackoverflow.com/questions/9505197/parsing-formatting-data-from-serial-port-c-sharp







// IMPLEMENTING TWO-FACTOR / ENCRYPTION ETC. ETC.
// http://brandonpotter.com/2014/09/07/implementing-free-two-factor-authentication-in-net-using-google-authenticator/









/*

public override void SerialDataEvent(byte[] received)
{

	try
	{



		int matchRSSI = 0; int matchOK = 0; int matchBOOT = 0;
		int offset = 0;

		for (int i = 0; i < received.Length; i++) {

			if(matchOK == i-offset) 
				matchOK = FastMatch(
					i, matchOK, ByteOK, ByteOK.Length, 
					received, received.Length - offset, 
					ByteCRLF, ByteCRLF.Length);
			else
				matchOK = 0;


			if(matchRSSI == i-offset) 
				matchRSSI = FastMatch(
					i, matchRSSI, ByteRSSI, ByteRSSI.Length, 
					received, received.Length - offset, 
					ByteCRLF, ByteCRLF.Length);
			else
				matchRSSI = 0;


			if(matchBOOT == i-offset) 
				matchBOOT = FastMatch(
					i, matchBOOT, ByteBOOT, ByteBOOT.Length, 
					received, received.Length - offset, 
					ByteCRLF, ByteCRLF.Length);
			else
				matchBOOT = 0;


			if(matchOK > (i-offset + 1))
			{ 
				Console.WriteLine ("Greater Than");
				i += matchOK;
				offset = i;
				matchOK = 0;
				matchRSSI = 0;
				matchBOOT = 0;
			}

			if(matchRSSI > (i-offset + 1))
			{ 
				Console.WriteLine ("Greater Than");
				i += matchRSSI;
				offset = i;
				matchRSSI = 0;
				matchOK = 0;
				matchBOOT = 0;
			}

			if(matchBOOT > (i-offset + 1))
			{ 
				Console.WriteLine ("Greater Than");
				i += matchBOOT;
				offset = i;
				matchBOOT = 0;
				matchRSSI = 0;
				matchBOOT = 0;
			}


			if(matchOK < (i-offset) && matchRSSI < (i-offset) && matchBOOT < (i-offset))
			{
				// no matches at all
				matchOK = 0; matchRSSI = 0; matchBOOT = 0; // start again but further along
				offset++;
			}

			Console.WriteLine ($"i = {i}, offset = {offset}\n");

		}
		//Console.WriteLine ($"received.Length ----------------> {received.Length}");
		//Console.WriteLine ($"ByteRSSI.Length ----------------> {ByteRSSI.Length}");
		//Console.WriteLine ($"matchRSSI ----------------------> {matchRSSI}");

		BigBuffer.Enqueue(this.Encoding.GetString(received));
	}
	catch (Exception ex) 
	{
		ex.Message.CW ();
	}
}

public override void SerialError (IndexOutOfRangeException ex)
{
	ex.Message.CW (n);
}


// only called so long as any chance of a match
private int FastMatch(
	int findIndex, 
	int matchingCount, 
	byte[] pattern, 
	int patternLength, 
	byte[] received, 
	int receivedLength, 
	byte[] delimiter, 
	int delimiterLength)
{
	Console.WriteLine (	$"In FastMatch {pattern.ToString()}\n");
	int whileCount = 0;
	if((findIndex<patternLength) && (pattern[findIndex] == received[findIndex]))  
	{
		if(++matchingCount == patternLength) // pattern matches - investigate further
		{
			int remaining = receivedLength - patternLength;
			bool foundMatch = false;

			while((remaining >= delimiterLength) && !foundMatch)
			{
				findIndex++; // look through received
				whileCount++;

				// match ByteCRLF entirely
				for (int responseEndIndex = 0; responseEndIndex < delimiterLength; responseEndIndex++) 
				{
					if(delimiter[responseEndIndex] != received[findIndex+responseEndIndex])
						break; // no point at this "i"
					else
						if(responseEndIndex == delimiterLength-1)
						{
							foundMatch = true;

							Console.WriteLine ("FOUND MATCH");
							// DELEGATE HERE??
							return matchingCount + whileCount;
						}
				}

				remaining--;
			}

		}
	}
	return matchingCount;
}
*/