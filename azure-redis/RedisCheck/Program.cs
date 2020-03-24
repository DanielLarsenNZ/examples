using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using System;
using System.IO;

namespace RedisCheck
{
    class Program
    {

        private static readonly Lazy<ConnectionMultiplexer> lazyConnection = new Lazy<ConnectionMultiplexer>(InitMultiplexer);
        private static readonly Lazy<ConnectionMultiplexer> lazyConnection2 = new Lazy<ConnectionMultiplexer>(InitMultiplexer2);

        public static ConnectionMultiplexer Connection => lazyConnection.Value;
        public static ConnectionMultiplexer Connection2 => lazyConnection2.Value;

        static void Main()
        {
            Console.WriteLine("Hello Redis!");

            // https://docs.microsoft.com/en-us/azure/azure-cache-for-redis/cache-dotnet-how-to-use-azure-redis-cache

            WriteInfo(Connection.GetDatabase());
            WriteInfo(Connection2.GetDatabase());

            lazyConnection.Value.Dispose();
        }

        private static void WriteInfo(IDatabase cache)
        {
            Console.WriteLine();
            Console.WriteLine("===============================================================");
            Console.WriteLine($"{cache.Multiplexer.Configuration.Substring(0, 60)}...");
            Console.WriteLine($"INFO: {cache.Execute("INFO")}");
            Console.WriteLine($"Ping: {cache.Ping()}");
            Console.WriteLine($"Multiplexer.GetStatus: {cache.Multiplexer.GetStatus()}");
            Console.WriteLine($"Multiplexer.GetEndPoints:");

            int i = 0;
            foreach (var endpoint in cache.Multiplexer.GetEndPoints())
            {
                i++;
                Console.WriteLine($"\t[{i}] {endpoint}");
            }

            Console.WriteLine();
        }

        private static ConnectionMultiplexer InitMultiplexer()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .AddEnvironmentVariables()
                .Build();

            string cacheConnection = config["RedisConnectionString"];
            return ConnectionMultiplexer.Connect(cacheConnection);
        }

        private static ConnectionMultiplexer InitMultiplexer2()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .AddEnvironmentVariables()
                .Build();

            string cacheConnection = config["Redis2ConnectionString"];
            return ConnectionMultiplexer.Connect(cacheConnection);
        }
    }
}
