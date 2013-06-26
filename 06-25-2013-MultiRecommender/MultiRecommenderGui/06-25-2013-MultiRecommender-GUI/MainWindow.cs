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

//TODO: Fix problem of prediction column not getting sorted on click

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

	public static List<int> usersList;
	public static List<int> artistsList;
	public static List<int> ratingsList;

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
		return Initialize.random.Next(0, Initialize.MAX_USER_ID+1);
	}

	public void mergeTrainValidationTestSplits()
	{
		usersList = new List<int>();
		usersList.AddRange(Initialize.trainUsersArray);
		usersList.AddRange(Initialize.validatationUsersArray);
		usersList.AddRange(Initialize.testUsersArray);

		artistsList = new List<int>();
		artistsList.AddRange(Initialize.trainItemsArray);
		artistsList.AddRange(Initialize.validationItemsArray);
		artistsList.AddRange(Initialize.testItemsArray);

		ratingsList = new List<int>();
		ratingsList.AddRange(Initialize.trainRatingsArray);
		ratingsList.AddRange(Initialize.validationRatingsArray);
		ratingsList.AddRange(Initialize.testRatingsArray);
	}

	public int[] FindAllIndexof(int[] values, int val)
	{
		return values.Select((b,i) => object.Equals(b, val) ? i : -1).Where(i => i != -1).ToArray();
	}


	public void fetchTopNArtist()
	{
		int numArtist = artistsList.Count;

		for (int i = 0; i < numArtist; i++) {
			if (!artistFrequency.ContainsKey(artistsList[i])) {
				artistFrequency.Add(artistsList[i], 0);
			}
			artistFrequency[artistsList[i]] = artistFrequency[artistsList[i]] + 1;
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
		for (int i = 0; i <= Initialize.MAX_ITEM_ID; i++) {
			predictions[i] = Eval.PredictRating(liveUserID, i);
		}
	}

//	public void ratingsLiveUser()
//	{
//		Console.WriteLine("Fetching ratings given by live user");
//		var indices = FindAllIndexof(usersList, liveUserID);
//		foreach(int indx in indices) { 
//			ratings.Add(artistsArray[indx], ratingsArray[indx]);
//		}
//	}

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

		prediction += 2*Initialize.globalAvg;
		double sigScore = Initialize.g(prediction);
		prediction = Initialize.MIN_RATING + sigScore * 
			(Initialize.MAX_RATING - Initialize.MIN_RATING);
				
		if (ratings.ContainsKey(artist.id)) {
			predictions[artist.id] = ratings[artist.id];
			prediction = ratings[artist.id];			                
		}

		string text;
		if (prediction < Initialize.MIN_RATING)
			text = "";
		//	text = string.Format(CultureInfo.InvariantCulture, "{0,0:0.00} ", prediction);
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

	//TODO: Merge sorting functions
	private int CompareTitleReversed(TreeModel model, TreeIter a, TreeIter b)
	{
		Artist artist1 = (Artist) model.GetValue(a, 0);
		Artist artist2 = (Artist) model.GetValue(b, 0);

		return string.Compare(artist2.name, artist1.name);
	}

	private int CompareTitle(TreeModel model, TreeIter a, TreeIter b)
	{
		Artist artist1 = (Artist) model.GetValue(a, 0);
		Artist artist2 = (Artist) model.GetValue(b, 0);

		return string.Compare(artist1.name, artist2.name);
	}


	private void ArtistColumnClicked(object o, EventArgs args)
	{
		if (artistColumn.SortOrder == SortType.Ascending)
		{
			artistColumn.SortOrder = SortType.Descending;
			sorter.DefaultSortFunc = CompareTitleReversed;
		}
		else
		{
			artistColumn.SortOrder = SortType.Ascending;
			sorter.DefaultSortFunc = CompareTitle;
		}
	}


	private int ComparePrediction(TreeModel model, TreeIter a, TreeIter b)
	{
		Artist artist1 = (Artist) model.GetValue(a, 0);
		Artist artist2 = (Artist) model.GetValue(b, 0);
		
		double prediction1 = -1;
		predictions.TryGetValue(artist1.id, out prediction1);
		double prediction2 = -1;
		predictions.TryGetValue(artist2.id, out prediction2);

		double diff = prediction1 - prediction2;

		if (diff > 0)
			return 1;
		if (diff < 0)
			return -1;
		return 0;
	}

	public int ComparePredictionReversed(TreeModel model, TreeIter a, TreeIter b)
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

	private void PredictionColumnClicked(object o, EventArgs args)
	{
		if (predictionColumn.SortOrder == SortType.Ascending)
		{
			predictionColumn.SortOrder = SortType.Descending;
			sorter.DefaultSortFunc = ComparePrediction;
		}
		else
		{
			predictionColumn.SortOrder = SortType.Ascending;
			sorter.DefaultSortFunc = ComparePredictionReversed;
		}
	}
	
	private int CompareRating(TreeModel model, TreeIter a, TreeIter b)
	{
		Artist artist1 = (Artist) model.GetValue(a, 0);
		Artist artist2 = (Artist) model.GetValue(b, 0);

		double rating1;
		ratings.TryGetValue(artist1.id, out rating1);
		double rating2;
		ratings.TryGetValue(artist2.id, out rating2);

		double diff = rating1 - rating2;

		if (diff > 0)
			return 1;
		if (diff < 0)
			return -1;
		return 0;
	}

	private int CompareRatingReversed(TreeModel model, TreeIter a, TreeIter b)
	{
		Artist artist1 = (Artist) model.GetValue(a, 0);
		Artist artist2 = (Artist) model.GetValue(b, 0);

		double rating1;	
		ratings.TryGetValue(artist1.id, out rating1);
		double rating2;
		ratings.TryGetValue(artist2.id, out rating2);

		double diff = rating2 - rating1;

		if (diff > 0)
			return 1;
		if (diff < 0)
			return -1;
		return 0;
	}

	private void RatingColumnClicked(object o, EventArgs args)
	{
		if (ratingColumn.SortOrder == SortType.Ascending)
		{
			ratingColumn.SortOrder = SortType.Descending;
			sorter.DefaultSortFunc = CompareRating;
		}
		else
		{
			ratingColumn.SortOrder = SortType.Ascending;
			sorter.DefaultSortFunc = CompareRatingReversed;
		}
	}

	private void RatingCellEdited(object o, EditedArgs args)
	{
		TreeIter iter;
		treeView.Model.GetIter(out iter, new TreePath(args.Path));

		Artist artist = (Artist) treeView.Model.GetValue(iter, 0);
		string input = args.NewText.Trim();

		if (input == string.Empty)
		{
			Console.WriteLine("Remove rating.");
			if (ratings.Remove(artist.id)) {
				RecommenderExtensions.removeRating(liveUserID, artist.id);
			}

			predictRatingsLiveUser();
			return;
		}

		try
		{
			double rating = double.Parse(input);
			if (rating > Initialize.MAX_RATING)
				rating = Initialize.MAX_RATING;
			if (rating < Initialize.MIN_RATING)
				rating = Initialize.MIN_RATING;

			if (ratings.ContainsKey(artist.id)) {
				RecommenderExtensions.removeRating(liveUserID, artist.id);
			}

			RecommenderExtensions.addRating(liveUserID, artist.id, (int)rating);
			ratings[artist.id] = rating;

			predictRatingsLiveUser();
		}
		catch (FormatException)
		{
			Console.Error.WriteLine("Could not parse input '{0}' as a number.", input);
		}
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
		predictionColumn.Clicked += new EventHandler(PredictionColumnClicked);

		// create a column for the rating
		var ratingCell = new CellRendererText();
		ratingCell.Editable = true;
		ratingCell.Edited += RatingCellEdited;
		ratingColumn.PackStart(ratingCell, true);
		ratingColumn.SortIndicator = true;
		ratingColumn.Clickable = true;
		ratingColumn.Clicked += new EventHandler(RatingColumnClicked);

		// set up a column for the movie title
		var artistCell = new CellRendererText();
		artistColumn.PackStart(artistCell, true);
		artistColumn.SortIndicator = true;
		artistColumn.Clickable = true;
		artistColumn.Clicked += new EventHandler(ArtistColumnClicked);
	
		// Add columns to treeView
		treeView.AppendColumn(artistColumn);
		treeView.AppendColumn(predictionColumn);
		treeView.AppendColumn(ratingColumn);

		predictionColumn.SetCellDataFunc(predictionCell, new TreeCellDataFunc(fetchPredictionColumn));
		ratingColumn.SetCellDataFunc(ratingCell, new TreeCellDataFunc(fetchRatingColumn));
		artistColumn.SetCellDataFunc(artistCell, new TreeCellDataFunc(fetchArtistColumn));

		predictionColumn.Title = "Prediction";
		ratingColumn.Title = "Rating";
		artistColumn.Title = "Artist";

		predictionColumn.Resizable = true;
		ratingColumn.Resizable = true;
		artistColumn.Resizable = true;

		var artistStore = new ListStore(typeof(Artist));

		foreach (Artist ar in artist.artistList) {	
			artistStore.AppendValues(ar);
		}

		preFilter = new TreeModelFilter(artistStore, null);
		preFilter.VisibleFunc = new TreeModelFilterVisibleFunc(PreFilter);

		nameFilter = new TreeModelFilter(preFilter, null);
		nameFilter.VisibleFunc =  new TreeModelFilterVisibleFunc(FilterByName);

		sorter = new TreeModelSort(nameFilter);
		sorter.DefaultSortFunc = ComparePredictionReversed;


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

	public MainWindow (): base (WindowType.Toplevel)
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

		Recommender recommender = new Recommender(MultiRecommenderMain.associationObj);

		var reader = Model.getReader("BPRSocialJointMF.model");
		Model.loadModel(reader);

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