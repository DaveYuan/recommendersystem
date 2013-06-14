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
			
			while (!reader.EndOfStream) {
				itrMinRMSE = 0;
				minRMSE = Double.MaxValue;
				string[] rowData = new string[10];
				for (int i = 1; i <= numEpochs; i++) {
					var line = reader.ReadLine();
					var values = line.Split(',');
					if (Convert.ToDouble(values[9]) < minRMSE) {
						minRMSE = Convert.ToDouble(values[9]);
						itrMinRMSE = i;
					}
					if (i == 1) {
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