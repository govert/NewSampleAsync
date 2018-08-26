using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExcelDna.Integration;

namespace SampleAsync.Utils
{
    // This wrapper is created before the underlying object is created or registered with the storage
    // The observable will only publish a string with the handle info when the object is created,
    // and then every time the object is updated.
    // If the object imploements IDisposable, Dispose will be called when the Observable is disposed.
    internal class IHasTimestampObservable :  IExcelObservable, IDisposable
    {
        private static Dictionary<string, int> s_indexMap = new Dictionary<string, int>();

        private readonly IHasTimestampStorage m_storage;
        private string m_handleBase;
        private int m_index;
        private IExcelObserver m_observer;
        public IHasTimestamp Value { get; private set; }

        public IHasTimestampObservable(IHasTimestampStorage storage, Task<IHasTimestamp> createTask) // Could add CancellationToken...
        {
            m_storage = storage;
            createTask.ContinueWith(t => CreateCompleted(t));
        }

        void CreateCompleted(Task<IHasTimestamp> createTask)
        {
            lock (this)
            {
                try
                {
                    Value = createTask.Result;
                    m_handleBase = Value.HandleBase;

                    // Check if this handleBase is already present in the indexMap, and use the next index from there (else the index becomes 1)
                    int lastIndex = 0;
                    s_indexMap.TryGetValue(m_handleBase, out lastIndex);
                    m_index = lastIndex + 1;

                    // Update the indexMap with the index we used
                    s_indexMap[m_handleBase] = m_index;

                    Value.Changed += hasTimestamp_Changed;
                    m_storage.Add(this);
                    if (m_observer != null)
                        m_observer.OnNext(HandleName);
                }
                catch (Exception exception)
                {
                    if (m_observer != null)
                        m_observer.OnError(exception);
                }
            }
        }

        private void hasTimestamp_Changed(object sender, EventArgs e)
        {
            lock (this)
            {
                if (m_observer != null)
                    m_observer.OnNext(HandleName);
            }
        }

        public IDisposable Subscribe(IExcelObserver observer)
        {
            lock (this)
            {
                m_observer = observer;
                // Might be a problem if we want to support valid null values.
                if (Value != null)
                    m_observer.OnNext(HandleName);
                return this;
            }
        }

        public string HandleName => $"{m_handleBase}/{m_index}/{Value?.Timestamp:yyyy-MM-dd HH:mm:ss}";

        public void Dispose()
        {
            lock (this)
            {
                // Safe to call Remove even if we never got a chance to add ourself to the storage
                m_storage.Remove(this);
                (Value as IDisposable)?.Dispose();
            }
        }
    }
}
