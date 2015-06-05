using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using Newtonsoft.Json;

namespace issOS
{
	/// <summary>
	/// Interaction logic for SpotifyWindow.xaml
	/// </summary>
	public partial class SpotifyWindow : Window
	{
		public SpotifyWindow()
		{
			InitializeComponent();
		}

		protected override void OnSourceInitialized(EventArgs e)
		{
			base.OnSourceInitialized(e);
			var hwnd = new WindowInteropHelper(this).Handle;
			WindowsServices.SetWindowExTransparent(hwnd);
		}

		Thread modCheck;

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			modCheck = new Thread(CheckKeyBoard);
			modCheck.SetApartmentState(ApartmentState.STA);
			modCheck.Start();
			spotifyBox.Text = "Spotify" + Environment.NewLine + "Stopped";
		}

		string token = "";
		string csrf = "";

		private void CheckKeyBoard()
		{
			while (true)
			{
				var modenabled = Keyboard.Modifiers != (ModifierKeys.Shift & ModifierKeys.Alt);
				RunOnUiThread((Action)delegate
				{
					var hwnd = new WindowInteropHelper(this).Handle;
					if (modenabled)
						WindowsServices.makeNormal(hwnd);
					else
						WindowsServices.SetWindowExTransparent(hwnd);
				});
				Thread.Sleep(1000);
				var currentSong = "Stopped";

				try
				{
					if (token == "")
					{
						using (WebClient wc = new WebClient())
						{
							var js = wc.DownloadString("http://open.spotify.com/token");

							var o = JsonConvert.DeserializeObject<SpotifyToken>(js);

							token = o.t;
						}
					}

					if (csrf == "")
					{
						using (WebClient wc = new WebClient())
						{
							wc.Headers.Add("Origin", "https://open.spotify.com");
							wc.Headers.Add("Referer", "https://open.spotify.com/remote");
							var js = wc.DownloadString("https://asjdasdkb.spotilocal.com:4370/simplecsrf/token.json");

							var o = JsonConvert.DeserializeObject<Dictionary<string, string>>(js);
							csrf = o["token"].ToString();
						}
					}

					var url = "https://asjdasdkb.spotilocal.com:4370/remote/status.json?csrf=" + csrf + "&oauth=" + token + "&returnon=login%2Clogout%2Cplay%2Cpause%2Cerror%2Cap&returnafter=1&ref=http%3A%2F%2Fopen.spotify.com%2Ftrack%2F20LKA97m0dz1u7l8aMBGqK&cors=";
					using (WebClient wc = new WebClient())
					{
						wc.Headers.Add("Origin", "https://open.spotify.com");
						wc.Headers.Add("Referer", "https://open.spotify.com/remote");
						var str = wc.DownloadString(url).Replace("\n", "");
						if (str.ToLower().Contains("expired token"))
						{
							token = "";
						}

						var o = JsonConvert.DeserializeObject<SpotifyREST>(str);
						if (o != null)
						{
							if (o.Playing)
							{
								if (o.Track != null)
								{
									if (o.Track.Artist_Resource != null)
									{
										if (o.Track.Track_Resource != null)
										{
											currentSong = o.Track.Artist_Resource.Name + " - " + o.Track.Track_Resource.Name;/* +
											" (" + TimeSpan.FromSeconds(o.Playing_Position).ToString("mm\\:ss") + "/" + TimeSpan.FromSeconds(o.Track.Length).ToString("mm\\:ss") + ") " +
											(o.Track.Track_Type != "local" ? "[sptfy: " + o.Track.Track_Resource.Location.Og + " ]" : ""));*/
										}
									}

								}
							}
							else
							{
								currentSong = "Stopped";
							}
						}
					}
				}
				catch
				{
				}

				spotifyBox.Dispatcher.Invoke((Action)delegate
				{
					spotifyBox.Text = "Spotify" + Environment.NewLine + currentSong;
				});
			}
		}


		private void ipBox_MouseMove(object sender, MouseEventArgs e)
		{
			var modenabled = Keyboard.Modifiers != (ModifierKeys.Shift & ModifierKeys.Alt);

			if (modenabled && e.LeftButton == MouseButtonState.Pressed)
			{
				spotifyWin.DragMove();
				RotateWindowSpherical();
			}
		}

		private void RotateWindowSpherical()
		{
		}

		public object RunOnUiThread(Delegate method)
		{
			return Dispatcher.Invoke(DispatcherPriority.Normal, method);
		}
	}

	public class SpotifyToken
	{
		public string t { get; set; }
	}

	public class SpotifyREST
	{
		public int Version { get; set; }
		public string Client_Version { get; set; }
		public SpotifyTrack Track { get; set; }
		public bool Playing { get; set; }
		public bool Shuffle { get; set; }
		public bool Repeat { get; set; }
		public bool Play_Enabled { get; set; }
		public bool Prev_Enabled { get; set; }
		public bool Next_Enabled { get; set; }
		public double Playing_Position { get; set; }
		public long Server_Time { get; set; }
		public double Volume { get; set; }
		public bool Running { get; set; }
	}

	public class SpotifyTrack
	{
		public SpotifyTrackResource Track_Resource { get; set; }
		public SpotifyTrackResource Artist_Resource { get; set; }
		public SpotifyTrackResource Album_Resource { get; set; }
		public int Length { get; set; }
		public string Track_Type { get; set; }
	}

	public class SpotifyTrackResource
	{
		public string Name { get; set; }
		public string Uri { get; set; }
		public SpotifyLocation Location { get; set; }
	}

	public class SpotifyLocation
	{
		public string Og { get; set; }
	}
}
