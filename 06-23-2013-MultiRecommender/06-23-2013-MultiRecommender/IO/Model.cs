using System;
using System.IO;

namespace MultiRecommender
{
	public class Model 
	{
		public static StreamWriter getWriter(string fileName, string recommenderName)
		{
			var writer = new StreamWriter("./models/"+fileName);
			writer.WriteLine(recommenderName);
			return writer;
		}

		public static void saveModel(StreamWriter writer)
		{
			using(writer) {
				writer.WriteLine(Init.globalAvg);
				writer.WriteLine(Init.MIN_RATING);
				writer.WriteLine(Init.MAX_RATING);
				MatrixExtension.WriteVector(writer, Init.USER);
				MatrixExtension.WriteMatrix(writer, Init.USER);
				MatrixExtension.WriteVector(writer, Init.ITEM);
				MatrixExtension.WriteMatrix(writer, Init.ITEM);
			}
		}
	}
}

