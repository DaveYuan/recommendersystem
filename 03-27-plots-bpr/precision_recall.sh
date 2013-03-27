#!/usr/bin/gnuplot -persist

set xlabel "recall"
set ylabel "Precision(recall)"

set title "Precision Plot. ML-100K"
set ytics 0,0.01

plot "precision_per_recall.txt" u 1:2 title 'Precision' w points
