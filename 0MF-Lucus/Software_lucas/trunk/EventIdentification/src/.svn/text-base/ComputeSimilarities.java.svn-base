import gnu.trove.TIntHashSet;
import gnu.trove.TIntIntHashMap;
import gnu.trove.TIntObjectHashMap;

import java.io.BufferedReader;
import java.io.BufferedWriter;
import java.io.FileReader;
import java.io.FileWriter;
import java.io.IOException;
import java.util.HashSet;
import java.util.StringTokenizer;

import utils.SparseArray;

import datagenerator.Blocker;
import datagenerator.DataInput;
import datagenerator.Deduplicator;
import datagenerator.PairsGenerator;
import datagenerator.SimilarityCalculator;
import de.ismll.bootstrap.CommandLineParser;
import de.ismll.bootstrap.Parameter;

public class ComputeSimilarities implements Runnable {

	@Parameter(cmdline = "directory", description = "The directory containing the data")
	private String directory = "/home/lucas/workspace/EventIdentification/data/upcoming/";
	// private String directory =
	// "/home/lucas/workspace/EventIdentification/data/toy/";
	// private String file =
	// "/home/lucas/workspace/EventIdentification/data/toy.txt";

	@Parameter(cmdline = "infile", description = "The file containing the output")
//	private String infile = "/home/lucas/workspace/EventIdentification/data/upcoming/upcoming-s2-testPairs.txt";
	private String infile = "/home/lucas/workspace/EventIdentification/data/upcoming/upcoming-datetaken.txt";
//	private String infile = "/home/lucas/upcoming-trainPairs.txt";

	@Parameter(cmdline = "outfile", description = "The file containing the output")
//	private String outfile = "./data/upcoming/upcoming-allsim-s2-test.txt";
	private String outfile = "/home/lucas/upcoming-fac";

	@Parameter(cmdline = "dataset", description = "The file containing the output")
	private String dataset = "upcoming";

	@Parameter(cmdline = "task", description = "the task to be performed:\n"
			+ "1 - Compute similarities"
			+ "\n2 - Sample Pairs"
			+ "\n3 - Block pairs"
			+ "\n4 - Deduplicate file")
	private int task = 2;
	
	@Parameter(cmdline="pairs", description="Maximum number of pairs per object")
	private int pairs = 100;
	
	@Parameter(cmdline="type", description="Positive (true) or negative (false) pairs")
	private boolean type = false;
	
	@Parameter(cmdline="test", description="Whether to sample test pairs")
	private boolean test = true;
	
	@Parameter(cmdline="split", description="Which split to sample")
	private int split = 1;
	
	@Parameter(cmdline="windowSize", description="Size of the window for the window based sampling")
	private int windowSize = 100;
	
	@Parameter(cmdline="sampleFrac", description="Fraction of pairs to sample for the training data (1/sampleFrac)")
	private int sampleFrac = 100;
	

	@Parameter(cmdline="samples", description="Maximum number of samples")
	private int samples = 1500;

	/**
	 * @param args
	 */
	public void run() {
		switch (task) {
		case 1:
			computeSimilarities();
			break;
			
		case 2:
			generatePairs();
			break;

		case 3:
			block();
			break;

		case 4:
			Deduplicator dp = new Deduplicator();
			dp.loadDataTime("./data/upcoming/upcoming-s1s2s3-pairs.txt",
					"./data/upcoming/upcoming-dedup.txt");
			break;

		case 5:
			samplePositivePairs();
			break;
			
		default:
			test();
			System.err.println("Task not found!");
			break;
		}

	}

