using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleAsync.Utils
{
    class TestHasTimestampFactory : IHasTimestampFactory
    {
        public async Task<IHasTimestamp> CreateObject(string objectType, object[] parameters)
        {
            await Task.Delay(TimeSpan.FromSeconds(5));
            if (objectType == "Test")
                return new TestHasTimestamp(parameters);

            // Invalid objectType
            throw new NotImplementedException();
        }
    }

    class TestHasTimestamp : IHasTimestamp, IDisposable
    {
        object[] m_parameters;

        public string HandleBase { get; }
        public DateTime Timestamp { get; private set; }
        public event EventHandler Changed;

        public int CurrentValue { get; private set; }

        public TestHasTimestamp(object[] parameters)
        {
            // We need a unique HandleBase here for each type of object
            // It can depend on the parameters or not
            HandleBase = $"TestHasTimeStamp:{parameters[0]}";
            // Could interpret and same the parameters in any way
            m_parameters = parameters;
            Timestamp = DateTime.Now;
        }

        public void BeginRefresh()
        {
            Task.Run(DoSlowRefresh);
        }

        // This implements the async refresh which should update the Timestamp and raise the Changed event if the object changed
        async Task DoSlowRefresh()
        {
            await Task.Delay(TimeSpan.FromSeconds(5));
            CurrentValue++;
            Timestamp = DateTime.Now;
            Changed?.Invoke(this, EventArgs.Empty);
        }

        // This is an example of a slow function
        public async Task<string> GetPropertyAsync()
        {
            await Task.Delay(TimeSpan.FromSeconds(5));
            return $"Current Value: {CurrentValue} ({m_parameters[0]})";
        }

        // Implementing IDisposable is optional, but can allow back-end resources to be cleanup when this object is no longer used
        public void Dispose()
        {
            Debug.Print("TestHasTimestamp Disposed");
        }
    }
}
