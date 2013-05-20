using System;
using System.IO;
using System.Linq; // Add System.Core in References
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.Generic;
using ProtoBuf;

namespace bprbininput
{
	class MainClass
	{
		private double regUser;
		private double regPostv;
		private double regNegtv;
		private double regBias;
		private double lrate;
		private int numEpochs;
		private int numUsers;
		private int numItems;
		private int numEntries;
		private int numFeatures;
		private int MAX_USER_ID;
		private int MAX_ITEM_ID;
		private int[] trainUsersArray;
		private int[] trainItemsArray;
		private int[] testUsersArray;
		private int[] testItemsArray;
		private int[] uniqueItemsArray;
		private double[,] userFeature;
		private double[,] itemFeature;
		private double[] userBias;
		private double[] itemBias;	
		private Random random;
		private Dictionary<int, int[]> trainRatedItems;
		private Dictionary<int, int[]> testRatedItems;
		
		public static void writeToConsole(string mssg) 
		{
			Console.WriteLine("\t- " + mssg);
		}
		
		public MainClass(Train trainObj, Test testObj)
		{
			lrate = 0.05;
			regUser = 0.0025;
			regPostv = 0.0025;
			regNegtv = 0.00025;
			regBias = 0.33;
			numEpochs = 30;
			numFeatures = 10;
			
			random = new Random();
			trainUsersArray = trainObj.usersList.ToArray();
			trainItemsArray = trainObj.itemsList.ToArray();
			testUsersArray = testObj.usersList.ToArray();
			testItemsArray = testObj.itemsList.ToArray();
			MAX_USER_ID = trainUsersArray.Max();
			MAX_ITEM_ID = trainItemsArray.Max();
			Console.WriteLine("\t\t- MAX_USER_ID: {0}, MAX_ITEM_ID: {1}", MAX_USER_ID, MAX_ITEM_ID);
		}

		public void init()
		{
			userFeature = new double[numFeatures,MAX_USER_ID+1];
			itemFeature = new double[numFeatures,MAX_ITEM_ID+1];
			userBias = new double[MAX_USER_ID+1];
			itemBias = new double[MAX_ITEM_ID+1];
			Random rd = new Random();
			
			for (int i = 0; i < numFeatures; i++) {
				for (int j = 0; j <= MAX_USER_ID; j++) {
//					userFeature[i,j] = (float) 0.1;
					userFeature[i,j] = (float) rd.NextDouble();
				}
			}
			
			rd = new Random();
			for (int i = 0; i < numFeatures; i++) {
				for (int j = 0; j <= MAX_ITEM_ID; j++) {
//					itemFeature[i,j] = (float) 0.1;
					itemFeature[i,j] = (float) rd.NextDouble();
				}
			}
		}
		
		public double dotProduct(int userId,
					    int itemId)
		{
			double dotProduct = 0.0;
			for (int i = 0; i < numFeatures; ++i) {
				dotProduct += userFeature[i,userId] * itemFeature[i,itemId];
			}
			return dotProduct;
		}
	
		public double PredictRating(int userId, int itemId) 
		{
			return itemBias[itemId] + 
				userBias[userId] + 
				dotProduct(userId, itemId);
		}
	
		public void trainItemsRatedByUser()
		{
			int user;
			int size = trainItemsArray.Length;
			double avgItemsRatedPerUser = 0.0;
			trainRatedItems = new Dictionary<int, int[]>();
			Dictionary<int, List<int>> trainRatedItemsTmp = new Dictionary<int, List<int>>();
			
			for (int i = 0; i < size; i++) {
				user = trainUsersArray[i];
				if (trainRatedItemsTmp.ContainsKey(user)) {
					trainRatedItemsTmp[user].Add(trainItemsArray[i]);
				} else {
					List<int> tmp = new List<int>();
					tmp.Add(trainItemsArray[i]);
					trainRatedItemsTmp.Add(user, tmp);
				}
			}
			
			// Convert Dictionary<int, List<int>> trainRatedItemsTmp => Dictionary<int, int[]> trainRatedItems 
			foreach (KeyValuePair<int, List<int>> ratedItem in trainRatedItemsTmp) {
				trainRatedItems.Add(ratedItem.Key, ratedItem.Value.ToArray());
				avgItemsRatedPerUser += ratedItem.Value.Count;
				
			}
			
			avgItemsRatedPerUser = avgItemsRatedPerUser / trainRatedItems.Count;
			Console.WriteLine("\t\t- Avg(TrainItemRatedPerUser): {0}", avgItemsRatedPerUser);
			GC.Collect();
		}

