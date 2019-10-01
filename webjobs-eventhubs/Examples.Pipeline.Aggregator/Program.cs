using Examples.Pipeline.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;

namespace Examples.Pipeline.Aggregator
{
    class Program
    {
        static Subject<Transaction> _subject;

        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                //.AddEnvironmentVariables()
                .Build();

            var client = new CosmosClient("XXXX");

            var observable = Observable.Create<Transaction>(
                async observer =>
                {
                    var processor = await StartChangeFeedProcessorAsync(client, config);
                    
                    // Dispose Action
                    return async () => await processor.StopAsync();
                });

            using (var subscription = observable.Subscribe(Console.WriteLine))
            using (var subscription2 = observable.Buffer(new TimeSpan(0, 1, 0)).Subscribe(b => Console.WriteLine($"Sum of Transaction Amounts = {b.Sum(c => c.Amount)}")))
                Console.ReadLine();

              
            

            //var observable = Start();
            //observable.Subscribe(
            //    p => Console.WriteLine("Progress {0}", p.Id),
            //    e => Console.WriteLine("Error {0}", e.Message),
            //    () => Console.WriteLine("Finished success"));

        }

        /// <summary>
        /// Start the Change Feed Processor to listen for changes and process them with the HandlerChangesAsync implementation.
        /// </summary>
        private static async Task<ChangeFeedProcessor> StartChangeFeedProcessorAsync(
            CosmosClient cosmosClient,
            IConfiguration configuration)
        {
            string databaseName = configuration["SourceDatabaseName"];
            string sourceContainerName = configuration["SourceContainerName"];
            string leaseContainerName = configuration["LeasesContainerName"];

            Container leaseContainer = cosmosClient.GetContainer(databaseName, leaseContainerName);
            ChangeFeedProcessor changeFeedProcessor = cosmosClient.GetContainer(databaseName, sourceContainerName)
                .GetChangeFeedProcessorBuilder<Transaction>("changeFeedSample", HandleChangesAsync)
                    .WithInstanceName("consoleHost")
                    .WithLeaseContainer(leaseContainer)
                    .Build();

            Console.WriteLine("Starting Change Feed Processor...");
            await changeFeedProcessor.StartAsync();
            Console.WriteLine("Change Feed Processor started.");
            return changeFeedProcessor;
        }

        /// <summary>
        /// The delegate receives batches of changes as they are generated in the change feed and can process them.
        /// </summary>
        static Task HandleChangesAsync(IReadOnlyCollection<Transaction> changes, CancellationToken cancellationToken)
        {
            Console.WriteLine("Started handling changes...");
            foreach (Transaction item in changes)
            {
                Console.WriteLine($"Detected operation for item with id {item.Id}.");
                // Simulate some asynchronous operation
                _subject.OnNext(item);
            }

            Console.WriteLine("Finished handling changes.");

            return Task.CompletedTask;
        }

        public static IObservable<Transaction> Start()
        {
            _subject = new Subject<Transaction>();
            //Task.Run(() => DoStuff(subject));
            return _subject;
        }

    }
}

