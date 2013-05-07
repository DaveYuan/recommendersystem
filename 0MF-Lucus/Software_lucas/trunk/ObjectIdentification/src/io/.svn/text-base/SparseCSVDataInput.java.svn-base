package io;

import gnu.trove.TIntObjectHashMap;

import java.io.BufferedReader;
import java.io.FileReader;
import java.io.IOException;
import java.util.StringTokenizer;

import util.SparseArray;

public class SparseCSVDataInput extends DataInput {

	@Override
	public void readData(String fileName) throws IOException {
		this.fileName = fileName;
		objectClusters = new TIntObjectHashMap<String>();
		objectFeatures = new TIntObjectHashMap<SparseArray>();
		
		BufferedReader in = new BufferedReader(new FileReader(fileName));
		
		String line = "";
		
		while((line = in.readLine()) != null){
			StringTokenizer st = new StringTokenizer(line,",");
			int id = Integer.parseInt(st.nextToken());
			String cluster = st.nextToken();
			
			objectClusters.put(id, cluster);
			objectFeatures.put(id, new SparseArray());
			
//			st.nextToken(); // Temporarily scale the first feature (Timestamps) : they are on another scale
//			StringTokenizer st2 = new StringTokenizer(st.nextToken(),":");
//			st2.nextToken();
//			long time = Long.parseLong(st2.nextToken()); 
//			double timeFeature = (double) time / (double) 3600000;
//			objectFeatures.get(id).set(0, timeFeature);
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

	}
	
	public int[] getOrderedIds() throws IOException {
		BufferedReader in = new BufferedReader(new FileReader(fileName));
		int[] ids = new int[objectFeatures.size()];
		String line = "";
		int lineNum = 0;
		while((line = in.readLine()) != null){
			StringTokenizer st = new StringTokenizer(line,",");
			int id = Integer.parseInt(st.nextToken());
			ids[lineNum++] = id;
			
		}
		in.close();		
		return ids;

	}
	
	

}
