﻿using System;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using UnityEngine;

namespace TrickModule.Randomizer
{
    /// <summary>
    /// https://www.codeproject.com/Articles/164087/Random-Number-Generation
    /// </summary>
    [Serializable]
    public sealed class MersenneTwisterRandom : IRandomizer
    {
        // Class MersenneTwister generates random numbers
        // from a uniform distribution using the Mersenne
        // Twister algorithm.
        private const int N = 624;
        private const int M = 397;
        private const uint MATRIX_A = 0x9908b0dfU;
        private const uint UPPER_MASK = 0x80000000U;
        private const uint LOWER_MASK = 0x7fffffffU;
        private const int MAX_RAND_INT = 0x7fffffff;
        [JsonIgnore] private uint[] mag01 = { 0x0U, MATRIX_A };
        [JsonIgnore] private uint[] mt = new uint[N];
#if !NO_UNITY
        [HideInInspector, SerializeField]
#endif
        [JsonProperty]
        private int mti = N + 1;
#if !NO_UNITY
        [SerializeField]
#endif
        [JsonProperty]
        private uint seed = 0;

        public uint GetSeed() => seed;

        public string Debug()
        {
            return $"[MersenneTwisterRandom] {mti} - {string.Join(",", mt).GetHashCode()}";
        }

        public static bool operator ==(MersenneTwisterRandom obj1, MersenneTwisterRandom obj2)
        {
            return Equals(obj1, obj2);
        }

        public static bool operator !=(MersenneTwisterRandom obj1, MersenneTwisterRandom obj2)
        {
            return !(obj1 == obj2);
        }

        protected bool Equals(MersenneTwisterRandom other)
        {
            return mt.SequenceEqual(other.mt) && mti == other.mti;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((MersenneTwisterRandom)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((mt != null ? mt.GetHashCode() : 0) * 397) ^ mti;
            }
        }


        [JsonConstructor]
        public MersenneTwisterRandom()
        {
            init_genrand(seed = (uint)DateTime.Now.Millisecond);
        }

        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            int lastMti = mti;
            init_genrand(seed);
            for (int i = 0; i < lastMti; i++)
            {
                Next();
            }
        }

        public MersenneTwisterRandom(int seed)
        {
            init_genrand(this.seed = (uint)seed);
        }

        public MersenneTwisterRandom(int seed, int mti)
        {
            init_genrand(this.seed = (uint)seed);
            for (int i = 0; i < mti; i++)
            {
                Next();
            }
        }

        public MersenneTwisterRandom(int[] init)
        {
            uint[] initArray = new uint[init.Length];
            for (int i = 0; i < init.Length; ++i)
                initArray[i] = (uint)init[i];
            init_by_array(initArray, (uint)initArray.Length);
        }

        public static readonly MersenneTwisterRandom Default = new MersenneTwisterRandom();

        public static int MaxRandomInt
        {
            get { return 0x7fffffff; }
        }

        public int Next()
        {
            return genrand_int31();
        }

        public int Next(int maxValue)
        {
            return Next(0, maxValue);
        }

        public int Next(int minValue, int maxValue)
        {
            if (minValue > maxValue)
            {
                int tmp = maxValue;
                maxValue = minValue;
                minValue = tmp;
            }

            return (int)(Math.Floor((maxValue - minValue) * genrand_real1() +
                                    minValue));
        }

        public float Next(float min, float max)
        {
            return min + (max - min) * NextFloat(false);
        }

        public float NextFloat()
        {
            return (float)genrand_real2();
        }

        public float NextFloat(bool includeOne)
        {
            if (includeOne)
            {
                return (float)genrand_real1();
            }

            return (float)genrand_real2();
        }

        public float NextFloatPositive()
        {
            return (float)genrand_real3();
        }

        public double NextDouble()
        {
            return genrand_real2();
        }

        public double NextDouble(bool includeOne)
        {
            if (includeOne)
            {
                return genrand_real1();
            }

            return genrand_real2();
        }

