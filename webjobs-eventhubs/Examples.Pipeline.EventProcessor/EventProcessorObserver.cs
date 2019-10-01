using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;

namespace Examples.Pipeline.EventProcessor
{
    class EventProcessorObserver : IObserver<long>
    {
        private readonly Subject<long> _subject = new Subject<long>();
        private readonly ConcurrentQueue<(DateTime, int)> _eventCounts = new ConcurrentQueue<(DateTime, int)>();
        private const int bucketSizeSeconds = 10;

        public EventProcessorObserver()
        {
            _subject.Buffer(TimeSpan.FromSeconds(bucketSizeSeconds)).Subscribe(b =>
            {
                var eps = (DateTime.UtcNow, b.Count);
                _eventCounts.Enqueue(eps);
                while (_eventCounts.Count > 3600 / bucketSizeSeconds) if (!_eventCounts.TryDequeue(out (DateTime, int) result)) break;
            });
        }

        public void OnCompleted()
        {
            _subject.OnCompleted();
        }

        public void OnError(Exception error)
        {
            _subject.OnError(error);
        }

        public void OnNext(long value)
        {
            _subject.OnNext(value);
        }

        public (DateTime, int)[] GetMetrics() => _eventCounts.ToArray();

        public string GetMetricsString() 
            => string.Join(
                "\r\n", 
                _eventCounts.TakeLast(10).Select(c => $"Event count for {bucketSizeSeconds} seconds to {c.Item1} = {c.Item2}").ToArray());
    }
}
