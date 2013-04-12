#!/bin/bash

NONEXSTUSR=$1
LINKSFILE=$2

cat $NONEXSTUSR | while read LINE
do
	sed -i "/^"$LINE"\t/d" "$LINKSFILE" 
	sed -i "/\t"$LINE"$/d" "$LINKSFILE"
done
