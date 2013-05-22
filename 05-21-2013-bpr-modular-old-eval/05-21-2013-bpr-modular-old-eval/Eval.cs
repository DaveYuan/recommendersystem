using System;
using System.Linq; // Add System.Core in References
using System.Threading.Tasks;
using System.Collections.Generic;

namespace bprmodularoldeval
{
	class Eval : PredictRecall
	{	
		protected static int TEST_NON_RATED_ITM_THRESHOLD = 1000;
				
		protected static List<string> sampleTestData()
		{			
			int usr;
			int itm;			
			int itmCnt;
			int randItm;
			int numTestEntries;
			int numUniqueTestUsers;
			
			Random random = new Random();
			List<string> usrPostvNegtvItms = new List<string>();
			Dictionary<int, string> usrNonRtdItmCache = new Dictionary<int, string>();
			var testUniqueItems = (new HashSet<int>(testItemsArray)).ToArray();
			numTestEntries = testUsersArray.Length;
			numUniqueTestUsers = testUniqueItems.Length;
			
			for (int i = 0; i < numTestEntries; i++) {
				itmCnt = 0;
				usr = testUsersArray[i];
				itm = testItemsArray[i];
				string usrNonRtdItms = null;
				string usrRtdItms = usr + " " + itm;
				
				if (usrNonRtdItmCache.ContainsKey(usr)) {
					usrPostvNegtvItms.Add(usrRtdItms + " " + usrNonRtdItmCache[usr]);
					continue;
				}
				
				while (itmCnt < TEST_NON_RATED_ITM_THRESHOLD) {
					randItm = testUniqueItems[random.Next(0, numUniqueTestUsers)];
					if (testRatedItems[usr].Contains(randItm)) {
						continue;
					} else {
						if (itmCnt == 0) {
							usrNonRtdItms = Convert.ToString(randItm);
						} else {
							usrNonRtdItms += " " + randItm; 
						}				
						itmCnt++;
					}
				}
				if(!usrNonRtdItmCache.ContainsKey(usr)) {
					usrNonRtdItmCache.Add(usr, usrNonRtdItms);
				}
				usrPostvNegtvItms.Add(usrRtdItms + " " + usrNonRtdItms);						
			}
			return usrPostvNegtvItms;
		}
		
		protected static List<Tuple<int, double>> predictItems(string[] usrItmArray) 
		{
			int usr;
			int itm;
			double rating;
			int RANKED_ITEM = 1;
			List<Tuple<int, double>>rankedList = new List<Tuple<int, double>>();
			List<Tuple<int, double>> predictList = new List<Tuple<int, double>>();
			
			numEntries = usrItmArray.Length;
			usr = Convert.ToInt32(usrItmArray[0]);
			
			for (int i = 1; i <= TEST_NON_RATED_ITM_THRESHOLD+RANKED_ITEM; i++) {
				itm = Convert.ToInt32(usrItmArray[i]);
				rating = PredictRating(usr, itm);
				predictList.Add(Tuple.Create(itm, rating));
			}
			
			rankedList = predictList.OrderByDescending(x => x.Item2).ToList();
			return rankedList;
		}
		
		
		protected static Dictionary<string, double> bprEval()
		{
			List<string> usrPostvNegtvItms = sampleTestData();
			int[] N = new int[] {5, 10, 15};

			Dictionary<string, double> result = initResult();
			List<string> keys = new List<string>(result.Keys);
			
			Parallel.ForEach(usrPostvNegtvItms, usrItms => {
				try {
					string[] stringSeparator = new string[] { " " };
					string[] usrItmArray = usrItms.Split(stringSeparator, StringSplitOptions.None);
					int ratedItm = Convert.ToInt32(usrItmArray[1]);
					var ItmList = predictItems(usrItmArray);
					var rankedList = (from t in ItmList select t.Item1).ToList();
					var resultAtN = predictAtN(ratedItm, rankedList, N);
					lock (result) 
					{
						foreach (string k in keys) {
							result[k] += resultAtN[k];
						}
					}
				} catch (Exception e) {
					Console.Error.WriteLine("Error: " + e.Message + e.StackTrace);
					throw;
				}
			});
			
			foreach (string k in keys) {
				result[k] /= usrPostvNegtvItms.Count;
			}				
			return result;
		}
	}
}