package main;


import java.io.BufferedWriter;
import java.io.FileWriter;
import java.io.IOException;

import matrixfactorization.BasicMF;
import matrixfactorization.BiasedMF;
import matrixfactorization.ExperimentalMF;
import matrixfactorization.MatrixFactorization;

import data.RatingsData;
import de.ismll.bootstrap.CommandLineParser;
import de.ismll.bootstrap.Parameter;
import evaluation.RMSEEvaluator;


//TODO Write and load the learned Model

public class FactorizationMain implements Runnable {

	@Parameter(cmdline="train", description="The training file")
	private String train = "---";
	
	@Parameter(cmdline="test", description="The test file")
	private String test = "---";
	
	@Parameter(cmdline="results", description="The file containing the predictions")
	private String results = "---";
	
	@Parameter(cmdline="testResiduals", description="The file containing the residuals on the test set")
	private String testResiduals = "---";
	
	@Parameter(cmdline="trainResiduals", description="The file containing the residuals on the training set")
	private String trainResiduals = "---";
	
	@Parameter(cmdline="regularization", description="The regularization parameter. Default: 0.05")
	private float  regularization = 0.05f;
	
	@Parameter(cmdline="learn", description="The learn rate. Default: 0.004")
	private float  learn = 0.004f;
	
	@Parameter(cmdline="dim", description="The number of features to be used. Default: 128")
	private int    dim = 128;
	
	@Parameter(cmdline="iter", description="The number of iterations to be performed. Defatul: 100")
	private int    iter = 100;
	
	@Parameter(cmdline="method", description="The method to be used:" +
			"\n\t\t    1 - basic matrix factorization" +
			"\n\t\t    2 - biased matrix factorization (default)")
	private int    method = 2;
	
	
	public static void main(String[] args) {
		FactorizationMain e = new FactorizationMain();
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
		
		
		e.run();
	}
	
	public void setParameters(MatrixFactorization p){
		p.setLearnRate(getLearn());
		p.setMaxIter(getIter());
		p.setNumFeatures(getDim());
		p.setReg(getRegularization());
	}
	

	public void run() {
		MatrixFactorization mf = null;
		switch(getMethod()){
		case 1:
			mf = new BasicMF();
			break;
		case 2:
			mf = new BiasedMF();
			break;
		case 3:
			mf = new ExperimentalMF();
			break;
		default:
			System.err.println("Method not found!!");
			System.err.println("Type \"java -jar MatrixFactorization.jar help\" for more info");
			System.exit(0);
		}
		
		
		setParameters(mf);
		
		RatingsData input = new RatingsData();
		System.out.println("Reading Data...");
		mf.data = input.loadTrainingData(getTrain());
		mf.testData = input.loadTestData(getTest());

		System.out.println("Data read!");

		mf.dimensions = new int[2];
		mf.dimensions[0] = input.userIds.size();
		mf.dimensions[1] = input.itemIds.size();
		System.out.println("Num Users: " + input.userIds.size());
		System.out.println("Num Items: " + input.itemIds.size());
		mf.initialize();
		System.out.println("Training");	//TODO Write and load the learned Model
		mf.train();
		
		System.out.println("Trained!!!");		
		double[] predictions = mf.generatePredictions(mf.testData); 
		System.out.println("*** Parameters: learnRate=" + getLearn() + " regularization=" + getRegularization() + 
				" dim=" + getDim() + " iter=" + getIter() +   " Final RMSE: " + RMSEEvaluator.rmse(mf.ratings, predictions));
		
		if(!getResults().equals("---")){
			try {
				BufferedWriter out = new BufferedWriter(new FileWriter(getResults()));
				
				for(int row = 0; row < mf.testData.length; row++){
					out.write(input.reverseUserIds[mf.testData[row][0]] + " " + input.reverseItemIds[mf.testData[row][1]] + " " + predictions[row] + "\n");
				}
								
				out.close();
			} catch (IOException e) {
				e.printStackTrace();
			}
		}
		
		if(!getTestResiduals().equals("---")){
			try {
				BufferedWriter out = new BufferedWriter(new FileWriter(getTestResiduals()));
				
				for(int row = 0; row < mf.testData.length; row++){
					out.write(input.reverseUserIds[mf.testData[row][0]] + " " + input.reverseItemIds[mf.testData[row][1]] + " " + (mf.testData[row][2] - predictions[row]) + "\n");
				}
								
				out.close();
			} catch (IOException e) {
				e.printStackTrace();
			}
		}
		
		if(!getTrainResiduals().equals("---")){
			try {
				BufferedWriter out = new BufferedWriter(new FileWriter(getTestResiduals()));
				
				for(int row = 0; row < mf.data.length; row++){
					out.write(input.reverseUserIds[mf.data[row][0]] + " " + input.reverseItemIds[mf.data[row][1]] + " " + (mf.data[row][2] - mf.predict(mf.data[row][0], mf.data[row][1])) + "\n");
				}
								
				out.close();
			} catch (IOException e) {
				e.printStackTrace();
			}
		}
		
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

	public void setResults(String results) {
		this.results = results;
	}

	public String getResults() {
		return results;
	}

	public void setRegularization(float regularization) {
		this.regularization = regularization;
	}

	public float getRegularization() {
		return regularization;
	}

	public void setLearn(float learn) {
		this.learn = learn;
	}

	public float getLearn() {
		return learn;
	}

	public void setDim(int dim) {
		this.dim = dim;
	}

	public int getDim() {
		return dim;
	}

	public void setIter(int iter) {
		this.iter = iter;
	}

	public int getIter() {
		return iter;
	}

	public int getMethod() {
		return method;
	}

	public void setMethod(int method) {
		this.method = method;
	}

	public String getTestResiduals() {
		return testResiduals;
	}

	public void setTestResiduals(String testResiduals) {
		this.testResiduals = testResiduals;
	}

	public String getTrainResiduals() {
		return trainResiduals;
	}

	public void setTrainResiduals(String trainResiduals) {
		this.trainResiduals = trainResiduals;
	}

}
