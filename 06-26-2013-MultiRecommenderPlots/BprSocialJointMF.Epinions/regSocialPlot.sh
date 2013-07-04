#!/usr/bin/gnuplot -persist

set xlabel "λ(Social)"
set ylabel "Root mean squared error"

set title "BprSocialJointMF(Epinions(660k))"

#set term jpeg
#set output "BprSocialJointMF.jpeg"

set xrange [-0.005:1] 
set yrange [0.9:1.12] 
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

plot './log.txt' u 1:2 title 'Epinions' with lp lt 4 pt 5
