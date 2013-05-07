package matrixfactorization;

import java.io.BufferedReader;
import java.io.BufferedWriter;
import java.io.FileReader;
import java.io.FileWriter;
import java.io.IOException;
import java.util.ArrayList;
import java.util.Collections;
import java.util.List;
import java.util.Random;
import java.util.StringTokenizer;

import javax.sound.sampled.ReverbType;


import data.RatingsData;

import evaluation.AUCEvaluator;
import evaluation.RMSEEvaluator;
import gnu.trove.TIntHashSet;
import gnu.trove.TIntIntHashMap;
import gnu.trove.TIntObjectHashMap;

public abstract class MatrixFactorization {
	
	public static final int USER = 0;
	public static final int ITEM = 1;
	public static final int RATING = 2;


	public abstract double predict(int u, int i, int rel);

	public abstract double error(int u, int i, int targetRow);

	public abstract void iterate();
	public void iterate(int rel){};
	protected double alpha;
	protected double reg;
	protected double reg_neg;
	private double learnRate;
	private int numFeatures;
	protected int maxIter;
	protected boolean abort;
	protected boolean computeFit;
	protected boolean computeError;
	private int shrinkage;
		
	public int[] numEntities;
	public double[] ratings, trainRatings;
	
	TIntHashSet trainItems;
	
	private boolean featuresFixed = false;
	
	protected double fit;
	protected double RMSE;
	
	protected boolean evaluateRMSE = false;

	protected int bestIteration;
	
	public int target;
	
	public int numEntityTypes;
	/**
	 * Entity types are the matrixes of the latent factors.
	 * This is a vector of matrixes Entity x Latent Feature
	 * Index 1 - the entity type
	 * Index 2 - the entity 
	 * Index 3 - the latent feature
	 */
	protected double[][][] entityTypes;
	
	protected double samples = 1.0;
	
	public int numRelations;
	/**
	 * A relation is defined between two entity types
	 * Index 1 - type of relation
	 * Index 2 - Entity type 1
	 * Index 3 - Entity type 2
	 */
	public int[][] relations; 
	
	/**
	 * Stores the relations in the traditional "movielens" triples-way:
	 *   user item rating
	 *   
	 * Index 1 - relation index
	 * Index 2 - row
	 * Index 3 - position in the triple (0 for user, 1 for items, 2 for ratings) 
	 */
	public double[][][] data; 
	public double[][] testData;
	
	private TIntHashSet testItems;
	
	protected double globalAverage;

	public MatrixFactorization() {
		super();
	}

	public void initialize() {		
		
		Random r = new Random();
		
		entityTypes = new double[numEntityTypes][][];
		for(int i = 0; i < numEntityTypes; i++){
			entityTypes[i] = new double[numEntities[i]][getNumFeatures()];
			
			for(int j = 0; j < numEntities[i]; j++){
				for(int k = 0; k < getNumFeatures(); k++){
					entityTypes[i][j][k] = (r.nextGaussian()*0.1);
				}
			}
		}
		
		
		ratings = new double[testData.length];
		for(int i = 0; i < ratings.length; i++){
			ratings[i] = testData[i][RATING];
		}
		
		/*********************CAUTION***************************/
		trainRatings = new double[data[target].length];
		for(int i = 0; i < trainRatings.length; i++){
			trainRatings[i] = data[target][i][RATING];
		}
		/*******************************************************/
		
		
		//Compute Global Average
		globalAverage = 0;
		for(double i : trainRatings){
			globalAverage += i;
		}
		globalAverage /= trainRatings.length;
		
		trainItems = new TIntHashSet();
		for(int row = 0; row < data[0].length; row++){
			trainItems.add((int)data[0][row][ITEM]);
		}
		
				
	}

	public void train(){
		double currRMSE = 100;
		double currFit = 100;
		double prevRMSE = 101;
		double prevFit = Double.MAX_VALUE;
		int iter = 0;
		
//		if(!featuresFixed){
//			do{		
//				iterate(1);
//									
//				if(computeFit){
//					currFit = RMSEEvaluator.rmse(trainRatings, generatePredictions(data[target], target));
//					System.out.print(" -- Fit: " + currFit + " -- Difference: " + (prevFit-currFit));
//					if((prevFit-currFit) < 0.00000001){
//						break;
//					}
//					prevFit = currFit;				
//				}
//			
//				if(computeError && iter % 10 == 0){
//					System.out.print("Iteration: " + iter);
////				if(evaluateRMSE){
////					currRMSE = RMSEEvaluator.rmse(ratings, generatePredictions(testData, target));
////				
////					if(abort && currRMSE > prevRMSE){
////						currRMSE = prevRMSE;
////						break;
////					}
////					prevRMSE = currRMSE;
////					System.out.print(" -- RMSE: " + currRMSE);
////				} else {				
//						currRMSE = evaluateAUC(1);
////				}
//					System.out.println();
//				}
////			System.out.println();	
//			}while(++iter <= maxIter);
//		}
		
		
		iter = 0;
		do{			
			iterate();												
			if(iter % 10 == 0){
				System.out.print("Iteration: " + iter);
				currRMSE = evaluateAUC(1);				
				System.out.println();
			}	
		}while(++iter <= maxIter);
		
		setFit(currFit);
		setRMSE(currRMSE);
		setBestIteration(iter);
		evaluateAUC(1);
	}


