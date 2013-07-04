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
			win.Title ="Bayesian social joint matrix factorization recommender";
			Application.Run ();
		}
	}
}
