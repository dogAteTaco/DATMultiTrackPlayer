using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DATMultiTrackPlayer.Model
{
	public class Track
	{
		public int Id { get; set; }
		public String Name { get; set; }
		public String Description { get; set; }
		public String Path { get; set; }
		public double Volume { get; set; }
		public List<String> Tags { get; set; }

		public String tooltip => $"Name: {Name}\nPath: {Path}";
		public Track(int Id,string name, string description, string path, double volume, List<string> tags)
		{
			this.Id = Id;
			Name = name;
			Description = description;
			Path = path;
			Volume = volume;
			Tags = tags;
		}

		public Track(int id, string name, string description, string path, List<string> tags)
		{
			Id = id;
			Name = name;
			Description = description;
			Path = path;
			Volume = 100;
			Tags = tags;
		}

		public Track()
		{

		}

		public String TagsToString()
		{
			return String.Join(",", Tags);
		}
	}
}
