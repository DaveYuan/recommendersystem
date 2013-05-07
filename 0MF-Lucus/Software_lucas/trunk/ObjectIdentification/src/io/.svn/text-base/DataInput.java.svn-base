package io;

import java.io.IOException;

import gnu.trove.TIntHashSet;
import gnu.trove.TIntObjectHashMap;
import gnu.trove.TObjectIntHashMap;
import util.SparseArray;



public abstract class DataInput {

	
	protected TObjectIntHashMap<String> clusterIds = null;
	protected TIntObjectHashMap<SparseArray> objectFeatures = null;
	protected TIntObjectHashMap<String> objectClusters = null;
	protected TIntObjectHashMap<TIntHashSet> objectToClusters = null;
	protected int features = 0;
	protected String fileName;
	
	
	public abstract void readData(String fileName) throws IOException;


	public TIntObjectHashMap<TIntHashSet> getObjectToClusters() {
		if(objectClusters == null) return null;
		if(objectToClusters == null){
			objectToClusters = new TIntObjectHashMap<TIntHashSet>();
			clusterIds = new TObjectIntHashMap<String>();
			TIntObjectHashMap<TIntHashSet> clusters = new TIntObjectHashMap<TIntHashSet>();
			
			for(int i : objectClusters.keys()){
				String cluster = objectClusters.get(i);
				if(clusterIds.containsKey(cluster)){
					clusters.get(clusterIds.get(cluster)).add(i);
				} else {
					int clusid = clusterIds.size();
					clusterIds.put(cluster, clusid);
					clusters.put(clusid, new TIntHashSet());
					clusters.get(clusid).add(i);
				}
				objectToClusters.put(i, clusters.get(clusterIds.get(cluster)));
			}
		}
		
		return objectToClusters;
	}

	public abstract int[] getOrderedIds() throws IOException; 
	
	public void setObjectToClusters(TIntObjectHashMap<TIntHashSet> objectToClusters) {
		this.objectToClusters = objectToClusters;
	}


	public TIntObjectHashMap<SparseArray> getObjectFeatures() {
		return objectFeatures;
	}


	public void setObjectFeatures(TIntObjectHashMap<SparseArray> objectFeatures) {
		this.objectFeatures = objectFeatures;
	}


	public TIntObjectHashMap<String> getObjectClusters() {
		return objectClusters;
	}


	public void setObjectClusters(TIntObjectHashMap<String> objectClusters) {
		this.objectClusters = objectClusters;
	}


	public int getFeatures() {
		return features;
	}


	public void setFeatures(int features) {
		this.features = features;
	}


	public TObjectIntHashMap<String> getClusterIds() {
		return clusterIds;
	}


	public void setClusterIds(TObjectIntHashMap<String> clusterIds) {
		this.clusterIds = clusterIds;
	}
}
