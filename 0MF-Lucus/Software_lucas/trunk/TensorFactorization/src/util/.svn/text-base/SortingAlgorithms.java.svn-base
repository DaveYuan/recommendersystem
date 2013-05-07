package util;

import java.util.Collections;
import java.util.Comparator;
import java.util.Iterator;
import java.util.LinkedList;
import java.util.List;
import java.util.Map;

public class SortingAlgorithms {
	public static LinkedList<Integer> sortByValue(Map map) {
        List<Integer> list = new LinkedList(map.entrySet());
        Collections.sort(list, new Comparator() {

            public int compare(Object o1, Object o2) {
                return -((Comparable) ((Map.Entry) (o1)).getValue())
              .compareTo(((Map.Entry) (o2)).getValue());
            }
        });
// logger.info(list);
        LinkedList<Integer> result = new LinkedList<Integer>();
        for (Iterator it = list.iterator(); it.hasNext();) {
            Map.Entry entry = (Map.Entry) it.next();
            result.add((Integer) entry.getKey());
        }
        return result;
    }
}
