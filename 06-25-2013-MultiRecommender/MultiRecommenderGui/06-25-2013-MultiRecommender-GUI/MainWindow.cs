using Gtk;
using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Diagnostics;
using System.Globalization;
using System.Collections.Generic;
using MultiRecommender;
using MultiRecommender.IO;
using MultiRecommender.Evaluation;
using MultiRecommender.JointFactorization;
using MultiRecommenderGUI;

public partial class MainWindow: Window
{	
	public static int N_ARTIST = 100;
	public static int liveUserID;
	public static string modelFile;
	public static string artistFile;
	public static string ratingsFile;

	public static VBox vBox;
	public static HBox filterBox;
	public static Label label1;
	public static Entry entry1;
	public static TreeView treeView;

	public static TreeModelFilter preFilter;
	public static bool SHOW_ONLY_TOP_ARTISTS = true;

	public static TreeModelFilter nameFilter;
	public static TreeModelSort sorter;

	public static TreeViewColumn artistColumn = new TreeViewColumn();
	public static TreeViewColumn predictionColumn = new TreeViewColumn();
	public static TreeViewColumn ratingColumn = new TreeViewColumn();

	public static int[] usersArray;
	public static int[] itemsArray;
	public static int[] ratingsArray;

	LastFMArtistInfo artist = new LastFMArtistInfo();

	public static HashSet<int> topNArtist = new HashSet<int>();
	public static Dictionary<int, int> artistFrequency = new Dictionary<int, int>();
	public static Dictionary<int, double> ratings = new Dictionary<int, double>();
	public static Dictionary<int, double> predictions = new Dictionary<int, double>();

	public void assignDataPaths()
	{
		ratingsFile = "../../../../data/ratings.txt";
		artistFile = "../../../../data/artists.dat";
		modelFile = "../../../../models/BPRSocialJointMF.model";
	}

	public int drawLiveUserId()
	{
		return MultiRecommender.Init.random.Next(0, MultiRecommender.Init.MAX_USER_ID+1);
	}

	public void mergeTrainValidationTestSplits()
	{
		var users = new List<int>();
		users.AddRange(MultiRecommender.Init.trainUsersArray);
		users.AddRange(MultiRecommender.Init.validatationUsersArray);
		users.AddRange(MultiRecommender.Init.testUsersArray);

		var items = new List<int>();
		items.AddRange(MultiRecommender.Init.trainItemsArray);
		items.AddRange(MultiRecommender.Init.validationItemsArray);
		items.AddRange(MultiRecommender.Init.testItemsArray);

		var ratings = new List<int>();
		ratings.AddRange(MultiRecommender.Init.trainRatingsArray);
		ratings.AddRange(MultiRecommender.Init.validationRatingsArray);
		ratings.AddRange(MultiRecommender.Init.testRatingsArray);

		usersArray = users.ToArray();
		itemsArray = items.ToArray();
		ratingsArray = ratings.ToArray();
	}

	public void fetchTopNArtist()
	{
		int numArtist = itemsArray.Length;

		for (int i = 0; i < numArtist; i++) {
			if (!artistFrequency.ContainsKey(itemsArray[i])) {
				artistFrequency.Add(itemsArray[i], 0);
			}
			artistFrequency[itemsArray[i]] = artistFrequency[itemsArray[i]] + 1;
		}

		var topN = from pair in artistFrequency 
				   orderby pair.Value descending
				   select pair.Key;

		for (int i = 0; i < N_ARTIST; i++) {
			topNArtist.Add(topN.ElementAt(i));
		}
	}
	
	public void predictRatingsLiveUser()
	{
		Console.WriteLine("\t- Predicting ratings for live user");
		for (int i = 0; i <= MultiRecommender.Init.MAX_ITEM_ID; i++) {
			predictions[i] = Eval.PredictRating(liveUserID, i);
		}
	}

	public void fetchArtistColumn(TreeViewColumn column, CellRenderer cell, TreeModel model, TreeIter iter)
	{
		Artist artist = (Artist) model.GetValue(iter, 0);
		string name = artist.name;
		(cell as CellRendererText).Text = name;
	}

	public void fetchPredictionColumn(TreeViewColumn column, CellRenderer cell, TreeModel model, TreeIter iter)
	{
		Artist artist = (Artist)model.GetValue(iter, 0);

		double prediction;

		if (!predictions.TryGetValue(artist.id, out prediction)) {
			Console.WriteLine("ArtistID not found");
		}

		if (ratings.ContainsKey(artist.id))
			prediction = ratings[artist.id];

		string text;
		if (prediction < MultiRecommender.Init.MIN_RATING)
			text = "";
		else if (prediction < 1.5)
			text = string.Format(CultureInfo.InvariantCulture, "{0,0:0.00} ★", prediction);
		else if (prediction < 2.5)
			text = string.Format(CultureInfo.InvariantCulture, "{0,0:0.00} ★★", prediction);
		else if (prediction < 3.5)
			text = string.Format(CultureInfo.InvariantCulture, "{0,0:0.00} ★★★", prediction);
		else if (prediction < 4.5)
			text = string.Format(CultureInfo.InvariantCulture, "{0,0:0.00} ★★★★", prediction);
		else
			text = string.Format(CultureInfo.InvariantCulture, "{0,0:0.00} ★★★★★", prediction);

		(cell as CellRendererText).Text = text;
	}

	public void fetchRatingColumn(TreeViewColumn column, CellRenderer cell, TreeModel model, TreeIter iter)
	{
		Artist artist = (Artist) model.GetValue(iter, 0);
		double rating = -1;

		if (ratings.TryGetValue(artist.id, out rating))
			(cell as CellRendererText).Text = string.Format(CultureInfo.InvariantCulture, "{0}", rating);
		else
			(cell as CellRendererText).Text = string.Empty;
	}


