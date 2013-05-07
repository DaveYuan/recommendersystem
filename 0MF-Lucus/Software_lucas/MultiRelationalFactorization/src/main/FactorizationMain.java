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
import evaluation.RMSEEvaluator;


//TODO Write and load the learned model

public class FactorizationMain implements Runnable {

	@Parameter(cmdline="train", description="The training file")
	private String train = "---";
	
	@Parameter(cmdline="test", description="The test file")
	private String test = "---";
	
	@Parameter(cmdline="testItems", description="The file containing Items to be evaluated")
	private String testItems = "---";
	
	@Parameter(cmdline="dataDir", description="The directory containing the relation files")
	private String dataDir = "---";
	
	@Parameter(cmdline="results", description="The file containing the predictions")
	private String results = "---";
	
	@Parameter(cmdline="testResiduals", description="The file containing the residuals on the test set")
	private String testResiduals = "---";
	
	@Parameter(cmdline="trainResiduals", description="The file containing the residuals on the training set")
	private String trainResiduals = "---";
	
	@Parameter(cmdline="readFeatures", description="The file containing the user features")
	private String readFeatures = "---";
	
	@Parameter(cmdline="regularization", description="The regularization parameter. Default: 0.06")
	private float  regularization = 0.01f;
	
	@Parameter(cmdline="reg_neg", description="The regularization parameter for the negative samples (BPR). Default: 0.00025")
	private float  reg_neg = 0.00001f;
	
	@Parameter(cmdline="learn", description="The learn rate. Default: 0.00125")
	private float  learn = 0.01f;
	
	@Parameter(cmdline="dim", description="The number of features to be used. Default: 128")
	private int    dim = 200;
	
	@Parameter(cmdline="iter", description="The number of iterations to be performed. Defatul: 100")
	private int    iter = 300;
	
	@Parameter(cmdline="alpha", description="Alpha. Default: 0.7")
	private float    alpha = 1f;
	
	@Parameter(cmdline="abortInc", description="Abort the training if the error increases." +
			" Default: false")
	private boolean    abortInc = false;
	
	@Parameter(cmdline="computeFit", description="Compute the fit after each iteration. Default: false")
	private boolean    computeFit = false;
	 
	@Parameter(cmdline="computeError", description="Compute the error on the test set after each iteration. Default: true")
	private boolean    computeError = true;
	

	@Parameter(cmdline="shrinkage", description="Determines the strength of the mean value when biasing predictions. Default: 0")
	private int shrinkage = 0;
	
	@Parameter(cmdline="method", description="The method to be used:" +
			"\n\t\t    1 - multirelational matrix factorization (default)" +
			"\n\t\t    2 - biased multirelational matrix factorization" + 
			"\n\t\t    3 - experimental multirelational matrix factorization" +
			"\n\t\t    4 - Weighted multirelational matrix factorization" +
			"n\t\t     5 - Multi Relational matrix factorization with sampling from Zeros")
	private int    method = 6;
	
	private MatrixFactorization matrixFactorization = null;
	
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
		if(e.getDataDir().equals("---")){
			System.err.println("Data directory missing.");
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
		p.setReg_neg(getReg_neg());
		p.setAbort(getAbortInc());
		p.setComputeFit(getComputeFit());
		p.setComputeError(isComputeError());
		p.setAlpha(getAlpha());		
	}
	

