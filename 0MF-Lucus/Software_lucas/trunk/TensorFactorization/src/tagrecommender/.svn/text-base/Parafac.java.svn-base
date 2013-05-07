package tagrecommender;

import gnu.trove.TIntHashSet;
import gnu.trove.TIntObjectHashMap;

import java.beans.FeatureDescriptor;
import java.io.BufferedWriter;
import java.io.FileWriter;
import java.io.IOException;
import java.util.HashMap;
import java.util.Iterator;
import java.util.LinkedList;
import java.util.Map;

import util.SortingAlgorithms;

import data.TripleDataInput;
import de.ismll.bootstrap.Parameter;

public class Parafac {
	
	@Parameter(cmdline="reg", description="regularization parameter")
	private float  reg = 0.0f;
	
	@Parameter(cmdline="learnRate", description="The learn rate")
	private float  learnRate = 0.01f;
	
	@Parameter(cmdline="numFeatures", description="The number of features to be used")
	private int    numFeatures = 2;
	
	@Parameter(cmdline="maxIter", description="The number of iterations to be performed")
	private int    maxIter = 1000;
	
	private double samples = 1.0;
	private float  tolerance = 0.000001f;
	
	public int[] dimensions;
	
	double[][] factor1, factor2, factor3;
	
	public int[][] data;
	public TIntHashSet tags;
	public HashMap<String, TIntHashSet> posts;
	
	double labels[];
	
	public Parafac(){
		
	}
	
	public void initialize(){
		factor1 = new double[dimensions[0]][getNumFeatures()];
		factor2 = new double[dimensions[1]][getNumFeatures()];
		factor3 = new double[dimensions[2]][getNumFeatures()];

		
		for(int i = 0; i < factor1.length; i++){
			for(int j = 0; j < getNumFeatures(); j++){
				factor1[i][j] = ((double) Math.random()*2)-1.0;
			}
		}
		
		for(int i = 0; i < factor2.length; i++){
			for(int j = 0; j < getNumFeatures(); j++){
				factor2[i][j] = ((double) Math.random()*2)-1.0;
			}
		}
		
		for(int i = 0; i < factor3.length; i++){
			for(int j = 0; j < getNumFeatures(); j++){
				factor3[i][j] = ((double) Math.random()*2)-1.0;
			}
		}
	}
	
	public void train(){
		double lastRMSE;
		double currRMSE = 100;
		int iter = 0;
		
		do{
			System.out.print(iter + ": ");
			iterate();
			System.out.println();
//			System.out.println("Iteration: " + iter + " -- RMSE: " + currRMSE + " -- Difference: " + (currRMSE - lastRMSE));
		}while(iter++ < getMaxIter());
	}
	
	public void iterate(){
		int numSamples = (int)(data.length*samples);
		for(int o = 0; o < numSamples; o++){
//			System.out.print(".");
			int row = (int) (Math.random() * (data.length-1));
			int u = data[row][0];
			int i = data[row][1];
			int ta = data[row][2];
			
			String post = u + " " + i;
			
			int tb = ta;
			
			do{
				tb = tags.toArray()[(int) (Math.random() * (tags.size()-1))];
			}while(posts.get(post).contains(tb));
			
			double yab = predict(u,i,ta) - predict(u,i,tb);
			double delta = 1 - sigma(yab);
			
			for(int f = 0; f < getNumFeatures(); f++){

				double grad1 = delta * (factor2[i][f]*(factor3[ta][f] - factor3[tb][f])) - getReg()*factor1[u][f];
				double grad2 = delta * (factor1[u][f]*(factor3[ta][f] - factor3[tb][f])) - getReg()*factor2[i][f];
				double grad3 = delta * (factor1[u][f]*factor2[i][f]) - getReg()*factor3[ta][f];
				double grad4 = -delta * (factor1[u][f]*factor2[i][f]) - getReg()*factor3[tb][f];

				factor1[u][f] += getLearnRate() * grad1;
				factor2[i][f] += getLearnRate() * grad2;
				factor3[ta][f] += getLearnRate() * grad3;
				factor3[tb][f] += getLearnRate() * grad4;
			}
		}

	}
	
	private double sigma(double a){
		return 1.0 / (1.0 + Math.exp(-a));
	}
	
