using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using ProtoBuf;

namespace mldatamanipulationnoidmapping
{
	class MainClass
	{
		static void writeToConsole(string mssg) 
		{			
			Console.WriteLine("\t" + mssg);
		}
		
		static string[] readAllLines(string fileName)
		{
			return System.IO.File.ReadAllLines(fileName);
		}
		
		static void createBin(string fileName)
		{
			Rating rating = new Rating();
			rating.usersList = new List<int>();
			rating.itemsList = new List<int>();
			rating.ratingsList = new List<int>();						
			
			writeToConsole(fileName + " data loading");
			
			int rowIndexCounter;
			string[] ratings = readAllLines(fileName);			
			
			foreach (string line in ratings) {
				string[] stringSeparator = new string[] { "\t" };			
				string[] result = line.Split(stringSeparator, StringSplitOptions.None);
				rowIndexCounter = 0;
				
				foreach (string s in result)
				{						
					if (rowIndexCounter == 0) {											
						rating.usersList.Add(Convert.ToInt32(s));
					} 
					if (rowIndexCounter == 1) {												
						rating.itemsList.Add(Convert.ToInt32(s));					
					}
					if (rowIndexCounter == 2) {
						rating.ratingsList.Add(Convert.ToInt32(s));
					}						
					rowIndexCounter++;
				}
			}		
			
			writeToConsole(fileName +" data loaded");						           		
			writeToConsole(fileName + ".bin creation in progress");
			
			using (FileStream file = File.Create(fileName + ".bin"))
			{
				Serializer.Serialize(file, rating);
			}
			
			writeToConsole(fileName + ".bin creation completed");			
			writeToConsole(fileName + "bin data loading");

			Rating r1;
			using (FileStream file = File.OpenRead(fileName + ".bin"))
			{
				r1 = Serializer.Deserialize<Rating>(file);
			}
			           
			writeToConsole(fileName + ".bin data loaded");			
			Console.WriteLine("OrigUser => User[0]: {0}, User[end]: {1}", rating.usersList[0], rating.usersList[rating.usersList.Count-1]);
			Console.WriteLine("ActUser => User[0]: {0}, User[end]: {1}", r1.usersList[0], r1.usersList[r1.usersList.Count-1]);
			
			Console.WriteLine("OrigItem => Item[0]: {0}, Item[end]: {1}", rating.itemsList[0] , rating.itemsList[rating.itemsList.Count-1]);
			Console.WriteLine("Actitem => Item[0]: {0}, Item[end]: {1}", r1.itemsList[0] , r1.itemsList[r1.itemsList.Count-1]);
			
			Console.WriteLine("OrigRating => Rating[0]: {0}, Rating[end]: {1}", rating.ratingsList[0] , rating.ratingsList[rating.ratingsList.Count-1]);			
			Console.WriteLine("ActRating => Rating[0]: {0}, Rating[end]: {1}", r1.ratingsList[0] , r1.ratingsList[r1.ratingsList.Count-1]);	
		}
		
		public static void Main (string[] args)
		{			
			createBin("u1.base");
			createBin("u1.test");

			Console.WriteLine("Done!");
		}
	}
}
