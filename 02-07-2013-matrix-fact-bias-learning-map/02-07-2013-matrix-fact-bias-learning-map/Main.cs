using System;
using System.Diagnostics;
using System.Collections.Generic;

namespace matrixfactbiaslearningmap
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
		                         ref List<string> userIdList,
		                         ref List<string> movieIdList,
		                         ref List<int> ratingList)
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
						userIdList.Add(s);
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
						movieIdList.Add(s);
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
						ratingList.Add(Convert.ToInt32(s));
					}
					
					rowIndexCounter++;
				}
			}	
		}	
		
		static double calcGlobalAverage(List<int> ratingList) 
		{
			int sum = 0;
			int size = ratingList.Count;
			double globalAverage;
			
			foreach (int rating in ratingList) {
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
		                    int[] ratingArray)
		{
			double sum;
			int size;
			
			foreach (KeyValuePair<string, List<int>> userId in userIdMapping) {
				size = userId.Value.Count;
				sum = 0.0;
				foreach (int indx in userId.Value) {
					sum += ratingArray[indx];
				}
				sum = sum / size;
				userIdAvgHash.Add(userId.Key, sum);
			}
		}
		
		static void movieAvg(ref Dictionary<string, double> movieIdAvgHash,
		                     Dictionary<string, List<int>> movieIdMapping,
		                     int[] ratingArray)
		{
			double sum;
			int size;
			
			foreach (KeyValuePair<string, List<int>> movieId in movieIdMapping) {
				size = movieId.Value.Count;
				sum = 0.0;
				foreach (int indx in movieId.Value) {
					sum += ratingArray[indx];
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
		                  ref double[,] userFeature,
		                  ref double[,] movieFeature,
		                  List<string> userIdList, 
		                  List<string> movieIdList,
		                  List<int> ratingList,
		                  Dictionary<string, List<int>> userIdMapping,
		                  Dictionary<string, List<int>> movieIdMapping		           
		                  )
		{	
			int uId;
			int mId;
			int itr;
			int numEntries;
			int userIdCounter;
			int movieIdCounter;						
			double uv;
			double err;				
			double errPerEpoch;		
			double predictRating;			
			double globalAverage;
			double userMovieProduct;
			
			string[] line = new string[epochs];
			Dictionary<int, double> errList = new Dictionary<int, double>();
			Dictionary<string, int> userIdHash = new Dictionary<string, int>();
			Dictionary<string, int> movieIdHash = new Dictionary<string, int>();
			Dictionary<string, double> userIdAvgHash = new Dictionary<string, double>();
			Dictionary<string, double> movieIdAvgHash = new Dictionary<string, double>();			
			Dictionary<string, double> userBiasHash = new Dictionary<string, double>();
			Dictionary<string, double> movieBiasHash = new Dictionary<string, double>();		
	
			numEntries = ratingList.Count;
			int[] ratingArray = ratingList.ToArray();			
			
			globalAverage = calcGlobalAverage(ratingList);			
		
			initFeatures(userIdMapping.Count, ref userFeature, 
			             movieIdMapping.Count, ref movieFeature,
			             numFeatures);			
			
			userIdCounter = movieIdCounter = 0;
			userAvg(ref userIdAvgHash, userIdMapping, ratingArray);
			movieAvg(ref movieIdAvgHash, movieIdMapping, ratingArray);
			
			
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
			
			for (int j = 0; j < numFeatures; ++j) {	
				errPerEpoch = 0.0;
				err = 0;
				for (itr = 0; itr < epochs; ++itr) {
					errPerEpoch = 0.0;
					for (int q = 0; q < numEntries; ++q) {						
																							
					//	userMovieProduct = adjustingFactor(userFeature, movieFeature, numFeatures, userIdHash[userIdList[q]], movieIdHash[movieIdList[q]]);						
						predictRating = globalAverage + userBiasHash[userIdList[q]] + movieBiasHash[movieIdList[q]];
	
						err = ratingArray[q] - predictRating;						 			
						uId = userIdHash[userIdList[q]];
						mId = movieIdHash[movieIdList[q]];
							
					//	uv = userFeature[j,uId];
					//	userFeature[j,uId] += lrate * (err * movieFeature[j,mId] - K * uv);
					//	movieFeature[j,mId] += lrate * (err * uv - K * movieFeature[j,mId]);	
					
						userBiasHash[userIdList[q]] += lrate * (err - K * userBiasHash[userIdList[q]]);
						movieBiasHash[movieIdList[q]] += lrate * (err - K * movieBiasHash[movieIdList[q]]);
						
						//userBiasHash[userIdList[q]] += lrate * (err);
						//movieBiasHash[movieIdList[q]] += lrate * (err);
						errPerEpoch += err*err;
					}															
					errPerEpoch = Math.Sqrt(errPerEpoch/numEntries);	
					
					Console.WriteLine( "Training feature = {0}, Epoch = {1}, errPerEpoch = {2},", j+1, itr, errPerEpoch);
				}					
				errList.Add(j, errPerEpoch);				
			}
			
			stopwatch.Stop();
			Console.WriteLine("Time elapsed: {0}", stopwatch.Elapsed);
			
			itr = 0;
			foreach (KeyValuePair<int, double> errId in errList)
            {
				string tmp = errId.Key + " " + errId.Value;
				line[itr] = tmp;
				itr++;                
            }
			System.IO.File.WriteAllLines("/home/whitepearl/upb/Program/02-07-2013-matrix-fact-bias-learning-map/error.txt", line);						
		}
		
		public static void Main (string[] args)
		{	
			int numUsers;
			int numMovies;			
			int epochs = 60;
			int numFeatures = 50;
			double K = 0.3;
			double lrate = 0.001;
						
			List<string> userIdList = new List<string>();
			List<string> movieIdList = new List<string>();
			List<int> ratingList = new List<int>();
			
			Dictionary<string, List<int>> userIdMapping = new Dictionary<string, List<int>>();
			Dictionary<string, List<int>> movieIdMapping = new Dictionary<string, List<int>>();			
			
			readTrainSet("/home/whitepearl/upb/Program/02-07-2013-matrix-fact-bias-learning-map/dataset_train.txt", 
			             ref userIdMapping, 
			             ref movieIdMapping, 
			             ref userIdList, 
			             ref movieIdList, 
			             ref ratingList);
			
			numUsers = userIdMapping.Count;
			numMovies = movieIdMapping.Count;
			
			double[,] userFeature = new double[numFeatures,numUsers];
			double[,] movieFeature = new double[numFeatures,numMovies];						

			train(K,		           
		          epochs,
		          numUsers,
		          numMovies,
		          numFeatures,
		          lrate,
			      ref userFeature,
			      ref movieFeature,
		          userIdList, 
		          movieIdList,
		          ratingList,
		          userIdMapping,
		          movieIdMapping
			      );
			
			Process.Start("/home/whitepearl/upb/Program/02-07-2013-matrix-fact-bias-learning-map/plot.sh");
		}		
	}
}