	private void OnFilterEntryTextChanged (object o, System.EventArgs args)
	{
		// Since the filter text changed, tell the filter to re-determine which rows to display
		nameFilter.Refilter ();
	}

	private bool PreFilter(TreeModel model, TreeIter iter)
	{
		Artist artist = (Artist) model.GetValue(iter, 0);
		if (SHOW_ONLY_TOP_ARTISTS && !topNArtist.Contains(artist.id)) {
			return false;
		}
		return true;
	}

	private bool FilterByName(TreeModel model, TreeIter iter)
	{
		Artist artist = (Artist) model.GetValue(iter, 0);
		string artistName = artist.name;

		if (entry1.Text == string.Empty)
			return true;

		if (artistName.Contains(entry1.Text))
			return true;
		else 
			return false;				                
	}

	public int predictionsReversed(TreeModel model, TreeIter a, TreeIter b)
	{
		Artist artist1 = (Artist) model.GetValue(a, 0);
		Artist artist2 = (Artist) model.GetValue(b, 0);

		double prediction1 = -1;
		predictions.TryGetValue(artist1.id, out prediction1);
		double prediction2 = -1;
		predictions.TryGetValue(artist2.id, out prediction2);

		double diff = prediction2 - prediction1;

		if (diff > 0) {
			return 1;
		}
		if (diff < 0) {
			return -1;
		}
		return 0;
	}

	public void createGUI()
	{
		vBox = new VBox ();
		filterBox = new HBox ();
		treeView = new TreeView ();

		entry1 = new Entry ();
		entry1.Changed += OnFilterEntryTextChanged;
		label1 = new Label ("Artist Search:");
	
		// create a column for the prediction
		var predictionCell = new CellRendererText();
		predictionColumn.PackStart(predictionCell, true);
		predictionColumn.SortIndicator = true;
		predictionColumn.Clickable = true;
//		predictionColumn.Clicked += new EventHandler(PredictionColumnClicked);

		// create a column for the rating
		var ratingCell = new CellRendererText();
		ratingCell.Editable = true;
//		ratingCell.Edited += RatingCellEdited;
		ratingColumn.PackStart(ratingCell, true);
		ratingColumn.SortIndicator = true;
		ratingColumn.Clickable = true;
//		ratingColumn.Clicked += new EventHandler( RatingColumnClicked );

		// set up a column for the movie title
		var artistCell = new CellRendererText();
		artistColumn.PackStart(artistCell, true);
		artistColumn.SortIndicator = true;
		artistColumn.Clickable = true;
//		artistColumn.Clicked += new EventHandler( MovieColumnClicked );
	
		// Add columns to treeView
		treeView.AppendColumn(artistColumn);
		treeView.AppendColumn(predictionColumn);
		treeView.AppendColumn(ratingColumn);

		predictionColumn.SetCellDataFunc(predictionCell, new TreeCellDataFunc(fetchPredictionColumn));
		ratingColumn.SetCellDataFunc(ratingCell, new TreeCellDataFunc(fetchRatingColumn));
		artistColumn.SetCellDataFunc(artistCell, new TreeCellDataFunc(fetchArtistColumn));
	
//		artistColumn.AddAttribute(artistCell, "text", 0);
//		predictionColumn.AddAttribute(predictionCell, "text", 1);
//		ratingColumn.AddAttribute(ratingCell, "text", 2);

		var artistStore = new ListStore(typeof(Artist));

		foreach (Artist ar in artist.artistList) {	
		//	for (int i = 0; i < 5; i++){
			artistStore.AppendValues(ar);
		}

		preFilter = new TreeModelFilter(artistStore, null);
		preFilter.VisibleFunc = new TreeModelFilterVisibleFunc(PreFilter);

		nameFilter = new TreeModelFilter(preFilter, null);
		nameFilter.VisibleFunc =  new TreeModelFilterVisibleFunc(FilterByName);

		sorter = new TreeModelSort(nameFilter);
		sorter.DefaultSortFunc = predictionsReversed;


		treeView.Model = sorter;	
		treeView.ShowAll();

		// Add the widgets to the box
		filterBox.PackStart (label1, false, false, 5);
		filterBox.PackStart (entry1, true, true, 5);
		ScrolledWindow sc = new ScrolledWindow();
		sc.Add(treeView);

		// Add the widgets to the box
		vBox.PackStart (filterBox, false, false, 5);
		vBox.PackStart (sc, true, true, 5);
		

		this.Add(vBox);
	}

	public MainWindow (): base (Gtk.WindowType.Toplevel)
	{
		Stopwatch mergeTime = new Stopwatch();

		assignDataPaths();
		MultiRecommenderMain.loadDatasetToMemory();
		MultiRecommenderMain.init();

		mergeTime.Start();
			mergeTrainValidationTestSplits();
		mergeTime.Stop();

		Console.WriteLine("\t- Time(merge-splits): {0}", mergeTime.Elapsed);

		Console.WriteLine("\t- Read ArtistInfo");
		artist.readArtistInfo(new StreamReader("../../../../data/artists.dat"));

		BprSocialJointMF socialReccomender = new BprSocialJointMF(MultiRecommenderMain.associationObj);
		fetchTopNArtist();

		liveUserID = drawLiveUserId();
		predictRatingsLiveUser();
		Console.WriteLine("\t\t- Live User operating this demo: {0}", liveUserID);

		createGUI();	
		Build ();
	}

	protected void OnDeleteEvent (object sender, DeleteEventArgs a)
	{
		Application.Quit ();
		a.RetVal = true;
	}
}