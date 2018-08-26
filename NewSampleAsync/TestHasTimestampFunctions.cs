using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExcelDna.Integration;

namespace SampleAsync.Utils
{
    public static class TestHasTimestampFunctions
    {
        static IHasTimestampStorage s_storage = new IHasTimestampStorage();
        static IHasTimestampFactory s_factory = new TestHasTimestampFactory();

        public static object CreateTestObject(string input)
        {
            var objectType = "Test";
            var parameters = new object[] { input };

            var result = ExcelAsyncUtil.Observe(objectType, parameters,
                            () => new IHasTimestampObservable(s_storage, s_factory.CreateObject(objectType, parameters)));

            if (result.Equals(ExcelError.ExcelErrorNA))
                return ExcelError.ExcelErrorGettingData;

            return result;
        }

        public static object GetTestObjectProperty(string handle)
        {
            var callerFunctionName = nameof(GetTestObjectProperty);
            var callerParameters = new object[] { handle };

            var result = ExcelTaskUtil.RunTask<object>(callerFunctionName, callerParameters, () => GetTestObjectPropertyAsync(handle));
            if (result.Equals(ExcelError.ExcelErrorNA))
                return ExcelError.ExcelErrorGettingData;

            return result;
        }

        static async Task<object> GetTestObjectPropertyAsync(string handle)
        {
            if (s_storage.TryFindObject(handle, out var hasTimestamp))
            {
                if (hasTimestamp is TestHasTimestamp testObject)
                {
                    return await testObject.GetPropertyAsync();
                }
            }
            return ExcelError.ExcelErrorValue;
        }

        [ExcelCommand(MenuName = "Test Async Handles", MenuText = "Refresh All")]
        public static void RefreshAll()
        {
            s_storage.RefreshAll();
        }
    }
}