	public void samplePositivePairs(){
		DataInput input = new DataInput();
		input.loadDataTime(directory + dataset + "-datetaken.txt");
		SimilarityCalculator sc = new SimilarityCalculator(input);

//		HashSet<String> positive = new HashSet<String>();
		TIntObjectHashMap<TIntHashSet> positive = new TIntObjectHashMap<TIntHashSet>();
		try {
			BufferedReader in = new BufferedReader(new FileReader(directory + dataset + "-s1-pairs-blocking.txt"));
			BufferedWriter out = new BufferedWriter(new FileWriter(directory + dataset + "-s1-pairs-blocking-final.txt"));
			
			String line = null;
			in.readLine();
			int neg = 0;									
			int pos = 0;
			while ((line = in.readLine()) != null) {
				StringTokenizer st = new StringTokenizer(line, " ");
				int a = Integer.parseInt(st.nextToken());
				int b = Integer.parseInt(st.nextToken());

				if(sc.sameEvent(a, b, dataset)){
					if(!positive.containsKey(a)){
						positive.put(a, new TIntHashSet());
					}
					positive.get(a).add(b);
//					if(Math.random() <= 1.0/(double)neg){
//						out.write(line + "\n");
//						pos++;
//					}
				}else{
					neg++;
					out.write(line + "\n");
				}
				
			}
			System.out.println("Num negs: " + neg);
			
//			for(int s = 0; s < neg; s++){
//				int index = (int) Math.random()*posi14375809tive.size();
//				out.write(positive.toArray()[index] + "\n");
//				positive.remove(positive.toArray()[index]);
//			}
			for(int s = 0; s < neg; s++){
				int index = (int) Math.random()*positive.size();
				int pic1 = positive.keys()[index];
				index = (int) Math.random()*positive.get(pic1).size();
				int pic2 = positive.get(pic1).toArray()[index];
				out.write(pic1 + " " + pic2 + "\n");
				pos++;
				positive.get(pic1).remove(pic2);
				if(positive.get(pic1).isEmpty()){
					positive.remove(pic1);
				}
			}
			System.out.println("Num pos: " + pos);
			System.out.println();

			in.close();
			out.close();
		} catch (IOException e) {
			e.printStackTrace();
		}
	}
	
	public void computeSimilarities() {
		DataInput input = new DataInput();
		// input.loadDataTime(directory + "toy-datetaken.txt");
		input.loadDataTime(directory + dataset + "-datetaken.txt");
		input.loadDataLocation(directory + dataset + "-alldata.txt");

		input.loadTextualData(directory + dataset + "-tag.txt");
		TIntObjectHashMap<SparseArray> tagVectors = input.getVectors();

		input.loadTextualData(directory + dataset + "-title.txt");
		TIntObjectHashMap<SparseArray> titleVectors = input.getVectors();

		input.loadTextualData(directory + dataset + "-desc.txt");
		TIntObjectHashMap<SparseArray> descVectors = input.getVectors();

		input.loadTextualData(directory + dataset + "-notes.txt");
		TIntObjectHashMap<SparseArray> notesVectors = input.getVectors();

		input.loadTextualData(directory + dataset + "-allText.txt");
		TIntObjectHashMap<SparseArray> allTextVectors = input.getVectors();

		input.loadTextualData(directory + dataset + "-geo.txt");
		TIntObjectHashMap<SparseArray> geoVectors = input.getVectors();

		SimilarityCalculator sc = new SimilarityCalculator(input);
		System.out.println("Data Read! Starting to compute the similarities!");
		try {
//			 BufferedReader in = new BufferedReader(new FileReader(directory +
//			 dataset + "-s1-pairs-blocking.txt"));
			BufferedReader in = new BufferedReader(new FileReader(infile));
//			 BufferedReader in = new BufferedReader(new FileReader(directory +
//			 "toy.txt"));
			BufferedWriter out = new BufferedWriter(new FileWriter(outfile));
//			out.write("picture_id_a,picture_id_b,label,sim_time,sim_location,sim_tag,sim_title,sim_desc,sim_owner,sim_notes,sim_geo,sim_allText\n");
			String line = null;
			int lineNum = 0;
			while ((line = in.readLine()) != null) {
				StringTokenizer st = new StringTokenizer(line, " ");
				int a = Integer.parseInt(st.nextToken());
				int b = Integer.parseInt(st.nextToken());
								
//				String label = sc.sameEvent(a, b, dataset) ? "1" : "0";
//				out.write(line + " " + label + "\n");
				String label = sc.sameEvent(a, b, dataset) ? "+1" : "-1";
				double timeSim = sc.timeSimilarity(a, b);
				double locSim = sc.locationSimilarity(a, b);
				double tagSim = sc.textualSimilarity(a, b, tagVectors);
				double titleSim = sc.textualSimilarity(a, b, titleVectors);
				double descSim = sc.textualSimilarity(a, b, descVectors);
				int ownerSim = sc.ownerSimilarity(a, b);
				double notesSim = sc.textualSimilarity(a, b, notesVectors);
				double geoSim = sc.textualSimilarity(a, b, geoVectors);
				double allSim = sc.textualSimilarity(a, b, allTextVectors);

//				out.write(label + " 1:" + timeSim + " 2:" + locSim + " 3:"
//						+ tagSim + "\n");
				
				out.write(label + " 1:" + timeSim + " 2:" + locSim + " 3:"
						+ tagSim + " 4:" + titleSim + " 5:" + descSim + " 6:"
						+ ownerSim + " 7:" + notesSim + " 8:" + geoSim + " 9:"
						+ allSim + "\n");
				
//				out.write(a +","+b + "," + label + "," + timeSim + "," + locSim + ","
//						+ tagSim + "," + titleSim + "," + descSim + ","
//						+ ownerSim + "," + notesSim + "," + geoSim + ","
//						+ allSim + "\n");

				lineNum++;
				if (lineNum % 1000000 == 0) {
					System.out.print(".");
				}
			}
			System.out.println();

			in.close();
			out.close();
		} catch (IOException e) {
			e.printStackTrace();
		}
	}

