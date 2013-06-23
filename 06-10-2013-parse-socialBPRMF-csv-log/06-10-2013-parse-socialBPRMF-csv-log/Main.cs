using System;
using System.Text;
using System.IO;

namespace parsesocialBPRMFcsvlog
{
	class MainClass
	{
		public static string csvFileName = "parsedLog.csv";
		public static void writeToLog(string[] rowData)
		{
			StringBuilder builder = new StringBuilder();
			builder.AppendLine(string.Join(",", rowData));
			File.AppendAllText(csvFileName, builder.ToString());
		}
		
		public static void Main (string[] args)
		{
			int numEpochs = 50;
			int itrMinRMSE;
			double minRMSE;		
			
			string[] csvHeadLine = new string[]{"#itr", "#feature", "lrate", "regSocial", "regBias", "regPostv", "regNegtv",
			"regUser", "regItem", "RMSE"};
			
			var reader = new StreamReader(File.OpenRead("06-05-2013-JointFactBPR.csv"));
			File.Open(csvFileName, FileMode.Create).Close();
			writeToLog(csvHeadLine);
			var line1 = reader.ReadLine();
			var line = reader.ReadLine();
			
			while (!reader.EndOfStream) {
				itrMinRMSE = 0;
				minRMSE = Double.MaxValue;
				string[] rowData = new string[10];
				while(true) {
					Console.WriteLine ("Hello!");			
					var values = line.Split(',');
					var values_prev = values;
					if (Convert.ToInt32(values_prev[0]) > Convert.ToInt32(values[0])) {
						break;
					}
					if (Convert.ToDouble(values[9]) < minRMSE) {
						minRMSE = Convert.ToDouble(values[9]);
						itrMinRMSE = Convert.ToInt32(values[0]);
					}
					if (Convert.ToInt32(values[0]) == 1) {
						rowData = values;
					}
				}
				rowData[0] = itrMinRMSE.ToString();
				rowData[9] = minRMSE.ToString();	
				writeToLog(rowData);
			}
			Console.WriteLine ("Hello World!");
		}
	}
}