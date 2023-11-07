using System;
using System.Collections.Generic;
using System.Linq;

namespace TrickModule.Randomizer
{
    public static class NumberExtensions
    {
        public static IEnumerable<float> SplitValue2SumPreviousValues(this IRandomizer rand, float value, int count, float pct)
        {
            var splitValue = SplitValue2(rand, value, count, pct).ToList();
            float sum = 0.0f;
            for (var index = 0; index < splitValue.Count; index++)
            {
                sum += splitValue[index];
                splitValue[index] = sum;
            }

            return splitValue;
        }

        public static IEnumerable<float> SplitValue2(this IRandomizer rand, float value, int count, float pct)
        {
            if (count <= 0) throw new ArgumentException("count must be greater than zero.", "count");
            var result = new float[count];

            float runningTotal = 0f;
            for (int i = 0; i < count; i++)
            {
                var remainder = value - runningTotal;
                float share;
                if (remainder > 0f)
                {
                    var v = (float) Math.Max(Math.Round(remainder / ((float) (count - i)), 2), .01f);
                    share = Math.Min((float) rand.Next(v * (1.0f - pct), v * (1.0f + pct)), value);
                }
                else
                    share = 0f;
                result[i] = share;
                runningTotal += share;
            }

            if (runningTotal < value) result[count - 1] += value - runningTotal;
            if (runningTotal > value) result[count - 1] += value - runningTotal;

            return result;
        }
    }
}