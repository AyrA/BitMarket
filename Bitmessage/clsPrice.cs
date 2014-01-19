using System;
using System.Collections.Generic;
using System.Text;

namespace SQL
{
    public class Price
    {
        public double[] Values
        { get; set; }

        public Price(string PriceValues)
        {
            string[] Parts = PriceValues.Split(':');

            Values = new double[Parts.Length];

            for (int i = 0; i < Parts.Length; i++)
            {
                if (!double.TryParse(Parts[i], out Values[i]) || Values[i] < 0)
                {
                    throw new Exception("Invalid value " + Parts[i]);
                }
            }
        }

        public Price()
        {
            Values = new double[] { 0.0 };
        }

        public override string ToString()
        {
            string[] tmp = new string[Values.Length];
            for (int i = 0; i < Values.Length; i++)
            {
                tmp[i] = Values[i].ToString();
            }
            return string.Join(":", tmp);
        }
    }
}
