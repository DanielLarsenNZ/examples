using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.IO;

namespace RedisCheck
{
    class Program
    {
        private static readonly Lazy<ConnectionMultiplexer> lazyConnection = new Lazy<ConnectionMultiplexer>(InitMultiplexer);
        private static readonly Lazy<ConnectionMultiplexer> lazyConnection2 = new Lazy<ConnectionMultiplexer>(InitMultiplexer2);

        public static ConnectionMultiplexer Connection => lazyConnection.Value;
        public static ConnectionMultiplexer Connection2 => lazyConnection2.Value;

        private static readonly List<Exception> _errors = new List<Exception>();

        static void Main()
        {
            Console.WriteLine("Hello Redis!");

            // https://docs.microsoft.com/en-us/azure/azure-cache-for-redis/cache-dotnet-how-to-use-azure-redis-cache

            while (!Console.KeyAvailable)
            {
                WriteInfo(Connection.GetDatabase());
                WriteInfo(Connection2.GetDatabase());

                GetSet(Connection.GetDatabase());
                GetSet(Connection2.GetDatabase());
            }

            lazyConnection.Value.Dispose();

            foreach(var ex in _errors)
            {
                Console.WriteLine();
                Console.WriteLine(ex);
            }
        }

        private static void WriteInfo(IDatabase cache)
        {
            try
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

                Console.WriteLine("CLIENT LIST:");
                Console.WriteLine(cache.Execute("CLIENT", "LIST").ToString());

                Console.WriteLine();

                Console.WriteLine("CLUSTER INFO:");

                try
                {
                    Console.WriteLine(cache.Execute("CLUSTER", "INFO").ToString());
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

            }
            catch (Exception ex)
            {
                _errors.Add(ex);
                Console.Error.WriteLine(ex.Message);
            }
        }

        static void GetSet(IDatabase cache)
        {
            // Perform cache operations using the cache object...
            for (int i = 1; i <= 200; i++)
            {
                string key = $"key{i}";
                string message = RandomBytesAsString(1024);

                try
                {
                    // Simple get and put of integral data types into the cache
                    Console.WriteLine($"SET {key} \"{message.Substring(0, 16)}...\": {cache.StringSet(key, message)}");
                    Console.WriteLine($"GET {key}: \"{cache.StringGet(key).ToString().Substring(0, 16)}...\"");
                }
                catch (Exception ex)
                {
                    _errors.Add(ex);
                    Console.Error.WriteLine(ex.Message);
                }
            }
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

        private static string RandomBytesAsString(int size)
        {
            // Construct a random string of data
            string data = "";
            var random = new Random();
            for (int i = 0; i < size; i++)
            {
                data += (char)random.Next(65, 90);
            }

            return data;
        }
    }
}
