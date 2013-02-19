#!/usr/bin/gnuplot -persist

set style line 1 lt 2 lc rgb "blue" lw 3

show style line

set xlabel "Epoch"
set ylabel "RMSE"

set title "Benchmarks"
set ytics 0,0.01

plot "train_error.txt" u 1:2 title 'Train Error' w lp, \
"test_error.txt" u 1:2 title 'Test Error' w lp 
