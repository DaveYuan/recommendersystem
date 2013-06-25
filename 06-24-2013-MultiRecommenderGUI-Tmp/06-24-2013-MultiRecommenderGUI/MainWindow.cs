using System;
using Gtk;

public partial class MainWindow: Gtk.Window
{	
	Gtk.Entry filterEntry;
	Gtk.TreeModelFilter filter;

	public MainWindow () : base (Gtk.WindowType.Toplevel)
	{
		this.SetSizeRequest (500,200);
		// Create an Entry used to filter the tree
		filterEntry = new Gtk.Entry ();

		// Fire off an event when the text in the Entry changes
		filterEntry.Changed += OnFilterEntryTextChanged;

		// Create a nice label describing the Entry
		Gtk.Label filterLabel = new Gtk.Label ("Artist Search:");

		// Put them both into a little box so they show up side by side
		Gtk.HBox filterBox = new Gtk.HBox ();
		filterBox.PackStart (filterLabel, false, false, 5);
		filterBox.PackStart (filterEntry, true, true, 5);

		// Create our TreeView
		Gtk.TreeView tree = new Gtk.TreeView ();
		Gtk.VBox box = new Gtk.VBox ();

		// Add the widgets to the box
		box.PackStart (filterBox, false, false, 5);
		box.PackStart (tree, true, true, 5);

		this.Add (box);
	//	this.Add (tree);

		// Create columns
		Gtk.TreeViewColumn artistColumn = new Gtk.TreeViewColumn();
		artistColumn.Title = "Artist";

		Gtk.TreeViewColumn predictionColumn = new Gtk.TreeViewColumn();
	    predictionColumn.Title = "Prediction";

		Gtk.TreeViewColumn ratingColumn = new Gtk.TreeViewColumn();
		ratingColumn.Title =  "Rating";

		// Add the columns to the TreeView
		tree.AppendColumn(artistColumn);
		tree.AppendColumn(predictionColumn);
		tree.AppendColumn(ratingColumn);

		// Create a model that will hold three strings - Artist, Prediction, Rating
		Gtk.ListStore musicListStore = new Gtk.ListStore (typeof (string), typeof(double), typeof (int));
		// Assign the model to the TreeView
		tree.Model = musicListStore;

		// Create the text cell that will display the artist name
		Gtk.CellRendererText artistCell = new Gtk.CellRendererText();
		artistColumn.PackStart(artistCell, true);
		Gtk.CellRendererText predictionCell = new Gtk.CellRendererText ();
		predictionColumn.PackStart(predictionCell, true);
		Gtk.CellRendererText ratingCell = new Gtk.CellRendererText ();
		ratingColumn.PackStart(ratingCell, true);

		artistColumn.AddAttribute(artistCell, "text", 0);
		predictionColumn.AddAttribute(predictionCell, "text", 1);
		ratingColumn.AddAttribute(ratingCell, "text", 2);
		musicListStore.AppendValues("Shivani", 4.5, 5);
		musicListStore.AppendValues("Divya", 4.5, 5);

		// Create the filter and tell it to use the musicListStore as it's base Model
		filter = new Gtk.TreeModelFilter (musicListStore, null);
		filter.VisibleFunc = new Gtk.TreeModelFilterVisibleFunc (FilterTree);
		tree.Model = filter;

		Build();
	}

	private void OnFilterEntryTextChanged (object o, System.EventArgs args)
	{
		// Since the filter text changed, tell the filter to re-determine which rows to display
		filter.Refilter ();
	}

	private bool FilterTree (Gtk.TreeModel model, Gtk.TreeIter iter)
	{
		string artistName = model.GetValue (iter, 0).ToString ();

		if (filterEntry.Text == "")
			return true;

		if (artistName.IndexOf (filterEntry.Text) > -1)
			return true;
		else
			return false;
	}

	protected void OnDeleteEvent (object sender, DeleteEventArgs a)
	{
		Application.Quit ();
		a.RetVal = true;
	}
}