	public void writePredictions(RatingsData input) {
				
		double[] predictions = new double[testData.length]; 
		TIntHashSet testUsers = new TIntHashSet();
		
		for(int row = 0; row < testData.length; row++){
			testUsers.add((int)testData[row][USER]);			
		}
		
		try {
			BufferedWriter out = new BufferedWriter(new FileWriter("predictions.txt"));
			for(int u : testUsers.toArray()){
				for(int label = 0; label < entityTypes[relations[0][2]].length; label++){
					double pred = predict(u, label, 0);
					out.write(input.reverseUserIds[u] + "\t" + input.reverseItemIds[label] + "\t" + pred + "\n");
				}
			}
			out.close();
		} catch (IOException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}
					
		
	}
	
	public void writePredictionsMatrix(RatingsData input) {
		
		TIntIntHashMap externalToInternalUserId = new TIntIntHashMap();
		
		for(int u = 0; u < input.reverseUserIds.length; u++){
			externalToInternalUserId.put(input.reverseUserIds[u], u);
		}
		
		TIntIntHashMap externalToInternalItemId = new TIntIntHashMap();
		
		for(int u = 0; u < input.reverseItemIds.length; u++){
			externalToInternalItemId.put(input.reverseItemIds[u], u);
		}
		
		double[] predictions = new double[testData.length]; 
		TIntHashSet testUsers = new TIntHashSet();
		
		for(int row = 0; row < testData.length; row++){
			testUsers.add((int)testData[row][USER]);			
		}
		
		List<Integer> externalTestIds = new ArrayList<Integer>();
		
		for(int u : testUsers.toArray()){
			externalTestIds.add(input.reverseUserIds[u]);
		}
		
		Collections.sort(externalTestIds);
		
		try {
			BufferedWriter out = new BufferedWriter(new FileWriter("bc500_90_0_BPR.predictions"));
			BufferedWriter ids = new BufferedWriter(new FileWriter("bc500_90_0_BPR.ids"));
			for(Integer user : externalTestIds){
				
				int u = externalToInternalUserId.get(user.intValue());				
				ids.write(user+ "\n");
				
				for(int l = 1; l <= entityTypes[relations[0][2]].length; l++){
					int label = externalToInternalItemId.get(l);
					
					double pred = predict(u, label, 0);
					out.write(pred + "");
					if(l == entityTypes[relations[0][2]].length){
						out.write("\n");						
					} else {
						out.write("\t");
					}
					
				}
			}
			out.close();
			ids.close();
		} catch (IOException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}
					
		
	}
	
	
	public double[] generatePredictions(double[][] testData, int rel) {
				
		double[] predictions = new double[testData.length]; 
			
		for(int row = 0; row < testData.length; row++){
			if(!trainItems.contains((int)testData[row][ITEM])){ //new item
				predictions[row] = globalAverage;
			} else {
				predictions[row] = predict((int)testData[row][USER], (int)testData[row][ITEM], rel);
			}
		}
		
		return predictions;
	}
	
