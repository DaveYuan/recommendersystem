using System;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;

namespace itempredictionbiaslearningworegularization
{
	class MainClass
	{
		static void displayTrainSet(string[] lines) 
		{
			System.Console.WriteLine("\tTrainset :\n");
	        foreach (string line in lines)
	        {	
				string[] stringSeparators = new string[] { "\t" };
				string[] result = line.Split(stringSeparators, StringSplitOptions.None);
				foreach (string s in result)
				{
				Console.Write("'{0}' ", s);
				}
				Console.WriteLine();
			}	
		}
		
		static void readTrainSet(String fileName, 
		                         ref Dictionary<string, List<int>> userIdMapping, 
		                         ref Dictionary<string, List<int>> movieIdMapping,
		                         ref List<string> trainUserIdList,
		                         ref List<string> trainMovieIdList,
		                         ref List<int> trainRatingList)
		{
			int userIdIndex;
			int movieIdIndex;
			int rowIndexCounter;	
			
			string[] lines = System.IO.File.ReadAllLines(fileName);
			//displayTrainSet(lines);
			
			userIdIndex = movieIdIndex = 0;
			
			foreach (string line in lines)
	        {	
				string[] stringSeparator = new string[] { "\t" };
				string[] result = line.Split(stringSeparator, StringSplitOptions.None);
				rowIndexCounter = 0;
				
				foreach (string s in result)
				{
					if (rowIndexCounter == 0) {
						trainUserIdList.Add(s);
						if (userIdMapping.ContainsKey(s))
						{
							userIdMapping[s].Add(userIdIndex);				
							userIdIndex++;
						} else {
							List<int> tmpIndexList = new List<int>();
							tmpIndexList.Add(userIdIndex);
							userIdMapping.Add(s,tmpIndexList);
							userIdIndex++;
						}
					} 
					
					if (rowIndexCounter == 1) {
						trainMovieIdList.Add(s);
						if (movieIdMapping.ContainsKey(s))
						{
							movieIdMapping[s].Add(movieIdIndex);				
							movieIdIndex++;
						} else {
							List<int> tmpIndexList = new List<int>();
							tmpIndexList.Add(movieIdIndex);
							movieIdMapping.Add(s,tmpIndexList);
							movieIdIndex++;
						}
					}
					
					if(rowIndexCounter == 2) {
						trainRatingList.Add(Convert.ToInt32(s));
					}
					
					rowIndexCounter++;
				}
			}	
		}	
		
		static void readTestSet(String fileName, 
		                         ref List<string> testUserIdList,
		                         ref List<string> testMovieIdList,
		                         ref List<int> testRatingList)
		{
			int rowIndexCounter;	
			
			string[] lines = System.IO.File.ReadAllLines(fileName);		
		//	displayTrainSet(lines);
				
			foreach (string line in lines)
	        {	
				string[] stringSeparator = new string[] { "\t" };
				string[] result = line.Split(stringSeparator, StringSplitOptions.None);
				rowIndexCounter = 0;
				
				foreach (string s in result)
				{
					if (rowIndexCounter == 0) {
						testUserIdList.Add(s);
					} 
					
					if (rowIndexCounter == 1) {
						testMovieIdList.Add(s);
					}
					
					if(rowIndexCounter == 2) {
						testRatingList.Add(Convert.ToInt32(s));
					}					
					rowIndexCounter++;
				}
			}	
		}
		
		static double calcGlobalAverage(List<int> trainRatingList) 
		{
			int sum = 0;
			int size = trainRatingList.Count;
			double globalAverage;
			
			foreach (int rating in trainRatingList) {
				sum = sum + rating;
			}
			globalAverage = (double)sum/(double)size;
			Console.WriteLine("\tGlobal average: {0}\n", globalAverage);
			return globalAverage;
		}
		
		static void initFeatures(int numUsers, ref double[,] userFeature, 
			             		int numMovies, ref double[,] movieFeature,
		                        int numFeatures)
		{
			int i;
			int j;
			
			for (i = 0; i < numFeatures; i++) {
				for (j = 0; j < numUsers; j++) {
					userFeature[i,j] = 0.1;
				}
			}
			
			for (i = 0; i < numFeatures; i++) {
				for (j = 0; j < numMovies; j++) {
					movieFeature[i,j] = 0.1;
				}
			}
		}
				
