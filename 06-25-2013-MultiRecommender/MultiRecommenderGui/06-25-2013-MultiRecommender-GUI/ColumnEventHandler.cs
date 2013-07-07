using Gtk;
using System;
using MultiRecommender;
using MultiRecommender.IO;
using MultiRecommender.Evaluation;
using MultiRecommender.JointFactorization;

namespace MultiRecommenderGUI
{
	public class ColumnEventHandler
	{
		public static void RatingCellEdited(object o, EditedArgs args)
		{
			TreeIter iter;
			MainWindow.treeView.Model.GetIter(out iter, new TreePath(args.Path));

			Artist artist = (Artist) MainWindow.treeView.Model.GetValue(iter, 0);
			string input = args.NewText.Trim();

			if (input == string.Empty)
			{
				Console.WriteLine("Remove rating.");
				if (MainWindow.ratings.Remove(artist.id)) {
					RecommenderExtensions.removeRating(MainWindow.liveUserID, artist.id);
				}

				MainWindow.predictRatingsLiveUser();
				return;
			}

			try
			{
				double rating = double.Parse(input);
				if (rating > Initialize.MAX_RATING)
					rating = Initialize.MAX_RATING;
				if (rating < Initialize.MIN_RATING)
					rating = Initialize.MIN_RATING;

				if (MainWindow.ratings.ContainsKey(artist.id)) {
					RecommenderExtensions.removeRating(MainWindow.liveUserID, artist.id);
				}

				RecommenderExtensions.addRating(MainWindow.liveUserID, artist.id, (int)rating);
				MainWindow.ratings[artist.id] = rating;

				MainWindow.predictRatingsLiveUser();
			}
			catch (FormatException)
			{
				Console.Error.WriteLine("Could not parse input '{0}' as a number.", input);
			}
		}

		
		public static void MakeFriend(object o, EventArgs args)
		{
			TreeIter iter;
			TreePath path;
			TreeViewColumn col;

			MainWindow.usersView.GetCursor(out path, out col);
			MainWindow.usersView.Model.GetIter(out iter, path);

			int user = (int) MainWindow.usersView.Model.GetValue(iter, 0);

			MainWindow.friendsStore.AppendValues(user);
			Console.WriteLine("New association: {0}<=>{1}", MainWindow.liveUserID, user);

			MainWindow.nonFriendsList.Remove(user);
			MainWindow.usersStore.Clear();
			foreach (int u in MainWindow.nonFriendsList) {
				MainWindow.usersStore.AppendValues(u);
			}

			Recommender.userSymmetricAssociations[MainWindow.liveUserID].Add(user);
			Recommender.retrainUserOnly(user);
			MainWindow.predictRatingsLiveUser();

		}

		//TODO: Merge sorting functions
		public static int CompareNameReversed(TreeModel model, TreeIter a, TreeIter b)
		{
			Artist artist1 = (Artist) model.GetValue(a, 0);
			Artist artist2 = (Artist) model.GetValue(b, 0);

			return string.Compare(artist2.name, artist1.name);
		}

		public static int CompareName(TreeModel model, TreeIter a, TreeIter b)
		{
			Artist artist1 = (Artist) model.GetValue(a, 0);
			Artist artist2 = (Artist) model.GetValue(b, 0);

			return string.Compare(artist1.name, artist2.name);
		}


		public static void ArtistColumnClicked(object o, EventArgs args)
		{
			if (MainWindow.artistColumn.SortOrder == SortType.Ascending)
			{
				MainWindow.artistColumn.SortOrder = SortType.Descending;
				MainWindow.sorter1.DefaultSortFunc = CompareNameReversed;
			}
			else
			{
				MainWindow.artistColumn.SortOrder = SortType.Ascending;
				MainWindow.sorter1.DefaultSortFunc = CompareName;
			}
		}

		public static int ComparePrediction(TreeModel model, TreeIter a, TreeIter b)
		{
			Artist artist1 = (Artist) model.GetValue(a, 0);
			Artist artist2 = (Artist) model.GetValue(b, 0);

			double prediction1 = -1;
			MainWindow.predictions.TryGetValue(artist1.id, out prediction1);
			double prediction2 = -1;
			MainWindow.predictions.TryGetValue(artist2.id, out prediction2);

			double diff = prediction1 - prediction2;

			if (diff > 0)
				return 1;
			if (diff < 0)
				return -1;
			return 0;
		}

