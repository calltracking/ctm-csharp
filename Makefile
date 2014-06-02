CC=mcs -debug
sources= lib/config.cs lib/request.cs lib/auth_token.cs lib/accounts.cs lib/page.cs lib/numbers.cs lib/source.cs lib/receiving_numbers.cs

libs=	-r:System.dll \
	-r:System.Data.dll \
	-r:Newtonsoft.Json.dll \
	-r:System.ServiceModel.Web.dll \
	-r:System.Web.dll \
	-r:System.Web.Services.dll \
	-r:System.Runtime.Serialization.dll

all: ctm.dll example.exe

ctm.dll: $(sources)
	$(CC) -target:library -debug --stacktrace -out:ctm.dll $(sources) $(libs)
example.exe:
	$(CC) example.cs -lib:`pwd` -debug --stacktrace $(libs) -r:ctm.dll -r:NUnit.Framework.dll
clean:
	rm -f example.exe example.exe.mdb ctm.dll ctm.dll.mdb
