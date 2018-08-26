using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleAsync.Utils
{
    class IHasTimestampStorage
    {
        List<IHasTimestampObservable> observables = new List<IHasTimestampObservable>();

        public void Add(IHasTimestampObservable observable)
        {
            observables.Add(observable);
        }

        public void Remove(IHasTimestampObservable observable)
        {
            observables.Remove(observable);
        }

        public bool TryFindObject(string handleName, out IHasTimestamp hasTimestamp)
        {
            // With some care this lookup can be made faster with a Dictionary, but remember that the HandleName can change,
            // so the dictionary would have to get update events too.
            var observable = observables.FirstOrDefault(ts => ts.HandleName == handleName);
            hasTimestamp = observable?.Value;
            return observable != null;
        }

        internal void RefreshAll()
        {
            foreach (var obs in observables)
            {
                obs.Value.BeginRefresh();
            }
        }
    }
}
