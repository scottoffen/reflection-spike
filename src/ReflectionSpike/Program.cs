﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FastMember;

namespace ReflectionSpike
{
    public static class Program
    {
        private static Stopwatch _watch;
        private static TestClass _test = new TestClass { Name = "Bob", Duration = 10 };
        private static int _iterations = 10000;
        private static Dictionary<string, long> _results = new Dictionary<string, long>();
        private static VariableType _vtype = VariableType.String;
        private static ElapsedTimeIn _elapsedTimeIn = ElapsedTimeIn.Ticks;

        public static void Main(string[] args)
        {
            RunAllTests();
            DumpTestResults();

            Console.ReadLine();
        }

        public static void RunAllTests()
        {
            Baseline();
            ReflectEveryTime();
            CachePropertyInfo();
            CreateStrongDelegate();
            UsingFastMember();
            UsingMagicMethod();
        }

        public static void Baseline()
        {
            if (_vtype == VariableType.String)
            {
                _watch = Stopwatch.StartNew();
                for (var i = 0; i < _iterations; i++)
                {
                    var x = _test.Name;
                }
            }
            else
            {
                _watch = Stopwatch.StartNew();
                for (var i = 0; i < _iterations; i++)
                {
                    var x = _test.Duration;
                }
            }

            _results.Add("Baseline (Direct Access)", _elapsedTimeIn == ElapsedTimeIn.Ticks ? _watch.ElapsedTicks : _watch.ElapsedMilliseconds);
        }

        public static void ReflectEveryTime()
        {
            var propName = _vtype == VariableType.String ? "Name" : "Duration";

            _watch = Stopwatch.StartNew();
            for (var i = 0; i < _iterations; i++)
            {
                var propInfo = typeof(TestClass).GetProperty(propName);
                propInfo.GetValue(_test);
            }

            _results.Add("Reflect Every Time", _elapsedTimeIn == ElapsedTimeIn.Ticks ? _watch.ElapsedTicks : _watch.ElapsedMilliseconds);
        }

        public static void CachePropertyInfo()
        {
            var propName = _vtype == VariableType.String ? "Name" : "Duration";
            var propInfo = typeof(TestClass).GetProperty(propName);

            _watch = Stopwatch.StartNew();
            for (var i = 0; i < _iterations; i++)
            {
                propInfo.GetValue(_test);
            }

            _results.Add("Cache PropertyInfo", _elapsedTimeIn == ElapsedTimeIn.Ticks ? _watch.ElapsedTicks : _watch.ElapsedMilliseconds);
        }

        public static void CreateStrongDelegate()
        {
            if (_vtype == VariableType.String)
            {
                var propInfo = typeof(TestClass).GetProperty("Name");
                Func<TestClass, string> func = (Func<TestClass, string>)Delegate.CreateDelegate(typeof(Func<TestClass, string>), null, propInfo.GetGetMethod());

                _watch = Stopwatch.StartNew();
                for (var i = 0; i < _iterations; i++)
                {
                    func(_test);
                }
            }
            else
            {
                var propInfo = typeof(TestClass).GetProperty("Duration");
                Func<TestClass, int> func = (Func<TestClass, int>)Delegate.CreateDelegate(typeof(Func<TestClass, int>), null, propInfo.GetGetMethod());

                _watch = Stopwatch.StartNew();
                for (var i = 0; i < _iterations; i++)
                {
                    func(_test);
                }
            }

            _results.Add("Create Strong Delegate", _elapsedTimeIn == ElapsedTimeIn.Ticks ? _watch.ElapsedTicks : _watch.ElapsedMilliseconds);
        }

        public static void UsingFastMember()
        {
            var accessor = TypeAccessor.Create(typeof(TestClass));
            var propName = _vtype == VariableType.String ? "Name" : "Duration";

            _watch = Stopwatch.StartNew();
            for (var i = 0; i < _iterations; i++)
            {
                var x = accessor[_test, propName];
            }

            _results.Add("Using FastMember Library", _elapsedTimeIn == ElapsedTimeIn.Ticks ? _watch.ElapsedTicks : _watch.ElapsedMilliseconds);
        }

        public static void UsingMagicMethod()
        {
            var propInfo = typeof(TestClass).GetProperty(_vtype == VariableType.String ? "Name" : "Duration");
            var method = MagicMethodMaker.MakeMagicMethod<TestClass>(propInfo.GetGetMethod());

            _watch = Stopwatch.StartNew();
            for (var i = 0; i < _iterations; i++)
            {
                var x = method(_test);
            }

            _results.Add("Using MagicMethod Pattern", _elapsedTimeIn == ElapsedTimeIn.Ticks ? _watch.ElapsedTicks : _watch.ElapsedMilliseconds);
        }

        public static void DumpTestResults()
        {
            var slowest = (int)_results.Values.Aggregate((long)0, (max, cur) => max > cur ? max : cur);
            var fastest = (int)_results.Values.Aggregate((long)0, (max, cur) => max > cur ? cur : max);

            var klen = _results.Keys.Aggregate("", (max, cur) => max.Length > cur.Length ? max : cur).Length;
            var vlen = $"{slowest:N0}".Length;

            Console.WriteLine();
            Console.WriteLine($"\tUsing {_iterations:N0} iterations for each test.");

            var results = _results.ToList();
            results.Sort((p1, p2) => p2.Value.CompareTo(p1.Value));

            var units = _elapsedTimeIn == ElapsedTimeIn.Milliseconds ? "ms" : "ticks";

            foreach (var result in results)
            {
                var perf = slowest != result.Value ? $"~{Math.Ceiling(Decimal.Divide(slowest, result.Value))}x faster" : "";

                var number = $"{result.Value:N0}";
                Console.WriteLine($"\t{result.Key.PadRight(klen)}: {number.PadLeft(vlen)} {units} {perf}");
            }

            _results.Clear();
        }
    }
}
