using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoTradeReporter.Util
{
    class MathUtil
    {
        private const int DEFAULT_PRECISION = 4;

        public static decimal round(decimal value_)
        {
            return Math.Round(value_, DEFAULT_PRECISION, MidpointRounding.AwayFromZero);
        }

        public static decimal round(decimal value_, int precision_)
        {
            return Math.Round(value_, precision_, MidpointRounding.AwayFromZero);
        }

        public static decimal computeAvgSlipage(List<decimal> values_)
        {
            return values_.Average();
        }

        public static decimal computeWeightedSlipage(List<decimal> values_, List<decimal> weights_)
        {
            if (values_.Count == 0 || weights_.Count == 0)
                return 0;

            decimal sum = 0;
            decimal volume = 0;
            for (int i = 0; i < values_.Count; i++)
            {
                sum += values_[i] * weights_[i];
                volume += weights_[i];
            }
            if (volume == 0)
            {
                return 0;
            }
            else
            {
                return sum / volume;
            }
        }

        public static decimal computeStdDev(List<decimal> value_)
        {
            if (value_.Count == 0 || value_.Count == 1)
                return 0;

            List<double> values = new List<double>();
            foreach (decimal dec in value_)
            {
                values.Add(Convert.ToDouble(dec));
            }
            double[] doubleValues = values.ToArray<double>();
            return Convert.ToDecimal(standardDeviation(doubleValues));
        }

        private static double standardDeviation(IEnumerable<double> list)
        {
            List<double> numbers = list.ToList();

            double mean = numbers.Average();
            double result = numbers.Sum(number => Math.Pow(number - mean, 2.0));

            return Math.Sqrt(result / (numbers.Count - 1));
        }
    }
}
