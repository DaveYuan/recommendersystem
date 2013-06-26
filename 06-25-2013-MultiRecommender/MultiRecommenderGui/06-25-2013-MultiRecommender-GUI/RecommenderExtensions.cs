using System;
using System.Linq;
using System.Collections.Generic;

namespace MultiRecommenderGUI
{
	public class RecommenderExtensions
	{
		public static int[] uniqueArtistsArray;
		public static Dictionary<int, List<int>> ratedArtists;

		public static void removeAt(int indx) {
			MainWindow.usersList.RemoveAt(indx);
			MainWindow.artistsList.RemoveAt(indx);
			MainWindow.ratingsList.RemoveAt(indx);
		}

		public static void removeRating(int userId, int artistId) {
			int size = MainWindow.usersList.Count;

			for (int i = 0; i < size; i++) {
				if (MainWindow.usersList[i] == userId && MainWindow.artistsList[i] == artistId) {
					removeAt(i);
				}
			}
			Recommender.retrainUser(userId);
			Recommender.reTrainArtist(artistId);
		}

		public static void addRating(int userId, int artistId, int rating)
		{
			MainWindow.usersList.Add(userId);
			MainWindow.artistsList.Add(artistId);
			MainWindow.ratingsList.Add(rating);
			Recommender.retrainUser(userId);
			Recommender.reTrainArtist(artistId);
		}

		public static void fetchUniqueArtist()
		{
			var hashArtists = new HashSet<int>(MainWindow.artistsList);
			uniqueArtistsArray = hashArtists.ToArray();
		}

		public static void artistsRatedByUser()
		{
			int user;
			int size = MainWindow.artistsList.Count;
			ratedArtists = new Dictionary<int, List<int>>();

			for (int i = 0; i < size; i++) {
				user = MainWindow.usersList[i];
				if (ratedArtists.ContainsKey(user)) {
					ratedArtists[user].Add(MainWindow.artistsList[i]);
				} else {
					List<int> tmp = new List<int>();
					tmp.Add(MainWindow.artistsList[i]);
					ratedArtists.Add(user, tmp);
				}
			}
		}
	}
}

