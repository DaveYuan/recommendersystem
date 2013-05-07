package data;


import gnu.trove.TIntHashSet;
import gnu.trove.TIntIntHashMap;
import gnu.trove.TIntObjectHashMap;

import java.io.BufferedReader;
import java.io.FileReader;
import java.io.IOException;
import java.util.Hashtable;
import java.util.StringTokenizer;

public class RatingsData {

	public static int MIN = 3;
	public static int MAX = 4;
	public static int RANGE = 5;
	public static int SPARSE = 6;
	
//	public TIntIntHashMap userIds, itemIds;
	public int[] reverseUserIds, reverseItemIds;
	public double[][] testData;
	public double[][][] trainingData;
	
	public String dataDir;
	
	public int[] entityTypes;
	public int[][] entityIds;
	public int[][] relations; 
	public Hashtable<Integer, Integer> internal2externalRelations;
	public Hashtable<Integer, Integer> external2internalRelations;
	public Hashtable<Integer, Integer> internal2externalEntityIds;
	public Hashtable<Integer, Integer> external2internalEntityIds;
	
	
	public TIntHashSet testItems;
	public TIntIntHashMap[] entities;
	
	public RatingsData(String dir){
		if(!dir.endsWith("/")){
			dir += "/";
		}
		dataDir = dir;
	}
	
	
	
	public void loadTrainingData(String fileName) throws IOException{
        
		loadRelations(dataDir + "relations.txt");
		loadEntities(dataDir + "entities.txt");
		     
        try {        	
        	for(int rel = 1; rel < relations.length; rel++){            	
            	loadMatrix(dataDir + "/relation-" + relations[rel][0] + ".txt", rel);          	            	
            }        	
        	
        	loadMatrix(fileName, 0); //user-item ratings matrix
        	
        	reverseUserIds = new int[entities[0].size()];
            for(int u : entities[0].keys()){
            	reverseUserIds[entities[0].get(u)] = u;
            }
            reverseItemIds = new int[entities[1].size()];
            for(int i : entities[1].keys()){
            	reverseItemIds[entities[1].get(i)] = i;
            }
        }  catch (Exception ex) {
            ex.printStackTrace();
        }
        
    }
	
	//This loads the user-item ratings relation!
	//TODO make this general
	public double[][] loadTestData(String fileName){        
        BufferedReader file = null;
    	int entityA = relations[0][1];
		int entityB = relations[0][2];
		
        try {
        	
            file = new BufferedReader(new FileReader(fileName));
            
            int numRows = 0; 
            
            while(file.readLine() != null){
            	numRows++;
            }
            
            file.close();
            
            file = new BufferedReader(new FileReader(fileName));
            String line;
            testData = new double[numRows][3];
            int curRow = 0;
            while ((line = file.readLine()) != null) {            

                StringTokenizer st = new StringTokenizer(line, "\t");
                int user = Integer.parseInt(st.nextToken());
                int item = Integer.parseInt(st.nextToken());
                double rating = Double.parseDouble(st.nextToken());
                                
                testData[curRow][0] = entities[entityA].get(user);
                testData[curRow][1] = entities[entityB].get(item);
                testData[curRow][2] = rating;
                
                curRow++;
            }
            file.close();

        }  catch (Exception ex) {
            ex.printStackTrace();

        }

        return testData;
    }
	
	public TIntObjectHashMap<double[]> loadPredictions(String predFileName, String idFileName, int numItems){        
        BufferedReader file = null;
        
        TIntIntHashMap externalToInternalUserId = new TIntIntHashMap();
		
		for(int u = 0; u < reverseUserIds.length; u++){
			externalToInternalUserId.put(reverseUserIds[u], u);
		}
		
		TIntIntHashMap externalToInternalItemId = new TIntIntHashMap();
		
		for(int u = 0; u < reverseItemIds.length; u++){
			externalToInternalItemId.put(reverseItemIds[u], u);
		}
    	
		TIntIntHashMap lineToId = new TIntIntHashMap();
		TIntObjectHashMap<double[]> predictions = new TIntObjectHashMap<double[]>();
		
        try {
        	
            file = new BufferedReader(new FileReader(idFileName));
            
            int numRows = 0; 
            String line;
            
            while((line = file.readLine()) != null){
            	int extId = Integer.parseInt(line);
            	lineToId.put(numRows, externalToInternalUserId.get(extId));
            	predictions.put(externalToInternalUserId.get(extId), new double[numItems]);
            	numRows++;
            }
            
            file.close();
            
            file = new BufferedReader(new FileReader(predFileName));
            
            
            int curRow = 0;
            while ((line = file.readLine()) != null) {            

                StringTokenizer st = new StringTokenizer(line, "\t");
                int item = 1;
                int user = lineToId.get(curRow);
                while(st.hasMoreTokens()){
                	double pred = Double.parseDouble(st.nextToken());
                	predictions.get(user)[externalToInternalItemId.get(item)] = pred;
                	item++;
                }
                
                
                curRow++;
            }
            file.close();

        }  catch (Exception ex) {
            ex.printStackTrace();

        }

        return predictions;
    }
	
