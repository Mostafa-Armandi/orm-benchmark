# orm-benchmark

Use docker compose to test the result (e.g):
```
docker-compose up --attach
```


Given result with the default setting:
```
                       Method | Rows |       Mean |     Error |    StdDev |   StdErr |      Min |        Max |     Median |
----------------------------- |----- |-----------:|----------:|----------:|---------:|---------:|-----------:|-----------:|
 EfCoreQueryWithASimpleClause |  100 |   879.6 us | 108.86 us | 320.97 us | 32.10 us | 459.2 us | 1,750.3 us |   800.7 us |
                 EfCoreInsert |  100 | 1,362.1 us | 142.22 us | 419.35 us | 41.94 us | 827.9 us | 2,342.6 us | 1,271.8 us |
 DapperQueryWithASimpleClause |  100 |   184.2 us |   2.03 us |   1.69 us |  0.47 us | 181.3 us |   187.0 us |   184.0 us |
                 DapperInsert |  100 |   327.0 us |   6.41 us |   6.58 us |  1.60 us | 317.6 us |   342.2 us |   324.5 us |
```    
             
