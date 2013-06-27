#!/usr/bin/gnuplot -persist

set xlabel "Epoch"
set ylabel "Root mean squared error"

set title "MatrixFactorizationBiasReg(LastFM-93k)"

set datafile sep ';'
set autoscale

# pt 1: +
# pt 2: x 
# pt 3: *
# pt 4: empty sq box
# pt 5: filled sq box
# pt 6: empty circl 
# pt 7: filled circle
# pt 8: empty triangle
# pt 9: filled triangle

plot './log/MatrixFactorizationBiasReg.csv' every ::2 u 1:9 title 'test' with lp lt 4 pt 7, \
'./log/MatrixFactorizationBiasReg.csv' every ::2 u 1:8 title 'train' with lp lt 3 pt 4
