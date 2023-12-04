using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DATMultiTrackPlayer.Model
{
	public class MultiTrack
	{
		public int Id { get; set; }
		public String Name { get; set; }
		public List<Track> Tracks { get; set; }

		public MultiTrack(int id, string name, List<Track> tracks)
		{
			Id = id;
			Name = name;
			Tracks = tracks;
		}

		public void AddTrack(Track track) { 
			Tracks.Add(track);
		}
	}
}
