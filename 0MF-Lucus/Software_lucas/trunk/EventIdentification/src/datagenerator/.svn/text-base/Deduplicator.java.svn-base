package datagenerator;

import java.io.BufferedReader;
import java.io.BufferedWriter;
import java.io.FileReader;
import java.io.FileWriter;
import java.io.IOException;
import java.util.StringTokenizer;

import gnu.trove.TIntHashSet;
import gnu.trove.TIntObjectHashMap;

public class Deduplicator {
	TIntObjectHashMap<TIntHashSet> pairs;
	TIntHashSet objects;
	
	public void loadDataTime(String fileName, String dedup){
		try {
			pairs = new TIntObjectHashMap<TIntHashSet>();
			
			objects = new TIntHashSet();
			
			BufferedReader in = new BufferedReader(new FileReader(fileName));
			BufferedWriter out = new BufferedWriter(new FileWriter(dedup));
						
			String line = null;
			int lineNum = 0;
		
			while((line = in.readLine())!=null){
				StringTokenizer st = new StringTokenizer(line," ");
				int p1 = Integer.parseInt(st.nextToken());
				int p2 = Integer.parseInt(st.nextToken());
				if(!objects.contains(p1)){
					objects.add(p1);
					if(p1 >= 497628) lineNum++;
				}
				if(!objects.contains(p2)){
					objects.add(p2);
					if(p2 >= 497628) lineNum++;
				}
				if(p1 == p2) continue;
				if(!pairs.containsKey(p1) && !pairs.containsKey(p2)){
					pairs.put(p1, new TIntHashSet());
					pairs.get(p1).add(p2);
					out.write(p1 + " " + p2 + "\n");
					continue;
				}
				if(pairs.containsKey(p1)){
					if(!pairs.get(p1).contains(p2)){
						pairs.get(p1).add(p2);
						out.write(p1 + " " + p2 + "\n");
					}
					continue;
				}
				if(pairs.containsKey(p2)){
					if(!pairs.get(p2).contains(p1)){
						pairs.get(p2).add(p1);
						out.write(p2 + " " + p1 + "\n");
					}
					continue;
				}
				
			}
			System.out.println(objects.size());
			System.out.println(lineNum);

			out.close();
			in.close();
		} catch (IOException e) {
			e.printStackTrace();
		}
	}
}
