using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DATMultiTrackPlayer.Model;
using DATMultiTrackPlayer.View;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using Button = System.Windows.Controls.Button;
using JsonSerializer = System.Text.Json.JsonSerializer;
using MessageBox = System.Windows.MessageBox;
using TextBox = System.Windows.Controls.TextBox;

namespace DATMultiTrackPlayer.ViewModel
{
	public partial class MainWindowViewModel : ObservableObject
	{
		[ObservableProperty]
		StackPanel? loadedTrackSP;

		[ObservableProperty]
		StackPanel? allTrackSP;

		[ObservableProperty]
		MultiTrack? currentMultiTrack;

		[ObservableProperty]
		List<MediaPlayer> mediaPlayers;

		[ObservableProperty]
		List<Track> foundTracks;

		[ObservableProperty]
		string currentFilter;

		[ObservableProperty]
		string currentPath;

		[ObservableProperty]
		double stacksHeight;

		[ObservableProperty]
		double buttonSize;

		[ObservableProperty]
		int mainVolume;

		public async Task InitAsync()
		{
			SaveSettings(LoadSettings());
			if(CurrentPath.Trim()!="")
				LoadTracks();
			ButtonSize = 50;
			MainVolume = 100;
		}

		private void SaveSettings(Setting settings)
		{
			
			string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions() { WriteIndented = true });

			using (StreamWriter writer = new StreamWriter("settings.json"))
			{
				writer.WriteLine(json);
			}
		}

		private Setting LoadSettings()
		{
			Setting settings = new Setting("");
			if (File.Exists("settings.json"))
			{
				string jsonString = File.ReadAllText("settings.json");
				settings = JsonSerializer.Deserialize<Setting>(jsonString);
			}
			CurrentPath = settings.Path;
			return settings;
		}

		private void AddToMulti(Object sender, EventArgs e)
		{
			var control = (TrackControl)((Grid)((StackPanel)((Button)sender).Parent).Parent).Parent;
			int id = Convert.ToInt32(control.IdTB.Text);

			Track track = FoundTracks.Where(t => t.Id == id).First();

			if (CurrentMultiTrack.Tracks.Where(t => t.Id == track.Id).Count() == 0)
			{
				TrackPlayerControl trackPlayerControl = new TrackPlayerControl();
				trackPlayerControl.TrackNameTB.Text = track.Name;
				trackPlayerControl.VolumeSlider.Value = 100*MainVolume/100;
				trackPlayerControl.VolumeSlider.ValueChanged += VolumeSlider_ValueChanged;
				trackPlayerControl.DeleteB.Click += DeleteTrackFromMulti;
				CurrentMultiTrack.Tracks.Add(track);
				LoadedTrackSP.Children.Add(trackPlayerControl);
				MediaPlayer player = new MediaPlayer();
				player.MediaEnded += MediaPlayer_MediaEnded;
				player.Open(new Uri(track.Path));
				player.Volume = track.Volume / 100;
				MediaPlayers.Add(player);
				StopCurrentMulti();
			}
			else
				MessageBox.Show("The track is already on the current Player.");

		}
		private List<Track> ScanTracks()
		{
			List<Track> scannedTracks = new List<Track>();
			this.CreateTrackFolder();

			string[] allfiles = Directory.GetFiles(CurrentPath, "*.mp3", SearchOption.AllDirectories);
			int idTrack = 0;
			foreach (var file in allfiles)
			{
				TagLib.File f = TagLib.File.Create(file);
				Track track = new Track(idTrack, f.Tag.Title, f.Tag.Title, file, new List<String>());
				scannedTracks.Add(track);
				idTrack++;
			}
			return scannedTracks;
		}

		private List<Track> ReadExistingTracks()
		{
			List<Track> existingTracks = new List<Track>();
			//Creates the tracks folder if it doesn't exist
			this.CreateTrackFolder();

			if (File.Exists("library.json"))
			{
				string jsonString = File.ReadAllText("library.json");
				var tracks = JsonSerializer.Deserialize<List<Track>>(jsonString);
				existingTracks = tracks == null ? new List<Track>() : tracks;
			}
			return existingTracks;
		}

		private void CreateTrackFolder()
		{
			if (!Directory.Exists("tracks"))
			{
				DirectorySecurity securityRules = new DirectorySecurity();
				string path = "tracks";
				Directory.CreateDirectory(path);

				DirectoryInfo directoryInfo = new DirectoryInfo(path);
				DirectorySecurity directorySecurity = directoryInfo.GetAccessControl();

				// Add a rule that grants Everyone full control
				directorySecurity.AddAccessRule(new FileSystemAccessRule("Everyone", FileSystemRights.FullControl, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow));

				// Apply the new security settings to the folder
				directoryInfo.SetAccessControl(directorySecurity);
			}
		}
		[RelayCommand]
		void LoadMultiTrack()
		{
			//MediaPlayer player = new MediaPlayer();
			//player.MediaEnded += MediaPlayer_MediaEnded;
			//player.Open(new Uri(track.Path));
			//player.Volume = track.Volume / 100;
			//MediaPlayers.Add(player);
			//MediaPlayers = new List<MediaPlayer>();
			//CurrentMultiTrack = multi;
		}
		private void DeleteTrackFromMulti(Object sender, EventArgs e)
		{
			TrackPlayerControl controlClicked = new TrackPlayerControl();
			foreach (TrackPlayerControl trackPlayerControl in LoadedTrackSP.Children)
			{
				if (trackPlayerControl.DeleteB == (Button)sender)
					controlClicked = trackPlayerControl;
			}

			int index = LoadedTrackSP.Children.IndexOf(controlClicked);
			LoadedTrackSP.Children.RemoveAt(index);
			MediaPlayers[index].Stop();
			MediaPlayers.RemoveAt(index);
			CurrentMultiTrack.Tracks.RemoveAt(index);
		}
		private void LoadTracks()
		{
			CurrentMultiTrack = new MultiTrack(1, "Current", new List<Track>());
			if (LoadedTrackSP != null)
			{
				CurrentFilter = "";
				MediaPlayers = new List<MediaPlayer>();
				RefreshLibrary();
			}
		}

