package clustering;

import java.io.IOException;
import java.util.List;

import edu.wlu.cs.levy.CG.KDTree;
import edu.wlu.cs.levy.CG.KeyDuplicateException;
import edu.wlu.cs.levy.CG.KeySizeException;
import factorizationmodel.GeneralModel;

import gnu.trove.TIntHashSet;
import gnu.trove.TIntIntHashMap;
import gnu.trove.TIntObjectHashMap;
import gnu.trove.TObjectIntHashMap;
import io.DataInput;

public class KNNClustering extends Clustering{
	
	private int k = 1;
	
	
	private TIntObjectHashMap<double[]> objMaps;

	DataInput train;
	DataInput test;
	GeneralModel model;
	
	public KNNClustering(double t, DataInput train, DataInput test, int k, GeneralModel model){
		super(t);
		this.k = k;
		this.train = train;
		this.test = test;
		this.model = model;
	}
	
	public void cluster() {
		documentsToClusters = new TIntIntHashMap();
		clusters = new TIntObjectHashMap<TIntHashSet>();
	
		objMaps = new TIntObjectHashMap<double[]>();
		
		int[] trainObjects = null;
		try {
			trainObjects = train.getOrderedIds();
		} catch (IOException e1) {
			e1.printStackTrace();
			return;
		}
//		int[] testObjects = test.getOrderedIds();
		
		KDTree<Integer> tree = new KDTree<Integer>(model.getK());
		
		try {
//			for(int i : train.getObjectClusters().keys()){
			int base = (trainObjects.length-10000);
			for(int obj = 0; obj < 10000; obj++){
				//TODO Avoid that there are repeated elements inserted here!!!
				int i = trainObjects[(int) ((int)base+Math.random()*10000)];
				try{
			    objMaps.put(i, model.map(train.getObjectFeatures().get(i)));
				tree.insert(objMaps.get(i), new Integer(i));
				
				String clus = train.getObjectClusters().get(i);
				documentsToClusters.put(i, train.getClusterIds().get(clus));
				
				}catch (KeyDuplicateException e) {					
				}						
			}
			
			System.out.println("KD-Tree for test constructed!");
			
			int count = 0;
			for(int i : test.getObjectClusters().keys()){
				if(count++%1000==0)	System.out.print(".");
//				if(count++==5) break;
				objMaps.put(i, model.map(test.getObjectFeatures().get(i)));
					
//				List<Integer> l = tree.nearestEuclidean(objMaps.get(i), getThreshold());
				List<Integer> l = tree.nearest(objMaps.get(i), getK());
				int s = l.size();
//				System.out.println(i + " ################################################################ ");

//				for(int neighbor : l){
//					System.out.println(neighbor + ": " + euclideanDistance(i, neighbor));	
//				}
					
				TIntIntHashMap votes = new TIntIntHashMap();					
				int maxval = 0;					
				int maxcluster=-1;
					

//				for(int neighbor : l){
				for(int n = l.size()-1; n >= 0; n--){
					int neighbor = l.get(n);
					if(euclideanDistance(i, neighbor) > getThreshold()) break;
					int c = documentsToClusters.get(neighbor);
					votes.adjustOrPutValue(c, 1, 1);
											
					if(votes.get(c) > maxval){
						maxcluster = c;
						maxval = votes.get(c);														
					} 
				}
				
				if(maxval==0){
					documentsToClusters.put(i, i);
					clusters.put(i, new TIntHashSet());
					clusters.get(i).add(i);				
				} else {
					documentsToClusters.put(i, maxcluster);
					if(!clusters.contains(maxcluster)){
						clusters.put(maxcluster, new TIntHashSet());
					}
					clusters.get(maxcluster).add(i);
				}
				try{
					tree.insert(objMaps.get(i), new Integer(i));
				}catch (KeyDuplicateException e) {					
				}
			}
		
		} catch (KeySizeException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		} 
		
		
	}
	
	private double euclideanDistance(int objA, int objB){		
		
		double d = 0.0;
		for(int i = 0; i < objMaps.get(objA).length; i++){
			d += (double)(objMaps.get(objA)[i] - objMaps.get(objB)[i])*(objMaps.get(objA)[i] - objMaps.get(objB)[i]);
		}
		
		return Math.sqrt(d);
//		return 1.0 / (1 + Math.exp(d));				
	}
	
	public int getK() {
		return k;
	}
	public void setK(int k) {
		this.k = k;
	}
	

}
