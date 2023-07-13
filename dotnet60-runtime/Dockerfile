FROM mcr.microsoft.com/dotnet/runtime:6.0-bookworm-slim AS build

RUN apt -y update \
 && DEBIAN_FRONTEND=noninteractive apt -y install odbcinst unixodbc \
 && apt clean

ENV DOTNET_CLI_TELEMETRY_OPTOUT 1
ENV ISC_PACKAGE_INSTALLDIR /usr/irissys
ENV LIBDIR lib
ENV ODBCDIR odbcdriver
ENV ISC_LIBDIR ${ISC_PACKAGE_INSTALLDIR}/dev/dotnet/bin/
ENV ISC_ODBC ${ISC_PACKAGE_INSTALLDIR}/dev/odbc/
WORKDIR /ADO
# ADO.NET files
COPY --from=containers.intersystems.com/intersystems/iris-community:2023.1.0.235.1 $ISC_LIBDIR/*.nupkg $LIBDIR/
# ODBC Driver files
COPY --from=containers.intersystems.com/intersystems/iris-community:2023.1.0.235.1 /usr/irissys/bin/libirisodbcur6435.so $ODBCDIR/libirisodbcur6435.so
COPY odbcinst.ini .
COPY odbc.ini .
RUN odbcinst -i -d -l -f ./odbcinst.ini && odbcinst -i -s -l -f ./odbc.ini