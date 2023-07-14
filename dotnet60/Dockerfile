FROM containers.intersystems.com/intersystems/iris-community:2023.1.1.380.0 AS IRIS
FROM mcr.microsoft.com/dotnet/sdk:6.0-bookworm-slim

RUN apt -y update \
 && DEBIAN_FRONTEND=noninteractive apt -y install odbcinst unixodbc \
 && apt clean

ENV DOTNET_CLI_TELEMETRY_OPTOUT 1
ENV ISC_PACKAGE_INSTALLDIR /usr/irissys
ENV LIBDIR lib
ENV ODBCDIR odbcdriver
ENV ISC_LIBDIR ${ISC_PACKAGE_INSTALLDIR}/dev/dotnet/bin/
ENV ISC_ODBC ${ISC_PACKAGE_INSTALLDIR}/dev/odbc/
WORKDIR /DOTNET
RUN mkdir ADO ODBC
# ADO.NET files
COPY --from=IRIS $ISC_LIBDIR/*.nupkg ./ADO/$LIBDIR/
# ODBC Driver files
COPY --from=IRIS /usr/irissys/bin/libirisod* ./ODBC/$ODBCDIR/
COPY odbcinst.ini .
COPY odbc.ini .
RUN odbcinst -i -d -l -f ./odbcinst.ini && odbcinst -i -s -l -f ./odbc.ini
