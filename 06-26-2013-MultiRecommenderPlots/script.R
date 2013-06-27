read.csv(file.choose(), sep=";") -> data1
read.csv(file.choose(), sep=";") -> data2
plot(data1$itr, data1$RMSE.T., type="l", main="BPRSocialJointMF v/s SocialMF",col="blue", xlab="Epoch", ylab="RMSE",ylim=range(data1$RMSE.T., data2$RMSE.T.))
lines(data2$X.itr, data2$RMSE.T., col="red")
