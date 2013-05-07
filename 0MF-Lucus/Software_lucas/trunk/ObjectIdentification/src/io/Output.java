package io;

import java.io.BufferedWriter;
import java.io.FileWriter;
import java.io.IOException;

import gnu.trove.TIntHashSet;
import gnu.trove.TIntObjectHashMap;

public class Output {

	public static void writeClustersToFile(TIntObjectHashMap<TIntHashSet> clusters, 
			String fileName) throws IOException{
		
		
		BufferedWriter out = new BufferedWriter(new FileWriter(fileName));
		
		for(int c : clusters.keys()){
			for(int doc : clusters.get(c).toArray()){
				out.write(doc + " " + c + "\n");
			}
		}
		
		out.close();
	}
}
