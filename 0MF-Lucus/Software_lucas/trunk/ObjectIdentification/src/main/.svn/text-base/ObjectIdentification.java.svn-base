package main;

import java.io.BufferedReader;
import java.io.File;
import java.io.FileNotFoundException;
import java.io.FileReader;
import java.io.IOException;
import java.io.PrintStream;

import clustering.Clustering;
import clustering.KNNClustering;
import clustering.SingleLinkageHAC;

import prepocess.WindowBlocker;
import util.DistanceCalculator;
import util.SparseArray;

import factorizationmodel.EDLogisticKernelModel;
import factorizationmodel.GeneralModel;
import gnu.trove.TIntHashSet;
import gnu.trove.TIntObjectHashMap;

import io.DataInput;
import io.Output;
import io.SparseCSVDataInput;
import de.ismll.bootstrap.CommandLineParser;
import de.ismll.bootstrap.Parameter;

/**
 * Melhores parametros até agora: 
 * dim=50
 * learn=0.00005
 * reg=0.005
 * iter=75
 * thres=0.1
 * @author lucas
 *
 */

public class ObjectIdentification implements Runnable {
	@Parameter(cmdline="train", description="File with Train Data")
	private String train = "./data/upcoming-s1-std.txt";
	
	@Parameter(cmdline="test", description="File with Test Data")
	private String test = "./data/upcoming-s2-std.txt";
	
	@Parameter(cmdline="results", description="The file output")
	private String results = "./clustering-s2";
	
	@Parameter(cmdline="save_model", description="Whether to store the factorization model on a file")
	private boolean save = true;
	
	@Parameter(cmdline="load_model", description="Whether to load the factorization model from a file (no training will be performed)")
	private boolean load = false;
	
	@Parameter(cmdline="model", description="File to which load/save the factorization model")
	private String modelFile = "model";
		
	@Parameter(cmdline="reg", description="The regularization parameter. Default: 0.001")
	private double reg = 0.005f;
	
	@Parameter(cmdline="learn", description="The learn rate. Default: 0.03")
	private double learn = 0.001f;
	
	@Parameter(cmdline="dim", description="The number of latent dimensions. Default: 30")
	private int dim = 25;
	
	@Parameter(cmdline="iter", description="The number of iterations to be performed. Defatult: 25")
	private int iter = 25;
	
	@Parameter(cmdline="window", description="The size of the blocker window. Defatult: 100")
	private int window = 100;
	
	@Parameter(cmdline="threshold", description="The threshold to be used by the cluster. Defatult: 0.1")
	private double threshold = 0.1;
	
	@Parameter(cmdline="k", description="The number of neighbors for the KNN algorithm. Default: 1")
	private int k = 1;
	
	@Parameter(cmdline="clustering", description="The clustering algorithm to be used:\n" +
			" 					* 1 - Single Linkage HAC (Default)\n" +
			" 					* 2 - Nearest Neighbors Search Based Clustering")
	private int clusterAlg = 1;
	
	
	@Override
	public void run() {
		
		////////////////////////////////////////////////////////////////////////////////////
		// Initialization
		////////////////////////////////////////////////////////////////////////////////////
		long init = System.currentTimeMillis();
		
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
		
		
		model.in = data;
		try {
			model.orderedIDs = data.getOrderedIds();
		} catch (IOException e2) {
			e2.printStackTrace();
		}
		////////////////////////////////////////////////////////////////////////////////////
		// Training
		////////////////////////////////////////////////////////////////////////////////////
		
		
		//Learn or load parameters for the mappings to the latent space
		System.out.println("Data read!");
		
		
		if(isLoad()){
			try {
				System.out.println("Loading the Factorization Model ...");
				BufferedReader modelFile = new BufferedReader(new FileReader(getModelFile()));
				model.loadModel(modelFile);
				modelFile.close();
				System.out.println("Model Loaded!");
			} catch (IOException e) {
				e.printStackTrace();
			}
		} else {
			System.out.println("Training the Factorization Model ...");
			model.init();
			model.train();
			System.out.println("Model trained!");
		}
		
		long trainingTime = System.currentTimeMillis() - init;
		
		if(isSave()){
			try {
				PrintStream modelFile = new PrintStream(getModelFile());
				model.writeModel(modelFile);
				modelFile.close();
			} catch (FileNotFoundException e1) {
				e1.printStackTrace();
			}
		}
		
		
		
		
				
		////////////////////////////////////////////////////////////////////////////////////
		// Predictions
		////////////////////////////////////////////////////////////////////////////////////
		init = System.currentTimeMillis();

		//Read Test Data
		DistanceCalculator.init(model);
		System.out.println("Reading  Test Data ...");
		DataInput testData = new SparseCSVDataInput();
		DistanceCalculator.data = testData;
		DistanceCalculator.init(model);
		try {
			
			testData.readData(test);
			
		}catch(FileNotFoundException e){
			System.err.println("Test file \"" + test + "\" not found!!!");
			System.err.println("Exiting...");
			System.exit(0);
		}catch (IOException e) {		
			e.printStackTrace();
		}
			
		//Block testPairs
		System.out.println("Data read!");
		System.out.println("Sampling Pairs ... ");
		
		WindowBlocker blocker = new WindowBlocker();
		blocker.setWindowSize(getWindow());
		String pairsFile = "";
		try {
			pairsFile = blocker.samplePairs(testData);
			blocker.report();
		} catch (IOException e) {
			e.printStackTrace();
			System.exit(0);
		}
		System.out.println("Pairs Sampled!");
		
		long blockingTime = System.currentTimeMillis() - init; 
		
		
		//Cluster
		init = System.currentTimeMillis();
		
		System.out.println("Clustering Objects ... ");
		Clustering clustering = null;
		switch(getClusterAlg()){
		case 1:
			clustering = new SingleLinkageHAC(threshold, pairsFile);
			break;
		case 2:
			clustering = new KNNClustering(threshold, data, testData, getK(), model);
			break;
		default:
			System.err.println("No valid Clustering Algorithm Specified!!!\nExiting...");
			System.exit(0);
		}
			
		clustering.cluster();
		
		try {
			Output.writeClustersToFile(clustering.getClusters(), results + "-" + threshold + ".txt");				
		} catch (IOException e) {
			e.printStackTrace();
		}

		long clusterTime = System.currentTimeMillis() - init;
		
		File f = new File(pairsFile);
		f.delete();
				
		System.out.println("Done!");
		double trainSecs = (double)trainingTime/1000.0;
		System.out.println("Training time: " + trainSecs + " s or " + trainSecs/60.0 + " mins"  );
		double blockerSecs = (double)blockingTime/1000.0;
		System.out.println("Blocking time: " + blockerSecs + " s or " + blockerSecs/60.0 + " mins"  );
		double clusterSecs = (double)clusterTime/1000.0;
		System.out.println("Clustering time: " + clusterSecs + " s or " + clusterSecs/60.0 + " mins"  );
		double total = trainSecs + blockerSecs + clusterSecs;
		System.out.println("Total time: " + total + " s or " + total/60.0 + " mins"  );
		
		
	}
	
	

