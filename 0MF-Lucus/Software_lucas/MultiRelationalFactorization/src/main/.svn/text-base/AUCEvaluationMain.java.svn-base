package main;

import java.io.BufferedWriter;
import java.io.FileWriter;
import java.io.IOException;

import matrixfactorization.BiasedMultiRelationalBPR;
import matrixfactorization.BiasedMultiRelationalMF;
import matrixfactorization.ExperimentalMRMF;
import matrixfactorization.MatrixFactorization;
import matrixfactorization.MultiRelationalBPR;
import matrixfactorization.MultiRelationalBprFoaf;
import matrixfactorization.MultiRelationalMF;
import matrixfactorization.MultiRelationalPrecBPR;
import matrixfactorization.PivotationMultiRelationalBPR;
import matrixfactorization.SamplingMultiRelationalMF;
import matrixfactorization.WeightedMRMF;
import matrixfactorization.WeightedMultiRelationalBPR;

import data.RatingsData;
import de.ismll.bootstrap.CommandLineParser;
import de.ismll.bootstrap.Parameter;
import evaluation.AUCEvaluator;
import evaluation.RMSEEvaluator;
import gnu.trove.TIntHashSet;
import gnu.trove.TIntObjectHashMap;


//TODO Write and load the learned model

public class AUCEvaluationMain implements Runnable {

	
	public static final int USER = 0;
	public static final int ITEM = 1;
	public static final int RATING = 2;
	
	@Parameter(cmdline="train", description="The training file")
	private String train = "---";
	
	@Parameter(cmdline="test", description="The test file")
	private String test = "---";
	
	@Parameter(cmdline="dataDir", description="The directory containing the relation files")
	private String dataDir = "---";
	
	
	@Parameter(cmdline="predictionsFile", description="The file containing the predicions matrix")
	private String predictionsFile = "---";
	
	@Parameter(cmdline="idsFile", description="The file containing the userIds")
	private String idsFile = "---";
	

	private MatrixFactorization matrixFactorization = null;
	
	public static void main(String[] args) {
		AUCEvaluationMain e = new AUCEvaluationMain();
		CommandLineParser.parseCommandLine(args, e);
		if(e.getTrain().equals("---")){
			System.err.println("Training file missing.");
			System.err.println("Type \"java -jar MatrixFactorization.jar help\" for more info");
			System.exit(0);
		}
		if(e.getTest().equals("---")){
			System.err.println("Test file missing.");
			System.err.println("Type \"java -jar MatrixFactorization.jar help\" for more info");
			System.exit(0);
		}
		if(e.getDataDir().equals("---")){
			System.err.println("Data directory missing.");
			System.err.println("Type \"java -jar MatrixFactorization.jar help\" for more info");
			System.exit(0);
		}
		
		e.run();
	}
	

	

	public void run() {

		RatingsData input = new RatingsData(getDataDir());
		System.out.println("Reading Data...");
		try {
			input.loadTrainingData(getTrain());
		} catch (IOException e1) {
			e1.printStackTrace();
		}
		
		
		input.loadTestData(getTest());
		
		
		System.out.println("Data read!");
		
						
		TIntObjectHashMap<TIntHashSet>	test = new TIntObjectHashMap<TIntHashSet>();
		

		for(int row = 0; row < input.testData.length; row++){
			if(!test.containsKey( (int) input.testData[row][USER]) ){
				test.put((int) input.testData[row][USER], new TIntHashSet());
			}
			test.get((int) input.testData[row][USER]).add((int) input.testData[row][ITEM]);
		}
		
		TIntHashSet testItems = null;
		
		if(testItems == null){
			testItems = new TIntHashSet();
			for(int u : test.keys()){
				for(int i : test.get(u).toArray()){
					testItems.add(i);
				}
			}

		}
					
				
		
		double auc =  AUCEvaluator.aucExternal(test, input.loadPredictions(getPredictionsFile(), getIdsFile(), input.entities[1].size()), testItems);
	
		System.out.println("AUC: " + auc);
	}

	public void setTrain(String train) {
		this.train = train;
	}

	public String getTrain() {
		return train;
	}

	public void setTest(String test) {
		this.test = test;
	}

	public String getTest() {
		return test;
	}

	
	public String getDataDir() {
		return dataDir;
	}

	public void setDataDir(String dataDir) {
		this.dataDir = dataDir;
	}

	

	public MatrixFactorization getMatrixFactorization() {
		return matrixFactorization;
	}

	public void setMatrixFactorization(MatrixFactorization matrixFactorization) {
		this.matrixFactorization = matrixFactorization;
	}
	
	public double getFit(){
		return getMatrixFactorization().getFit();
	}
	
	public double getRMSE(){
		return getMatrixFactorization().getRMSE();
	}
	
	public int getBestIteration(){
		return getMatrixFactorization().getBestIteration();
	}




	public void setPredictionsFile(String predictionsFile) {
		this.predictionsFile = predictionsFile;
	}




	public String getPredictionsFile() {
		return predictionsFile;
	}




	public void setIdsFile(String idsFile) {
		this.idsFile = idsFile;
	}




	public String getIdsFile() {
		return idsFile;
	}

	
	



}
