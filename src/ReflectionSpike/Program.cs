using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using FastMember;

namespace ReflectionSpike
{
    public static class Program
    {
        private static Stopwatch _stopWatch;
        private static TestClass _testClass = new TestClass();

        public static void Main(string[] args)
        {
            var dunits = DurationUnits.Milliseconds;

            RunPropertyAccessorTests(ValueType.Integer, dunits);
            RunPropertyAccessorTests(ValueType.String, dunits);

            // RunDictionaryCreationTests(dunits);

            Console.ReadLine();
        }

        private static void RunPropertyAccessorTests(ValueType vtype = ValueType.String, DurationUnits dunits = DurationUnits.Ticks)
        {
            var testparams = new TestParameters { TestValueType = vtype, DurationUnits = dunits };
            var results = new TestResults { TestParameters = testparams };

            GetValueUsingReflection(results);
            GetValueUsingDirectAccess(results);
            GetValueUsingCachedPropertyInfo(results);
            GetValueUsingMagicMethod(results);
            GetValueUsingFastMember(results);

            TestResults.Display(results, $"Property Accessors for {vtype}");
        }

        private static void RunDictionaryCreationTests(DurationUnits dunits = DurationUnits.Ticks)
        {
            var testparams = new TestParameters { Iterations = 50000, DurationUnits = dunits };
            var results = new TestResults { TestParameters = testparams };

            CreateDictionaryUsingReflection(results);
            CreateDictionaryUsingLinq(results);
            CreateDictionaryUsingCachedPropertyInfo(results);
            CreateDictionaryUsingDirectAccess(results);
            CreateDictionaryUsingMagicMethod(results);
            CreateDictionaryUsingGenericStatic(results);

            TestResults.Display(results, "Dictionary Creation");
        }

        private static void GetValueUsingCachedPropertyInfo(TestResults results)
        {
            var testName = "Get Property Value Using Cached PropertyInfo";
            var propName = results.TestParameters.TestValueType == ValueType.String
                ? "Name"
                : "Duration";
            var prop = _testClass.GetType().GetProperty(propName);

            _stopWatch = Stopwatch.StartNew();
            for (var i = 0; i < results.TestParameters.Iterations; i++)
            {
                var x = prop.GetValue(_testClass);
            }
            _stopWatch.Stop();

            results.AddResult(testName, _stopWatch);
        }

        private static void GetValueUsingDirectAccess(TestResults results)
        {
            var testName = "Get Property Value Using Direct Access";

            if (results.TestParameters.TestValueType == ValueType.String)
            {
                _stopWatch = Stopwatch.StartNew();
                for (var i = 0; i < results.TestParameters.Iterations; i++)
                {
                    var x = _testClass.Name;
                }
                _stopWatch.Stop();
            }
            else if (results.TestParameters.TestValueType == ValueType.String)
            {
                _stopWatch = Stopwatch.StartNew();
                for (var i = 0; i < results.TestParameters.Iterations; i++)
                {
                    var x = _testClass.Duration;
                }
                _stopWatch.Stop();
            }

            results.AddResult(testName, _stopWatch);
        }

        private static void GetValueUsingReflection(TestResults results)
        {
            var testName = "Get Property Value Using Reflection";
            var propName = results.TestParameters.TestValueType == ValueType.String
                ? "Name"
                : "Duration";

            _stopWatch = Stopwatch.StartNew();
            for (var i = 0; i < results.TestParameters.Iterations; i++)
            {
                var prop = _testClass.GetType().GetProperty(propName);
                var x = prop.GetValue(_testClass);

            }
            _stopWatch.Stop();

            results.AddResult(testName, _stopWatch);
        }

