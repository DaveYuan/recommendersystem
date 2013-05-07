import java.io.BufferedReader;
import java.io.FileReader;
import java.io.IOException;
import java.util.StringTokenizer;

import FactorizationModel.DimReductionModel;
import FactorizationModel.GeneralModel;

import gnu.trove.TIntHashSet;
import gnu.trove.TIntObjectHashMap;
import utils.SparseArray;
import datagenerator.DataInput;
import datagenerator.SimilarityCalculator;


public class RunFactorizationModel {

	/**
	 * @param args
	 */
	public static void main(String[] args) {
		String directory = "/home/lucas/workspace/EventIdentification/data/upcoming/";
		String dataset = "upcoming";
		
		
		DataInput input = new DataInput();
		input.loadDataTime(directory + dataset + "-datetaken.txt");
		int f = input.loadTextualData(directory + dataset + "-tag.txt", 0, 116665);
		
		TIntObjectHashMap<SparseArray> tagVectors = input.getVectors();
		SimilarityCalculator sc = new SimilarityCalculator(input);
		
		String infile = directory + "upcoming-s1-trainPairs.txt";
		
		TIntObjectHashMap<TIntHashSet> pos = new TIntObjectHashMap<TIntHashSet>();
		TIntObjectHashMap<TIntHashSet> neg = new TIntObjectHashMap<TIntHashSet>();
		
		try {
			BufferedReader in = new BufferedReader(new FileReader(infile));
			
			String line = null;
			while ((line = in.readLine()) != null) {
				StringTokenizer st = new StringTokenizer(line, " ");
				int a = Integer.parseInt(st.nextToken());
				int b = Integer.parseInt(st.nextToken());
								
				if(sc.sameEvent(a, b, dataset)){
					if(!pos.containsKey(a)){
						pos.put(a, new TIntHashSet());
					}
					pos.get(a).add(b);
				} else {
					if(!neg.containsKey(a)){
						neg.put(a, new TIntHashSet());
					}
					neg.get(a).add(b);
				}
			}
			
			in.close();
		} catch (IOException e) {
			e.printStackTrace();
		}
		
		System.out.println("Data Loaded! Training the model ...");
		
		GeneralModel model = new DimReductionModel();
		
		model.setK(2);
		model.setF(f);
		model.setLearnRate(0.0005);
		model.setMaxIter(80);
		model.setReg(0.001);
		model.setClassConst(0.5);
		model.setFeatureVectors(tagVectors);
		model.setPositivePairs(pos);
		model.setNegativePairs(neg);
		model.init();
		model.train();
	}

}
