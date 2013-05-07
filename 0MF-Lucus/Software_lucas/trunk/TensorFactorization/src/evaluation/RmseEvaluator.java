package evaluation;

import java.io.BufferedReader;
import java.io.FileReader;
import java.io.IOException;
import java.util.HashMap;
import java.util.Map;
import java.util.StringTokenizer;

/**
 * RmseEvaluator
 * 
 * Computes the RMSE from 3 different data formats:
 *  - double arrays
 *  - text files
 *  - numeric values
 *  
 *  
 * @author lucas
 *
 */
public class RmseEvaluator {
	
	private double error;
	
	private int numObservations;
	
	
	public RmseEvaluator(){
		init();
	}
	
	public void init(){
		error = 0;
		numObservations = 0;
	}
	
	/**
	 * Compute the RMSE given two arrays. 
	 * Ex:
	 * 
	 * double[] predicted = {0.7,0.9,0.3};
	 * double[] realRatings = {1,1,0};
	 * 
	 * RmseEvaluator evaluator = new RmseEvaluator();
	 * evaluator.rmse(predicted, realRatings);
	 * 
	 * @param predicted - array containing the predicted values
	 * @param realRatings - array containing the actual ratings 
	 * @return the RMSE value
	 */
	public double rmse(double[] predicted, double[] realRatings){
		error = 0;
		
		for(int i = 0; i < predicted.length; i++){
			error += (predicted[i]-realRatings[i])*(predicted[i]-realRatings[i]);
		}
		
		error /= predicted.length;
		
		return Math.sqrt(error);
	}
	
	/**
	 * Example:  
	 * RmseEvaluator evaluator = new RmseEvaluator();
	 * 
	 * evaluator.newObservation(0.7,1);
	 * evaluator.newObservation(0.9,1);
	 * evaluator.newObservation(0.3,0);
	 * 
	 * evaluator.rmse();
	 * 
	 * 
	 * @return the RMSE for the observations entered with the newObservation method
	 */
	public double rmse(){
		return Math.sqrt(error/(double)numObservations);
	}
	
	/**
	 * Example:
	 * Given the file "predicted.txt":
	 * 1	1	0.7
	 * 1	2	0.9
	 * 2	2	0.3
	 * 
	 * and the file "actualRatings.txt":
	 * 1	1	1
	 * 1	2	1
	 * 2	2	0
	 * 
	 * The usage is as follows:
	 * 
	 * RmseEvaluator evaluator = new RmseEvaluator(); 
	 * evaluator.rmse("predicted.txt","actualRatings.txt");
	 * 
	 * @param predictedFile file containing the ratings predicted by the model (movielens format)
	 * @param realRatingsFile file containing the actual ratings (movielens format)
	 * @return the RMSE given the two input files
	 * @throws IOException
	 */
	public double rmse(String predictedFile, String realRatingsFile) throws IOException{
		
		Map<String,Double> predicted = loadRatings(predictedFile);
		Map<String,Double> realRatings = loadRatings(realRatingsFile);
		
		error = 0;
		
		for(String s : predicted.keySet()){
			error += (predicted.get(s)-realRatings.get(s))*(predicted.get(s)-realRatings.get(s));
		}
		
		error /= predicted.size();
		
		return Math.sqrt(error);
	}
	
	/**
	 * Adds a new observation for the computation of the RMSE (to be used 
	 * in conjuction with the method rmse())
	 * 
	 * @param predicted
	 * @param realRating
	 */
	public void newObservation(double predicted, double realRating){
		error += (predicted-realRating)*(predicted-realRating);
		numObservations++;
	}		
		
	public Map<String, Double> loadRatings(String fileName)throws IOException{
		BufferedReader file = new BufferedReader(new FileReader(fileName));		
		
		String line;
		Map<String, Double> ratings = new HashMap<String, Double>();
		
		while((line = file.readLine())!=null && line.length() > 1){
			
			StringTokenizer st = new StringTokenizer(line);
			String u = st.nextToken();
			String i = st.nextToken();
			double r = Double.parseDouble(st.nextToken());								
						
			ratings.put(u + " " + i, r);			
		}
		
		return ratings;
	}

}