        private static void GetValueUsingMagicMethod(TestResults results)
        {
            var testName = "Get Property Value Using MagicMethod";
            var propName = results.TestParameters.TestValueType == ValueType.String
                ? "Name"
                : "Duration";
            var method = MagicMethodMaker.MakeMagicMethod<TestClass>(_testClass.GetType().GetProperty(propName).GetGetMethod());

            _stopWatch = Stopwatch.StartNew();
            for (var i = 0; i < results.TestParameters.Iterations; i++)
            {
                var x = method(_testClass);
            }
            _stopWatch.Stop();

            results.AddResult(testName, _stopWatch);
        }

        private static void GetValueUsingFastMember(TestResults results)
        {
            var testName = "Get Property Value Using FastMember";
            var accessor = TypeAccessor.Create(typeof(TestClass));
            var propName = results.TestParameters.TestValueType == ValueType.String
                ? "Name"
                : "Duration";

            _stopWatch = Stopwatch.StartNew();
            for (var i = 0; i < results.TestParameters.Iterations; i++)
            {
                var x = accessor[_testClass, propName];
            }
            _stopWatch.Stop();

            results.AddResult(testName, _stopWatch);
        }

        private static void CreateDictionaryUsingDirectAccess(TestResults results)
        {
            var testName = "Create Dictionary Using Direct Access";

            _stopWatch = Stopwatch.StartNew();
            for (var i = 0; i < results.TestParameters.Iterations; i++)
            {
                var dictionary = new Dictionary<string, object>();
                dictionary.Add("Name", _testClass.Name);
                dictionary.Add("Duration", _testClass.Duration);
                dictionary.Add("IsTrue", _testClass.IsTrue);
                dictionary.Add("LastHappened", _testClass.LastHappened);
                dictionary.Add("MyData", _testClass.MyData);
            }
            _stopWatch.Stop();

            results.AddResult(testName, _stopWatch);
        }

        private static void CreateDictionaryUsingCachedPropertyInfo(TestResults results)
        {
            var testName = "Create Dictionary Using Cached PropertyInfo";
            PropertyInfoCache<TestClass>.Init();

            _stopWatch = Stopwatch.StartNew();
            for (var i = 0; i < results.TestParameters.Iterations; i++)
            {
                var dictionary = PropertyInfoCache<TestClass>.ToDictionary(_testClass);
            }
            _stopWatch.Stop();

            results.AddResult(testName, _stopWatch);
        }

        private static void CreateDictionaryUsingLinq(TestResults results)
        {
            var testName = "Create Dictionary Using Linq";

            _stopWatch = Stopwatch.StartNew();
            for (var i = 0; i < results.TestParameters.Iterations; i++)
            {
                var dictionary = _testClass.AsDictionary();
            }
            _stopWatch.Stop();

            results.AddResult(testName, _stopWatch);
        }

        private static void CreateDictionaryUsingReflection(TestResults results)
        {
            var testName = "Create Dictionary Using Reflection";

            _stopWatch = Stopwatch.StartNew();
            for (var i = 0; i < results.TestParameters.Iterations; i++)
            {
                var dictionary = new Dictionary<string, object>();
                foreach (var property in _testClass.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
                {
                    dictionary.Add(property.Name, property.GetValue(_testClass));
                }
            }
            _stopWatch.Stop();

            results.AddResult(testName, _stopWatch);
        }

        private static void CreateDictionaryUsingMagicMethod(TestResults results)
        {
            var testName = "Create Dictionary Using MagicMethod";

            var cache = new Dictionary<string, Func<TestClass, object>>();
            foreach (var property in _testClass.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                cache.Add(property.Name, MagicMethodMaker.MakeMagicMethod<TestClass>(property.GetGetMethod()));
            }

            _stopWatch = Stopwatch.StartNew();
            for (var i = 0; i < results.TestParameters.Iterations; i++)
            {
                var dictionary = new Dictionary<string, object>();
                foreach (var kvp in cache)
                {
                    dictionary.Add(kvp.Key, kvp.Value(_testClass));
                }
            }
            _stopWatch.Stop();

            results.AddResult(testName, _stopWatch);
        }

        private static void CreateDictionaryUsingGenericStatic(TestResults results)
        {
            // throw new NotImplementedException();
        }
    }
}