		public void testItemsRatedByUser()
		{
			int user;
			int size = testItemsArray.Length;
			double avgItemsRatedPerUser = 0.0;
			testRatedItems = new Dictionary<int, int[]>();
			Dictionary<int, List<int>> testRatedItemsTmp = new Dictionary<int, List<int>>();
			
			for (int i = 0; i < size; i++) {
				user = testUsersArray[i];
				if (testRatedItemsTmp.ContainsKey(user)) {
					testRatedItemsTmp[user].Add(testItemsArray[i]);
				} else {
					List<int> tmp = new List<int>();
					tmp.Add(testItemsArray[i]);
					testRatedItemsTmp.Add(user, tmp);
				}
			}
			
			// Convert Dictionary<int, List<int>> testRatedItemsTmp => Dictionary<int, int[]> testRatedItems 
			foreach (KeyValuePair<int, List<int>> ratedItem in testRatedItemsTmp) {
				testRatedItems.Add(ratedItem.Key, ratedItem.Value.ToArray());
				avgItemsRatedPerUser += ratedItem.Value.Count;
				
			}
			
			avgItemsRatedPerUser = avgItemsRatedPerUser / testRatedItems.Count;
			Console.WriteLine("\t\t- Avg(TestItemRatedPerUser): {0}", avgItemsRatedPerUser);
			GC.Collect();

		}
		
		public void calUniqueUsersnItems() 
		{
			// Create a HashSet of uniqueItems and uniqueUsers
			var hashItems = new HashSet<int>(trainItemsArray);
			uniqueItemsArray = hashItems.ToArray();
			
			var hashUsers = new HashSet<int>(trainUsersArray);
			
			numUsers = hashUsers.Count;
			numItems = uniqueItemsArray.Length;
			
			Console.WriteLine("\t\t- Uniq(#Users): {0}, Uniq(#Items): {1}", numUsers, numItems);
		}
		
		public double sigmoid(double x)
		{
			return (1.0 / ( 1.0 + Math.Exp(-x)));
		}
		
		public double updateFeatures(int userId, int itemIdPostv, int itemIdNegtv)
		{
			double xuPostv;
			double xuNegtv;
			double xuPostvNegtv;
			double dervxuPostvNegtv;
			double oneByOnePlusExpX;

			xuPostv = userBias[userId] + itemBias[itemIdPostv] +  dotProduct(userId, itemIdPostv);
			xuNegtv = userBias[userId] + itemBias[itemIdNegtv] + dotProduct(userId, itemIdNegtv);
			xuPostvNegtv = xuPostv - xuNegtv;
			oneByOnePlusExpX = 1.0 / (1.0 + Math.Exp(xuPostvNegtv));
		
			userBias[userId] += (double) (lrate * (oneByOnePlusExpX - regBias * userBias[userId]));
			itemBias[itemIdPostv] += (double) (lrate * (oneByOnePlusExpX - regBias * itemBias[itemIdPostv]));
			itemBias[itemIdNegtv] += (double) (lrate * (-oneByOnePlusExpX - regBias * itemBias[itemIdNegtv]));

			for (int f = 0; f < numFeatures; f++) {
				double userF = userFeature[f, userId];
				double itemPostvF = itemFeature[f, itemIdPostv];
				double itemNegtvF = itemFeature[f, itemIdNegtv];

				dervxuPostvNegtv = itemPostvF - itemNegtvF;
				userFeature[f, userId] += (double) (lrate * (oneByOnePlusExpX * dervxuPostvNegtv -
									  regUser * userF));
				
				dervxuPostvNegtv = userF;
				itemFeature[f, itemIdPostv] += (double) (lrate * (oneByOnePlusExpX * dervxuPostvNegtv -
										    regPostv * itemPostvF));
				
				dervxuPostvNegtv = -userF;
				itemFeature[f, itemIdNegtv] += (double) (lrate * (oneByOnePlusExpX * dervxuPostvNegtv -
											    regNegtv * itemNegtvF));
			}
			return xuPostvNegtv;
		}	
		
