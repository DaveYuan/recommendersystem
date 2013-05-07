package clustering;

import java.io.BufferedReader;
import java.io.FileReader;
import java.io.IOException;
import java.util.StringTokenizer;

import util.DistanceCalculator;

import gnu.trove.TIntHashSet;
import gnu.trove.TIntIntHashMap;
import gnu.trove.TIntObjectHashMap;

public class SingleLinkageHAC extends Clustering{

	
	String pairsFile;
	
	public SingleLinkageHAC(double t, String pairs){
		super(t);		
		pairsFile = pairs;
	}
	
	
	public void cluster(){
		
		try {
			
			BufferedReader in = new BufferedReader(new FileReader(pairsFile));
			String line = null;
			
			documentsToClusters = new TIntIntHashMap();
			clusters = new TIntObjectHashMap<TIntHashSet>();
			
			int count = 0;
			while((line = in.readLine()) != null){
				if(count++%1000000 == 0)
					System.err.print(".");
				StringTokenizer st = new StringTokenizer(line);
				
				int pic1 = Integer.parseInt(st.nextToken());
				int pic2 = Integer.parseInt(st.nextToken());

				double distance = DistanceCalculator.computeDistance(pic1, pic2);
				
				if(!documentsToClusters.containsKey(pic1)){
					documentsToClusters.put(pic1, pic1);
					clusters.put(pic1, new TIntHashSet());
					clusters.get(pic1).add(pic1);
				}
				if(!documentsToClusters.containsKey(pic2)){
					documentsToClusters.put(pic2, pic2);
					clusters.put(pic2, new TIntHashSet());
					clusters.get(pic2).add(pic2);
				}
				
				if(distance > threshold || documentsToClusters.get(pic1) == documentsToClusters.get(pic2)) continue;
				
				//Merge the two clusters
				int c1 = documentsToClusters.get(pic1);
				int c2 = documentsToClusters.get(pic2);

				clusters.get(c1).addAll(clusters.get(c2).toArray());
				
				for(int pic : clusters.get(c2).toArray()){
					documentsToClusters.put(pic, c1);
				}
				
				clusters.remove(c2);
				
			}
			
			in.close();
			
		} catch (IOException e) {
			e.printStackTrace();
		}
		
	}


	
}
