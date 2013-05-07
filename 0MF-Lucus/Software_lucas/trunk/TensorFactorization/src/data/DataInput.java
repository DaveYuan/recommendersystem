package data;

import gnu.trove.TIntHashSet;
import gnu.trove.TIntObjectHashMap;

import java.io.BufferedReader;
import java.io.FileReader;
import java.util.StringTokenizer;

public class DataInput {
	
		
	private TIntObjectHashMap<TIntHashSet> users, itemAttributes; 
	private TIntHashSet items;
	
	private int biggestItem;
	private int featureNum;
	private int biggestUser;

	

	/*
	 * This method loads users and their observed actions which are stored in a parse manner
	 */
    public TIntObjectHashMap<TIntHashSet> loadTrainingData(String fileName){
        users = new TIntObjectHashMap<TIntHashSet>();
        items = new TIntHashSet(); 

        BufferedReader usersFile = null;
        
        try {
        	
            usersFile = new BufferedReader(new FileReader(fileName));        
            String line;

            while ((line = usersFile.readLine()) != null) {            

                StringTokenizer st = new StringTokenizer(line, "\t");
                int id = Integer.parseInt(st.nextToken());
                
                //if(id > 300) continue; 2318                
                if(id > biggestUser)
                    biggestUser = id;
                
                id--;
                
                if(!users.containsKey(id))
                    users.put(id, new TIntHashSet());

                int item = Integer.parseInt(st.nextToken());
                item--;
                
                if(itemAttributes.containsKey(item)){
                    users.get(id).add(item);
                    if(!items.contains(item)){
                    	items.add(item);
                    }
                } 

            }
            usersFile.close();

        }  catch (Exception ex) {
            ex.printStackTrace();

        }

        return users;
    }
    
    
    
    public TIntObjectHashMap<TIntHashSet> getUsers() {
		return users;
	}


	public TIntObjectHashMap<TIntHashSet> getItemAttributes() {
		return itemAttributes;
	}


	public TIntHashSet getItems() {
		return items;
	}


	public int getBiggestItem() {
		return biggestItem;
	}


	public int getFeatureNum() {
		return featureNum;
	}

	public int getBiggestUser() {
		return biggestUser;
	}
}
