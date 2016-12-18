using System;
using System.Text;

namespace MonoSerialConsole
{
	public static class Utility
	{
		public static void CW(this string s, string newline = "") => Console.Write ($"{s}{newline}");
		public static void CW(this byte[] b, BaseSerialPort sp, string newline = "", int startByte = 0, int endByte = 0)
		{
			byte[] piece = b;

			if (endByte > 0 || startByte > 0) 
			{
				if (startByte >= 0 && endByte <= b.Length && endByte - startByte > 0) 
				{
					piece = new byte[endByte - startByte];
					Buffer.BlockCopy (b, startByte, piece, 0, endByte - startByte);

				}
			}
			Encoding encoding = sp.Encoding;

			$"{encoding.GetString(piece)}".CW(""); $", piece.Length = {piece.Length}".CW(newline);
		}
		public static string GetSubString(this byte[] b, BaseSerialPort sp, int startByte = 0, int endByte = 0)
		{
			byte[] piece = b;
			Encoding encoding = sp.Encoding;

			if (b.Length > 0) {
				if (endByte > 0 || startByte > 0) {
					if (startByte >= 0 && endByte <= b.Length && endByte - startByte > 0) {
						piece = new byte[endByte - startByte];
						Buffer.BlockCopy (b, startByte, piece, 0, endByte - startByte);
					}
				}
				return encoding.GetString (piece);
			}

			return "";
		}


		public static bool IsLinux
		{
			get
			{
				int p = (int) Environment.OSVersion.Platform;
				return (p == 4) || (p == 6) || (p == 128);
			}
		}
	}
}

