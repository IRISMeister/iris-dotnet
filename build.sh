#!/bin/bash
docker compose --progress plain build
docker compose up -d
docker compose exec dotnet-dev /source/dotnet60/build.sh

docker compose exec dotnet-run bash -c "cat ./isql.txt | isql irisdatasource"