	/**
	 * @param args
	 */
	public static void main(String[] args) {
		ObjectIdentification e = new ObjectIdentification();
		CommandLineParser.parseCommandLine(args, e);
		e.run();
	}
	
	public static double posAvg = 0;
	public static double negAvg = 0;
	
	public static double estimateThreshold(DataInput data){
		
		TIntObjectHashMap<TIntHashSet> mappings = data.getObjectToClusters();
		TIntObjectHashMap<SparseArray> tagVectors = data.getObjectFeatures();		
			
		int maxsamp = 10000;
		double sumsqr = 0;
		double negsumsqr = 0;
		for (int samp = 0; samp < maxsamp; samp++) {
		
			int count = 0;
			int ob1;
			do{
				ob1 = mappings.keys()[(int) (mappings.size() * Math.random())];
			}while(tagVectors.get(ob1) == null);
			
			int ob2;
			do{
				ob2 = mappings.get(ob1).toArray()[(int) (mappings.get(ob1).size() * Math.random())];
				count++;
				if(count == 10){
					do{
						ob1 = mappings.keys()[(int) (mappings.size() * Math.random())];
					}while(tagVectors.get(ob1) == null);
					count = 0;
				}
			}while(ob1 == ob2 || tagVectors.get(ob2) == null);
			
			double d = DistanceCalculator.computeDistance(ob1, ob2);
			posAvg += d;
			sumsqr += d*d;
			
			int negob1;
			do{
				negob1 = mappings.keys()[(int) (mappings.size() * Math.random())];
			}while(tagVectors.get(negob1) == null);
			count = 0;
			int negob2;
			do{
				negob2 = mappings.keys()[(int) (mappings.size() * Math.random())];
				count++;
				if(count==10){
					do{
						negob1 = mappings.keys()[(int) (mappings.size() * Math.random())];
					}while(tagVectors.get(negob1) == null);
					count = 0;
				}
			}while(mappings.get(negob1).contains(negob2) || tagVectors.get(negob2) == null);
			d = DistanceCalculator.computeDistance(negob1, negob2);
			negAvg += d;
			negsumsqr += d*d;
		}
		double var = (sumsqr - posAvg*(posAvg/maxsamp))/(maxsamp-1);
		double negvar = (negsumsqr - negAvg*(negAvg/maxsamp))/(maxsamp-1);
		posAvg/=(double)maxsamp;
		negAvg/=(double)maxsamp;
		
		System.out.println("Average Distances:");
		System.out.println("Positive Pairs: " + posAvg);
		System.out.println("Variance of Positive Pairs: " + var);
		System.out.println("Negative Pairs: " + negAvg);
		System.out.println("Variance of Negative Pairs: " + negvar);
		System.out.println("Average: " + ((posAvg+negAvg)/2.0));
		System.out.println();
		System.out.println();
		return ((posAvg+negAvg)/2.0);
		
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



	public boolean isSave() {
		return save;
	}



	public void setSave(boolean save) {
		this.save = save;
	}



	public boolean isLoad() {
		return load;
	}



	public void setLoad(boolean load) {
		this.load = load;
	}



	public String getModelFile() {
		return modelFile;
	}



	public void setModelFile(String model) {
		this.modelFile = model;
	}



	public double getThreshold() {
		return threshold;
	}



	public void setThreshold(double threshold) {
		this.threshold = threshold;
	}



	public int getK() {
		return k;
	}



	public void setK(int k) {
		this.k = k;
	}



	public int getClusterAlg() {
		return clusterAlg;
	}



	public void setClusterAlg(int clusterAlg) {
		this.clusterAlg = clusterAlg;
	}

}
