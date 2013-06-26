using System;
using System.IO;

namespace MultiRecommender.IO
{
	public class Model 
	{
		public static StreamWriter getWriter(string fileName, string recommenderName)
		{
			var writer = new StreamWriter("../../../../models/"+fileName);
			writer.WriteLine(recommenderName);
			return writer;
		}

		public static StreamReader getReader(string fileName)
		{
			var reader = new StreamReader("../../../../models/"+fileName);
			reader.ReadLine(); //to ignore name of recommender sys
			return reader;
		}

		//TODO: Save model parameters also with description
		public static void saveModel(StreamWriter writer)
		{
			using(writer) {
				writer.WriteLine(Initialize.globalAvg);
				writer.WriteLine(Initialize.MIN_RATING);
				writer.WriteLine(Initialize.MAX_RATING);
				writer.WriteLine(Initialize.MAX_USER_ID);
				writer.WriteLine(Initialize.MAX_ITEM_ID);
				MatrixExtension.WriteVector(writer, Initialize.USER);
				MatrixExtension.WriteMatrix(writer, Initialize.USER);
				MatrixExtension.WriteVector(writer, Initialize.ITEM);
				MatrixExtension.WriteMatrix(writer, Initialize.ITEM);
			}
		}

		public static void loadModel(StreamReader reader)
		{
			using(reader) {
				Initialize.globalAvg = Double.Parse(reader.ReadLine());
				Initialize.MIN_RATING = int.Parse(reader.ReadLine());
				Initialize.MAX_RATING = int.Parse(reader.ReadLine());
				Initialize.MAX_USER_ID = int.Parse(reader.ReadLine());
				Initialize.MAX_ITEM_ID = int.Parse(reader.ReadLine());
				MatrixExtension.ReadVector(reader, Initialize.USER);
				MatrixExtension.ReadMatrix(reader, Initialize.USER);
				MatrixExtension.ReadVector(reader, Initialize.ITEM);
				MatrixExtension.ReadMatrix(reader, Initialize.ITEM);
			
//				if (_userFeatures.Length != _itemFeatures.Length) {
//					throw new IOException(string.Format(
//						"Feature dimensions do not match {0}:{1}, MAX_USER_ID: {2}, MAX_ITEM_ID: {3}",
//						_userFeatures.Length, _itemFeatures.Length, _maxUserId, _maxItemId));
//				}
			}
		}
	}
}

