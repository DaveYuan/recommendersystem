using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace csvfilereadwrite
{
	class MainClass
	{
		private string[][] data;
		
		MainClass() 
		{
			data = new string[][]{  
	                new string[]{"Col1 Row1", "Col2-Row1", "Col3-Row1"},  
 	                new string[]{"Col1-Row2", "Col2-Row2", "Col3-Row2"}  
 	            };  			
		}
		
		public void writeToCSV(string fileName)
		{
			int len = data.GetLength(0);
			StringBuilder builder = new StringBuilder();
			
			for (int indx = 0; indx < len; indx++) {
				builder.AppendLine(string.Join(",", data[indx]));
			}
			
			File.WriteAllText(fileName, builder.ToString());
			//File.AppendAllText(fileName, builder.ToString());
		}		
		
		public static void Main (string[] args)
		{
			MainClass mainclass = new MainClass();
			File.Open("CSV_file_created.csv", FileMode.Create).Close();
			
			mainclass.writeToCSV("CSV_file_created.csv");
			Console.WriteLine ("File created!");
		}
	}
}
