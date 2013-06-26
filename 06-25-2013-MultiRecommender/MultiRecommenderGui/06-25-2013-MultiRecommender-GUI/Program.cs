using System;
using Gtk;

namespace MultiRecommenderGUI
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			Application.Init();

			MainWindow win = new MainWindow ();
			win.Show ();
			win.Title ="Social BPR-MF Recommender";
			Application.Run ();
		}
	}
}
