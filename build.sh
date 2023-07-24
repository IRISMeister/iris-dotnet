#!/bin/bash
docker compose build --progress plain
docker compose up -d
docker compose exec dotnet-dev /source/dotnet60/build.sh