	public void run() {

		System.out.println("Train: " + getTrain());
		System.out.println("Test: " + getTest());

		System.out.println("*** Parameters: learnRate=" + getLearn() + " regularization=" + getRegularization() + 
				" dim=" + getDim() + " iter=" + getIter() + " alpha=" + getAlpha());
		
		switch(getMethod()){
		case 1:
			matrixFactorization = new MultiRelationalMF();
			break;
		case 2:
			matrixFactorization = new BiasedMultiRelationalMF();
			break;
		case 3:
			matrixFactorization = new ExperimentalMRMF();
			break;
		case 4:
			matrixFactorization = new WeightedMRMF();
			break;
		case 5:
			matrixFactorization = new SamplingMultiRelationalMF();
			break;	
		case 6:
			matrixFactorization = new MultiRelationalBPR();
			break;
		case 7:
			matrixFactorization = new PivotationMultiRelationalBPR();
			break;	
		case 8:
//			matrixFactorization = new MultiRelationalBprFoaf();
			matrixFactorization = new WeightedMultiRelationalBPR();
			break;
		default:
			System.err.println("Method not found!!");
			System.err.println("Type \"java -jar MatrixFactorization.jar help\" for more info");
			System.exit(0);
		}
		
		
		setParameters(matrixFactorization);
		
		RatingsData input = new RatingsData(getDataDir());
		System.out.println("Reading Data...");
		try {
			input.loadTrainingData(getTrain());
		} catch (IOException e1) {
			e1.printStackTrace();
		}
					
		matrixFactorization.data =  input.trainingData;
		matrixFactorization.testData = input.loadTestData(getTest());

		
		if(!getTestItems().equals("---")){
			input.loadTestItems(getTestItems());
			matrixFactorization.setTestItems(input.testItems);
		}
		System.out.println("Data read!");
		
		matrixFactorization.numEntityTypes = input.entityTypes.length;
		matrixFactorization.numEntities = new int[matrixFactorization.numEntityTypes];
		for(int i = 0; i < matrixFactorization.numEntities.length; i++){
//			matrixFactorization.numEntities[i] = input.entities[i].size();
//			System.out.println("Num of Entities of Type " + i + ": " + input.entities[i].size());
			matrixFactorization.numEntities[i] = input.entityIds[i].length;
			System.out.println("Num of Entities of Type " + i + ": " + input.entityIds[i].length);
		}
		matrixFactorization.relations = input.relations;
		
		System.out.println("Relations:");
		for(int i = 0; i < matrixFactorization.relations.length; i++){			 
			System.out.println("Num of instances of Relation " + i + ": " + matrixFactorization.data[i].length);
		}
		matrixFactorization.initialize();
		System.out.println("Training");	//TODO Write and load the learned Model
		
		if(!getReadFeatures().equals("---")){
			matrixFactorization.readFeatures(getReadFeatures(), input);
		}
		matrixFactorization.train();
		
//		matrixFactorization.writePredictionsMatrix(input);
//		matrixFactorization.printFeatures("youtube-d" + getDim() + "-l" + getLearn(), input);
		
//		System.out.println("Trained!!!");		
		double[] predictions = matrixFactorization.generatePredictions(matrixFactorization.testData,0); 
//		System.out.println("*** Parameters: learnRate=" + getLearn() + " regularization=" + getRegularization() + 
//				" dim=" + getDim() + " iter=" + getIter() + " last iteration=" + getBestIteration() + " alpha=" + getAlpha() + " Final RMSE: " + RMSEEvaluator.rmse(matrixFactorization.ratings, predictions));
		
		if(!getResults().equals("---")){
			try {
				BufferedWriter out = new BufferedWriter(new FileWriter(getResults()));
				
				for(int row = 0; row < matrixFactorization.testData.length; row++){
					out.write(input.reverseUserIds[(int)matrixFactorization.testData[row][0]] + " " + input.reverseItemIds[(int)matrixFactorization.testData[row][1]] + " " + predictions[row] + "\n");
				}
								
				out.close();
			} catch (IOException e) {
				e.printStackTrace();
			}
		}
		
		if(!getTestResiduals().equals("---")){
			try {
				BufferedWriter out = new BufferedWriter(new FileWriter(getTestResiduals()));
				
				for(int row = 0; row < matrixFactorization.testData.length; row++){
					out.write(input.reverseUserIds[(int)matrixFactorization.testData[row][0]] + " " + input.reverseItemIds[(int)matrixFactorization.testData[row][1]] + " " + (matrixFactorization.testData[row][2] - predictions[row]) + "\n");
				}
								
				out.close();
			} catch (IOException e) {
				e.printStackTrace();
			}
		}
		
		if(!getTrainResiduals().equals("---")){
			try {
				BufferedWriter out = new BufferedWriter(new FileWriter(getTrainResiduals()));
				
				for(int row = 0; row < matrixFactorization.data[0].length; row++){
					out.write(input.reverseUserIds[(int)matrixFactorization.data[0][row][0]] + " " + input.reverseItemIds[(int)matrixFactorization.data[0][row][1]] + " " + (matrixFactorization.data[0][row][2] - matrixFactorization.predict((int)matrixFactorization.data[0][row][0], (int)matrixFactorization.data[0][row][1], 0)) + "\n");
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

	public String getDataDir() {
		return dataDir;
	}

	public void setDataDir(String dataDir) {
		this.dataDir = dataDir;
	}

	public boolean getAbortInc() {
		return abortInc;
	}

	public void setAbortInc(boolean abortInc) {
		this.abortInc = abortInc;
	}

	public boolean getComputeFit() {
		return computeFit;
	}

	public void setComputeFit(boolean computeFit) {
		this.computeFit = computeFit;
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

	public float getAlpha() {
		return alpha;
	}

	public void setAlpha(float alpha) {
		this.alpha = alpha;
	}

	public String getTestResiduals() {
		return testResiduals;
//		return "residuals-" + getTest().substring(getTest().lastIndexOf("/")+1);
	}

	public void setTestResiduals(String testResiduals) {
		this.testResiduals = testResiduals;
	}

	public String getTrainResiduals() {
		return trainResiduals;
//		return "residuals-" + getTrain().substring(getTrain().lastIndexOf("/")+1);
	}

	public void setTrainResiduals(String trainResiduals) {
		this.trainResiduals = trainResiduals;
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

	public void setReg_neg(float reg_neg) {
		this.reg_neg = reg_neg;
	}

	public float getReg_neg() {
		return reg_neg;
	}

	public void setTestItems(String testItems) {
		this.testItems = testItems;
	}

	public String getTestItems() {
		return testItems;
	}

	public void setReadFeatures(String readFeatures) {
		this.readFeatures = readFeatures;
	}

	public String getReadFeatures() {
		return readFeatures;
	}

}
