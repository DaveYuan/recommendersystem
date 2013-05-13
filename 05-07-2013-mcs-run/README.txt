In order to run a c# script from command line:

gmcs -r:/home/whitepearl/upb/Program/04-15-2013-protobufnet-dll/protobuf-net.dll -pkg:dotnet Main.cs protobufnet-class.cs

Can remove -pkg:dotnet

Add protobuf-net.dll to the pwd
Main.exe created
chmod +x Main.exe
now run mono --debug Main.exe
