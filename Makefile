CC=mcs
sources=  lib/config.cs lib/request.cs lib/auth_token.cs lib/accounts.cs lib/page.cs lib/numbers.cs lib/source.cs  lib/report.cs
converted=lib/config.cs lib/request.cs lib/auth_token.cs lib/accounts.cs lib/page.cs lib/numbers.cs

libs=	-r:System.dll \
	-r:System.Data.dll \
	-r:Newtonsoft.Json.dll \
	-r:System.ServiceModel.Web.dll \
	-r:System.Web.dll \
	-r:System.Web.Services.dll \
	-r:System.Runtime.Serialization.dll

default: stub test

stub: $(converted)
	$(CC) -target:library -debug --stacktrace -out:ctm.dll $(converted) $(libs)

all: ctm.dll test http_server

ctm.dll: $(sources)
	$(CC) -target:library -debug --stacktrace -out:ctm.dll $(sources) $(libs)
test:
	$(CC) test.cs -lib:`pwd` -debug --stacktrace $(libs) -r:ctm.dll -r:NUnit.Framework.dll
#	mono test.exe
http_server:
	$(CC) examples/report_server.cs examples/http_server/http_request.cs examples/http_server/http_server.cs -lib:`pwd` -debug --stacktrace $(libs) -r:ctm.dll -r:NUnit.Framework.dll
buy_number:
	$(CC) examples/buy_number.cs -lib:`pwd` -debug --stacktrace $(libs) -r:ctm.dll -r:NUnit.Framework.dll
clean:
	rm -f test.exe test.exe.mdb ctm.dll
