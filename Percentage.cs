using System;
using System.Collections.Generic;
using System.Text;

namespace CashCam
{
    struct Percentage
    {
        public int PercentageInt { get { return (int)Math.Round((double)Divisor / Dividend); } }
        public float PercentageFloat { get { return (float)Divisor / Dividend; } }
        public double PercentageDouble { get { return (double)Divisor / Dividend; } }

        public long Divisor { get; set; }
        public long Dividend { get; set; }

        public Percentage(long divisor, long dividend)
        {
            this.Divisor = divisor;
            this.Dividend = dividend;
        }

        public override string ToString()
        {
            return Divisor + " / " + Dividend + " = " + (PercentageFloat * 100) + "%";
        }
    }
}
