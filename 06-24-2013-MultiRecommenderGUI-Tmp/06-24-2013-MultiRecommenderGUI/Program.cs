using System;
using Gtk;

namespace MultiRecommenderGUI
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			Application.Init ();
			MainWindow win = new MainWindow ();
			win.Title = "BPR Social Joint MF";
			win.Show();

			Application.Run ();
		}
	}
}