		static void userAvg(ref Dictionary<string, double> userIdAvgHash, 
		                    Dictionary<string, List<int>> userIdMapping,
		                    int[] trainRatingArray)
		{
			double sum;
			int size;
			
			foreach (KeyValuePair<string, List<int>> userId in userIdMapping) {
				size = userId.Value.Count;
				sum = 0.0;
				foreach (int indx in userId.Value) {
					sum += trainRatingArray[indx];
				}
				sum = sum / size;
				userIdAvgHash.Add(userId.Key, sum);
			}
		}
		
		static void movieAvg(ref Dictionary<string, double> movieIdAvgHash,
		                     Dictionary<string, List<int>> movieIdMapping,
		                     int[] trainRatingArray)
		{
			double sum;
			int size;
			
			foreach (KeyValuePair<string, List<int>> movieId in movieIdMapping) {
				size = movieId.Value.Count;
				sum = 0.0;
				foreach (int indx in movieId.Value) {
					sum += trainRatingArray[indx];
				}
				sum = sum / size;
				movieIdAvgHash.Add(movieId.Key, sum);
			}
		}
			
		static void train(double K,		              
		                  int epochs,
		                  int numUsers,
		                  int numMovies,
		                  int numFeatures,
		                  double lrate,
		                  ref double globalAverage,
		                  ref double[,] userFeature,
		                  ref double[,] movieFeature,
		                  ref Dictionary<string, double> userBiasHash,
		                  ref Dictionary<string, double> movieBiasHash,
		                  List<string> trainUserIdList, 
		                  List<string> trainMovieIdList,
		                  List<int> trainRatingList,
		                  Dictionary<string, List<int>> userIdMapping,
		                  Dictionary<string, List<int>> movieIdMapping                                 
		                  )
		{	
			int itr;
			int numEntries;
			int userIdCounter;
			int movieIdCounter;						
			double err;				
			double errPerEpoch;		
			double predictRating;								
			
			string[] line = new string[epochs];
			Dictionary<int, double> errList = new Dictionary<int, double>();
			Dictionary<string, double> userIdAvgHash = new Dictionary<string, double>();
			Dictionary<string, double> movieIdAvgHash = new Dictionary<string, double>();			
			
			numEntries = trainRatingList.Count;
			int[] ratingArray = trainRatingList.ToArray();			
			
			globalAverage = calcGlobalAverage(trainRatingList);			
		
			initFeatures(userIdMapping.Count, ref userFeature, 
			             movieIdMapping.Count, ref movieFeature,
			             numFeatures);			
			
			userIdCounter = movieIdCounter = 0;
			userAvg(ref userIdAvgHash, userIdMapping, ratingArray);
			movieAvg(ref movieIdAvgHash, movieIdMapping, ratingArray);
			
			
			foreach (KeyValuePair<string, List<int>> userId in userIdMapping)
            {				
				userBiasHash.Add(userId.Key, userIdAvgHash[userId.Key] - globalAverage);			
				userIdCounter++;
            }
			
			foreach (KeyValuePair<string, List<int>> movieId in movieIdMapping)
            {              
				movieBiasHash.Add(movieId.Key, movieIdAvgHash[movieId.Key] - globalAverage);			
				movieIdCounter++;
            }
					
			Stopwatch stopwatch = new Stopwatch();

			stopwatch.Start();		
			
			for (itr = 0; itr < epochs; ++itr) {
				errPerEpoch = 0.0;
				for (int q = 0; q < numEntries; ++q) {
					predictRating = globalAverage + userBiasHash[trainUserIdList[q]] + movieBiasHash[trainMovieIdList[q]];
					err = ratingArray[q] - predictRating;						 			
					errPerEpoch += err*err;
					userBiasHash[trainUserIdList[q]] += lrate * (err);
					movieBiasHash[trainMovieIdList[q]] += lrate * (err);						
				}		
				errPerEpoch = Math.Sqrt(errPerEpoch/numEntries);	
				Console.WriteLine( "Epoch = {0}, errPerEpoch = {1},", itr, errPerEpoch);
				errList.Add(itr, errPerEpoch);				
			}
			
			stopwatch.Stop();
			Console.WriteLine("\n\tTrain Time: {0}\n", stopwatch.Elapsed);
			
			itr = 0;
			foreach (KeyValuePair<int, double> errId in errList)
            {
				string tmp = errId.Key + " " + errId.Value;
				line[itr] = tmp;
				itr++;                
            }
			System.IO.File.WriteAllLines("train_error.txt", line);				
			
	/*		
			int uId;
			int mId;
			int itr;
			int trainNumEntries;	
			int userIdCounter;
			int movieIdCounter;						
			double uv;
			double trainErr;				
			double trainErrPerEpoch;					
			double trainPredictRating;				
			double trainUserMovieProduct;
			
			string[] line = new string[epochs];
			Dictionary<int, double> trainErrList = new Dictionary<int, double>();
			Dictionary<string, double> userIdAvgHash = new Dictionary<string, double>();
			Dictionary<string, double> movieIdAvgHash = new Dictionary<string, double>();			
		
			trainNumEntries = trainRatingList.Count;
			
			int[] trainRatingArray = trainRatingList.ToArray();	
			
			globalAverage = calcGlobalAverage(trainRatingList);			
		
			initFeatures(userIdMapping.Count, ref userFeature, 
			             movieIdMapping.Count, ref movieFeature,
			             numFeatures);			
			
			userIdCounter = movieIdCounter = 0;
			userAvg(ref userIdAvgHash, userIdMapping, trainRatingArray);
			movieAvg(ref movieIdAvgHash, movieIdMapping, trainRatingArray);
			
			
			foreach (KeyValuePair<string, List<int>> userId in userIdMapping)
            {
                userIdHash.Add(userId.Key, userIdCounter);				
				userIdCounter++;
            }
			
			foreach (KeyValuePair<string, List<int>> movieId in movieIdMapping)
            {
                movieIdHash.Add(movieId.Key, movieIdCounter);
				movieIdCounter++;
            }
			
			Stopwatch stopwatch = new Stopwatch();

			stopwatch.Start();		
			
			for (itr = 0; itr < epochs; ++itr) {			
				trainErrPerEpoch = 0.0;	

				for (int q = 0; q < trainNumEntries; ++q) {					
					trainUserMovieProduct = adjustingFactor(userFeature, movieFeature, numFeatures, userIdHash[trainUserIdList[q]], movieIdHash[trainMovieIdList[q]]);												
				
					trainPredictRating = globalAverage + trainUserMovieProduct;								
					trainErr = trainRatingArray[q] - trainPredictRating;						 						
				
					uId = userIdHash[trainUserIdList[q]];
					mId = movieIdHash[trainMovieIdList[q]];
					trainErrPerEpoch += trainErr * trainErr;			
					
					for (int j = 0; j < numFeatures; ++j) {																																																			
						uv = userFeature[j,uId];
						userFeature[j,uId] += lrate * (trainErr * movieFeature[j,mId]);
						movieFeature[j,mId] += lrate * (trainErr * uv);
					}																
				}
			
				trainErrPerEpoch = Math.Sqrt(trainErrPerEpoch/trainNumEntries);				

				Console.WriteLine( "Epoch = {0}, trainErrPerEpoch = {1}", itr, trainErrPerEpoch);
			
				trainErrList.Add(itr, trainErrPerEpoch);
			}						
			
			stopwatch.Stop();
			Console.WriteLine("\n\tTrain Time: {0}\n", stopwatch.Elapsed);
			
			itr = 0;
			foreach (KeyValuePair<int, double> errId in trainErrList)
            {
				string tmp = errId.Key + " " + errId.Value;
				line[itr] = tmp;
				itr++;                
            }
			*/				
		}
			
