package main;

import io.DataInput;
import io.Output;
import io.SparseCSVDataInput;

import java.io.BufferedReader;
import java.io.BufferedWriter;
import java.io.DataInputStream;
import java.io.FileInputStream;
import java.io.FileNotFoundException;
import java.io.FileReader;
import java.io.FileWriter;
import java.io.IOException;
import java.io.PrintStream;
import java.util.StringTokenizer;

import prepocess.WindowBlocker;
import clustering.Clustering;
import clustering.KNNClustering;
import clustering.SingleLinkageHAC;

import util.DistanceCalculator;
import util.SparseArray;
import de.ismll.bootstrap.CommandLineParser;
import de.ismll.bootstrap.Parameter;
import factorizationmodel.EDLogisticKernelModel;
import factorizationmodel.GeneralModel;
import gnu.trove.TIntObjectHashMap;

public class Sandbox implements Runnable {
	@Parameter(cmdline="train", description="File with Train Data")
	private String train = "./data/upcoming-s1-std.txt";
	
	@Parameter(cmdline="test", description="File with Test Data")
	private String test = "./data/upcoming-s2-std.txt";
	
	@Parameter(cmdline="results", description="The file output")
	private String results = "./clustering-s2";
		
	@Parameter(cmdline="reg", description="The regularization parameter. Default: 0.001")
	private double reg = 0.005f;
	
	@Parameter(cmdline="learn", description="The learn rate. Default: 0.03")
	private double learn = 0.00005f;
	
	@Parameter(cmdline="dim", description="The number of latent dimensions. Default: 30")
	private int dim = 50;
	
	@Parameter(cmdline="iter", description="The number of iterations to be performed. Defatult: 25")
	private int iter = 75;
	
	@Parameter(cmdline="window", description="The size of the blocker window. Defatult: 100")
	private int window = 100;
	
	@Parameter(cmdline="k", description="The number of neighbors for the KNN algorithm. Default: 1")
	private int k = 1;
	
	@Parameter(cmdline="threshold", description="The threshold to be used by the cluster. Defatult: 0.1")
	private double threshold = 0.2;
	
	

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
		model.setClassConst(0.5);
		model.setFeatureVectors(data.getObjectFeatures());		
		model.setMappings(data.getObjectToClusters());
		try {
			BufferedReader modelFile = new BufferedReader(new FileReader("./model"));
			model.loadModel(modelFile);
			modelFile.close();
		} catch (IOException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}
			
		DistanceCalculator.init(model);

		System.out.println("Model trained!");
						
		//Read Test Data
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
		
		try {
			samplePairs(testData);
		} catch (IOException e) {
			e.printStackTrace();
			System.exit(0);
		}
		System.exit(0);

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
		//Cluster		
		System.out.println("Clustering Objects ... ");

//		Clustering clustering = new SingleLinkageHAC(threshold, pairsFile);
		Clustering clustering = new KNNClustering(threshold, data, testData, getK(), model);
			
		clustering.cluster();
		
		try {
			Output.writeClustersToFile(clustering.getClusters(), results + "-" + threshold + ".txt");				
		} catch (IOException e) {

			e.printStackTrace();
		}

		
			
		
		System.out.println("Done!");
		
		
	}
	
	
	public static void main(String[] args) {
//		try {
//			readData("./data/upcoming-s2.txt", "./data/upcoming-s2-std.txt");
//		} catch (IOException e) {
//			// TODO Auto-generated catch block
//			e.printStackTrace();
//		}
		Sandbox e = new Sandbox();
		CommandLineParser.parseCommandLine(args, e);
		e.run();
	}
	
	
	public void samplePairs(DataInput in) throws IOException {
	    
		int tp = 0;
		int tn = 0;
		int fp = 0;
		int fn = 0;
		
		double posDist = 0;
		double negDist = 0;
		
		int[] objects = in.getOrderedIds();
		
		for(int i = 0; i < objects.length; i++){ 
			for(int j = i; j < (i + getWindow()) && j < objects.length; j++){
				
				Double distance = DistanceCalculator.computeDistance(objects[i], objects[j]);
				if(distance < getThreshold()){ //positive
					if(in.getObjectClusters().get(objects[i]).equals(in.getObjectClusters().get(objects[j]))){
						posDist += distance;
						tp++;
					} else {
						negDist += distance;
						fp++;
					}
				} else { // negative
					if(in.getObjectClusters().get(objects[i]).equals(in.getObjectClusters().get(objects[j]))){
						posDist += distance;
						fn++;
					} else {
						negDist += distance;
						tn++;
					}
				
				}
				
									
			}
		}
		System.out.println("Pos dist: " + posDist);
		System.out.println("Neg dist: " + negDist);
		System.out.println("Obj: " + (posDist-negDist));
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

	
	public static void readData(String fileName, String outFile) throws IOException {
		
		TIntObjectHashMap<String> objectClusters = new TIntObjectHashMap<String>();
		TIntObjectHashMap<SparseArray> objectFeatures = new TIntObjectHashMap<SparseArray>();
		int[] ids = new int[116666];
		int features = 0;
		
		BufferedReader in = new BufferedReader(new FileReader(fileName));
		
		
		
		String line = "";
		int lineNum = 0;
		while((line = in.readLine()) != null){
			StringTokenizer st = new StringTokenizer(line,",");
			
			int id = Integer.parseInt(st.nextToken());
			ids[lineNum++] = id;
			
			String cluster = st.nextToken();			
			
			objectClusters.put(id, cluster);
			objectFeatures.put(id, new SparseArray());
						
			
			while(st.hasMoreTokens()){
				StringTokenizer st2 = new StringTokenizer(st.nextToken(),":");
				int index = Integer.parseInt(st2.nextToken());
				if(index > features){
					features = index;
				}
				double value = Double.parseDouble(st2.nextToken());
				
				objectFeatures.get(id).set(index-1, value);
			}
			
		}
		
		in.close();
		
		System.out.println("Read!");
		double[] min = new double[features];
		double[] max = new double[features];
		
		for(int i = 0; i < features; i++){
			min[i] = Double.MAX_VALUE;
			max[i] = Double.MIN_VALUE;
		}

				
		for(int i : objectFeatures.keys()){
			for(int j : objectFeatures.get(i).getNonZeroIndexes()){
				double d = objectFeatures.get(i).get(j);
				if(d < min[j]) 
					min[j] = d;				
			    if(d > max[j]) 
					max[j] = d;
			}
		}
		
		
		
		System.out.println("Params computed!");
		BufferedWriter out = new BufferedWriter(new FileWriter(outFile));
		
		for(int i : ids){
			out.write(i + "," + objectClusters.get(i));
			for(int j : objectFeatures.get(i).getNonZeroIndexes()){
				double d = objectFeatures.get(i).get(j);
				double newFeat = 100*((d-min[j])/(max[j]-min[j]));
				if(Double.isNaN(newFeat))
//					System.out.println("min " + min[j] + " max " + max[j] + " feature# " + j + " feature val " + d);
					continue;
				if(Double.isInfinite(newFeat))
					System.out.println("min " + min[j] + " max " + max[j] + " feature# " + j + " feature val " + d);
//					continue;
				out.write("," + (j+1) + ":" + newFeat);				
			}
			out.write("\n");			
		}
		
		out.close();

		System.out.println("Done!");
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

	public int getK() {
		return k;
	}

	public void setK(int k) {
		this.k = k;
	}


}
