#!/usr/bin/gnuplot -persist

set xlabel "N"
set ylabel "Precision(N)"

set title "Precision Plot. ML-100K"
set ytics 0,0.0010

plot "precision_per_n.txt" u 1:2 title 'Precision' w points 