	public double predict(int[] triple){
		double pred = 0;	
		for(int i = 0; i < getNumFeatures(); i++){			
			pred += factor1[triple[0]][i]*factor2[triple[1]][i]*factor3[triple[2]][i];			
		}
		return pred;
	}
	
	public double predict(int u, int i, int t){
		double pred = 0;
		
		for(int f = 0; f < getNumFeatures(); f++){			
			pred += factor1[u][f]*factor2[i][f]*factor3[t][f];			
		}
		return pred;
	}
	
	public double evaluate(){
		double error = 0;
		for(int row = 0; row < data.length; row++){
			error += labels[row]-predict(data[row]);
			
		}
		return error;
	}
	
	public static void main(String args[]){
		Parafac parafac = new Parafac();
		String dataset = "beatles2";
		
		TripleDataInput input = new TripleDataInput();
		System.out.println("Reading Data...");
//		parafac.data = input.loadTrainingData("/home/lucas/workspace/RDFParsing/tagrecommender/" + dataset + "-0.train.txt");
		parafac.data = input.loadTrainingData("/home/lucas/tools/tagrecommender/experiments/ex-0.train.txt");

		System.out.println("Data read!");

		parafac.dimensions = new int[3];
		parafac.dimensions[0] = input.biggest1+1;
		parafac.dimensions[1] = input.biggest2+1;
		parafac.dimensions[2] = input.biggest3+1;
		
		parafac.posts = input.posts;
		parafac.tags = input.tags;
		
		parafac.initialize();
		System.out.print("Training");
		parafac.train();
		
		System.out.print("Writing Predictions");
		TIntObjectHashMap<TIntHashSet> data = input.loadTestData("/home/lucas/tools/tagrecommender/experiments/ex-0.test.txt");
		try {
//			parafac.writePredictions("/home/lucas/workspace/RDFParsing/results/baselines/" + dataset + "-"+ parafac.getNumFeatures() +"-parafacbpr-0", data);
			parafac.writePredictions("/home/lucas/" + dataset + "-"+ parafac.getNumFeatures() +"-parafacbpr-0", data);
		} catch (IOException e) {
			e.printStackTrace();
		}
		parafac.printResults();
		System.out.println(parafac.predict(1, 1, 4));
		System.out.println(parafac.predict(1, 1, 5));
		
	}
	
	public void writePredictions(String results, TIntObjectHashMap<TIntHashSet> data) throws IOException{
		BufferedWriter out = new BufferedWriter(new FileWriter(results));
		
		for(int u : data.keys()){
			for(int i : data.get(u).toArray()){
				HashMap<Integer, Double> predictions = new HashMap<Integer, Double>();
				for(int t : tags.toArray()){
					predictions.put(t, predict(u,i,t));
				}
				
				LinkedList<Integer> orderedPredictions = SortingAlgorithms.sortByValue(predictions);
								
				for(int index = 0; index < 100 && index< orderedPredictions.size(); index++){
					Integer t = orderedPredictions.get(index);
					out.write(u + " " + i + " " + t + " " + predictions.get(t) + "\n");
				}
			}
		}
		
		out.close();
	}
	
	private void printResults(){
		System.out.println("Factor1: ");
		for(int i = 0; i < factor1.length; i++){
			for(int j = 0; j < getNumFeatures(); j++){
				System.out.print(factor1[i][j] + " ");
			}
			System.out.println();
		}
		
		System.out.println("\n\nFactor2: ");
		for(int i = 0; i < factor2.length; i++){
			for(int j = 0; j < getNumFeatures(); j++){
				System.out.print(factor2[i][j] + " ");
			}
			System.out.println();
		}
		System.out.println("\n\nFactor3: ");
		for(int i = 0; i < factor3.length; i++){
			for(int j = 0; j < getNumFeatures(); j++){
				System.out.print(factor3[i][j] + " ");
			}
			System.out.println();
		}
	}


	public void setReg(float reg) {
		this.reg = reg;
	}

	public float getReg() {
		return reg;
	}

	public void setLearnRate(float learnRate) {
		this.learnRate = learnRate;
	}

	public float getLearnRate() {
		return learnRate;
	}

	public void setNumFeatures(int numFeatures) {
		this.numFeatures = numFeatures;
	}

	public int getNumFeatures() {
		return numFeatures;
	}

	public void setMaxIter(int maxIter) {
		this.maxIter = maxIter;
	}

	public int getMaxIter() {
		return maxIter;
	}
}
