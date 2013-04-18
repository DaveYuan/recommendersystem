#!/usr/bin/gnuplot -persist

set style line 3 lc rgb 'black' pt 9

#show style line

set xlabel "N"
set ylabel "Recall(N)"

set title "Recall Plot. ML-100K"
set ytics 0,0.010

plot "recall.txt" u 1:2 title 'BPR Learning' w points
#"matrix-fact-bias-user-item.txt" u 1:2 title 'MAP regularization + bias learning matrix factorization' w lp, \
#"simon-funk.txt" u 1:2 title 'MAP regularization Simon Funk' w lp
