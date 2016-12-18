using System;
using System.Collections.Generic;

namespace MonoSerialConsole
{
	// TODO BOUND CHECKING ETC !!!!!!!!!!!!!!!!!!!!!
	public class LittleBuffer1<T>
	{
		
		private T[] meArray;

		public T this [int index] {
			get { return meArray [index]; }
			set { meArray [index] = value; }
		}

		public LittleBuffer1(int size)
		{
			meArray = new T[size];
		}

	}
}

