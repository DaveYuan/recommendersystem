package datagenerator;


import gnu.trove.TIntHashSet;
import gnu.trove.TIntIntHashMap;
import gnu.trove.TIntObjectHashMap;

import java.io.BufferedReader;
import java.io.FileReader;
import java.io.IOException;
import java.util.StringTokenizer;

import utils.SparseArray;

public class DataInput {
	public static final int ID = 0;
	public static final int PICID = 1;
	public static final int DATETAKEN = 2;
	/*------------------------------------------------------------------*/
	public static final int LATITUDE = 0;
	public static final int LONGITUDE = 1;
	int[][] int_data;
	double[][] double_data;
	
	String[] tags, owners;
	
	TIntObjectHashMap<SparseArray> vectors;
	

	public void loadDataTime(String fileName){
		try {
			BufferedReader in = new BufferedReader(new FileReader(fileName));
			in.readLine();
			int size = 0;
			while(in.readLine()!=null){
				size++;
			}
			in.close();
			int_data = new int[size][3];
			tags = new String[size];
			
			in = new BufferedReader(new FileReader(fileName));
			in.readLine();
			
			String line = null;
			int lineNum = 0;
			while((line = in.readLine())!=null){
				StringTokenizer st = new StringTokenizer(line,"\t");
				int_data[lineNum][ID] = Integer.parseInt(st.nextToken());
				int_data[lineNum][PICID] = Integer.parseInt(st.nextToken());
				int_data[lineNum][DATETAKEN] = Integer.parseInt(st.nextToken());
				tags[lineNum] = st.nextToken();
				lineNum++;
			}
			
			in.close();
		} catch (IOException e) {
			e.printStackTrace();
		}
	}
	
	public void loadDataLocation(String fileName){
		try {
			BufferedReader in = new BufferedReader(new FileReader(fileName));
			in.readLine();
			int size = 0;
			while(in.readLine()!=null){
				size++;
			}
			in.close();
			double_data = new double[size][2];
			
			owners = new String[size];
			
			in = new BufferedReader(new FileReader(fileName));
			in.readLine();
			
			String line = null;
			int lineNum = 0;
			while((line = in.readLine())!=null){
				StringTokenizer st = new StringTokenizer(line,"\t");
					
				st.nextToken(); //id
				st.nextToken(); //picture_id
				st.nextToken(); //flickr_picture_id
				
				owners[lineNum] = st.nextToken(); // owner
				
				//secret,tags,machine_tags,title
				for(int i = 0; i < 4; i++){
					st.nextToken();
				}

				double_data[lineNum][LATITUDE] = Double.parseDouble(st.nextToken());   //latitude
				double_data[lineNum][LONGITUDE] = Double.parseDouble(st.nextToken());  //longitude
				
				lineNum++;
			}
			
			in.close();
		} catch (IOException e) {
			e.printStackTrace();
		}
	}
	
	
	public void loadTextualData(String fileName){
		try {
			BufferedReader in = new BufferedReader(new FileReader(fileName));
			in.readLine();
			
			vectors = new TIntObjectHashMap<SparseArray>();
			
			String line = null;
			int lineNum = 0;
			while((line = in.readLine())!=null){
				StringTokenizer st = new StringTokenizer(line,"\t");
				
				int picId = Integer.parseInt(st.nextToken());
				int tagId = Integer.parseInt(st.nextToken());
				double tagTfIdf = Double.parseDouble(st.nextToken());
				
				if(!vectors.containsKey(picId)){
					vectors.put(picId, new SparseArray());
				}
				
				vectors.get(picId).set(tagId, tagTfIdf);
				
				lineNum++;
			}
			
			in.close();
		} catch (IOException e) {
			e.printStackTrace();
		}
	}
	
	
	public int loadTextualData(String fileName, int min, int max){
		TIntHashSet selected = new TIntHashSet();
		for(int i = min; i <= max; i++){
			selected.add(int_data[i][PICID]);
		}
		TIntIntHashMap tagIds = new TIntIntHashMap();
		
		try {
			BufferedReader in = new BufferedReader(new FileReader(fileName));
			in.readLine();
			
			vectors = new TIntObjectHashMap<SparseArray>();
			
			String line = null;
			int lineNum = 0;
			while((line = in.readLine())!=null){
				StringTokenizer st = new StringTokenizer(line,"\t");
				
				int picId = Integer.parseInt(st.nextToken());
				int tagId = Integer.parseInt(st.nextToken());
				double tagTfIdf = Double.parseDouble(st.nextToken());
				if(!tagIds.contains(tagId)){
					tagIds.put(tagId,tagIds.size());
				}
				if(selected.contains(picId)){
					if(!vectors.containsKey(picId)){
						vectors.put(picId, new SparseArray());
					}
				
					vectors.get(picId).set(tagIds.get(tagId), tagTfIdf);
				}
				lineNum++;
			}
			
			in.close();
		} catch (IOException e) {
			e.printStackTrace();
		}
		return tagIds.size();
	}
	

	public int[][] getData_time() {
		return int_data;
	}

	public void setData_time(int[][] dataTime) {
		int_data = dataTime;
	}

	public double[][] getData_location() {
		return double_data;
	}

	public void setData_location(double[][] dataLocation) {
		double_data = dataLocation;
	}

	public String[] getTags() {
		return tags;
	}

	public void setTags(String[] tags) {
		this.tags = tags;
	}
	
	
	public String[] getOwners() {
		return owners;
	}

	public void setOwners(String[] owners) {
		this.owners = owners;
	}

	public TIntObjectHashMap<SparseArray> getVectors() {
		return vectors;
	}

	public void setVectors(TIntObjectHashMap<SparseArray> vectors) {
		this.vectors = vectors;
	}

}