		[RelayCommand]
		void RefreshLibrary()
		{
			var existingTracks = ReadExistingTracks();
			var scannedTracks = ScanTracks();

			foreach (var scannedtrack in scannedTracks)
			{
				var found = existingTracks.Where(t => t.Path == scannedtrack.Path).ToList();
				if (found.Count > 0)
				{
					scannedtrack.Tags = found.First().Tags;
				}
			}

			FoundTracks = scannedTracks;
			AllTrackSP.Children.Clear();
			int i = 1;
			foreach (Track track in scannedTracks)
			{
				TrackControl control = new TrackControl();
				control.TrackNameTB.Text = track.Name;
				control.DescriptionTB.Text = track.Description;
				control.TagsTB.Text = track.TagsToString();
				control.ToolTip = track.tooltip;
				control.PathTB.Text = track.Path;
				control.IdTB.Text = track.Id.ToString();
				control.AddToMultiB.Click += AddToMulti;
				if (i % 2 == 0)
					control.Background = new SolidColorBrush(Color.FromRgb(58, 58, 58));
				i++;
				if (AllTrackSP != null)
				{
					
					AllTrackSP.Children.Add(control);
				}
					
			}

			string json = JsonSerializer.Serialize(scannedTracks, new JsonSerializerOptions() { WriteIndented = true });

			using (StreamWriter writer = new StreamWriter("library.json"))
			{
				writer.WriteLine(json);
			}
		}
		private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			double volume = ((Slider)sender).Value;
			TrackPlayerControl controlClicked = new TrackPlayerControl();
			foreach (TrackPlayerControl trackPlayerControl in LoadedTrackSP.Children)
			{
				if (trackPlayerControl.VolumeSlider == (Slider)sender)
					controlClicked = trackPlayerControl;
			}
			int index = LoadedTrackSP.Children.IndexOf(controlClicked);
			if (CurrentMultiTrack != null)
				CurrentMultiTrack.Tracks[index].Volume = volume;
			MediaPlayers[index].Volume = volume / 100 * MainVolume /100;
		}

		[RelayCommand]
		public void PlayCurrentMulti()
		{
			foreach (MediaPlayer player in MediaPlayers)
			{
				player.Play();
			}
		}

		public void ChangeVolume(Object sender, EventArgs e)
		{
			for (int i = 0; i < MediaPlayers.Count; i++)
			{
				MediaPlayers[i].Volume = CurrentMultiTrack.Tracks[i].Volume / 100 * MainVolume / 100;
			}
		}
		[RelayCommand]
		public void PauseCurrentMulti()
		{
			foreach (MediaPlayer player in MediaPlayers)
			{
				player.Pause();
			}
		}

		public void FilterChanged(Object sender, EventArgs events)
		{
			if (AllTrackSP != null && AllTrackSP.Children.Count > 0)
				foreach (TrackControl control in AllTrackSP.Children)
				{
					var filter = ((TextBox)sender).Text.ToLower();
					if (!control.TrackNameTB.Text.ToLower().Contains(filter)&&!control.TagsTB.Text.ToLower().Contains(filter))
						control.Visibility = Visibility.Collapsed;
					else
						control.Visibility = Visibility.Visible;
				}
		}
		[RelayCommand]
		public void StopCurrentMulti()
		{
			foreach (MediaPlayer player in MediaPlayers)
			{
				player.Stop();
			}
		}

		private void MediaPlayer_MediaEnded(object sender, EventArgs e)
		{
			MediaPlayer mediaPlayer = (MediaPlayer)sender;
			mediaPlayer.Position = TimeSpan.Zero;
			mediaPlayer.Play();
		}

		[RelayCommand]
		void SelectPath()
		{
			FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
			if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
			{
				CurrentPath = folderBrowserDialog.SelectedPath;
				LoadTracks();
				SaveSettings(new Setting(CurrentPath));
			}
		}

		[RelayCommand]
		void SaveTags()
		{
			foreach (TrackControl trackControl in AllTrackSP.Children)
			{
				string path = trackControl.PathTB.Text;

				Track track = FoundTracks.Where(t => t.Path == path).First();
				track.Tags = trackControl.TagsTB.Text.Split(",").ToList();
				string json = JsonSerializer.Serialize(FoundTracks, new JsonSerializerOptions() { WriteIndented = true });
				TagLib.File f = TagLib.File.Create(trackControl.PathTB.Text);
				f.Tag.Title = trackControl.TrackNameTB.Text;
				try
				{
					f.Save();
				}
				catch(Exception ex)
				{

				}
				using (StreamWriter writer = new StreamWriter("library.json"))
				{
					writer.WriteLine(json);
				}
			}
		}
	}
}
