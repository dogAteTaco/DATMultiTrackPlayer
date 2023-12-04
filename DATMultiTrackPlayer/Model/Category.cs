using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DATMultiTrackPlayer.Model
{
	public class Category
	{

		public int Id { get; set; }
		public string Description { get; set; }

		public Category(int id, string description)
		{
			Id = id;
			Description = description;
		}
	}
}