	public void loadEntities(String fileName) throws IOException{
		BufferedReader file = null;	
	
//        try {
        	
            file = new BufferedReader(new FileReader(fileName));
                                     
            String line;
            int numRows = Integer.parseInt(file.readLine());
            entityTypes = new int[numRows];
            entities = new TIntIntHashMap[numRows];
            
            int curRow = 0;
            while ((line = file.readLine()) != null) {            

                StringTokenizer st = new StringTokenizer(line, "\t");
                int entity = Integer.parseInt(st.nextToken());
                entityTypes[curRow++] = entity;
            }
            file.close();

            entityIds = new int[numRows][];
            for(int e = 0; e < entityTypes.length; e++){
            	
            	if(entities[e] == null){
         			entities[e] = new TIntIntHashMap();
         		}
            	file = new BufferedReader(new FileReader(dataDir + "/entity-" + entityTypes[e] + ".txt"));
            	
            	int numEnt = 0;                 
                while(file.readLine() != null){ numEnt++; }
                file.close();
                
                entityIds[e] = new int[numEnt];
                
                file = new BufferedReader(new FileReader(dataDir + "/entity-" + entityTypes[e] + ".txt"));
                curRow = 0;
                while ((line = file.readLine()) != null) {      
                	
                	if (line.isEmpty()) {
						break;
					}

//                    StringTokenizer st = new StringTokenizer(line, "\t");
//                    int ent = Integer.parseInt(st.nextToken());
                 
                	int ent = Integer.parseInt(line);
                    entityIds[e][curRow] = ent;
                    
                    if(!entities[e].containsKey(ent)){
                    	entities[e].put(ent,curRow);
                    }
                    curRow++;
                }
                file.close();
            	
            }
 //       }  catch (Exception ex) {
   //         ex.printStackTrace();

     //   }
	}
	
	public void loadRelations(String fileName){
		BufferedReader file = null;
		String line = null;
        try {
        	
            file = new BufferedReader(new FileReader(fileName));
            file.readLine();
            
            int numRows = Integer.parseInt(file.readLine());
            relations = new int[numRows][7];
            trainingData = new double[numRows][][];
            int curRow = 0;
            
            int idCounter = 0;
            external2internalEntityIds = new Hashtable<Integer, Integer>();
            internal2externalEntityIds = new Hashtable<Integer, Integer>();
            
            while ((line = file.readLine()) != null) {            
                StringTokenizer st = new StringTokenizer(line, "\t");
                int relation = Integer.parseInt(st.nextToken());
                int eA = Integer.parseInt(st.nextToken());
                int eB = Integer.parseInt(st.nextToken());
                int minVal = Integer.parseInt(st.nextToken());
                int maxVal = Integer.parseInt(st.nextToken());
                int sparse = Integer.parseInt(st.nextToken());
                
                int internalEA = -1;
                if (!external2internalEntityIds.contains(eA)) {
					external2internalEntityIds.put(eA, idCounter);
					internal2externalEntityIds.put(idCounter++, eA);
				}
				internalEA = external2internalEntityIds.get(eA);
                
                int internalEB = -1;
                if (!external2internalEntityIds.contains(eB)) {
					external2internalEntityIds.put(eB, idCounter);
					internal2externalEntityIds.put(idCounter++, eB);
				}
				internalEB = external2internalEntityIds.get(eB);
      
                relations[curRow][0] = relation;
                relations[curRow][1] = internalEA;
                relations[curRow][2] = internalEB;
                relations[curRow][MIN] = minVal;
                relations[curRow][MAX] = maxVal;
                relations[curRow][RANGE] = maxVal - minVal;
                relations[curRow][SPARSE] = sparse;
                
                curRow++;
            }
            file.close();

        }  catch (Exception ex) {
            ex.printStackTrace();

        }
	}
	
	public void loadTestItems(String fileName){
		BufferedReader file = null;
		String line = null;
        try {
        	
        	int itemEntity = relations[0][2];
        	testItems = new TIntHashSet();
            file = new BufferedReader(new FileReader(fileName));
                       
            while ((line = file.readLine()) != null) {            
                StringTokenizer st = new StringTokenizer(line, "\t");
                int item = Integer.parseInt(st.nextToken());
                testItems.add(entities[itemEntity].get(item));          
            }
            file.close();

        }  catch (Exception ex) {
            ex.printStackTrace();

        }
        
	}
	
	public void loadMatrix(String fileName, int relation){
	    int entityA = relations[relation][1];
	    int entityB = relations[relation][2];    
		
//	    if(entities[entityA] == null){
//			entities[entityA] = new TIntIntHashMap();
//		}
//	    if(entities[entityB] == null){
//			entities[entityB] = new TIntIntHashMap();
//		}
//		
        BufferedReader file = null;
                
        try {
        	
            file = new BufferedReader(new FileReader(fileName));
            
            int numRows = 0; 
            
            while(file.readLine() != null){
            	numRows++;
            }
            
            file.close();
            
            file = new BufferedReader(new FileReader(fileName));
            String line;
            trainingData[relation] = new double[numRows][3];
            int curRow = 0;
            
            while ((line = file.readLine()) != null) {            

                StringTokenizer st = new StringTokenizer(line, "\t");
                int user = Integer.parseInt(st.nextToken());
                int item = Integer.parseInt(st.nextToken());
                double rating = Double.parseDouble(st.nextToken());
                             
//                if(!entities[entityA].containsKey(user)){
//                	int id =  entities[entityA].size();
//                	entities[entityA].put(user, id);
//                }
//                if(!entities[entityB].containsKey(item)){
//                	int id = entities[entityB].size();
//                	entities[entityB].put(item, id);
//                }
                                
                trainingData[relation][curRow][0] = entities[entityA].get(user);
                trainingData[relation][curRow][1] = entities[entityB].get(item);
                trainingData[relation][curRow][2] = rating;
                
                curRow++;
            }
            file.close();
            
//            reverseUserIds = new int[userIds.size()];
//            for(int u : userIds.keys()){
//            	reverseUserIds[userIds.get(u)] = u;
//            }
//            reverseItemIds = new int[itemIds.size()];
//            for(int i : itemIds.keys()){
//            	reverseItemIds[itemIds.get(i)] = i;
//            }
            
        }  catch (Exception ex) {
            ex.printStackTrace();

        }

    }
}
