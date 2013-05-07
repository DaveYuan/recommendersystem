package datagenerator;

import gnu.trove.TIntArrayList;
import gnu.trove.TIntHashSet;
import gnu.trove.TIntIntHashMap;
import gnu.trove.TIntObjectHashMap;
import gnu.trove.TObjectIntHashMap;

import java.io.BufferedReader;
import java.io.BufferedWriter;
import java.io.FileOutputStream;
import java.io.FileReader;
import java.io.FileWriter;
import java.io.IOException;
import java.io.PrintStream;
import java.util.HashSet;
import java.util.Iterator;
import java.util.Set;
import java.util.StringTokenizer;
import java.util.Vector;

public class PairsGenerator {

	int numPairs = 300;
	Set<String> positive, negative;

	TIntObjectHashMap<TIntHashSet> pos;

	TObjectIntHashMap<String> evIds;
	TIntObjectHashMap<TIntHashSet> clusters;
	TIntIntHashMap picLines;
	TIntHashSet s1, s2, s3;

	String out;

	SimilarityCalculator simCalc;

	int[][] data;
	String[] tags;
	private int maxSamples;
	private String dataset;
	private int topS1, topS2;

	public void generatePairs(String fileName, String dataset, int upperS1,
			int upperS2, boolean type, boolean test, int split, int n, int sampleFrac) {
		this.dataset = dataset;
		topS1 = upperS1;
		topS2 = upperS2;
		int splitBegin = 0;
		int splitEnd = 0;
		
		generateClusters(fileName);
		
		switch(split){
		case 1:
			splitBegin = 0;
			splitEnd = topS1;
			break;
		case 2:
			splitBegin = topS1+1;
			splitEnd = topS2;
			break;
		case 3:
			splitBegin = topS2+1;
			splitEnd = data.length-1;
		}
		
		if(test){			
			sampleTestPairsTimeMovingWindow(splitBegin, splitEnd, n);
		} else {
			sampleTrainPairsTimeMovingWindow(splitBegin, splitEnd, n, sampleFrac);
		}
		
		
//		if (test) {
//			sampleTestPairs();
//		} else {
//			if (type) {
//				samplePositivePairs();
//			} else {
//				sampleNegativePairs();
//			}
//		}
		
		

	}

	public void generateClusters(String fileName) {
		clusters = new TIntObjectHashMap<TIntHashSet>();
		evIds = new TObjectIntHashMap<String>();

		s1 = new TIntHashSet();
		s2 = new TIntHashSet();
		s3 = new TIntHashSet();

		picLines = new TIntIntHashMap();

		DataInput in = new DataInput();
		in.loadDataTime(fileName);
		simCalc = new SimilarityCalculator(in);
		// in.loadDataTime("/home/lucas/workspace/EventIdentification/data/lastfm-test.txt");

		data = in.int_data;
		tags = in.tags;
		/*
		 * 1 - 497.628 (S1) 497.629 - 995.255 (S2) 995.256 - 1.492.883 (S3)
		 */
		for (int i = 0; i < data.length; i++) {
			String event = getEvent(tags[i]);
			picLines.put(data[i][in.PICID], i);
			if (evIds.containsKey(event)) {
				clusters.get(evIds.get(event)).add(data[i][in.PICID]);
			} else {
				int id = evIds.size();
				evIds.put(event, id);
				clusters.put(id, new TIntHashSet());
				clusters.get(id).add(data[i][in.PICID]);
			}
			int id = data[i][in.ID];
			if (id <= topS1) {
				s1.add(data[i][in.PICID]);
			} else if (id <= topS2) {
				s2.add(data[i][in.PICID]);
			} else {
				s3.add(data[i][in.PICID]);
			}

		}
		System.out.println("S1: " + s1.size());
		System.out.println("S2: " + s2.size());
		System.out.println("S3: " + s3.size());
		// for(int i : s1.toArray()){
		// System.out.print(i + " ");
		// }
		//		
		// System.out.println();
		// System.out.println();
		//		
		// System.out.print("S2: ");
		// for(int i : s2.toArray()){
		// System.out.print(i + " ");
		// }
	}

	public void samplePositivePairs() {

		try {
			BufferedWriter output = new BufferedWriter(new FileWriter(out
					+ "-positive.txt"));

			// positive = new HashSet<String>();
			System.out.println(evIds.size());
			int count = 0;
			TIntHashSet pics = new TIntHashSet();
			for (int evId : clusters.keys()) {
				if (count % 1000 == 0) {
					System.out.print(".");
				}
				count++;

				int[] event = clusters.get(evId).toArray();
				if (event.length > 1) {
					int c;

					for (int picA : event) {
						c = 0;

						/********/
						if (s3.contains(picA)) {
							continue;
						}
						/********/

						int pairs = numPairs > event.length - 1 ? event.length - 1
								: numPairs;
						pics.clear();
						pics.addAll(event);
						pics.remove(picA);

						for (int p = 0; p < pairs && pics.size() > 0; p++) {
							int b = (int) Math.floor(Math.random()
									* pics.size());
							int picB = pics.toArray()[b];
							String pair = picA + " " + picB;

							if (simCalc.timeSimilarity(picA, picB) < 0.94
									|| s3.contains(picB)) {
								p--;
								pics.remove(picB);
								continue;
							}
							c++;
							// positive.add(pair);
							output.write(pair + "\n");
							pics.remove(picB);
						}

					}
				}

			}
			output.close();
		} catch (IOException e) {
			e.printStackTrace();
		}

		// writePairs(positive, out + "-positive.txt");

	}