		static Dictionary<string, List<string>> createRatedItemListPerUser(List<string> testUserIdList, List<string> testMovieIdList)
		{
			Dictionary<string, List<string>> testRatedItemsPerUser = new Dictionary<string, List<string>>();
			int testNumEntries = testUserIdList.Count;
			
			for (int i = 0; i < testNumEntries; i++) {
				if (testRatedItemsPerUser.ContainsKey(testUserIdList[i])) {
					testRatedItemsPerUser[testUserIdList[i]].Add(testMovieIdList[i]);
				} else {
					List<string> tmp = new List<string>();
					tmp.Add(testMovieIdList[i]);
 					testRatedItemsPerUser.Add(testUserIdList[i],tmp);
				}
			}
			return testRatedItemsPerUser;
		}
		
		static Dictionary<string, List<string>> createNonRatedItemListPerUser(Dictionary<string, List<string>> testRatedItemsPerUser, int randNumItems)
		{
			int numItems;
			int cnt;
			bool flag;
			Dictionary<string, List<string>> testNonRatedItemsPerUser = new Dictionary<string, List<string>>();			
			
			foreach (KeyValuePair<string, List<string>> mapping1 in testRatedItemsPerUser) {				
				cnt = 0;
				flag = false;
				List<string> ratedItems = mapping1.Value;
				List<string> nonRatedItems = new List<string>();
				
				foreach (KeyValuePair<string, List<string>> mapping2 in testRatedItemsPerUser) {
					if(mapping2.Key != mapping1.Key) {
						numItems = mapping2.Value.Count;
						for (int i = 0; i < numItems; i++) {
							if ( !(ratedItems.Contains(mapping2.Value[i])) ) {
								if ( !(nonRatedItems.Contains(mapping2.Value[i])) ) {									
									nonRatedItems.Add(mapping2.Value[i]);
									cnt++;
									if (cnt == randNumItems) {
										flag = true;			
										break;
									}
								}
							}
						}
						if (flag) {
							break;
						}
					}
				}
				
/*
				Console.Write("User: {0}, Items: ", mapping1.Key);
				foreach (string s in nonRatedItems) {
					Console.Write("{0}, ", s);
				}
				Console.WriteLine();
*/
				testNonRatedItemsPerUser.Add(mapping1.Key, nonRatedItems);
			}
			
			return testNonRatedItemsPerUser;
		}
				
