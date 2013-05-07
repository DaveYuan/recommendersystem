package data;


import gnu.trove.TIntIntHashMap;

import java.io.BufferedReader;
import java.io.FileReader;
import java.util.StringTokenizer;

public class RatingsData {

	public TIntIntHashMap userIds, itemIds;
	public int[] reverseUserIds, reverseItemIds;
	public int[][] trainingData, testData;	
	
	public int[][] loadTrainingData(String fileName){
        userIds = new TIntIntHashMap();
        itemIds = new TIntIntHashMap();
        
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
            trainingData = new int[numRows][3];
            int curRow = 0;
            while ((line = file.readLine()) != null) {            

                StringTokenizer st = new StringTokenizer(line, "\t");
                int user = Integer.parseInt(st.nextToken());
                int item = Integer.parseInt(st.nextToken());
                int rating = Integer.parseInt(st.nextToken());
                             
                if(!userIds.containsKey(user)){
                	int id =  userIds.size();
                	userIds.put(user, id);
                }
                if(!itemIds.containsKey(item)){
                	int id = itemIds.size();
                	itemIds.put(item, id);
                }
                                
                trainingData[curRow][0] = userIds.get(user);
                trainingData[curRow][1] = itemIds.get(item);
                trainingData[curRow][2] = rating;
                
                curRow++;
            }
            file.close();
            
            reverseUserIds = new int[userIds.size()];
            for(int u : userIds.keys()){
            	reverseUserIds[userIds.get(u)] = u;
            }
            reverseItemIds = new int[itemIds.size()];
            for(int i : itemIds.keys()){
            	reverseItemIds[itemIds.get(i)] = i;
            }
        }  catch (Exception ex) {
            ex.printStackTrace();

        }

        return trainingData;
    }
	
	public int[][] loadTestData(String fileName){        
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
            testData = new int[numRows][3];
            int curRow = 0;
            while ((line = file.readLine()) != null) {            

                StringTokenizer st = new StringTokenizer(line, "\t");
                int user = Integer.parseInt(st.nextToken());
                int item = Integer.parseInt(st.nextToken());
                int rating = Integer.parseInt(st.nextToken());
                             
                if(!userIds.containsKey(user)){
//                	continue;
//                	int id =  userIds.size();
//                	userIds.put(user, id);
                }
                if(!itemIds.containsKey(item)){
//                	continue;
//                	int id = itemIds.size();
//                	itemIds.put(item, id);
                }
                                
                testData[curRow][0] = userIds.get(user);
                testData[curRow][1] = itemIds.get(item);
                testData[curRow][2] = rating;
                
                curRow++;
            }
            file.close();

        }  catch (Exception ex) {
            ex.printStackTrace();

        }

        return testData;
    }
}
