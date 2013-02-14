#!/bin/bash

# Path of training set
FILES=/home/whitepearl/upb/Program/02-04-2013-netflix-data-manipulation/files/*

rm -rf training_set.txt

for f in $FILES

do
	line_cnt=0

  	cat $f | while read LINE
	do
		let line_cnt++;
		if [ "$line_cnt" == 1 ]; then
			MOVIEID=`echo "$LINE" | awk -F":" '{print $1}'`
		else
			echo "$LINE" | awk -F"," '{print $1","'$MOVIEID'","$2","$3}' >> training_set.txt
		fi
	done
done
