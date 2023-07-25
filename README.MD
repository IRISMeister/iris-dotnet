# ADO.NET VS ODBC.NET

# environment

By using CPF Merge file to,

- set global buffer size to 4GB.
- pre-expand TEST database where tables are populated.

By using doker-compose.yaml 

- Allocate appropriate mem_limit.
Be aware that it shouldn't exceed the memory allocation to your VM or WSL2(.wslconfig)

```
[wsl2]
memory=8GB
processors=4
swap=0
```

# How to start

```
$ ./build.sh
```
It will build container images and c# projects (./ADO/ADO.csproj and ./ODBC/ODBC.csproj)

After that you will see three containers.
```
$ docker stats
CONTAINER ID   NAME                       CPU %     MEM USAGE / LIMIT   MEM %     NET I/O           BLOCK I/O   PIDS
f9d5c5266b7e   iris-dotnet-iris-1         0.21%     582.5MiB / 5GiB     11.38%    9.9kB / 6.05kB    0B / 0B     58
c7f6ce6ae819   iris-dotnet-dotnet-run-1   0.00%     3.84MiB / 1GiB      0.37%     1.25kB / 0B       0B / 0B     1
b1ccab5116ef   iris-dotnet-dotnet-dev-1   0.00%     105MiB / 512MiB     20.51%    9.56MB / 82.8kB   0B / 0B     1
```

iris-dotnet-iris is an IRIS server.

iris-dotnet-dotnet-run is where your c# programs are excecuted.  

- C# ODBC client app talks to the IRIS by using dotnet60-runtime/odbcinst.ini and dotnet60-runtime/odbc.ini .

iris-dotnet-dotnet-dev is where you build and publish your c# projects.

# Steps to execute
## Create tables via isql

```
$ docker compose exec dotnet-run isql irisdatasource
+---------------------------------------+
| Connected!                            |
|                                       |
| sql-statement                         |
| help [tablename]                      |
| echo [string]                         |
| quit                                  |
|                                       |
+---------------------------------------+
SQL> CREATE TABLE DWH.ODBC (SENT_AT TIMESTAMP,TOPIC VARCHAR(256),RECEIVED_AT TIMESTAMP, BINARY  VARBINARY(1200) )
SQLRowCount returns 0
SQL> CREATE TABLE DWH.LOGS (SENT_AT TIMESTAMP,TOPIC VARCHAR(256),RECEIVED_AT TIMESTAMP, BINARY  VARBINARY(1200) )
SQLRowCount returns 0
SQL>
$
```

## Run each app

This will run ODBC/Program.cs
```
$ docker compose exec dotnet-run dotnet /source/app/ODBC.dll
```

This will run ADO/Program.cs
```
$ docker compose exec dotnet-run dotnet /source/app/ADO.dll
```

You may want to truncate them for each run. Otherwise database size will keep growing beyond 4GB and negatively affect the results.

```
$ docker compose exec dotnet-run isql irisdatasource
SQL> truncate table DWH.ODBC
SQL> truncate table DWH.LOGS
```

# rebuild your projects

You can manually rebuild projects by following command.(whole source tree is bind-mounted to dotnet-dev container by the name /source)

```
$ docker compose exec dotnet-dev /source/dotnet60/build.sh
```

# notes

UnixODBC driver requires GLIBC which is available in Debian/bookworm(dotnet/runtime:6.0-bookworm).

If you use older version before bookworm, you will see the error messages like this when you try to use it.

```
root@e5e029312b24:/ADO# isql irisdatasource -v
[01000][unixODBC][Driver Manager]Can't open lib '/ADO/ODBC/odbcdriver/libirisodbcur6435.so' : file not found
[ISQL]ERROR: Could not SQLConnect
```

```
root@2687e40170d5:/app# ldd /usr/irissys/bin/libirisodbcur6435.so
/usr/irissys/bin/libirisodbcur6435.so: /usr/lib/x86_64-linux-gnu/libstdc++.so.6: version `GLIBCXX_3.4.29' not found (required by /usr/irissys/bin/libirisodbcur6435.so)
/usr/irissys/bin/libirisodbcur6435.so: /lib/x86_64-linux-gnu/libc.so.6: version `GLIBC_2.34' not found (required by /usr/irissys/bin/libirisodbcur6435.so)
        linux-vdso.so.1 (0x00007ffff2f24000)
        libstdc++.so.6 => /usr/lib/x86_64-linux-gnu/libstdc++.so.6 (0x00007f9059405000)
        libc.so.6 => /lib/x86_64-linux-gnu/libc.so.6 (0x00007f9059245000)
        libgcc_s.so.1 => /lib/x86_64-linux-gnu/libgcc_s.so.1 (0x00007f905922b000)
        libm.so.6 => /lib/x86_64-linux-gnu/libm.so.6 (0x00007f90590a8000)
        /lib64/ld-linux-x86-64.so.2 (0x00007f9059634000)

```