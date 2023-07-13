FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build

ENV DOTNET_CLI_TELEMETRY_OPTOUT 1
ENV ISC_PACKAGE_INSTALLDIR /usr/irissys
ENV LIBDIR lib
ENV ISC_LIBDIR ${ISC_PACKAGE_INSTALLDIR}/dev/dotnet/bin/
WORKDIR /ADO
COPY --from=containers.intersystems.com/intersystems/iris-community:2023.1.0.235.1 $ISC_LIBDIR/*.nupkg $LIBDIR/
