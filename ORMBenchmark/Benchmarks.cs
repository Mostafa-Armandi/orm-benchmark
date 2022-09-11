using BenchmarkDotNet.Attributes;
using Benchmarks;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Npgsql;

namespace ORMBenchmark;

[MemoryDiagnoser]
public class Benchmarks
{
    private DbContextOptions<BloggingContext> _options;
    private PooledDbContextFactory<BloggingContext> _poolingFactory;

    private static readonly Func<BloggingContext, IEnumerable<Blog>> _compiledQuery
        = EF.CompileQuery((BloggingContext context) =>
            context.Blogs
                .Where(b => b.Url.StartsWith("http://"))
                .Where(b => b.Posts.Count > 3)
                .Where(b => b.Posts.Any(p => p.Timestamp > DateTime.Now.AddDays(5).ToUniversalTime()))
                .Take(1)
        );

    [Params(1, 10)] public int NumBlogs { get; set; }


    [GlobalSetup]
    public void Setup()
    {
        _options = new DbContextOptionsBuilder<BloggingContext>()
            .UseNpgsql(DbConnection.ConnectionString)
            .UseSnakeCaseNamingConvention()
            //.LogTo(Console.WriteLine)
            .Options;

        using var context = new BloggingContext(_options);
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
        context.SeedDataAsync(NumBlogs);

        _poolingFactory = new PooledDbContextFactory<BloggingContext>(_options);
    }


    public class BloggingContext : DbContext
    {
        public DbSet<Blog> Blogs { get; set; }

        public DbSet<Post> Posts { get; set; }

        public BloggingContext(DbContextOptions options) : base(options)
        {
        }

        public void SeedDataAsync(int numBlogs)
        {
            Blogs.AddRange(Enumerable.Range(0, numBlogs)
                .Select(i => new Blog { Url = $"http://www.someblog{i}.com" }));

            SaveChanges();

            foreach (var blog in Blogs)
                for (var i = 0; i < 10; i++)
                    Posts.Add(new Post()
                    {
                        BlogId = blog.Id,
                        Timestamp = DateTime.Now.AddDays(blog.Id).AddHours(i).ToUniversalTime(),
                        Title = $"Post No.{i} of blog {blog.Id}"
                    });

            SaveChanges();
        }
    }

    public class Blog
    {
        public int Id { get; set; }
        public string Url { get; set; }

        public List<Post> Posts { get; set; }
    }

    public class Post
    {
        public int Id { get; set; }


        public string Title { get; set; }
        public DateTime Timestamp { get; set; }
        public int BlogId { get; set; }
        public Blog Blog { get; set; }
    }

    [Benchmark]
    public Blog? WithNormalQuery()
    {
        using var context = new BloggingContext(_options);

        return context.Blogs
            .Where(b => b.Url.StartsWith("http://"))
            .Where(b => b.Posts.Count > 3)
            .Where(b => b.Posts.Any(p => p.Timestamp > DateTime.Now.AddDays(5).ToUniversalTime()))
            .Take(1)
            .FirstOrDefault();
    }

    [Benchmark]
    public Blog? WithCompiledQuery()
    {
        using var context = new BloggingContext(_options);

        return _compiledQuery(context).FirstOrDefault();
    }

    [Benchmark]
    public Blog? WithCompiledQueryAndContextPooling()
    {
        using var context = _poolingFactory.CreateDbContext();

        return _compiledQuery(context)
            .FirstOrDefault();
    }

    [Benchmark]
    public Blog? WithCompiledQueryAndContextPoolingAsNoTracking()
    {
        using var context = _poolingFactory.CreateDbContext();

        return _compiledQuery(context)
            .AsQueryable()
            .AsNoTracking()
            .FirstOrDefault();
    }

    [Benchmark]
    public Blog DapperQuery()
    {
        using var connection = new NpgsqlConnection(DbConnection.ConnectionString);

        // grabbed by EF interception
        var query = $@"
      SELECT t.id, t.url
      FROM (
          SELECT b.id, b.url
          FROM blogs AS b
          WHERE ((((b.url IS NOT NULL)) AND (b.url LIKE 'http://%')) AND ((
              SELECT COUNT(*)::INT
              FROM posts AS p
              WHERE b.id = p.blog_id) > 3)) AND EXISTS (
              SELECT 1
              FROM posts AS p0
              WHERE (b.id = p0.blog_id) AND (p0.timestamp > CAST((now()::timestamp + INTERVAL '5 days') AS timestamp with time zone)))
          LIMIT 1
      ) AS t
      LIMIT 1
";
        return connection.Query<Blog>(query).FirstOrDefault();
    }
}