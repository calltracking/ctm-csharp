CC=mcs
sources=config.cs request.cs auth_token.cs source.cs numbers.cs
libs=-r:System.dll -r:System.Data.dll -r:System.Json.dll \
     -r:System.ServiceModel.Web.dll -r:System.Web.dll \
		 -r:System.Web.Services.dll -r:System.Runtime.Serialization.dll

all: ctm.dll test

ctm.dll: $(sources)
	$(CC) -target:library -out:ctm.dll $(sources) $(libs)
test:
	$(CC) test.cs -lib:`pwd` -debug --stacktrace $(libs) -r:ctm.dll
	mono test.exe
clean:
	rm -f test.exe test.exe.mdb ctm.dll