	public void block() {
		DataInput input = new DataInput();
		input.loadDataTime(directory + dataset + "-datetaken.txt");

		SimilarityCalculator sc = new SimilarityCalculator(input);
		Blocker b = new Blocker();
		b.setSimilarities(sc);
		b.block(infile, outfile);
	}

	
	public void test() {
		DataInput input = new DataInput();
		input.loadDataTime(directory + dataset + "-datetaken.txt");
		SimilarityCalculator sc = new SimilarityCalculator(input);
		
		try {
			BufferedReader in = new BufferedReader(new FileReader("./data/upcoming/foo"));

			int s1 = 0;
			int s2 = 0;
			int s3 = 0;
			int total = 0;
			String line = null;
			int lineNum = 0;
			while ((line = in.readLine()) != null) {
				StringTokenizer st = new StringTokenizer(line, " ");
				int a = Integer.parseInt(st.nextToken());
				int id = input.getData_time()[sc.getPicIds2position().get(a)][input.ID];
				if(id < 116666){
					s1++;
				} else if(id < 233332){
					s2++;
				}else{
					s3++;
				}

				total++;
			}
			System.out.println("S1: " + s1);
			System.out.println("S2: " + s2);
			System.out.println("S3: " + s3);
			System.out.println("total: " + total);
			System.out.println("total2: " + (s1+s2+s3));
			System.out.println();

			in.close();
		} catch (IOException e) {
			e.printStackTrace();
		}
	}
	
	public void generatePairs() {

		PairsGenerator p = new PairsGenerator();
		p.setNumPairs(pairs);
		p.setOut(outfile);
		p.setMaxSamples(samples);
		
		int s1 = 0;
		int s2 = 0;
		
		if(dataset.equals("lastfm")){
			s1 = 497628;
			s2 = 995255;
		} else if(dataset.equals("upcoming")){
			s1 = 116665;
			s2 = 233331;
		} 
		
		p.generatePairs(infile, dataset,s1,s2, type, test, split, windowSize, sampleFrac);
//		p.samplePairsTimeDD(0, s1, 500);
//		p.sampleTestPairsTimeDD(s1+1, s2, 100);
		
	}

	public static void main(String[] args) {
		ComputeSimilarities e = new ComputeSimilarities();
		CommandLineParser.parseCommandLine(args, e);
		e.run();
	}

	public String getDirectory() {
		return directory;
	}

	public void setDirectory(String directory) {
		this.directory = directory;
	}

	public String getOutfile() {
		return outfile;
	}

	public void setOutfile(String outfile) {
		this.outfile = outfile;
	}

	public String getDataset() {
		return dataset;
	}

	public void setDataset(String dataset) {
		this.dataset = dataset;
	}

	public int getTask() {
		return task;
	}

	public void setTask(int task) {
		this.task = task;
	}

	public String getInfile() {
		return infile;
	}

	public void setInfile(String infile) {
		this.infile = infile;
	}

	public int getPairs() {
		return pairs;
	}

	public void setPairs(int pairs) {
		this.pairs = pairs;
	}

	public boolean isType() {
		return type;
	}

	public void setType(boolean type) {
		this.type = type;
	}

	public int getSamples() {
		return samples;
	}

	public void setSamples(int samples) {
		this.samples = samples;
	}
	
	public int getSplit() {
		return split;
	}

	public void setSplit(int split) {
		this.split = split;
	}

	public boolean isTest() {
		return test;
	}

	public void setTest(boolean test) {
		this.test = test;
	}

	public int getWindowSize() {
		return windowSize;
	}

	public void setWindowSize(int windowSize) {
		this.windowSize = windowSize;
	}

	public int getSampleFrac() {
		return sampleFrac;
	}

	public void setSampleFrac(int sampleFrac) {
		this.sampleFrac = sampleFrac;
	}

}
