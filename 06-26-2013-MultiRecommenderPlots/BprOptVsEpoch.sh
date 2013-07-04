#!/usr/bin/gnuplot -persist

set xlabel "Epoch"
set ylabel "Bayesian Personalised Ranking Optimization Criteria(BPR-OPT)"

set title "BprSocialJointMF(LastFM-93k)"

#set term jpeg
#set output "BprSocialJointMF.jpeg"

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

plot './log/BprSocialJointMF.csv' every ::2 u 1:11 title 'BPR-OPT' with lp lt 4 pt 7
