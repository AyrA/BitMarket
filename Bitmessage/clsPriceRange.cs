using System;
using System.Collections.Generic;
using System.Text;

namespace SQL
{
    public class PriceRange
    {
        public const int INFINITY = int.MaxValue;

        public int From
        { get; set; }
        public int Step
        { get; set; }
        public int To
        { get; set; }

        public bool IsValid
        {
            get
            {
                return Step > 0 && To >= From && From > 0 && (To - From) % Step == 0;
            }
        }

        public PriceRange(string Range)
        {
            string[] Parts = Range.Split(':');
            switch (Parts.Length)
            {
                case 1:
                    From = int.Parse(Parts[0]);
                    Step = 1;
                    To = INFINITY;
                    break;
                case 2:
                    From = int.Parse(Parts[0]);
                    Step = int.Parse(Parts[1]);
                    To = INFINITY;
                    break;
                case 3:
                    From = int.Parse(Parts[0]);
                    Step = int.Parse(Parts[1]);
                    To = int.Parse(Parts[2]);
                    break;
                default:
                    throw new Exception("Invalid Range");
            }
        }

        public PriceRange()
        {
            Step = From = 1;
            To = INFINITY;
        }

        public bool includes(int Amount)
        {
            if (Amount == From || Amount == To)
            {
                return true;
            }
            if (Amount > From && Amount < To)
            {
                return (Amount - From) % Step == 0;
            }
            return false;
        }

        public override string ToString()
        {
            if (To == INFINITY)
            {
                if (Step == 1)
                {
                    return From.ToString();
                }
                else
                {
                    return string.Format("{0}:{1}", From, Step);
                }
            }
            else
            {
                return string.Format("{0}:{1}:{2}", From, Step, To);
            }
        }
    }
}
