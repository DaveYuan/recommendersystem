all:
	/cm/shared/apps/pc2/MONO/3.0.7/bin/mcs -r:protobuf-net.dll ../../Main.cs ../../protobuf-classes.cs
	mv ../../Main.exe .
	/cm/shared/apps/pc2/MONO/3.0.7/bin/mono --debug Main.exe
