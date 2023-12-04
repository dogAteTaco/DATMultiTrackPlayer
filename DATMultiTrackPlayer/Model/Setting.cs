using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DATMultiTrackPlayer.Model
{
	public class Setting
	{
		public String Path { get; set; }

		public Setting(string path)
		{
			Path = path;
		}
	}
}
