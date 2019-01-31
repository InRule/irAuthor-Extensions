using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive.Linq;

namespace UndoExtension
{
    public class ObservableDonutStack<T> : ObservableCollection<T>
    {
        private readonly int desiredBufferSize;

        public ObservableDonutStack(int bufferSize) : base(new List<T>(bufferSize))
        {
            desiredBufferSize = bufferSize;
        }
        public ObservableDonutStack() : this(10) {} 

        public IObservable<NotifyCollectionChangedEventArgs> CollectionChanges
        {
            get
            {
                return Observable
                    .FromEventPattern<NotifyCollectionChangedEventHandler, NotifyCollectionChangedEventArgs>(
                        h => CollectionChanged += h,
                        h => CollectionChanged -= h)
                    .Select(x => x.EventArgs);
            }
        }
        public IObservable<int> ItemCount { get { return CollectionChanges.Select(x => Items.Count); } }

        public void Push(T item)
        {
            if (Count >= desiredBufferSize)
            {
                Pop();
            }
            Insert(0, item);
        }

        public T Pop()
        {
            var item = this.FirstOrDefault();
            RemoveAt(0);
            return item;
        }
    }
}