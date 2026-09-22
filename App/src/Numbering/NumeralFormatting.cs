namespace BlueHeighliner.DocumentGenerator.Numbering;

/// <summary>Formatting of positive integers as Roman numerals or bijective alphabetic numerals.</summary>
internal static class NumeralFormatting
{
    private static readonly (int Value, string Numeral)[] romanNumerals =
    [
        (1000, "M"), (900, "CM"), (500, "D"), (400, "CD"),
        (100, "C"), (90, "XC"), (50, "L"), (40, "XL"),
        (10, "X"), (9, "IX"), (5, "V"), (4, "IV"), (1, "I"),
    ];

    extension(int number)
    {
        /// <summary>Formats this number as an uppercase Roman numeral.</summary>
        /// <returns>The Roman numeral text.</returns>
        /// <exception cref="ArgumentOutOfRangeException">The number is not between 1 and 3999.</exception>
        public string ToRoman()
        {
            if (number is < 1 or > 3999)
            {
                throw new ArgumentOutOfRangeException(nameof(number), number, "Roman numerals only support values between 1 and 3999.");
            }

            StringBuilder builder = new();
            int remaining = number;
            foreach ((int value, string numeral) in romanNumerals)
            {
                while (remaining >= value)
                {
                    builder.Append(numeral);
                    remaining -= value;
                }
            }

            return builder.ToString();
        }

        /// <summary>
        /// Formats this number as an uppercase bijective base-26 alphabetic numeral (1 = "A", 26 = "Z",
        /// 27 = "AA", ...).
        /// </summary>
        /// <returns>The alphabetic numeral text.</returns>
        /// <exception cref="ArgumentOutOfRangeException">The number is less than 1.</exception>
        public string ToAlphabetic()
        {
            if (number < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(number), number, "Alphabetic numerals only support values of 1 or greater.");
            }

            StringBuilder builder = new();
            int remaining = number;
            while (remaining > 0)
            {
                remaining--;
                builder.Insert(0, (char)('A' + (remaining % 26)));
                remaining /= 26;
            }

            return builder.ToString();
        }
    }
}
