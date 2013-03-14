using System;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;

namespace bprmatrixfact
{
	class MainClass
	{
		static void readTrainSet(String fileName, 
		                         ref Dictionary<string, List<int>> userIdMapping, 
		                         ref Dictionary<string, List<int>> itemIdMapping,
		                         ref Dictionary<string, List<string>> ratedItemsPerUser,
		                         ref List<string> trainUserIdList,
		                         ref List<string> trainItemIdList,
		                         ref List<int> trainRatingList
		                         )
		{
			int userIdIndex;
			int itemIdIndex;
			int rowIndexCounter;	
			
			string[] lines = System.IO.File.ReadAllLines(fileName);
			
			userIdIndex = itemIdIndex = 0;
			
			foreach (string line in lines)
	        {	
				string[] stringSeparator = new string[] { "\t" };
				string user = null;
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
						user = s;
					} 
					
					if (rowIndexCounter == 1) {
						trainItemIdList.Add(s);
						if (itemIdMapping.ContainsKey(s))
						{
							itemIdMapping[s].Add(itemIdIndex);				
							itemIdIndex++;
						} else {
							List<int> tmpIndexList = new List<int>();
							tmpIndexList.Add(itemIdIndex);
							itemIdMapping.Add(s,tmpIndexList);
							itemIdIndex++;
						}
						if (ratedItemsPerUser.ContainsKey(user))
						{
							ratedItemsPerUser[user].Add(s);
						} else {
							List<string> tmpIndexList = new List<string>();
							tmpIndexList.Add(s);
							ratedItemsPerUser.Add(user, tmpIndexList);
						}
					}
					
					if(rowIndexCounter == 2) {
						trainRatingList.Add(Convert.ToInt32(s));
					}					
					rowIndexCounter++;
				}
			}	

/*			
			foreach (KeyValuePair<string, List<string>> ratedItem in ratedItemsPerUser) {
				Console.Write("{0}: ", ratedItem.Key);
				foreach (string str in ratedItem.Value) {
					Console.Write( "{0}, ", str);
				}
				Console.WriteLine();
			}
*/						
		}
		
		public static void Main (string[] args)
		{			
			List<string> trainUserIdList = new List<string>();
			List<string> trainItemIdList = new List<string>();
			List<int> trainRatingList = new List<int>();
			
			Dictionary<string, List<int>> userIdMapping = new Dictionary<string, List<int>>();
			Dictionary<string, List<int>> itemIdMapping = new Dictionary<string, List<int>>();	
			Dictionary<string, List<string>> ratedItemsPerUser = new Dictionary<string, List<string>>();
			
			readTrainSet("train.txt.mtx", 
			             ref userIdMapping, 
			             ref itemIdMapping, 
			             ref ratedItemsPerUser,
			             ref trainUserIdList, 
			             ref trainItemIdList, 
			             ref trainRatingList
			             );						
		}
	}
}

