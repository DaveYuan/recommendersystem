all:
	/cm/shared/apps/pc2/MONO/3.0.7/bin/mcs -r:protobuf-net.dll ../../Main.cs ../../Eval.cs ../../Init.cs ../../PredictRecall.cs ../../Protobufnet.cs
	mv ../../Main.exe .
	/cm/shared/apps/pc2/MONO/3.0.7/bin/mono --debug Main.exe
