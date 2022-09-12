# Summary  
This basic repo is created to benchmark EF Core 6 and Dapper to compare their performance.  
There are some [advanced performance topics](https://docs.microsoft.com/en-us/ef/core/performance/advanced-performance-topics?tabs=with-di%2Cwith-constant) for EF that are benchmarked here that is highly worth reading.   
This repo is highly influenced by the [existing benchmarks](https://github.com/dotnet/EntityFramework.Docs/tree/main/samples/core/Benchmarks) for EF.   
A kind of complex query is used to test query compiling time on EF and the compiled query from EF is used directly on Dapper benchmark.  

Considering the `Mean` column, both EF and Dapper look to perform somehow similar with a trivial difference.  
While Compiled Query provides around 20% improvement, in the web environment with transient context, it doesn't look to boost the application (Caching might fix this issue!).  
However, the performance gain is very noticeable when Context pooling is used that applying it is very straightforward on the application.


```
                                         Method | NumBlogs |     Mean |   Error |  StdDev |   Gen 0 | Allocated |
----------------------------------------------- |--------- |---------:|--------:|--------:|--------:|----------:|
                                WithNormalQuery |        1 | 494.2 us | 7.21 us | 6.02 us | 50.7813 |     53 KB |
                              WithCompiledQuery |        1 | 384.0 us | 4.73 us | 4.43 us | 34.1797 |     35 KB |
             WithCompiledQueryAndContextPooling |        1 | 288.0 us | 3.42 us | 3.04 us |  1.4648 |      2 KB |
 WithCompiledQueryAndContextPoolingAsNoTracking |        1 | 288.2 us | 4.90 us | 4.34 us |  1.4648 |      2 KB |
                                    DapperQuery |        1 | 325.1 us | 4.83 us | 4.52 us |  9.2773 |     10 KB |
                                WithNormalQuery |       10 | 533.7 us | 8.41 us | 7.86 us | 52.7344 |     54 KB |
                              WithCompiledQuery |       10 | 433.0 us | 3.04 us | 2.84 us | 35.6445 |     36 KB |
             WithCompiledQueryAndContextPooling |       10 | 344.5 us | 5.14 us | 4.81 us |  2.4414 |      3 KB |
 WithCompiledQueryAndContextPoolingAsNoTracking |       10 | 341.4 us | 3.11 us | 2.75 us |  2.4414 |      3 KB |
                                    DapperQuery |       10 | 370.5 us | 6.23 us | 5.82 us |  9.7656 |     10 KB |
```  

# How To Run

From the project's folder run: 
```
docker compose up
```
