# ADO.NET VS ODBC.NET

投入データが全てオンメモリに収まる前提でのパフォーマンス比較。

# 環境

IRIS データベースバッファサイズ: 4192 MB  

コンテナ起動時にTESTデータベースを5GBで作成。

mem_limitによりdotnetコンテナへの割り当てを1GBに設定している。  
VM使用時はメモリ割り当てを最低でも8GB、wsl2使用時は.wslconfigにてメモリ設定を行うことが望ましい。
```
[wsl2]
memory=8GB
processors=4
swap=0
```

```
$ docker stats
CONTAINER ID   NAME                       CPU %     MEM USAGE / LIMIT   MEM %     NET I/O          BLOCK I/O   PIDS
94ff1ed830dd   iris-dotnet-iris-1         0.05%     634.7MiB / 5GiB     12.40%    9.93kB / 6.2kB   0B / 0B     58
a732145b7c76   iris-dotnet-dotnet-dev-1   0.00%     788KiB / 1GiB       0.08%     1.36kB / 0B      0B / 0B     1
6b47edecc790   iris-dotnet-dotnet-run-1   0.00%     800KiB / 1GiB       0.08%     1.28kB / 0B      0B / 0B     1```

ODBCドライバで使用しているGLIBCのバージョンに合わせて、dotnetランタイム環境にはDebian/bookworm版(dotnet/runtime:6.0-bookworm)を使用。

bookworm以前のリリースではisql実行時に(不親切な)エラーメッセージがでる。
(dotnet/sdk:6.0はbullseyeなので、このエラーが出る。ビルドは可能だが実行は出来ない。)

```
root@e5e029312b24:/ADO# isql irisdatasource -v
[01000][unixODBC][Driver Manager]Can't open lib '/ADO/ODBC/odbcdriver/libirisodbcur6435.so' : file not found
[ISQL]ERROR: Could not SQLConnect
```

原因は、ライブラリが存在しないこと。
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

# dotnet+odbc での事前作業(疎通確認を兼ねる)
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

# プロジェクトのビルド
プロジェクトファイル一式をvolume bind mountしているので、コンテナ起動後も修正&再ビルドが可能。
```
$ docker compose exec dotnet-dev /source/dotnet60/build.sh
```

# 実行
```
$ docker compose exec dotnet-run dotnet /source/app/ODBC.dll
$ docker compose exec dotnet-run dotnet /source/app/ADO.dll
```

実行するたびに、テーブルを空にすること。
```
SQL> truncate table DWH.ODBC
SQL> truncate table DWH.LOGS
```