		static int testingMethodology( 
		                               	int N,
		                                int T,
		                       			int randNumItems, 
		                                double globalAverage,
		                                int numFeatures,
			                   			double[,] userFeature, 
			                   			double[,] movieFeature, 
			                   			Dictionary<string, double> userBiasHash,
			                   			Dictionary<string, double> movieBiasHash,
			                   			List<string> testUserIdList, 
			                   			List<string> testMovieIdList, 
			                   			Dictionary<string, List<string>> testNonRatedItemsPerUser			                   
			                   		)
		{
			int rank;	
			int hits = 0;
			double testUserMovieProduct;
			double testPredictRating;
			
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();		
			
			for (int i = 0; i < T; i++) {
				rank = 0;
				Dictionary<string, double> itemRatingMapping = new Dictionary<string, double>();
				
				testPredictRating = globalAverage + userBiasHash[testUserIdList[i]] + movieBiasHash[testMovieIdList[i]];				
				itemRatingMapping.Add(testMovieIdList[i], testPredictRating);
				
				foreach (string nonRatedItem in testNonRatedItemsPerUser[testUserIdList[i]])
            	{
					testPredictRating = globalAverage + userBiasHash[testUserIdList[i]] + movieBiasHash[testMovieIdList[i]];				
					itemRatingMapping.Add(nonRatedItem, testPredictRating);						
				}
				
				
				var sortedItemRatingMapping = from pair in itemRatingMapping
								orderby pair.Value descending
								select pair;
				
				foreach (KeyValuePair<string, double> pair in sortedItemRatingMapping)
				{
					rank++;
					if (pair.Key == testMovieIdList[i]) {
						hits++;
					}
					if (rank == N) {
						break;
					}
				}
			}	
			
			stopwatch.Stop();
			Console.WriteLine("\n\tTesting Time: {0}\n", stopwatch.Elapsed);
			
			return hits;
		}
		