	public void sampleNegativePairs() {
		try {
			BufferedWriter output = new BufferedWriter(new FileWriter(out
					+ "-negative.txt"));
			// negative = new HashSet<String>();
			int count = 0;
			/****************/
			TIntHashSet pics = new TIntHashSet(s1.toArray());
			pics.addAll(s2.toArray());
			/****************/
			for (int picA : pics.toArray()) {
				if (count % 10000 == 0) {
					System.out.print(".");
				}
				count++;

				int p = 0;
				while (p < numPairs) {
					int b, picB;
					int samples = 0;
					do {
						// b = (int) Math.floor(Math.random() * 995254);
						b = (int) Math.floor(Math.random() * topS2);
						picB = data[b][DataInput.PICID];
					} while (simCalc.timeSimilarity(picA, picB) < 0.94
							&& samples++ < maxSamples && s3.contains(picB));
					if (samples >= maxSamples) {
						p++;
						continue;
					}
					String pair = picA + " " + picB;
					if (!simCalc.sameEvent(picA, picB, dataset)) {
						// negative.add(pair);
						output.write(pair + "\n");
						p++;
					}

				}

			}
			output.close();
		} catch (IOException e) {
			e.printStackTrace();
		}
		// writePairs(negative, out + "-negative.txt");
	}

	
	public void sampleTestPairs() {
		try {
			BufferedWriter output = new BufferedWriter(new FileWriter(out
					+ "-testPairs.txt"));
			int count = 0;
			
			/****************/
			TIntHashSet pics = new TIntHashSet(s3.toArray());
			/****************/
			
			for (int picA : pics.toArray()) {
				if (count % 10000 == 0) {
					System.out.print(".");
				}
				count++;
				int p = 0;
				while (p < numPairs) {
					int b, picB;
					int samples = 0;
					do {
						b = (int) Math.floor(Math.random() * (data.length - topS2));
						picB = data[b+topS2][DataInput.PICID];
						samples++;
					} while (simCalc.timeSimilarity(picA, picB) < 0.997
							&& samples < maxSamples);
					if (samples >= maxSamples) {
						p++;
						continue;
					}
					String pair = picA + " " + picB;
					output.write(pair + "\n");
					p++;
				}

			}
			output.close();
		} catch (IOException e) {
			e.printStackTrace();
		}
	}

	public void samplePairsTimeDD(int splitBegin, int splitEnd, int n){
		try {
			BufferedWriter output = new BufferedWriter(new FileWriter(out
					+ "-trainPairs.txt"));
			
			HashSet<String> positive = new HashSet<String>();
			HashSet<String> negative = new HashSet<String>();
			
			for(int i = splitBegin; i < (splitBegin + n); i++){
				for(int j = i; j < (splitBegin + n); j++){
					if(simCalc.timeSimilarity(data[i][DataInput.PICID], data[j][DataInput.PICID]) > 0.94){
						if(simCalc.sameEvent(data[i][DataInput.PICID], data[j][DataInput.PICID], dataset)){
							positive.add(data[i][DataInput.PICID] + " " + data[j][DataInput.PICID]);
						} else {
							negative.add(data[i][DataInput.PICID] + " " + data[j][DataInput.PICID]);
						}
					}
					
				}
			}
			System.out.println("positive: " + positive.size());
			System.out.println("negative: " + negative.size());
			
			Object[] pos = positive.toArray();
			Object[] neg = negative.toArray();
			
			TIntArrayList posIndexes = new TIntArrayList();
			for(int i = 0; i < pos.length; i++){
				posIndexes.add(i);				
			}
			TIntArrayList negIndexes = new TIntArrayList();
			for(int i = 0; i < neg.length; i++){
				negIndexes.add(i);				
			}
			
			int size = (positive.size() + negative.size())/10;
//			int size = positive.size()*2;
			System.out.println("Size: " + size);
			for(int i = 0; i < size/2; i++){
				int p = (int) Math.random()*posIndexes.size();
				Object pair = pos[posIndexes.remove(p)];
				output.write(pair + "\n");
			}
			
			for(int i = 0; i < size/2; i++){
				int p = (int) Math.random()*negIndexes.size();
				Object pair = neg[negIndexes.remove(p)];
				output.write(pair + "\n");
			}
			
			output.close();
		} catch (IOException e) {
			e.printStackTrace();
		}
			
	}
	