		public static int ComparePredictionReversed(TreeModel model, TreeIter a, TreeIter b)
		{
			Artist artist1 = (Artist) model.GetValue(a, 0);
			Artist artist2 = (Artist) model.GetValue(b, 0);

			double prediction1 = -1;
			MainWindow.predictions.TryGetValue(artist1.id, out prediction1);
			double prediction2 = -1;
			MainWindow.predictions.TryGetValue(artist2.id, out prediction2);

			double diff = prediction2 - prediction1;

			if (diff > 0) {
				return 1;
			}
			if (diff < 0) {
				return -1;
			}
			return 0;
		}

		public static void PredictionColumnClicked(object o, EventArgs args)
		{
			if (MainWindow.predictionColumn.SortOrder == SortType.Ascending)
			{
				MainWindow.predictionColumn.SortOrder = SortType.Descending;
				MainWindow.sorter1.DefaultSortFunc = ComparePrediction;
			}
			else
			{
				MainWindow.predictionColumn.SortOrder = SortType.Ascending;
				MainWindow.sorter1.DefaultSortFunc = ComparePredictionReversed;
			}
		}

		public static int CompareRating(TreeModel model, TreeIter a, TreeIter b)
		{
			Artist artist1 = (Artist) model.GetValue(a, 0);
			Artist artist2 = (Artist) model.GetValue(b, 0);

			double rating1;
			MainWindow.ratings.TryGetValue(artist1.id, out rating1);
			double rating2;
			MainWindow.ratings.TryGetValue(artist2.id, out rating2);

			double diff = rating1 - rating2;

			if (diff > 0)
				return 1;
			if (diff < 0)
				return -1;
			return 0;
		}

		public static int CompareRatingReversed(TreeModel model, TreeIter a, TreeIter b)
		{
			Artist artist1 = (Artist) model.GetValue(a, 0);
			Artist artist2 = (Artist) model.GetValue(b, 0);

			double rating1;	
			MainWindow.ratings.TryGetValue(artist1.id, out rating1);
			double rating2;
			MainWindow.ratings.TryGetValue(artist2.id, out rating2);

			double diff = rating2 - rating1;

			if (diff > 0)
				return 1;
			if (diff < 0)
				return -1;
			return 0;
		}

		public static void RatingColumnClicked(object o, EventArgs args)
		{
			if (MainWindow.ratingColumn.SortOrder == SortType.Ascending)
			{
				MainWindow.ratingColumn.SortOrder = SortType.Descending;
				MainWindow.sorter1.DefaultSortFunc = CompareRating;
			}
			else
			{
				MainWindow.ratingColumn.SortOrder = SortType.Ascending;
				MainWindow.sorter1.DefaultSortFunc = CompareRatingReversed;
			}
		}

		public static int CompareUser(TreeModel model, TreeIter a, TreeIter b)
		{
			int user1 = (int) model.GetValue(a, 0);
			int user2 = (int) model.GetValue(b, 0);

			double diff = user1 - user2;

			if (diff > 0)
				return 1;
			if (diff < 0)
				return -1;
			return 0;
		}

		public static int CompareUserReversed(TreeModel model, TreeIter a, TreeIter b)
		{
			int user1 = (int) model.GetValue(a, 0);
			int user2 = (int) model.GetValue(b, 0);

			double diff = user2 - user1;

			if (diff > 0)
				return 1;
			if (diff < 0)
				return -1;
			return 0;
		}

		public static void FriendsColumnClicked(object o, EventArgs args)
		{
			if (MainWindow.friendsColumn.SortOrder == SortType.Ascending)
			{
				MainWindow.friendsColumn.SortOrder = SortType.Descending;
				MainWindow.sorter2.DefaultSortFunc = CompareUser;
			}
			else
			{
				MainWindow.friendsColumn.SortOrder = SortType.Ascending;
				MainWindow.sorter2.DefaultSortFunc = CompareUserReversed;
			}
		}

		public static void UsersColumnClicked(object o, EventArgs args)
		{		
			if (MainWindow.usersColumn.SortOrder == SortType.Ascending)
			{
				MainWindow.usersColumn.SortOrder = SortType.Descending;
				MainWindow.sorter3.DefaultSortFunc = CompareUser;
			}
			else
			{
				MainWindow.usersColumn.SortOrder = SortType.Ascending;
				MainWindow.sorter3.DefaultSortFunc = CompareUserReversed;
			}
		}
	}
}