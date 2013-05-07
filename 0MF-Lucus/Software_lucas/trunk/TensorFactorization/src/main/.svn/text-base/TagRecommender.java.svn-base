package main;

import gnu.trove.TIntHashSet;
import gnu.trove.TIntObjectHashMap;

import java.io.File;
import java.io.IOException;


import data.TripleDataInput;
import de.ismll.bootstrap.CommandLineParser;
import de.ismll.bootstrap.Parameter;
import tagrecommender.Parafac;

public class TagRecommender implements Runnable {

	
	@Parameter(cmdline="train", description="The training file")
	private String train = "---";
	
	@Parameter(cmdline="test", description="The test file")
	private String test = "---";
	
	@Parameter(cmdline="results", description="The file containing the predictions")
	private String results = "---";
	
	@Parameter(cmdline="regularization", description="The regularization parameter. Default: 0.0")
	private float  regularization = 0.05f;
	
	@Parameter(cmdline="learn", description="The learn rate. Default: 0.01")
	private float  learn = 0.01f;
	
	@Parameter(cmdline="dim", description="The number of features to be used. Default: 45")
	private int    dim = 32;
	
	@Parameter(cmdline="iter", description="The number of iterations to be performed. Defatul: 1000")
	private int    iter = 1000;
	
	
	public static void main(String[] args) {
		TagRecommender e = new TagRecommender();
		CommandLineParser.parseCommandLine(args, e);
		if(e.getTrain().equals("---")){
			System.err.println("Training file missing");
			System.exit(0);
		}
		if(e.getTest().equals("---")){
			System.err.println("Test file missing");
			System.exit(0);
		}
		if(e.getResults().equals("---")){
			System.err.println("Predictions file missing");
			System.exit(0);
		}
		
		e.run();
	}
	
	public void setParameters(Parafac p){
		p.setLearnRate(getLearn());
		p.setMaxIter(getIter());
		p.setNumFeatures(getDim());
		p.setReg(getRegularization());
	}
	public void run() {
		Parafac parafac = new Parafac();
		
		setParameters(parafac);
		
		TripleDataInput input = new TripleDataInput();
		System.out.println("Reading Data...");
		parafac.data = input.loadTrainingData(getTrain());

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
		TIntObjectHashMap<TIntHashSet> data = input.loadTestData(getTest());
		try {
			parafac.writePredictions(getResults(), data);
		} catch (IOException e) {
			e.printStackTrace();
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
}
