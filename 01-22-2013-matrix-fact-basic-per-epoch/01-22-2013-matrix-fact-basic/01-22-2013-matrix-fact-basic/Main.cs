using System;
using System.Diagnostics;
using System.Collections.Generic;

namespace matrixfactbasic
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
			displayTrainSet(lines);
				
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
			Console.WriteLine("Global average {0}", globalAverage);
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
		
		static double adjustingFactor(double[,] userFeature, 
		                         double[,] movieFeature, 
		                         int numFeatures, 
		                         int userId,
		                         int movieId)
		{
			double dotProduct = 0.0;
			for (int i = 0; i < numFeatures; ++i) {
				dotProduct += userFeature[i,userId] * movieFeature[i,movieId];				
			}
			return dotProduct;
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
		
		static void trainTestError(double K,		           
		                  int epochs,
		                  int numUsers,
		                  int numMovies,
		                  int numFeatures,
		                  double lrate,
		                  ref double[,] userFeature,
		                  ref double[,] movieFeature,
		                  List<string> trainUserIdList, 
		                  List<string> trainMovieIdList,
		                  List<int> trainRatingList,
		                  Dictionary<string, List<int>> userIdMapping,
		                  Dictionary<string, List<int>> movieIdMapping,
		                  List<string> testUserIdList, 
		                  List<string> testMovieIdList,
		                  List<int> testRatingList                    
		                  )
		{	
			int uId;
			int mId;
			int itr;
			int numEntries;
			int userIdCounter;
			int movieIdCounter;						
			double uv;
			double trainErr;				
			double testErr;
			double testErrPerEpoch;
			double trainErrPerEpoch;					
			double trainPredictRating;	
			double testPredictRating;
			double globalAverage;
			double trainUserMovieProduct;
			double testUserMovieProduct;
			
			string[] line = new string[epochs];
			Dictionary<int, double> trainErrList = new Dictionary<int, double>();
			Dictionary<int, double> testErrList = new Dictionary<int, double>();
			Dictionary<string, int> userIdHash = new Dictionary<string, int>();
			Dictionary<string, int> movieIdHash = new Dictionary<string, int>();
			Dictionary<string, double> userIdAvgHash = new Dictionary<string, double>();
			Dictionary<string, double> movieIdAvgHash = new Dictionary<string, double>();			
			Dictionary<string, double> userBiasHash = new Dictionary<string, double>();
			Dictionary<string, double> movieBiasHash = new Dictionary<string, double>();		
	
			numEntries = trainRatingList.Count;
			int[] trainRatingArray = trainRatingList.ToArray();	
			int[] testRatingArray = testRatingList.ToArray();
			
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
				userBiasHash.Add(userId.Key, userIdAvgHash[userId.Key] - globalAverage);			
				userIdCounter++;
            }
			
			foreach (KeyValuePair<string, List<int>> movieId in movieIdMapping)
            {
                movieIdHash.Add(movieId.Key, movieIdCounter);
				movieBiasHash.Add(movieId.Key, movieIdAvgHash[movieId.Key] - globalAverage);			
				movieIdCounter++;
            }
					
			Stopwatch stopwatch = new Stopwatch();

			stopwatch.Start();		
			
			for (itr = 0; itr < epochs; ++itr) {			
				trainErrPerEpoch = 0.0;	
				testErrPerEpoch = 0.0;
				for (int q = 0; q < numEntries; ++q) {					
					trainUserMovieProduct = adjustingFactor(userFeature, movieFeature, numFeatures, userIdHash[trainUserIdList[q]], movieIdHash[trainMovieIdList[q]]);												
					if(!userIdHash.ContainsKey(testUserIdList[q])) {
						Console.WriteLine("User-id: key {0} not present",testUserIdList[q]);
					}
					if(!movieIdHash.ContainsKey(testMovieIdList[q])) {
						Console.WriteLine("Movie-id: key {0} not present",testMovieIdList[q]);
					}
					testUserMovieProduct = adjustingFactor(userFeature, movieFeature, numFeatures, userIdHash[testUserIdList[q]], movieIdHash[testMovieIdList[q]]);
					trainPredictRating = globalAverage + trainUserMovieProduct;
					testPredictRating = globalAverage + testUserMovieProduct;
					
					trainErr = trainRatingArray[q] - trainPredictRating;						 			
					testErr = testRatingArray[q] - testPredictRating;
				
					uId = userIdHash[trainUserIdList[q]];
					mId = movieIdHash[trainMovieIdList[q]];
					trainErrPerEpoch += trainErr * trainErr;
					testErrPerEpoch += testErr * testErr;
					
					for (int j = 0; j < numFeatures; ++j) {																																																			
						uv = userFeature[j,uId];
						userFeature[j,uId] += lrate * (trainErr * movieFeature[j,mId]);
						movieFeature[j,mId] += lrate * (trainErr * uv);
					}																
				}
				
				trainErrPerEpoch = Math.Sqrt(trainErrPerEpoch/numEntries);	
				testErrPerEpoch = Math.Sqrt(testErrPerEpoch/numEntries);
					
				Console.WriteLine( "Epoch = {0}, trainErrPerEpoch = {1} testErrPerEpoch = {2}", itr, trainErrPerEpoch, testErrPerEpoch);
				trainErrList.Add(itr, trainErrPerEpoch);
				testErrList.Add(itr, testErrPerEpoch);
			}
			
			stopwatch.Stop();
			Console.WriteLine("Time elapsed: {0}", stopwatch.Elapsed);
			
			itr = 0;
			foreach (KeyValuePair<int, double> errId in trainErrList)
            {
				string tmp = errId.Key + " " + errId.Value;
				line[itr] = tmp;
				itr++;                
            }
			System.IO.File.WriteAllLines("train_error.txt", line);
			
			itr = 0;
			foreach (KeyValuePair<int, double> errId in testErrList)
            {
				string tmp = errId.Key + " " + errId.Value;
				line[itr] = tmp;
				itr++;                
            }
			System.IO.File.WriteAllLines("test_error.txt", line);
		}
		
		public static void Main (string[] args)
		{	
			int numUsers;
			int numMovies;			
			int epochs = 100;
			int numFeatures = 50;
			double K = 25;
			double lrate = 0.001;
						
			List<string> trainUserIdList = new List<string>();
			List<string> trainMovieIdList = new List<string>();
			List<int> trainRatingList = new List<int>();
			
			List<string> testUserIdList = new List<string>();
			List<string> testMovieIdList = new List<string>();
			List<int> testRatingList = new List<int>();
			
			Dictionary<string, List<int>> userIdMapping = new Dictionary<string, List<int>>();
			Dictionary<string, List<int>> movieIdMapping = new Dictionary<string, List<int>>();			
			
			readTrainSet("/home/whitepearl/upb/Program/dataset/dataset_train.txt", 
			             ref userIdMapping, 
			             ref movieIdMapping, 
			             ref trainUserIdList, 
			             ref trainMovieIdList, 
			             ref trainRatingList);
			
			readTestSet("/home/whitepearl/upb/Program/dataset/dataset_test.txt", 
			             ref testUserIdList, 
			             ref testMovieIdList, 
			             ref testRatingList);
			
			numUsers = userIdMapping.Count;
			numMovies = movieIdMapping.Count;
			
			double[,] userFeature = new double[numFeatures,numUsers];
			double[,] movieFeature = new double[numFeatures,numMovies];						

			trainTestError(K,		           
		          epochs,
		          numUsers,
		          numMovies,
		          numFeatures,
		          lrate,
			      ref userFeature,
			      ref movieFeature,
		          trainUserIdList, 
		          trainMovieIdList,
		          trainRatingList,
		          userIdMapping,
		          movieIdMapping,
			      testUserIdList, 
		          testMovieIdList,
		          testRatingList
			      );
			
			Process.Start("plot.sh");

		}		
	}
}

