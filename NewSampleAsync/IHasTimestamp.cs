using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleAsync
{
    public interface IHasTimestamp
    {
        string HandleBase { get; }
        DateTime Timestamp { get; }
        event EventHandler Changed; // Called if there were updates and the Timestamp has now changed
        void BeginRefresh();
    }

    public interface IHasTimestampFactory
    {
        Task<IHasTimestamp> CreateObject(string objectType, object[] parameters);
    }
}
