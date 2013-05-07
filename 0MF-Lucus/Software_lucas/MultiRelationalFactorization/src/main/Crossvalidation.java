package main;

import java.io.BufferedWriter;
import java.io.File;
import java.io.FileWriter;
import java.io.IOException;

import de.ismll.bootstrap.CommandLineParser;
import de.ismll.bootstrap.Parameter;

public class Crossvalidation {
	@Parameter(cmdline="folds", description="The number of folds for the cross-validation")
	private int folds = 10;
		
	@Parameter(cmdline="train", description="The training file. The wildcard is \"?\"")
	private String train = "---";
	
	@Parameter(cmdline="test", description="The test file. The wildcard is \"?\"")
	private String test = "---";
	
	@Parameter(cmdline="dataDir", description="The directory containing the relation files")
	private String dataDir = "---";
	
	@Parameter(cmdline="outputDir", description="The directory for the summary file")
	private String outputDir = "---";
		
	@Parameter(cmdline="results", description="The file containing the predictions")
	private String results = "---";
	
	@Parameter(cmdline="regularization", description="The regularization parameter. Default: 0.06")
	private float  regularization = 0.06f;
	
	@Parameter(cmdline="learn", description="The learn rate. Default: 0.00125")
	private float  learn = 0.00125f;
	
	@Parameter(cmdline="dim", description="The number of features to be used. Default: 128")
	private int    dim = 128;
	
	@Parameter(cmdline="iter", description="The number of iterations to be performed. Defatul: 100")
	private int    iter = 100;
	
	@Parameter(cmdline="alpha", description="Alpha. Default: 0.7")
	private float    alpha = 0.001f;
	
	@Parameter(cmdline="abortInc", description="Abort the training if the error increases." +
			" Default: false")
	private boolean    abortInc = false;
	
	@Parameter(cmdline="computeFit", description="Compute the fit after each iteration. Default: false")
	private boolean    computeFit = false;

	@Parameter(cmdline="shrinkage", description="Determines the strength of the mean value when biasing predictions. Default: 0")
	private int shrinkage = 0;
	
	@Parameter(cmdline="method", description="The method to be used:" +
			"\n\t\t    1 - multirelational matrix factorization (default)" +
			"\n\t\t    2 - biased multirelational matrix factorization" + 
			"\n\t\t    3 - experimental multirelational matrix factorization" +
			"\n\t\t    4 - Weighted multirelational matrix factorization")
	private int    method = 5;
	