        public double NextDoublePositive()
        {
            return genrand_real3();
        }

        public double Next53BitRes()
        {
            return genrand_res53();
        }

        public void Initialize()
        {
            init_genrand((uint)DateTime.Now.Millisecond);
        }

        public void Initialize(int seed)
        {
            init_genrand((uint)seed);
        }

        public void Initialize(int[] init)
        {
            uint[] initArray = new uint[init.Length];
            for (int i = 0; i < init.Length; ++i)
                initArray[i] = (uint)init[i];
            init_by_array(initArray, (uint)initArray.Length);
        }

        private void init_genrand(uint s)
        {
            mt[0] = s & 0xffffffffU;
            for (mti = 1; mti < N; mti++)
            {
                mt[mti] = (uint)(1812433253U * (mt[mti - 1] ^ (mt[mti - 1] >> 30)) + mti);
                mt[mti] &= 0xffffffffU;
            }
        }

        private void init_by_array(uint[] init_key, uint key_length)
        {
            int i, j, k;
            init_genrand(19650218U);
            i = 1;
            j = 0;
            k = (int)(N > key_length ? N : key_length);
            for (; k > 0; k--)
            {
                mt[i] = (uint)((uint)(mt[i] ^ ((mt[i - 1] ^ (mt[i - 1] >> 30)) * 1664525U)) + init_key[j] + j);
                mt[i] &= 0xffffffffU;
                i++;
                j++;
                if (i >= N)
                {
                    mt[0] = mt[N - 1];
                    i = 1;
                }

                if (j >= key_length) j = 0;
            }

            for (k = N - 1; k > 0; k--)
            {
                mt[i] = (uint)((uint)(mt[i] ^ ((mt[i - 1] ^ (mt[i - 1] >> 30)) *
                                               1566083941U)) - i);
                mt[i] &= 0xffffffffU;
                i++;
                if (i >= N)
                {
                    mt[0] = mt[N - 1];
                    i = 1;
                }
            }

            mt[0] = 0x80000000U;
        }

        uint genrand_int32()
        {
            uint y;
            if (mti >= N)
            {
                int kk;
                if (mti == N + 1)
                    init_genrand(5489U);
                for (kk = 0; kk < N - M; kk++)
                {
                    y = (mt[kk] & UPPER_MASK) | (mt[kk + 1] & LOWER_MASK);
                    mt[kk] = mt[kk + M] ^ (y >> 1) ^ mag01[y & 0x1U];
                }

                for (; kk < N - 1; kk++)
                {
                    y = (mt[kk] & UPPER_MASK) | (mt[kk + 1] & LOWER_MASK);
                    mt[kk] = mt[kk + (M - N)] ^ (y >> 1) ^ mag01[y & 0x1U];
                }

                y = (mt[N - 1] & UPPER_MASK) | (mt[0] & LOWER_MASK);
                mt[N - 1] = mt[M - 1] ^ (y >> 1) ^ mag01[y & 0x1U];
                mti = 0;
            }

            y = mt[mti++];
            y ^= (y >> 11);
            y ^= (y << 7) & 0x9d2c5680U;
            y ^= (y << 15) & 0xefc60000U;
            y ^= (y >> 18);
            return y;
        }

        private int genrand_int31()
        {
            return (int)(genrand_int32() >> 1);
        }

        double genrand_real1()
        {
            return genrand_int32() * (1.0 / 4294967295.0);
        }

        double genrand_real2()
        {
            return genrand_int32() * (1.0 / 4294967296.0);
        }

        double genrand_real3()
        {
            return (((double)genrand_int32()) + 0.5) * (1.0 / 4294967296.0);
        }

        double genrand_res53()
        {
            uint a = genrand_int32() >> 5, b = genrand_int32() >> 6;
            return (a * 67108864.0 + b) * (1.0 / 9007199254740992.0);
        }
    }
}