using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace issOS
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
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
			ipBox.Text = Environment.UserName;
			LoadIPAddress();
		}

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
				Thread.Sleep(100);
			}
		}

		private async Task LoadIPAddress()
		{
			using (WebClient wc = new WebClient())
			{
				string ipAddress = await wc.DownloadStringTaskAsync("http://icanhazip.com/");
				ipBox.Text = ipAddress.Trim();
			}
		}

		private void ipBox_MouseMove(object sender, MouseEventArgs e)
		{
			var modenabled = Keyboard.Modifiers != (ModifierKeys.Shift & ModifierKeys.Alt);

			if (modenabled && e.LeftButton == MouseButtonState.Pressed)
			{
				mainWin.DragMove();
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

	public static class WindowsServices
	{
		const int WS_EX_TRANSPARENT = 0x00000020;
		const int GWL_EXSTYLE = (-20);

		[DllImport("user32.dll")]
		static extern int GetWindowLong(IntPtr hwnd, int index);

		[DllImport("user32.dll")]
		static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);

		public static void SetWindowExTransparent(IntPtr hwnd)
		{
			var extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
			SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_TRANSPARENT);
		}

		public static void makeNormal(IntPtr hwnd)
		{
			int extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
			SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle & ~WS_EX_TRANSPARENT);
		}
	}
}