	public static void main(String[] args) throws IOException {
		Crossvalidation e = new Crossvalidation();
		CommandLineParser.parseCommandLine(args, e);
		if(e.getTrain().equals("---")){
			System.err.println("Training file missing.");
			System.err.println("Type \"java -jar Crossvalidation.jar help\" for more info");
			System.exit(0);
		}
		if(e.getTest().equals("---")){
			System.err.println("Test file missing.");
			System.err.println("Type \"java -jar Crossvalidation.jar help\" for more info");
			System.exit(0);
		}
		if(e.getDataDir().equals("---")){
			System.err.println("Data directory missing.");
			System.err.println("Type \"java -jar Crossvalidation.jar help\" for more info");
			System.exit(0);
		}
		else {
			System.err.println("Data directory is: " + e.getDataDir());
		}
		
		System.out.println("Starting cross-validation.");

		// check for wildcard
		if (e.getTrain().contains("?")) {
		}
		else {
			System.err.println("Wildcard \"?\" not found in Training file.");
			return;
		}
		
		if (e.getTest().contains("?")) {
		}
		else {
			System.err.println("Wildcard \"?\" not found in Training file.");
			return;
		}
		
		// storage for target values
		double[] fits = new double[e.getFolds()];
		double[] rmses = new double[e.getFolds()];
		int[] bestIters = new int[e.getFolds()];
		
		// for each fold
		for (int i = 0; i < e.getFolds(); i++) {

			System.out.println("Starting fold " + (i+1) + " of " + e.getFolds());
			
			// Construct algorithm and set parameters
			FactorizationMain m = new FactorizationMain();
			m.setDataDir(e.getDataDir());
			m.setDim(e.getDim());
			m.setIter(e.getIter());
			m.setLearn(e.getLearn());
			m.setMethod(e.getMethod());
			m.setRegularization(e.getRegularization());
			m.setAbortInc(e.getAbortInc());
			m.setComputeFit(e.getComputeFit());
			m.setAlpha(e.getAlpha());
			
			// adapt file names
			m.setTest(e.getTest().replace("?", Integer.toString(i)));
			m.setTrain(e.getTrain().replace("?", Integer.toString(i)));
			
			// run the algorithm
			m.run();
			
			// receive the result
			fits[i] = m.getFit();
			rmses[i] = m.getRMSE();
			bestIters[i] = m.getBestIteration();
		}
		
		// craft file name
		String filename = (new File(e.getTrain())).getName().replace("?", "_") 
			+ "_method" + e.getMethod()
			+ "_dim" + e.getDim()
			+ "_iter" + e.getIter()
			+ "_learn" + e.getLearn()
			+ "_reg" + e.getRegularization()
			+ "_alpha" + e.getAlpha()
			+ "_abort" + e.getAbortInc()
			+ "_fit" + e.getComputeFit()
			+ ".xml";
		
		// save results to file
		// TODO save results to XML file
		StringBuilder result = new StringBuilder(
			"<result>" +
				"<dataset>" + (new File(e.getTrain())).getName() + "</dataset>"+ "\n" +
				"<method>" + e.getMethod() + "\n"
					+ "<parameter> <dim>" + e.getDim() + "</dim> </parameter>"+ "\n"
					+ "<parameter> <iteration>" + e.getIter() + "</iteration> </parameter>" + "\n"
					+ "<parameter> <learnRate>" + e.getLearn() + "</learnRate> </parameter>" + "\n"
					+ "<parameter> <regularization>" + e.getRegularization() + "</regularization> </parameter>" + "\n"
					+ "<parameter> <alpha>" + e.getAlpha() + "</alpha> </parameter>" + "\n" +
				"</method>" +
				"<abortWhenOverfitting>" + e.getAbortInc() + "</abortWhenOverfitting>" + "\n" +
				"<computeFit>" + e.getComputeFit() + "</computeFit>"+ "\n");
		
		// Fit
		if (e.getComputeFit()) {
			double minFit = Double.MAX_VALUE;
			double maxFit = 0d;
			double avgFit = 0d;
			
			for (int j = 0; j < fits.length; j++) {
				if (minFit > fits[j]) {
					minFit = fits[j];
				}
				if (maxFit < fits[j]) {
					maxFit = fits[j];
				}
				avgFit += fits[j];
			}
			avgFit /= fits.length;
			
			result.append("<fit min=\""+ minFit + "\" avg=\"" + avgFit + "\" max=\"" + maxFit + "\">" + avgFit + "</fit>" + "\n");
		} else {
			// nothing to do
		}
		
		// BestIter
		{
			double minBIter = Double.MAX_VALUE;
			double maxBIter = 0d;
			double avgBIter = 0d;
			
			for (int j = 0; j < bestIters.length; j++) {
				if (minBIter > bestIters[j]) {
					minBIter = bestIters[j];
				}
				if (maxBIter < bestIters[j]) {
					maxBIter = bestIters[j];
				}
				avgBIter += bestIters[j];
			}
			avgBIter /= bestIters.length;
			
			result.append("<bestIteration min=\""+ minBIter + "\" avg=\"" + avgBIter + "\" max=\"" + maxBIter + "\">" + avgBIter + "</bestIteration>" + "\n");
		}
		
		// RMSE
		{
			double minRmse = Double.MAX_VALUE;
			double maxRmse = 0d;
			double avgRmse = 0d;
			
			for (int j = 0; j < rmses.length; j++) {
				if (minRmse > rmses[j]) {
					minRmse = rmses[j];
				}
				if (maxRmse < rmses[j]) {
					maxRmse = rmses[j];
				}
				avgRmse += rmses[j];
			}
			avgRmse /= rmses.length;
			
			result.append("<rmse min=\""+ minRmse + "\" avg=\"" + avgRmse+ "\" max=\"" + maxRmse+ "\">" + avgRmse+ "</rmse>" + "\n");		
		}
		
		result.append("</result>" + "\n");
		
		System.out.println(result.toString());
		
		BufferedWriter writer;
		if(e.getOutputDir().equals("---")){
			writer = new BufferedWriter(new FileWriter(filename));
		}
		else {
			writer = new BufferedWriter(new FileWriter(e.getOutputDir() + File.pathSeparator + filename));
		}
				
		writer.write(result.toString());
		writer.close();
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


	public void setFolds(int folds) {
		this.folds = folds;
	}


	public int getFolds() {
		return folds;
	}


	public void setOutputDir(String outputDir) {
		this.outputDir = outputDir;
	}


	public String getOutputDir() {
		return outputDir;
	}


	public void setAlpha(float alpha) {
		this.alpha = alpha;
	}


	public float getAlpha() {
		return alpha;
	}

	
	public void setShrinkage(int shrinkage) {
		this.shrinkage = shrinkage;
	}


	public int getShrinkage() {
		return shrinkage;
	}
}
