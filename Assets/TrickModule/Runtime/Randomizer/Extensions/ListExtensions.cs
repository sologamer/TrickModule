using System;
using System.Collections.Generic;
using System.Linq;
using Random = System.Random;

namespace TrickModule.Randomizer
{
    public static class ListExtensions
    {
        public static T Random<T>(this List<T> list, IRandomizer randomGenerator, int startIndex = 0)
        {
            int random = randomGenerator.Next(startIndex, list.Count);
            return list.Count > random ? list[random] : default(T);
        }

        public static T Random<T>(this T[] array, IRandomizer randomGenerator, int startIndex = 0)
        {
            int random = randomGenerator.Next(startIndex, array.Length);
            return array.Length > random ? array[random] : default(T);
        }

        public static T Random<T>(this IEnumerable<T> enumerable, IRandomizer randomGenerator, int startIndex = 0)
        {
            var array = enumerable.ToArray();
            int random = randomGenerator.Next(startIndex, array.Length);
            return array.Length > random ? array[random] : default(T);
        }

        public static T Random<T>(this Array array, IRandomizer randomGenerator, int startIndex = 0)
        {
            int random = randomGenerator.Next(startIndex, array.Length);
            return (T)(array.Length > random ? array.GetValue(random) : default(T));
        }


        public static T RandomAndPop<T>(this List<T> list, Random randomGenerator, int startIndex = 0)
        {
            int random = randomGenerator.Next(startIndex, list.Count);
            if (list.Count <= random) return default(T);
            var ret = list[random];
            list.RemoveAt(random);
            return ret;
        }

        public static T RandomAndPop<T>(this List<T> list, IRandomizer randomGenerator, int startIndex = 0)
        {
            int random = randomGenerator.Next(startIndex, list.Count);
            if (list.Count <= random) return default(T);
            var ret = list[random];
            list.RemoveAt(random);
            return ret;
        }

        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> enumerable, IRandomizer rnd)
        {
            var list = enumerable.ToArray();

            for (int i = 1; i < list.Length; i++)
            {
                int pos = rnd.Next(0, i + 1);
                (list[i], list[pos]) = (list[pos], list[i]);
            }

            return list;
        }
    }
}