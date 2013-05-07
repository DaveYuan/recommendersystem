package data;

import gnu.trove.TIntHashSet;
import gnu.trove.TIntObjectHashMap;

import java.io.BufferedReader;
import java.io.FileReader;
import java.util.HashMap;
import java.util.StringTokenizer;

import no.uib.cipr.matrix.sparse.SparseVector;

public class TripleDataInput {

	int[][] data; 
	public double[] labels;
	public int biggest1, biggest2, biggest3;
	public TIntHashSet tags;
	public HashMap<String, TIntHashSet> posts;
	
	   
	/*
	 * This method loads users and their observed actions which are stored in a parse manner
	 */
    public int[][] loadTrainingData(String fileName){
           	    	
    	biggest1 = biggest2 = biggest3 = 0;
    	
    	BufferedReader dataFile = null;
        
        try {
        	
            dataFile = new BufferedReader(new FileReader(fileName));        
            
            int numLines = 0;
            String line;
            while(dataFile.readLine() != null){
            	numLines++;
            }
            
            dataFile.close();
            
            dataFile = new BufferedReader(new FileReader(fileName));
            
            data = new int[numLines][4];
            labels = new double[numLines];
            tags = new TIntHashSet();
            posts = new HashMap<String, TIntHashSet>();
            
            int currLine = 0;
            while ((line = dataFile.readLine()) != null) {            

                StringTokenizer st = new StringTokenizer(line, "\t");
                int id1 = Integer.parseInt(st.nextToken());
                                                
                if(id1 > biggest1)
                	biggest1 = id1;
                
                int id2 = Integer.parseInt(st.nextToken());
                
                if(id2 > biggest2)
                	biggest2 = id2;
                
                int id3 = Integer.parseInt(st.nextToken());
                
                if(id3 > biggest3)
                	biggest3 = id3;
                
                tags.add(id3);
                String post = id1 + " " + id2;
                if(!posts.containsKey(post)){
                	posts.put(post, new TIntHashSet());
                }
                posts.get(post).add(id3);
                
//                double value = Double.parseDouble(st.nextToken());
                double value = 1.0;
                                
                data[currLine][0] = id1;
                data[currLine][1] = id2;
                data[currLine][2] = id3;
                labels[currLine] = value;
                currLine++;
            }
            
            dataFile.close();

        }  catch (Exception ex) {
            ex.printStackTrace();

        }

        return data;
    }
    
    public TIntObjectHashMap<TIntHashSet> loadTestData(String fileName){
	    	    	
    	BufferedReader dataFile = null;
    	TIntObjectHashMap<TIntHashSet> testData = new TIntObjectHashMap<TIntHashSet>();
        try {
        	
            dataFile = new BufferedReader(new FileReader(fileName));        
            
            int numLines = 0;
            String line;
                        
            
            int currLine = 0;
            while ((line = dataFile.readLine()) != null) {            

                StringTokenizer st = new StringTokenizer(line, "\t");
                int id1 = Integer.parseInt(st.nextToken());
                                                
                if(id1 > biggest1)
                	biggest1 = id1;
                
                int id2 = Integer.parseInt(st.nextToken());
                
                if(id2 > biggest2)
                	biggest2 = id2;
                
                int id3 = Integer.parseInt(st.nextToken());
                
                if(id3 > biggest3)
                	biggest3 = id3;
         
                if(!testData.containsKey(id1)){
                	testData.put(id1, new TIntHashSet());
                }
                testData.get(id1).add(id2);
            }
            
            dataFile.close();

        }  catch (Exception ex) {
            ex.printStackTrace();

        }

        return testData;
    }
    
}
