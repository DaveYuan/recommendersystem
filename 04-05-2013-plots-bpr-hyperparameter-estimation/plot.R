data_table <- read.table("~/upb/Program/04-05-2013-plots-bpr-hyperparameter-estimation/recall.txt", header=T, sep=" ")
plot3d(data_table, xlab="Epoch", ylab="K", zlab="Recall")
  