using System;

namespace VerificationCodeGenerator
{
    public static class RandomGenerator
    {
        private static Random _global = new Random();
        [ThreadStatic]
        private static Random _local;

        public static string Next(int min, int max)
        {
            int initialLength = 0;
            Random inst = _local;

            if (inst == null)
            {
                int seed;
                lock (_global)
                {
                    seed = _global.Next(min, max);
                }
                _local = inst = new Random(seed);
            }

            initialLength = inst.Next(min, max);
            char[] result = new char[initialLength];

            for (long i = 0; i < initialLength; i++)
            {
                int randomDecimalValue = inst.Next(33, 128);
                result[i] = (char)randomDecimalValue;
            }

            return new string(result);
        }
    }
}

