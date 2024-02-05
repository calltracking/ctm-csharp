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

NUNIT_PATH=$(shell find ./packages -name 'nunit.framework.dll' -print | grep net462)

test:
	$(CC) test.cs -lib:`pwd`,`pwd`/$(NUNIT_PATH) -debug --stacktrace $(libs) -r:ctm.dll -r:$(NUNIT_PATH)
#	mono test.exe

http_server:
	$(CC) examples/report_server.cs examples/http_server/http_request.cs examples/http_server/http_server.cs -lib:`pwd` -debug --stacktrace $(libs) -r:ctm.dll #-r:NUnit.Framework.dll

webhook_server:
	$(CC) examples/webhook_server.cs -lib:`pwd` -debug --stacktrace $(libs) -r:ctm.dll #-r:NUnit.Framework.dll

buy_number:
	$(CC) examples/buy_number.cs -lib:`pwd` -debug --stacktrace $(libs) -r:ctm.dll -r:NUnit.Framework.dll

setup-mac:
	brew install mono dotnet nuget
	nuget install NUnit -Source https://api.nuget.org/v3/index.json

clean:
	rm -f example.exe example.exe.mdb ctm.dll ctm.dll.mdb
