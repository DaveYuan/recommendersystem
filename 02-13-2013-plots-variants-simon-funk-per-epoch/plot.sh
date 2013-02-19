#!/usr/bin/gnuplot -persist

set style line 1 lt 2 lc rgb "red" lw 3
set style line 2 lt 2 lc rgb "green" lw 2
set style line 2 lt 2 lc rgb "blue" lw 2
set style line 2 lt 2 lc rgb "black" lw 2
set style line 2 lt 2 lc rgb "brown" lw 2
set style line 2 lt 2 lc rgb "black" lw 2

show style line

set xlabel "Epoch"
set ylabel "RMSE"

set title "Simon Funk Variants, #Training Features: 50"
set ytics 0,0.010

plot "matrix-fact-glb-avg.txt" u 1:2 title 'Global Average' w lp, \
"matrix-fact-basic.txt" u 1:2 title 'Basic matrix factorizaton' w lp , \
"matrix-fact-bias-learning.txt" u 1:2 title 'Bias learning without MAP' w lp , \
"matrix-fact-bias-learning-map.txt" u 1:2 title 'Bias learning with MAP' w lp , \
"matrix-fact-bias-user-item.txt" u 1:2 title 'MAP regularization + bias learning matrix factorization' w lp, \
"simon-funk.txt" u 1:2 title 'MAP regularization Simon Funk' w lp
