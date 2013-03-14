#!/usr/bin/gnuplot -persist

set style line 1 lt 2 lc rgb "blue" lw 3

show style line

set xlabel "N"
set ylabel "Recall"

set title "Benchmarks"
set ytics 0,0.01

plot "recall.txt" u 1:2 title 'Recall plot(Movie lens)' w lp
