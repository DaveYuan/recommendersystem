#Remove timestamp
cut -f-1,2,3 u1.test > _u1.test && mv _u1.test u1.test
cut -f-1,2,3 u1.base > _u1.base && mv _u1.base u1.base

#create test and probe set
awk '$3 != "5" {print $1"\t"$2"\t"$3}' u1.test > _u1.probe && mv _u1.probe u1.probe
awk '$3 == "5" {print $1"\t"$2"\t"$3}' u1.test > _u1.test && mv _u1.test u1.test

