using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;

namespace ReflectionSpike
{
    public class TestClass
    {
        public string Name { get; set; } = "Sam";

        public int Duration { get; set; } = 100;

        public bool IsTrue { get; set; } = true;

        public DateTimeOffset LastHappened { get; } = DateTimeOffset.Now;

        public DataTable MyData { get; set; } = new DataTable();
    }

    public class TestResults
    {
        private Dictionary<string, long> _results { get; } = new Dictionary<string, long>();

        public TestParameters TestParameters { get; set; } = new TestParameters();

        public void AddResult(string text, Stopwatch watch)
        {
            var duration = TestParameters.UsingTicks
                ? watch.ElapsedTicks
                : watch.ElapsedMilliseconds;

            _results[text] = duration;
        }

        public void AddResult(KeyValuePair<string, long> result)
        {
            _results.Append(result);
        }

        public void AddResult(string text, long duration)
        {
            _results[text] = duration;
        }

        public void ClearResults()
        {
            _results.Clear();
        }

        public List<KeyValuePair<string, long>> GetResults(bool sortAscending = false)
        {
            var results = _results.ToList();

            if (sortAscending)
            {
                results.Sort((p1, p2) => p1.Value.CompareTo(p2.Value));
            }
            else
            {
                results.Sort((p1, p2) => p2.Value.CompareTo(p1.Value));
            }

            return results;
        }

        public int GetFastestResult()
        {
            return (int)_results.Values.Aggregate((long)0, (max, cur) => max > cur ? cur : max);
        }

        public int GetSlowestResult()
        {
            return (int)_results.Values.Aggregate((long)0, (max, cur) => max > cur ? max : cur);
        }

        public int GetLongestResultStringLenght()
        {
            return _results.Keys.Aggregate("", (max, cur) => max.Length > cur.Length ? max : cur).Length;
        }

        public int GetLongestDurationStringLength()
        {
            return FormatResultDuration(GetSlowestResult()).Length;
        }

        public static string FormatResultDuration(int duration)
        {
            return $"{duration:N0}";
        }

        public static void Display(TestResults report, string reportTitle = "Test Results")
        {
            var slowest = report.GetSlowestResult();
            var fastest = report.GetFastestResult();
            var keylen = report.GetLongestResultStringLenght();
            var vallen = report.GetLongestDurationStringLength();
            var unitsLabel = report.TestParameters.GetUnitsLable();

            Console.WriteLine();
            Console.WriteLine($"\t{reportTitle} : Using {report.TestParameters.Iterations:N0} iterations");

            foreach (var result in report.GetResults())
            {
                var current = (result.Value > 0) ? result.Value : 1;
                var perf = slowest != result.Value ? $"~{Math.Round(Decimal.Divide(slowest, current), report.TestParameters.Precision)}x faster" : "";

                var number = $"{result.Value:N0}";
                Console.WriteLine($"\t{result.Key.PadRight(keylen)}: {number.PadLeft(vallen)} {unitsLabel} {perf}");
            }

            Console.WriteLine();
            report.ClearResults();
        }
    }

    public class TestParameters
    {
        public int Precision { get; set; } = 1;

        public int Iterations { get; set; } = 1000000;

        public ValueType TestValueType { get; set; } = ValueType.String;

        public DurationUnits DurationUnits { get; set; } = DurationUnits.Ticks;

        public string GetUnitsLable()
        {
            return UsingTicks ? "ticks" : "ms";
        }

        public bool UsingTicks => DurationUnits == DurationUnits.Ticks;
    }
}
