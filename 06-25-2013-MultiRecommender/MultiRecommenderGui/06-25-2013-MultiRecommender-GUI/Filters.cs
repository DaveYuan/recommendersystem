using Gtk;
using System;

namespace MultiRecommenderGUI
{
	public class Filters
	{
		public static void OnFilterEntryTextChanged (object o, System.EventArgs args)
		{
			// Since the filter text changed, tell the filter to re-determine which rows to display
			MainWindow.nameFilter.Refilter ();
		}

		public static bool PreFilter(TreeModel model, TreeIter iter)
		{
			Artist artist = (Artist) model.GetValue(iter, 0);
			if (MainWindow.SHOW_ONLY_TOP_ARTISTS && !MainWindow.topNArtist.Contains(artist.id)) {
				return false;
			}
			return true;
		}

		public static bool FilterByName(TreeModel model, TreeIter iter)
		{
			Artist artist = (Artist) model.GetValue(iter, 0);
			string artistName = artist.name;

			if (MainWindow.entry1.Text == string.Empty)
				return true;

			if (artistName.Contains(MainWindow.entry1.Text))
				return true;
			else 
				return false;				                
		}
	}
}

