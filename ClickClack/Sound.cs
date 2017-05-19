using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickClack
{
	class Sound
	{
		public byte[] Data;

		public static Sound FromS16LE(byte[] data)
		{
			//Clone the data
			byte[] clonedData = new byte[data.Length];
			Array.Copy(data, clonedData, data.Length);

			return new Sound() { Data = clonedData };
		}
	}
}
