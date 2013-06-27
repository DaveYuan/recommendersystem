#!/usr/bin/gnuplot -persist

set xlabel "Epoch"
set ylabel "Root mean squared error"

set title "Plots overlay(LastFM-93k:Test)"

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

plot './log/MatrixFactorization.csv' every ::2 u 1:5 title 'MatrixFactorization' with lp lt 9 pt 6, \
'./log/MatrixFactorizationBiasReg.csv' every ::2 u 1:9 title 'MatrixFactorizationBiasReg' with lp lt 1 pt 7, \
'./log/SocialMF.csv' every ::2 u 1:10 title 'SocialMF' with lp lt 2 pt 5, \
'./log/BprSocialJointMF.csv' every ::2 u 1:12 title 'BprSocialJointMF' with lp lt 3 lw 2 pt 9
