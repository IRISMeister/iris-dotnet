#!/bin/bash
cp -fR /source/ADO/* ./ADO
cp -fR /source/ODBC/* ./ODBC
cp /source/dotnet60/ADO.csproj ./ADO/ADO.csproj
cp /source/dotnet60/ODBC.csproj ./ODBC/ODBC.csproj
dotnet restore ADO/ADO.csproj
dotnet publish ADO/ADO.csproj -c release -o /app
dotnet restore ODBC/ODBC.csproj
dotnet publish ODBC/ODBC.csproj -c release -o /app

echo "dotnet /app/ADO.dll to run"

cp -fR /app/* /source/app/