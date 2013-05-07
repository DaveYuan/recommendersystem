package matrixfactorization;

import evaluation.RMSEEvaluator;

public abstract class MatrixFactorization {
	
	public static final int USER = 0;
	public static final int ITEM = 1;


	public abstract double predict(int u, int i);

	public abstract double error(int u, int i, int targetRow);

	public abstract void iterate();

	protected double reg = 0.001f;
	private double learnRate = 0.02f;
	private int numFeatures = 30;
	protected int maxIter = 15;
	public int[] dimensions;
	public int[] ratings;
	protected double[][] factor1;
	protected double[][] factor2;
	protected double samples = 1.0;
	public int[][] data;
	public int[][] testData;
	
	protected double globalAverage;

	public MatrixFactorization() {
		super();
	}

	public void initialize() {
		
		
		factor1 = new double[dimensions[USER]][getNumFeatures()];
		factor2 = new double[dimensions[ITEM]][getNumFeatures()];

		
		for(int i = 0; i < factor1.length; i++){
			for(int j = 0; j < getNumFeatures(); j++){
				factor1[i][j] = ((double) Math.random()*0.02)-0.01;
			}
		}
		
		for(int i = 0; i < factor2.length; i++){
			for(int j = 0; j < getNumFeatures(); j++){
				factor2[i][j] = ((double) Math.random()*0.02)-0.01;
			}
		}
		
		ratings = new int[testData.length];
		for(int i = 0; i < ratings.length; i++){
			ratings[i] = testData[i][2];
		}
		
		//Compute Global Average
		globalAverage = 0;
		for(int i : ratings){
			globalAverage += i;
		}
		globalAverage /= ratings.length;
				
	}

	public void train(){
		double currRMSE = 100;
		int iter = 0;
		do{			
			iterate();
			currRMSE = RMSEEvaluator.rmse(ratings, generatePredictions(testData));
			System.err.println("Iteration: " + iter + " -- RMSE: " + currRMSE );
		}while(iter++ < maxIter);
	}
	
	public double[] generatePredictions(int[][] testData) {
		
		double[] predictions = new double[testData.length]; 
			
		for(int row = 0; row < testData.length; row++){
			predictions[row] = predict(testData[row][USER], testData[row][ITEM]);
		}
		
		return predictions;
	}

	public void setNumFeatures(int numFeatures) {
		this.numFeatures = numFeatures;
	}

	public int getNumFeatures() {
		return numFeatures;
	}

	public double getReg() {
		return reg;
	}

	public void setReg(double reg) {
		this.reg = reg;
	}

	public double getLearnRate() {
		return learnRate;
	}

	public void setLearnRate(double learnRate) {
		this.learnRate = learnRate;
	}

	public int getMaxIter() {
		return maxIter;
	}

	public void setMaxIter(int maxIter) {
		this.maxIter = maxIter;
	}

}