	public void sampleTrainPairsTimeMovingWindow(int splitBegin, int splitEnd, int n, int sampleFrac){
		try {
			BufferedWriter output = new BufferedWriter(new FileWriter(out
					+ "-trainPairs.txt"));
//			PrintStream output = new PrintStream(new FileOutputStream(out
//					+ "-trainPairs.txt"));

			HashSet<String> positive = new HashSet<String>();
			HashSet<String> negative = new HashSet<String>();
			
			for(int i = splitBegin; i < splitEnd; i++){
				for(int j = i; j < (i + n) && j < splitEnd; j++){
					if(simCalc.timeSimilarity(data[i][DataInput.PICID], data[j][DataInput.PICID]) > 0.94){
//						output.write(data[i][DataInput.PICID] + " " + data[j][DataInput.PICID] + "\n");
						if(simCalc.sameEvent(data[i][DataInput.PICID], data[j][DataInput.PICID], dataset)){
							positive.add(data[i][DataInput.PICID] + " " + data[j][DataInput.PICID]);
						} else {
							negative.add(data[i][DataInput.PICID] + " " + data[j][DataInput.PICID]);
						}
					}
					
				}
			}
			Object[] pos = positive.toArray();
			Object[] neg = negative.toArray();
			
			TIntArrayList posIndexes = new TIntArrayList();
			for(int i = 0; i < pos.length; i++){
				posIndexes.add(i);				
			}
			TIntArrayList negIndexes = new TIntArrayList();
			for(int i = 0; i < neg.length; i++){
				negIndexes.add(i);				
			}
			
			System.out.println("positive: " + positive.size());
			System.out.println("negative: " + negative.size());
			
			int size = (positive.size() + negative.size())/sampleFrac;

			System.out.println("Size: " + size);
			size /= 2;
			for(int i = 0; i < size; i++){
				int p = (int) Math.random()*posIndexes.size();
				Object pair = pos[posIndexes.remove(p)];
//				Object pair = pos[p];
				output.write(pair + "\n");
//				output.println(pair);
			}
			
			for(int i = 0; i < size; i++){
				int p = (int) Math.random()*negIndexes.size();
				Object pair = neg[negIndexes.remove(p)];
				output.write(pair + "\n");
//				output.println(pair);
//				negative.remove(pair);
			}
			
			output.close();
		} catch (IOException e) {
			e.printStackTrace();
		}
	}
	
	public void sampleTestPairsTimeMovingWindow(int splitBegin, int splitEnd, int n){
		try {
			BufferedWriter output = new BufferedWriter(new FileWriter(out
					+ "-testPairs.txt"));

			int positive = 0;
			int negative = 0;
			for(int i = splitBegin; i < splitEnd; i++){
				for(int j = i; j < (i + n) && j < splitEnd; j++){
					if(simCalc.timeSimilarity(data[i][DataInput.PICID], data[j][DataInput.PICID]) > 0.94){
						output.write(data[i][DataInput.PICID] + " " + data[j][DataInput.PICID] + "\n");
						if(simCalc.sameEvent(data[i][DataInput.PICID], data[j][DataInput.PICID], dataset)){
							positive++;
						} else {
							negative++;
						}
					}
					
				}
			}
			System.out.println("positive: " + positive);
			System.out.println("negative: " + negative);
			
			int size = positive + negative;
			System.out.println("Size: " + size);
//			for(int i = 0; i < size/2; i++){
//				int p = (int) Math.floor(Math.random()*positive.size());
//				Object pair = positive.toArray()[p];
//				output.write(pair + "\n");
//				positive.remove(pair);
//			}
//			
//			for(int i = 0; i < size/2; i++){
//				int p = (int) Math.floor(Math.random()*negative.size());
//				Object pair = negative.toArray()[p];
//				output.write(pair + "\n");
//				negative.remove(pair);
//			}
//			
			output.close();
		} catch (IOException e) {
			e.printStackTrace();
		}
			
	}
	
	public String getEvent(String tagSet) {
		StringTokenizer st = new StringTokenizer(tagSet, " ");
		String ev = "-1";
		while (st.hasMoreTokens()) {
			String tmp = st.nextToken();
			if (tmp.startsWith(dataset + ":event=")) {
				ev = tmp.substring((dataset + ":event=").length());
				break;
			}
		}
		return ev;
	}

	public void writePairs(Set<String> pairs, String fileName) {
		try {
			BufferedWriter out = new BufferedWriter(new FileWriter(fileName));
			Iterator<String> it = pairs.iterator();
			while (it.hasNext()) {
				out.write(it.next() + "\n");
			}
			out.close();
		} catch (IOException e) {
			e.printStackTrace();
		}
	}

	public String getOut() {
		return out;
	}

	public void setOut(String out) {
		this.out = out;
	}

	public int getNumPairs() {
		return numPairs;
	}

	public void setNumPairs(int numPairs) {
		this.numPairs = numPairs;
	}

	public int getMaxSamples() {
		return maxSamples;
	}

	public void setMaxSamples(int maxSamples) {
		this.maxSamples = maxSamples;
	}

	public String getDataset() {
		return dataset;
	}

	public void setDataset(String dataset) {
		this.dataset = dataset;
	}

}
