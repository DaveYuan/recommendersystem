#!/bin/sh 

#set xlabel "λ(social)"
#set ylabel "Root mean squared error"

rm -rf regSocialLog.txt

touch regSocialLog.txt

for file in ./regSocialvsRMSE/*
do
	echo "`awk -F";" 'NR==2{print $4}' $file` `awk -F";" '{print $12}' $file | sort | head -1`" >> regSocialLog.txt
	    #whatever you need with "$file"
done
