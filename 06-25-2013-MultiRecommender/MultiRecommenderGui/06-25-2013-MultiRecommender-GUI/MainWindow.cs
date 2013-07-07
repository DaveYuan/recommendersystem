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
	public static TreeView friendsView;
	public static TreeView usersView;

	public static TreeModelFilter preFilter;
	public static bool SHOW_ONLY_TOP_ARTISTS = true;

	public static TreeModelFilter nameFilter;
	public static TreeModelSort sorter1;
	public static TreeModelSort sorter2;
	public static TreeModelSort sorter3;

	public static TreeViewColumn artistColumn = new TreeViewColumn();
	public static TreeViewColumn predictionColumn = new TreeViewColumn();
	public static TreeViewColumn ratingColumn = new TreeViewColumn();
	public static TreeViewColumn friendsColumn = new TreeViewColumn();
	public static TreeViewColumn usersColumn = new TreeViewColumn();

	public static List<int> usersList;
	public static List<int> artistsList;
	public static List<int> ratingsList;
	public static List<int> nonFriendsList;
		
	public static ListStore artistStore = new ListStore(typeof(Artist));
	public static ListStore friendsStore = new ListStore(typeof(int));
	public static ListStore usersStore = new ListStore(typeof(int));

	LastFMArtistInfo artist = new LastFMArtistInfo();

	public static HashSet<int> topNArtist = new HashSet<int>();
	public static Dictionary<int, int> artistFrequency = new Dictionary<int, int>();
	public static Dictionary<int, double> ratings = new Dictionary<int, double>();
	public static Dictionary<int, double> predictions = new Dictionary<int, double>();

	public void assignDataPaths()
	{
		ratingsFile = "../../../../data/ratings.txt";
		artistFile = "../../../../data/artists.dat";
		modelFile = "BPRSocialJointMF.model";
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
	
	public static void predictRatingsLiveUser()
	{
		Console.WriteLine("\t- Predicting ratings for live user");
		for (int i = 0; i <= Initialize.MAX_ITEM_ID; i++) {
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

	public void fetchFriendsColumn(TreeViewColumn column, CellRenderer cell, TreeModel model, TreeIter iter)
	{
		int user = (int) model.GetValue(iter, 0);
		(cell as CellRendererText).Text = string.Format(CultureInfo.InvariantCulture, "{0}", user);
	}

	public void fetchUsersColumn(TreeViewColumn column, CellRenderer cell, TreeModel model, TreeIter iter)
	{
		int user = (int) model.GetValue(iter, 0);
		(cell as CellRendererText).Text = string.Format(CultureInfo.InvariantCulture, "{0}", user);
	}

	public void createGUI()
	{
		vBox = new VBox ();
		filterBox = new HBox ();
		treeView = new TreeView ();
		friendsView = new TreeView();
		usersView = new TreeView();
		nonFriendsList = new List<int>();

		entry1 = new Entry ();
		entry1.Changed += Filters.OnFilterEntryTextChanged;
		label1 = new Label ("Artist Search:");
	
		// create a column for the prediction
		var predictionCell = new CellRendererText();
		predictionColumn.PackStart(predictionCell, true);
		predictionColumn.SortIndicator = true;
		predictionColumn.Clickable = true;
		predictionColumn.Clicked += new EventHandler(ColumnEventHandler.PredictionColumnClicked);

		// create a column for the rating
		var ratingCell = new CellRendererText();
		ratingCell.Editable = true;
		ratingCell.Edited += ColumnEventHandler.RatingCellEdited;
		ratingColumn.PackStart(ratingCell, true);
		ratingColumn.SortIndicator = true;
		ratingColumn.Clickable = true;
		ratingColumn.Clicked += new EventHandler(ColumnEventHandler.RatingColumnClicked);

		// set up a column for the movie title
		var artistCell = new CellRendererText();
		artistColumn.PackStart(artistCell, true);
		artistColumn.SortIndicator = true;
		artistColumn.Clickable = true;
		artistColumn.Clicked += new EventHandler(ColumnEventHandler.ArtistColumnClicked);

		var friendsCell = new CellRendererText();
		friendsColumn.PackStart(friendsCell, true);
		friendsColumn.SortIndicator = true;
		friendsColumn.Clickable = true;
		friendsColumn.Clicked += new EventHandler(ColumnEventHandler.FriendsColumnClicked);

		var usersCell = new CellRendererText();
		usersColumn.PackStart(usersCell, true);
		usersColumn.SortIndicator = true;
		usersColumn.Clickable = true;
		usersColumn.Clicked += new EventHandler(ColumnEventHandler.UsersColumnClicked);

		// Add columns to treeView
		treeView.AppendColumn(artistColumn);
		treeView.AppendColumn(predictionColumn);
		treeView.AppendColumn(ratingColumn);
		friendsView.AppendColumn(friendsColumn);
		usersView.AppendColumn(usersColumn);

		predictionColumn.SetCellDataFunc(predictionCell, new TreeCellDataFunc(fetchPredictionColumn));
		ratingColumn.SetCellDataFunc(ratingCell, new TreeCellDataFunc(fetchRatingColumn));
		artistColumn.SetCellDataFunc(artistCell, new TreeCellDataFunc(fetchArtistColumn));
		friendsColumn.SetCellDataFunc(friendsCell, new TreeCellDataFunc(fetchFriendsColumn));
		usersColumn.SetCellDataFunc(usersCell, new TreeCellDataFunc(fetchUsersColumn));

		predictionColumn.Title = "Prediction";
		ratingColumn.Title = "Rating";
		artistColumn.Title = "Artist";
		friendsColumn.Title = "Friends";
		usersColumn.Title = "Make friend";

		predictionColumn.Resizable = true;
		ratingColumn.Resizable = true;
		artistColumn.Resizable = true;

		foreach (Artist ar in artist.artistList) {	
			artistStore.AppendValues(ar);
		}

		var friends = Recommender.userSymmetricAssociations[liveUserID];
		foreach (int user in friends) {
			friendsStore.AppendValues(user);
		}

		HashSet<int> users = new HashSet<int>(usersList);
		foreach (int user in users) {
			if (!friends.Contains(user)) {
				usersStore.AppendValues(user);
				nonFriendsList.Add(user);
//				Console.WriteLine(user);
			}
		}

		preFilter = new TreeModelFilter(artistStore, null);
		preFilter.VisibleFunc = new TreeModelFilterVisibleFunc(Filters.PreFilter);

		nameFilter = new TreeModelFilter(preFilter, null);
		nameFilter.VisibleFunc =  new TreeModelFilterVisibleFunc(Filters.FilterByName);

		sorter1 = new TreeModelSort(nameFilter);
		sorter1.DefaultSortFunc = ColumnEventHandler.ComparePredictionReversed;
		sorter2 = new TreeModelSort(friendsStore);
		sorter2.DefaultSortFunc = ColumnEventHandler.CompareUser;
		sorter3 = new TreeModelSort(usersStore);
		sorter3.DefaultSortFunc = ColumnEventHandler.CompareUser;

		treeView.Model = sorter1;
		friendsView.Model = sorter2;
		usersView.Model = sorter3;
	
		treeView.ColumnsAutosize();
		friendsView.ColumnsAutosize();
		usersView.ColumnsAutosize();

		usersView.CursorChanged += new EventHandler(ColumnEventHandler.MakeFriend);

		treeView.ShowAll();
		friendsView.ShowAll();
		usersView.ShowAll();

		// Add the widgets to the box
		filterBox.PackStart (label1, false, false, 5);
		filterBox.PackStart (entry1, true, true, 5);
		ScrolledWindow sc1 = new ScrolledWindow();
		ScrolledWindow sc2 = new ScrolledWindow();
		ScrolledWindow sc3 = new ScrolledWindow();

		sc1.Add(treeView);
		sc2.Add(friendsView);
		sc3.Add(usersView);

		HBox hbox = new HBox();
		hbox.PackStart(sc1);
		hbox.PackStart(sc2);
		hbox.PackStart(sc3);

		// Add the widgets to the box
		vBox.PackStart (filterBox, false, false, 5);
		vBox.PackStart (hbox, true, true, 5);		

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
		artist.readArtistInfo(new StreamReader(artistFile));

		Recommender recommender = new Recommender(MultiRecommenderMain.associationObj);

		var reader = Model.getReader(modelFile);
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