		public int drawUser()
		{
			int userId;
			
			while(true) {
				userId = random.Next(0, MAX_USER_ID);
				if (!trainRatedItems.ContainsKey(userId)) {
					continue;
				}
				return userId;
			}
		}

		public double bprTrain()
		{
			int numRatedItems = 0;
			int userId;
			int itemIdPostv = 1;
			int itemIdNegtv = 1;
			double xuPostvNegtv = 1;
			double bprOpt = 0.0;
			
			numEntries = trainItemsArray.Length;
			Console.WriteLine("\t\t- #TrainEntries: {0}", numEntries);
		
			for (int epoch = 1; epoch <= numEpochs; epoch++) {
				bprOpt = 0.0;
				for (int n = 0; n < numEntries; n++) {
					do {
						userId = drawUser();
						numRatedItems = trainRatedItems[userId].Length;
					
						itemIdPostv = trainRatedItems[userId][random.Next(0, numRatedItems)];
						itemIdNegtv = uniqueItemsArray[random.Next(0, numItems)];
					
						while (trainRatedItems[userId].Contains(itemIdNegtv)) {
							itemIdNegtv = uniqueItemsArray[random.Next(0, numItems)];
						}
					} while( userId > MAX_USER_ID || itemIdPostv > MAX_ITEM_ID || itemIdNegtv > MAX_ITEM_ID);
					
					xuPostvNegtv = updateFeatures(userId, itemIdPostv, itemIdNegtv);
					bprOpt += Math.Log(sigmoid(xuPostvNegtv));
				}
//				Console.WriteLine("\t\t- {0}: Opt(Usr+Itm): {1}", epoch, bprOpt);
			}	
			return bprOpt;
		}
		
		public Dictionary<int, double> removeRatedItems(int userId, 
					      			List<int> removeItems)
		{ 
			int itemId;
			int uniqueItemsCnt = uniqueItemsArray.Count();
			Dictionary<int, double> predictItemRating = new Dictionary<int, double>();

			for (int i = 0; i < uniqueItemsCnt; i++) {
				itemId = uniqueItemsArray[i];
				if (!removeItems.Contains(itemId)) {
					predictItemRating.Add(itemId, PredictRating(userId, itemId));
				}
			}

			return predictItemRating;
		}

		public double hitCount(List<int> ratedItems, List<int> rankedItems, int n)
		{
			int len;
			int hits = 0;
			int itemId;

			len = rankedItems.Count;
			for (int i = 0; i < len; i++) {
				itemId = rankedItems[i];
				if (!ratedItems.Contains(itemId)) {
					continue;
				}
				
				if (i < n) {
					hits++;
				} else {
					break;
				}
			}
			return hits;
		}

		public Dictionary<int, double> predictAtN(List<int> ratedItems, List<int> rankedItems, int[] N)
		{
			var precAtN = new Dictionary<int, double>();
			foreach (int n in N) {
				precAtN[n] =  (double) hitCount(ratedItems, rankedItems, n) / (double) n;	
			}
			return precAtN;
		}

