package clustering;

import gnu.trove.TIntHashSet;
import gnu.trove.TIntIntHashMap;
import gnu.trove.TIntObjectHashMap;

public abstract class Clustering {

	double threshold = 0.1;
	
	/**
	 * Maps each cluster ID to the actual clusters
	 */
	TIntObjectHashMap<TIntHashSet> clusters;
	/**
	 * Maps each document to its respective cluster ID
	 */
	TIntIntHashMap documentsToClusters;
	
	
	public Clustering(double t){
		threshold = t;
	}
	
	public abstract void cluster();
	
	
	public double getThreshold() {
		return threshold;
	}
	public void setThreshold(double threshold) {
		this.threshold = threshold;
	}
	public TIntObjectHashMap<TIntHashSet> getClusters() {
		return clusters;
	}
	public void setClusters(TIntObjectHashMap<TIntHashSet> clusters) {
		this.clusters = clusters;
	}
	public TIntIntHashMap getDocumentsToClusters() {
		return documentsToClusters;
	}
	public void setDocumentsToClusters(TIntIntHashMap documentsToClusters) {
		this.documentsToClusters = documentsToClusters;
	}
}