		public static void Main (string[] args)
		{		
			int T;
			int hits;
			int numUsers;
			int numMovies;
			int N = 60;			
			int epochs = 100;
			int numFeatures = 50;		
			int randNumItems = 100;
			
			double recall;
			double precision;
			double K = 25;
			double globalAverage = 0;
			double lrate = 0.001;
						
			List<string> trainUserIdList = new List<string>();
			List<string> trainMovieIdList = new List<string>();
			List<int> trainRatingList = new List<int>();
			
			List<string> testUserIdList = new List<string>();
			List<string> testMovieIdList = new List<string>();
			List<int> testRatingList = new List<int>();
	
		//	Dictionary<string, int> userIdHash = new Dictionary<string, int>();
		//	Dictionary<string, int> movieIdHash = new Dictionary<string, int>();		
			Dictionary<string, List<int>> userIdMapping = new Dictionary<string, List<int>>();
			Dictionary<string, List<int>> movieIdMapping = new Dictionary<string, List<int>>();	
			Dictionary<string, double> userBiasHash = new Dictionary<string, double>();
			Dictionary<string, double> movieBiasHash = new Dictionary<string, double>();					
			Dictionary<string, List<string>> testRatedItemsPerUser = new Dictionary<string, List<string>>();
			Dictionary<string, List<string>> testNonRatedItemsPerUser = new Dictionary<string, List<string>>();

			
			readTrainSet("/afs/uni-paderborn.de/user/s/shivani/Projects/dataset/train.txt.mtx", 
			             ref userIdMapping, 
			             ref movieIdMapping, 
			             ref trainUserIdList, 
			             ref trainMovieIdList, 
			             ref trainRatingList);
			
			readTestSet("/afs/uni-paderborn.de/user/s/shivani/Projects/dataset/test.txt.mtx", 
			             ref testUserIdList, 
			             ref testMovieIdList, 
			             ref testRatingList);
			
			numUsers = userIdMapping.Count;
			numMovies = movieIdMapping.Count;
			
			double[,] userFeature = new double[numFeatures,numUsers];
			double[,] movieFeature = new double[numFeatures,numMovies];						

			Console.WriteLine("\n###############################################\n");
			Console.WriteLine("\t\tTraining progress");
			Console.WriteLine("\n###############################################\n");			
					
			
			train(K,			      
		          epochs,
		          numUsers,
		          numMovies,
		          numFeatures,
		          lrate,
			      ref globalAverage,
			      ref userFeature,
			      ref movieFeature,
			      ref userBiasHash,
			      ref movieBiasHash,
		          trainUserIdList, 
		          trainMovieIdList,
		          trainRatingList,
		          userIdMapping,
		          movieIdMapping			  
			      );		
			
			T = testUserIdList.Count;
			testRatedItemsPerUser = createRatedItemListPerUser(testUserIdList, testMovieIdList);
			testNonRatedItemsPerUser = createNonRatedItemListPerUser(testRatedItemsPerUser, randNumItems);	
			
			Console.WriteLine("\n###############################################\n");
			Console.WriteLine("\tCalculating recall and prediction");
			Console.WriteLine("\n###############################################\n");			
					
			hits = testingMethodology(N,
			                   T,
			                   randNumItems, 
			                   globalAverage,
			                   numFeatures,
			                   userFeature, 
			                   movieFeature, 
			                   userBiasHash,
			                   movieBiasHash,
			                   testUserIdList, 
			                   testMovieIdList, 
			                   testNonRatedItemsPerUser			                  
			                   );
			
			recall = (double)hits / (double)T;
			precision = (double)recall / (double)N;
			Console.WriteLine("\t#Test: {0}",T);
			Console.WriteLine("\t#Hits: {0}", hits);
			Console.WriteLine("\tRecall: {0}, Precision: {1}", recall, precision);
			
			Process.Start("plot.sh");
		}		
	}
}

