import java.io.BufferedReader;
import java.io.BufferedWriter;
import java.io.FileReader;
import java.io.FileWriter;
import java.io.IOException;
import java.util.StringTokenizer;


public class GenerateNegativePairs {

	/**
	 * @param args
	 */
	public static void main(String[] args) {
		try {
			BufferedReader in = new BufferedReader(new FileReader(args[0]));
			BufferedWriter pos = new BufferedWriter(new FileWriter(args[1]));
			BufferedWriter neg = new BufferedWriter(new FileWriter(args[2]));
						
			
			String line = "";
			
			while((line=in.readLine()) != null){
				if(line.startsWith("picture_id")){
					continue;
				}
				StringTokenizer st = new StringTokenizer(line,",");
				String id1 = st.nextToken();
				String id2 = st.nextToken();
				String tags1 = st.nextToken();
				String tags2 = st.nextToken();
				
				st = new StringTokenizer(tags1, " ");
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
					neg.write(id1 + " " + id2 + "\n");
				}else{
					pos.write(id1 + " " + id2 + "\n");
				}
			}
			
			in.close();
			pos.close();
			neg.close();
		} catch (IOException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}
		
		

	}

}
