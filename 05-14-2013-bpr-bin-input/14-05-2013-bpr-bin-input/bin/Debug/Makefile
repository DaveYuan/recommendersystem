all:
	gmcs -r:protobuf-net.dll ../../Main.cs ../../protobuf-classes.cs>/dev/null
	mv ../../Main.exe .
	mono --debug Main.exe
