#!/usr/bin/gnuplot -persist

set xlabel "λ(Social)"
set ylabel "Root mean squared error"

set title "BprSocialJointMF(LastFM-93k)"

#set term jpeg
#set output "BprSocialJointMF.jpeg"

set xrange [-0.1:1.2] 
set yrange [0.8:0.95] 
#set autoscale

# pt 1: +
# pt 2: x 
# pt 3: *
# pt 4: empty sq box
# pt 5: filled sq box
# pt 6: empty circl 
# pt 7: filled circle
# pt 8: empty triangle
# pt 9: filled triangle

plot './regSocialLog.txt' u 1:2 title 'Last.fm' with lp lt 3 pt 9