		public Dictionary<int, double> bprEval()
		{
			int numUniqueTestUsers;
			HashSet<int> testUniqueUsers = new HashSet<int>(testUsersArray);
			HashSet<int> testUniqueItems = new HashSet<int>(testItemsArray);
			Dictionary<int, double> result = new Dictionary<int, double>();

			int[] N = new int[] {5, 10, 15};
			result.Add(5, 0.0);
			result.Add(10, 0.0);
			result.Add(15, 0.0);

			numUniqueTestUsers = testUniqueUsers.Count();
			Console.WriteLine("\t\t- TestEntries: {0}, Uniq(#Users): {1}, Uniq(#Items): {2}", testUsersArray.Length, testUniqueUsers.Count(), testUniqueItems.Count());
			List<int> candidateItems = testItemsArray.Union(trainItemsArray).ToList();

			Parallel.ForEach(testUniqueUsers, userId => {		
				try {
					var ratedItems = new HashSet<int>(testRatedItems[userId]);
					ratedItems.IntersectWith(candidateItems);
					if(ratedItems.Count == 0) return;
				
					var itemsToRemove = new HashSet<int>(trainRatedItems[userId]);
					itemsToRemove.IntersectWith(candidateItems);

					var predictItemRating = removeRatedItems(userId, itemsToRemove.ToList());
					var rankedItems = from pair in predictItemRating
									orderby pair.Value descending
									select pair.Key;
					var precAtN = predictAtN(ratedItems.ToList(), rankedItems.ToList(), N);				
					lock (result) 
					{
						result[5] += precAtN[5];
						result[10] += precAtN[10];
						result[15] += precAtN[15];
//						Console.WriteLine("\t\t- UserId: {0}, #Rnkd: {1}, #Rtd: {2}, #Rmv: {3}, #CndItm: {4}, prec[5]: {5}", 					userId, rankedItems.ToList().Count, ratedItems.ToList().Count, itemsToRemove.ToList().Count,candidateItems.Count, precAtN[5]);
					}
				} catch (Exception e) {
					Console.Error.WriteLine("Error: " + e.Message + e.StackTrace);
					throw;
				}
			});

			result[5] /= numUniqueTestUsers;
			result[10] /= numUniqueTestUsers;
			result[15] /= numUniqueTestUsers;
			return result;
		}

		public static void Main (string[] args)
		{			
			Train trainObj;
			Test testObj;
			Stopwatch loadTime = new Stopwatch();
			Stopwatch trainTime = new Stopwatch();
			Stopwatch testTime = new Stopwatch();
			
			loadTime.Start();
			writeToConsole("Loading u1.base.bin");
			using (FileStream file = File.OpenRead("u1.base.bin"))
			{
				trainObj = Serializer.Deserialize<Train>(file);
			}
			
			writeToConsole("Loading u1.test.bin");
			using (FileStream file = File.OpenRead("u1.test.bin"))
			{
				testObj = Serializer.Deserialize<Test>(file);
			}
			loadTime.Stop();
			
			writeToConsole("Constructor call");
			MainClass mainclass = new MainClass(trainObj, testObj);
			GC.Collect();
			
			writeToConsole("Find: RatedItemsPerUser");
			mainclass.trainItemsRatedByUser();
			mainclass.testItemsRatedByUser();
			
			writeToConsole("Find: uniqueUsers and uniqueItems");
			mainclass.calUniqueUsersnItems();
			
			writeToConsole("Initialize features");
			mainclass.init();
			
			trainTime.Start();
			writeToConsole("Bpr Training");
			mainclass.bprTrain();
			trainTime.Stop();
		
			testTime.Start();
			writeToConsole("Bpr Evaluation");
			var result = mainclass.bprEval();	
			testTime.Stop();

			Console.WriteLine("\t\t- Prec[5]: {0}, Prec[10]: {1}, Prec[15]: {2}", result[5], result[10], result[15]);
			Console.WriteLine("\t- Time(load): {0}, Time(train): {1}, Time(test): {2}", 
					loadTime.Elapsed, trainTime.Elapsed, testTime.Elapsed);

			writeToConsole("Done!");
		}
	}
}
