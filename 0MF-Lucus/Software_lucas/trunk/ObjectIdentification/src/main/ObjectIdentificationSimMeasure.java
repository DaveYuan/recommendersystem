package main;

import java.io.FileNotFoundException;
import java.io.IOException;

import prepocess.WindowBlocker;

import util.DistanceCalculator;

import factorizationmodel.EDLogisticKernelModel;
import factorizationmodel.GeneralModel;

import io.DataInput;
import io.SparseCSVDataInput;
import de.ismll.bootstrap.CommandLineParser;
import de.ismll.bootstrap.Parameter;


public class ObjectIdentificationSimMeasure implements Runnable {
	@Parameter(cmdline="train", description="File with Train Data")
	private String train = "./data/upcoming-s1-std.txt";
	
	@Parameter(cmdline="test", description="File with Test Data")
	private String test = "./data/upcoming-s2-std.txt";
	
	@Parameter(cmdline="results", description="The file output")
	private String results = "./clustering-s2";
		
	@Parameter(cmdline="reg", description="The regularization parameter. Default: 0.001")
	private double reg = 0.005f;
	
	@Parameter(cmdline="learn", description="The learn rate. Default: 0.03")
	private double learn = 0.00001f;
	
	@Parameter(cmdline="dim", description="The number of latent dimensions. Default: 30")
	private int dim = 15;
	
	@Parameter(cmdline="iter", description="The number of iterations to be performed. Defatult: 25")
	private int iter = 10;
	
	@Parameter(cmdline="window", description="The size of the blocker window. Defatult: 100")
	private int window = 100;
	
	@Parameter(cmdline="threshold", description="The threshold to be used by the cluster. Defatult: 0.1")
	private double threshold = 0.1;
	
	@Override
	public void run() {
		
		
		//Read Training Data
		DataInput data = new SparseCSVDataInput();
		try {
			
			data.readData(train);
			DistanceCalculator.data = data;
		}catch(FileNotFoundException e){
			System.err.println("Training file \"" + train + "\" not found!!!");
			System.err.println("Exiting...");
			System.exit(0);
		}catch (IOException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}
		
		GeneralModel model = new EDLogisticKernelModel();

		//Initialize Parameters
		model.setK(getDim());
		model.setF(data.getFeatures());
		model.setLearnRate(getLearn());
		model.setMaxIter(getIter());
		model.setReg(getReg());
		model.setClassConst(1);
		model.setFeatureVectors(data.getObjectFeatures());		
		model.setMappings(data.getObjectToClusters());
		
		//Learn mappings to the latent space
		System.out.println("Data read!");
		System.out.println("Training the Factorization Model ...");
		model.init();
		model.train();
		DistanceCalculator.init(model);
		System.out.println("Model trained!");
		
		
		
		//Read Test Data
		System.out.println("Reading  Test Data ...");
		data = new SparseCSVDataInput();
		DistanceCalculator.data = data;
		DistanceCalculator.init(model);
		try {
			
			data.readData(test);
			
		}catch(FileNotFoundException e){
			System.err.println("Test file \"" + test + "\" not found!!!");
			System.err.println("Exiting...");
			System.exit(0);
		}catch (IOException e) {		
			e.printStackTrace();
		}
		

		//Block testPairs
		System.out.println("Data read!");
		WindowBlocker blocker = new WindowBlocker();
		blocker.setWindowSize(getWindow());
		
		try {
			blocker.samplePairs(data);
			blocker.report();
		} catch (IOException e) {
			e.printStackTrace();
			System.exit(0);
		}
		
		
		System.out.println("Evaluating... ");
		
	
		try {
			samplePairs(data);
		} catch (IOException e) {
			e.printStackTrace();
			System.exit(0);
		}
					
	}
	
	
	public void samplePairs(DataInput in) throws IOException {
		    
			int tp = 0;
			int tn = 0;
			int fp = 0;
			int fn = 0;
			
			int[] objects = in.getOrderedIds();
			
			for(int i = 0; i < objects.length; i++){ 
				for(int j = i; j < (i + getWindow()) && j < objects.length; j++){
					
					Double distance = DistanceCalculator.computeDistance(objects[i], objects[j]);
					if(distance < getThreshold()){ //positive
						if(in.getObjectClusters().get(objects[i]).equals(in.getObjectClusters().get(objects[j]))){
							tp++;
						} else {
							fp++;
						}
					} else { // negative
						if(in.getObjectClusters().get(objects[i]).equals(in.getObjectClusters().get(objects[j]))){
							fn++;
						} else {
							tn++;
						}
					
					}
					
										
				}
			}
			
			System.out.println("tp: " + tp);
			System.out.println("fp: " + fp);
			System.out.println("tn: " + tn);
			System.out.println("fn: " + fn);
			double prec = (double)tp/ (double)(tp+fp);
		    double recall = (double) tp / (double) (tp+fn);
		    double fmeas = 2 * (prec * recall) / (prec+recall);
		    double accuracy = (double) (tp + tn) / (double) (tp + tn + fp + fn);		    
		    System.out.println("Precision: " + prec);
			System.out.println("Recall: " + recall);
			System.out.println("F-Measure: " + fmeas);
			System.out.println("Accuracy: " + accuracy);
			
			System.out.println("Precision  Recall  F-Measure  Accuracy");
		    System.out.println(prec + " " + recall  + " " + fmeas  + " " + accuracy);
			
	}

	/**
	 * @param args
	 */
	public static void main(String[] args) {
		ObjectIdentificationSimMeasure e = new ObjectIdentificationSimMeasure();
		CommandLineParser.parseCommandLine(args, e);
		e.run();
	}


	public String getTrain() {
		return train;
	}

	public void setTrain(String train) {
		this.train = train;
	}

	public String getTest() {
		return test;
	}

	public void setTest(String test) {
		this.test = test;
	}

	public String getResults() {
		return results;
	}

	public void setResults(String results) {
		this.results = results;
	}

	public double getReg() {
		return reg;
	}

	public void setReg(double reg) {
		this.reg = reg;
	}

	public double getLearn() {
		return learn;
	}

	public void setLearn(double learn) {
		this.learn = learn;
	}

	public int getDim() {
		return dim;
	}

	public void setDim(int dim) {
		this.dim = dim;
	}

	public int getIter() {
		return iter;
	}

	public void setIter(int iter) {
		this.iter = iter;
	}

	public void setWindow(int window) {
		this.window = window;
	}

	public int getWindow() {
		return window;
	}



	public double getThreshold() {
		return threshold;
	}



	public void setThreshold(double threshold) {
		this.threshold = threshold;
	}

}
