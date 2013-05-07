package FactorizationModel;

import java.io.File;
import java.io.FileNotFoundException;
import java.io.PrintStream;

import gnu.trove.TIntHashSet;
import gnu.trove.TIntObjectHashMap;
import utils.SparseArray;
import datagenerator.DataInput;


public abstract class GeneralModel {

	/**
	 * Parameter matrix k x F where:
	 * k: number of new dimensions
	 * F: number of features
	 */
	protected double[][] params;
	
	double learnRate;
	double reg;
	
	double classConst;
	
	int k, f;

	private int maxIter;

	
	protected TIntObjectHashMap<SparseArray> featureVectors;
	protected TIntObjectHashMap<TIntHashSet> positivePairs, negativePairs;
	
	
	
	
	public void init(){
		
		
		
		params = new double[k][f];
		
		for(int i = 0; i < k; i++){
			for(int j = 0; j < f; j++){				
				params[i][j] = ((double) Math.random())*0.0001-0.0001;
			}
		}
		
		
		
	}
	
	
	public void train(){
		double currRMSE = 100;
		int iter = 0;
		do{			
			iterate();
//			currRMSE = loss();
			System.err.println("Iteration: " + iter + " -- Loss: " + currRMSE );
			if(iter %10 == 0){
				try {
					PrintStream out = new PrintStream(new File("/home/lucas/bla-"+iter+".txt"));
					printResults(out);
				} catch (FileNotFoundException e) {
					// TODO Auto-generated catch block
					e.printStackTrace();
				}
			}
		}while(iter++ < maxIter);
		
		
	}

	
	public abstract void printResults(PrintStream out);

	public abstract void iterate();
	
	public abstract double[] map(int object); 
	
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


	public TIntObjectHashMap<TIntHashSet> getPositivePairs() {
		return positivePairs;
	}


	public void setPositivePairs(TIntObjectHashMap<TIntHashSet> positivePairs) {
		this.positivePairs = positivePairs;
	}


	public TIntObjectHashMap<TIntHashSet> getNegativePairs() {
		return negativePairs;
	}


	public void setNegativePairs(TIntObjectHashMap<TIntHashSet> negativePairs) {
		this.negativePairs = negativePairs;
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
}
