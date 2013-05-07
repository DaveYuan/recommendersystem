package datagenerator;

import gnu.trove.TIntHashSet;
import gnu.trove.TIntIntHashMap;
import gnu.trove.TIntObjectHashMap;

import java.io.BufferedReader;
import java.io.BufferedWriter;
import java.io.FileReader;
import java.io.FileWriter;
import java.io.IOException;
import java.util.StringTokenizer;

public class Generator {

	public static final int ID = 0;
	public static final int PICID = 1;
	public static final int DATETAKEN = 2;
	public static int[][] data_time;	
	private static TIntHashSet objects;

	/**
	 * @param args
	 */
	public static void main(String[] args) {
		try {
			
			
			
			loadDataTime("./data/lastfm-s2.txt");
			TIntIntHashMap s2 = new TIntIntHashMap();
			System.out.println("ha");
			for(int i = 0; i < data_time.length; i++){
				s2.put(data_time[i][ID],data_time[i][PICID]);
			}
			
			TIntHashSet existing = new TIntHashSet();
			
//			String fileName = "./data/lastfm-positive.txt";
//			String fileName = "./data/newPos.txt";
			String fileName = "./data/lastfm-pairs-s2-s1.txt";
			BufferedReader in = new BufferedReader(new FileReader(fileName));
									
			String line = null;			
			TIntHashSet s2values = new TIntHashSet(s2.getValues());
			System.out.println("ha");
			while((line = in.readLine())!=null){
				StringTokenizer st = new StringTokenizer(line," ");
				int p1 = Integer.parseInt(st.nextToken());
				int p2 = Integer.parseInt(st.nextToken());
				if(!existing.contains(p1) && s2values.contains(p1)){
					existing.add(p1);
				}
//				if(!existing.contains(p2) /*&& s2values.contains(p2)*/){
//					existing.add(p2);
//				}
			
			}
			
			in.close();
			System.out.println((995255-497628));
			System.out.println("Size of existing: " + existing.size());
	
			objects = new TIntHashSet();
			
			for(int id : s2.keys()){
				if(!existing.contains(s2.get(id))){
					objects.add(id);
				}
			}
			
			System.out.println("Size of objects: " + objects.size());
			generatePairs();
			
		} catch (IOException e) {
			e.printStackTrace();
		}

	}
	
	public static int[][] data;
	public static String[] tags;
	
	public static void generatePairs() throws IOException{
		
		
		DataInput in = new DataInput();
		in.loadDataTime("/home/lucas/workspace/EventIdentification/data/lastfm-datetaken.txt");
		data = in.int_data;
		tags = in.tags;

		String file = "./data/newPos.txt";			
		BufferedWriter out = new BufferedWriter(new FileWriter(file));
		
		/*
		 * LastFm
		 * 1 - 497.628 (S1)
		 * 497.629 - 995.255 (S2)
		 * 995.256 - 1.492.883 (S3)
		 * 
		 * Upcoming
		 * 1 - 116.665 (S1)
		 * 116.666 - 233.331 (S2)
		 * 233.332 - 349.996 (S3)
		 */
		System.out.println("Size of objects: " + objects.size());
		int count = 0;
		for(int i : objects.toArray()){
			count++;
			if(count%1000 == 0){
				System.out.println("." + count);
			}
			
			int attempts = 0;
			int numPairs = 0;
			for(int p = 0; p < 5; p++){
				int b = (int) Math.floor(Math.random()*995254);
																
				if(timeSimilarity(i, b) < 0.94 || !sameEvent(tags[i], tags[b]) /*|| 
						(pos.containsKey(p1) && pos.get(p1).contains(p2)) || (pos.containsKey(p2) && pos.get(p2).contains(p1))*/){
					if(attempts < 500000 /*|| numPairs < 1*/){
						p--;
						attempts++;
					}
					continue;
				}
				
				numPairs++;				
				out.write(data[i][1] + " " + data[b][1] + "\n");
			}
		}
				
		out.close();
				
	}
	
	
	public static double timeSimilarity(int picA, int picB){
		return 1.0 - 
			Math.abs(
			   data[picA][DataInput.DATETAKEN] - data[picB][DataInput.DATETAKEN])/31536000.0;
		
	}
	
	public static boolean sameEvent(String tags1, String tags2){
		
		StringTokenizer st = new StringTokenizer(tags1, " ");
		String ev1 = "-1";
		while(st.hasMoreTokens()){
			String tmp = st.nextToken();
			if(tmp.startsWith("lastfm:event=")){
				ev1 = tmp.substring(13);
				break;
			}
		}
		
		st = new StringTokenizer(tags2, " ");
		String ev2 = "-2";
		while(st.hasMoreTokens()){
			String tmp = st.nextToken();
			if(tmp.startsWith("lastfm:event=")){
				ev2 = tmp.substring(13);
				break;
			}
		}
		
		if(!ev1.equals(ev2)){					
			return false;
		}else{
			return true;
		}
	}
	
	
	public static void loadDataTime(String fileName){
		try {
			BufferedReader in = new BufferedReader(new FileReader(fileName));
			in.readLine();
			int size = 0;
			while(in.readLine()!=null){
				size++;
			}
			in.close();
			data_time = new int[size][2];
			in = new BufferedReader(new FileReader(fileName));
			in.readLine();
			
			String line = null;
			int lineNum = 0;
			while((line = in.readLine())!=null){
				StringTokenizer st = new StringTokenizer(line,",");
				data_time[lineNum][ID] = Integer.parseInt(st.nextToken());
				data_time[lineNum][PICID] = Integer.parseInt(st.nextToken());
				
				lineNum++;
			}
			
			in.close();
		} catch (IOException e) {
			e.printStackTrace();
		}
	}

}
