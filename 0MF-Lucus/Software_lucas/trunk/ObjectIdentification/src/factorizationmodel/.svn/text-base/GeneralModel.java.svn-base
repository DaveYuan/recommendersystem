package factorizationmodel;

import io.DataInput;

import java.io.BufferedReader;
import java.io.IOException;
import java.io.PrintStream;
import java.util.StringTokenizer;

import gnu.trove.TIntHashSet;
import gnu.trove.TIntObjectHashMap;
import util.SparseArray;


public abstract class GeneralModel {

	/**
	 * Parameter matrix k x F where:
	 * k: number of latent dimensions
	 * F: number of features
	 */
	protected double[][] params;
	
	double learnRate;
	double reg;
	private int maxIter;
	
	double classConst;
	
	int k, f;

	

	
	protected TIntObjectHashMap<SparseArray> featureVectors;
	protected TIntObjectHashMap<TIntHashSet> objectToCluster;
	
	
	public int[] orderedIDs;
	public DataInput in;
	
	public void init(){
						
		params = new double[k][f];
		
		for(int i = 0; i < k; i++){
			for(int j = 0; j < f; j++){				
				params[i][j] = ((double) Math.random())*0.01-0.01;
			}
		}
						
	}
	
	
	public void train(){
		double currLoss = 100;
		int iter = 0;
		
		do{			
			iterate();
			currLoss = loss();
			System.err.println("Iteration: " + iter + " Loss: " + currLoss);							
		}while(iter++ < maxIter);				
	}

	
	public void writeModel(PrintStream out){
		out.println(k + " " + f);
		for(int i = 0; i < k; i++){
			for(int j = 0; j < f; j++){				
				out.print(params[i][j] + " ");
			}
			out.println();
		}
	}
	
	public void loadModel(BufferedReader in) throws IOException{
		String line = in.readLine();
		StringTokenizer st = new StringTokenizer(line);
		setK(Integer.parseInt(st.nextToken()));
		setF(Integer.parseInt(st.nextToken()));
		
		params = new double[k][f];
		
		for(int i = 0; i < k; i++){
			line = in.readLine();			
			st = new StringTokenizer(line);
			
			for(int j = 0; j < f; j++){				
				params[i][j] = Double.parseDouble(st.nextToken());
			}			
		}
		
	}
	
	public abstract void printResults(PrintStream out);

	public abstract void iterate();
	
	public abstract double[] map(int object); 
	
	public abstract double[] map(SparseArray object);
	
	public abstract double loss();
	

	public double getLearnRate() {
		return learnRate;
	}


	public void setLearnRate(double learnRate) {
		this.learnRate = learnRate;
	}


	public double getReg() {
		return reg;
	}


	public void setReg(double reg) {
		this.reg = reg;
	}


	public int getK() {
		return k;
	}


	public void setK(int k) {
		this.k = k;
	}


	public int getMaxIter() {
		return maxIter;
	}


	public void setMaxIter(int maxIter) {
		this.maxIter = maxIter;
	}


	public TIntObjectHashMap<SparseArray> getFeatureVectors() {
		return featureVectors;
	}


	public void setFeatureVectors(TIntObjectHashMap<SparseArray> featureVectors) {
		this.featureVectors = featureVectors;
	}


	public int getF() {
		return f;
	}


	public void setF(int f) {
		this.f = f;
	}


	public double getClassConst() {
		return classConst;
	}


	public void setClassConst(double classConst) {
		this.classConst = classConst;
	}


	public TIntObjectHashMap<TIntHashSet> getMappings() {
		return objectToCluster;
	}


	public void setMappings(TIntObjectHashMap<TIntHashSet> mappings) {
		this.objectToCluster = mappings;
	}
}
