using System;
using System.Collections.Generic;

namespace Osrs.Oncor.Excel
{
    public class NumericTypes : HashSet<Type>
    {
        public static bool IsNumeric(Type typeUnderTest)
        {
            NumericTypes test = new NumericTypes
                {
                    typeof(decimal),
                    typeof(byte), typeof(sbyte),
                    typeof(short), typeof(ushort),
                    typeof(int), typeof(uint),
                    typeof(long), typeof(ulong),
                    typeof(float), typeof(double)
                };
            return test.Contains(typeUnderTest);
        }
    }
}