	public void printFeatures(String fileName, RatingsData input){
		TIntIntHashMap externalToInternalId = new TIntIntHashMap();
		
		for(int u = 0; u < input.reverseUserIds.length; u++){
			externalToInternalId.put(input.reverseUserIds[u], u);
		}
		
		try {
			BufferedWriter out = new BufferedWriter(new FileWriter(fileName));
			for(int externalUserID = 1; externalUserID <= entityTypes[relations[0][1]].length; externalUserID++){
				int internalUserId = externalToInternalId.get(externalUserID);
				for(int k = 0; k < getNumFeatures(); k++){
					out.write(entityTypes[relations[0][1]][internalUserId][k] + "");
					if(k < getNumFeatures()-1){
						out.write("\t");
					}
				}
				out.write("\n");
			}
			out.close();
		} catch (IOException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}
	}

	
	public void readFeatures(String fileName, RatingsData input){
		TIntIntHashMap externalToInternalId = new TIntIntHashMap();
		
		for(int u = 0; u < input.reverseUserIds.length; u++){
			externalToInternalId.put(input.reverseUserIds[u], u);
		}
		
		try {
			BufferedReader in = new BufferedReader(new FileReader(fileName));
			String line;
			int externalUserID = 1;
            while ((line = in.readLine()) != null) {            
                StringTokenizer st = new StringTokenizer(line, "\t");
                int internalUserId = externalToInternalId.get(externalUserID);
                
				for(int k = 0; k < getNumFeatures(); k++){
					entityTypes[relations[0][1]][internalUserId][k] = Double.parseDouble(st.nextToken());					
				}
				externalUserID++;
            }					
			
			in.close();
			featuresFixed = true;
		} catch (IOException e) {
			e.printStackTrace();
		}
	}
	
	public double evaluateAUC(int numSamples){
		TIntObjectHashMap<TIntHashSet>	train = new TIntObjectHashMap<TIntHashSet>();
		TIntObjectHashMap<TIntHashSet>	test = new TIntObjectHashMap<TIntHashSet>();
		
	
		for(int row = 0; row < data[0].length; row++){
			if(!train.containsKey( (int) data[0][row][USER]) ){
				train.put((int) data[0][row][USER], new TIntHashSet());
			}
			train.get((int) data[0][row][USER]).add((int) data[0][row][ITEM]);
		}
		
	
		for(int row = 0; row < testData.length; row++){
			if(!test.containsKey( (int) testData[row][USER]) ){
				test.put((int) testData[row][USER], new TIntHashSet());
			}
			test.get((int) testData[row][USER]).add((int) testData[row][ITEM]);
		}
		
		if(testItems == null){
			testItems = new TIntHashSet();
			for(int u : test.keys()){
				for(int i : test.get(u).toArray()){
					testItems.add(i);
				}
			}
//			for(int label = 0; label < entityTypes[relations[0][2]].length; label++){
//				testItems.add(label);
//			}
		}
					
		
		
		
		double prec5 = AUCEvaluator.precAtNTang(train, test, this, testItems, entityTypes[relations[0][2]].length);
		double auc =  AUCEvaluator.auc(train, test, this, testItems);
//		double rec5= 0;
//		double prec5= 0;
//		for(int i = 0; i < numSamples; i++){			
//			auc += AUCEvaluator.auc(train, test, this, testItems);			
//			double tmp = AUCEvaluator.recallAtNKoren(train, test, this, entityTypes[1].length, 5);
//			
//			rec5 += tmp;
//			prec5 += tmp / 5.0;
//		}
		System.out.print(" -- AUC: " + auc/numSamples);
		
//		System.out.print(" -- recall@5: " + rec5/numSamples);
//		
//		System.out.print(" -- precision@5: " + prec5/numSamples);
//		
//		System.out.print(" -- f@5: " + 2*rec5*prec5/(prec5+rec5));
		return 0;
		
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

	public double getFit() {
		return fit;
	}

	public void setFit(double fit) {
		this.fit = fit;
	}

	public boolean isAbort() {
		return abort;
	}

	public void setAbort(boolean abort) {
		this.abort = abort;
	}

	public boolean isComputeFit() {
		return computeFit;
	}

	public void setComputeFit(boolean computeFit) {
		this.computeFit = computeFit;
	}
	
	public double getRMSE() {
		return RMSE;
	}

	public void setRMSE(double rMSE) {
		RMSE = rMSE;
	}

	public int getBestIteration() {
		return bestIteration;
	}

	public void setBestIteration(int bestIteration) {
		this.bestIteration = bestIteration;
	}

	public double getAlpha() {
		return alpha;
	}

	public void setAlpha(double alpha) {
		this.alpha = alpha;
	}

	public void setShrinkage(int shrinkage) {
		this.shrinkage = shrinkage;
	}

	public int getShrinkage() {
		return shrinkage;
	}

	public boolean isComputeError() {
		return computeError;
	}

	public void setComputeError(boolean computeError) {
		this.computeError = computeError;
	}

	public double getReg_neg() {
		return reg_neg;
	}

	public void setReg_neg(double regNeg) {
		reg_neg = regNeg;
	}

	public void setTestItems(TIntHashSet testItems) {
		this.testItems = testItems;
	}

	public TIntHashSet getTestItems() {
		return testItems;
	}

}