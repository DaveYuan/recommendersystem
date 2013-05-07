
import datagenerator.Blocker;
import datagenerator.DataInput;
import datagenerator.Deduplicator;
import datagenerator.PairsGenerator;
import datagenerator.SimilarityCalculator;
import de.ismll.bootstrap.CommandLineParser;
import de.ismll.bootstrap.Parameter;


public class Main implements Runnable{

	
	@Parameter(cmdline="file", description="The file containing the data")
	private String file = "/home/lucas/workspace/EventIdentification/data/lastfm-datetaken.txt";
//	private String file = "/home/lucas/workspace/EventIdentification/data/toy.txt";
	
	@Parameter(cmdline="out", description="The file containing the output")
	private String out = "./lastfm";
		
	@Parameter(cmdline="pairs", description="The file containing the data")
	private int pairs = 40;
	
	@Parameter(cmdline="type", description="Positive (true) or negative (false) pairs")
	private boolean type = false;
	
	@Parameter(cmdline="samples", description="Maximum number of samples")
	private int samples = 1000;



	/**
	 * @param args
	 */
	public void run() {
		
//		ClassCounts counter = new ClassCounts(2, "\t");
//		
//		counter.createClassCounts("/home/lucas/data/event-identification/100000.csv.trans.test", "100000.test-plot.txt");
		
//		PairsGenerator p = new PairsGenerator();
////		int pairs = Integer.parseInt(args[0]);
//		int pairs = 10;
//		p.setNumPairs(pairs);
////		boolean type = Boolean.parseBoolean(args[1]);
////		p.generatePairs(type);
//		for(int i = 30; i < 41; i++)
//			p.generatePairs(true,i);
		
//		PairsGenerator p = new PairsGenerator();
//		p.setNumPairs(pairs);
//		p.setOut(out);
//		p.setMaxSamples(samples);		
//		p.generatePairs(file, type);
//		
//		Deduplicator dp = new Deduplicator();
//		dp.loadDataTime("./data/lastfm-positive.txt", "./data/lastfm-pos.txt");
		
		DataInput input = new DataInput();
		input.loadDataTime("./data/upcoming/upcoming-datetaken.txt");
		
		SimilarityCalculator sc = new SimilarityCalculator(input);
		Blocker b = new Blocker();
		b.setSimilarities(sc);
		b.block("./data/upcoming/upcoming-train-pairs.txt", "./data/upcoming/upcoming-s1-blocking.txt");

	}
	
	public static void main(String[] args) {
		Main e = new Main();
		CommandLineParser.parseCommandLine(args, e);		
		e.run();
	}
	public String getFile() {
		return file;
	}
	public void setFile(String file) {
		this.file = file;
	}
	public int getPairs() {
		return pairs;
	}
	public void setPairs(int pairs) {
		this.pairs = pairs;
	}
	public String getOut() {
		return out;
	}

	public void setOut(String out) {
		this.out = out;
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

}
