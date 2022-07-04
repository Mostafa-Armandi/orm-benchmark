using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Dapper;
using Npgsql;

BenchmarkRunner.Run<Benchmarks>();

[MeanColumn,StdErrorColumn,StdDevColumn,MinColumn,MaxColumn,MedianColumn]
public class Benchmarks
{
    private static readonly string ConnectionString =  new NpgsqlConnectionStringBuilder
    {
        Host = "postgres",
        Port = 5432,
        Database = "postgres",
        Username = "postgres",
        Password = "postgres",
        ConnectionIdleLifetime = 15,
        Enlist = false,
        MaxAutoPrepare = 10
    }.ConnectionString;

    [Params(100)]
    public int Rows { get; set; }

    private DbContextOptions<EfCoreContext> _efCoreOptions;


    [GlobalSetup]
    public void Setup()
    {
        var createTableScript = @"
DROP TABLE IF EXISTS posts;
create table posts
        (
            id uuid DEFAULT gen_random_uuid (),
            title text,
            content text,
            PRIMARY KEY (id)
        );
";

        using NpgsqlConnection _connection = new(ConnectionString);
        using var command = new NpgsqlCommand(createTableScript, _connection);
        _connection.Open();
        command.ExecuteNonQuery();

        
        for (var i = 0; i < Rows; i++)
        {
            command.CommandText =
                $"INSERT INTO posts(title,content) VALUES ('{i} old man and the sea', 'This is a story')";
            command.ExecuteNonQuery();
        }

        var efCoreOptionsBuilder = new DbContextOptionsBuilder<EfCoreContext>();
        efCoreOptionsBuilder.UseNpgsql(ConnectionString);
        _efCoreOptions = efCoreOptionsBuilder.Options;
        
    }

    
    [Benchmark]
    public void EfCoreQueryWithASimpleClause()
    {
        using var context = new EfCoreContext(_efCoreOptions);

        _ = EntityFrameworkQueryableExtensions.AsNoTracking(context.Posts).Where(p => p.Title.StartsWith("6"))
            .ToList();
    }
    
    [Benchmark]
    public void EfCoreInsert()
    {
        using var context = new EfCoreContext(_efCoreOptions);

        context.Add(new Post() { Title = "test title", Content = "test content" });

       _ = context.SaveChanges();
    }

    [Benchmark]
    public void DapperQueryWithASimpleClause()
    {
        using var connection = new NpgsqlConnection(ConnectionString);
        _ = connection.Query<Post>("SELECT id,title,content FROM posts WHERE title like '6%'").ToList();
    }
    
    [Benchmark]
    public void DapperInsert()
    {
        using var connection = new NpgsqlConnection(ConnectionString);
        var script = "INSERT INTO posts(title,content) VALUES(@id, @content)";
        var entity = new Post() { Title = "test title", Content = "test content" };
        _ = connection.ExecuteScalar(script, entity);
    }

    public class EfCoreContext : DbContext
    {
        public EfCoreContext(DbContextOptions<EfCoreContext> options)
            : base(options)
        {
        }
        public DbSet<Post> Posts { get; set; }
    }

    [Table("posts")]
    public class Post
    {
        [Column("id")]
        public Guid Id { get; set; }

        [StringLength(50)]
        [Column("title")]
        public string? Title { get; set; }

        [StringLength(1000)]
        [Column("content")]
        public string? Content { get; set; }
    }
}