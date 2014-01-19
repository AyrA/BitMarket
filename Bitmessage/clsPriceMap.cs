using System;
using System.Collections.Generic;
using System.Text;

namespace SQL
{
    public class PriceMap
    {
        public string[] Currencies
        { get; set; }

        public PriceRange[] Ranges
        { get; set; }

        public Price[] Prices
        { get; set; }

        public PriceMap(string Map)
        {
            string[] Parts = Map.Split('|');
            
            Currencies = Parts[0].Split(':');
            Ranges = new PriceRange[Parts.Length - 1];
            Prices = new Price[Parts.Length - 1];

            for (int i = 1; i < Parts.Length; i++)
            {
                if (Parts[i].Contains(";"))
                {
                    try
                    {
                        Ranges[i - 1] = new PriceRange(Parts[i].Split(';')[0]);
                    }
                    catch
                    {
                        throw new Exception("Specificed Range construct is invalid: " + Parts[i].Split(';')[0]);
                    }
                    try
                    {
                        Prices[i - 1] = new Price(Parts[i].Split(';')[1]);
                    }
                    catch
                    {
                        throw new Exception("Specificed Price construct is invalid: " + Parts[i].Split(';')[1]);
                    }
                    if (Prices[i - 1].Values.Length != Currencies.Length)
                    {
                        throw new Exception("Prices do not mach Currencies in Price Map part: " + Parts[i]);
                    }
                    if (!Ranges[i - 1].IsValid)
                    {
                        throw new Exception("Ranges are not possible to be reached in Price Map part: " + Parts[i]);
                    }
                }
                else
                {
                    throw new Exception("Invalid Price Map part: " + Parts[i]);
                }
            }
        }

        public double getValue(int Amount,int CurrencyIndex)
        {
            if (CurrencyIndex >= 0 && CurrencyIndex < Currencies.Length)
            {
                for (int i = 0; i < Ranges.Length; i++)
                {
                    if (Ranges[i].includes(Amount))
                    {
                        return Prices[i].Values[CurrencyIndex] * (double)Amount;
                    }
                }
            }
            return double.NaN;
        }

        public override string ToString()
        {
            StringBuilder SB = new StringBuilder();
            SB.Append(string.Join(":", Currencies));
            for (int i = 0; i < Prices.Length; i++)
            {
                SB.AppendFormat("|{0};{1}", Ranges[i].ToString(), Prices[i].ToString());
            }
            return SB.ToString();
        }
    }
}
