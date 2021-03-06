﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace issOS
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		protected override void OnStartup(StartupEventArgs e)
		{
			ClockWindow cw = new ClockWindow();
			cw.Show();

			MainWindow mw = new MainWindow();
			mw.Show();

			SpotifyWindow sw = new SpotifyWindow();
			sw.Show();

			base.OnStartup(e);
		}
	}
}
