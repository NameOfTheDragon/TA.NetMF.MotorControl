using System;
using Microsoft.SPOT;

namespace TA.AdafruitMotorShield
{
    /// <summary>
    /// Struct Octet - an immutable representation of an 8-bit integer, with each bit individually addressable.
    /// </summary>
    internal struct Octet
    {
        bool[] bits;
        static Octet zero = Octet.FromInt(0);
        static Octet max = Octet.FromInt(0xFF);
        /// <summary>
        /// Gets an Octet with all the bits set to zero.
        /// </summary>
        public static Octet Zero { get { return zero; } }

        /// <summary>
        /// Gets an Octet set to the maximum value (i.e. all the bits set to one).
        /// </summary>
        public static Octet Max { get { return max; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="Octet"/> struct from a ready-made bit array.
        /// </summary>
        /// <param name="bits">The bits.</param>
        Octet(bool[] bits)
        {
            this.bits = bits;
        }

        public bool this[int bit]
        {
            get { return bits[bit]; }
        }

        /// <summary>
        /// Factory method: create an Octet from an integer.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns>Octet.</returns>
        public static Octet FromInt(int source)
        {
            var bits = new bool[8];
            for (int i = 0; i < 8; i++)
            {
                var bit = source & 0x01;
                bits[i] = bit == 0 ? false : true;
                source >>= 1;
            }
            return new Octet(bits);
        }

        /// <summary>
        /// Factory method: create an Octet from an unisgned integer.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns>Octet.</returns>
        public static Octet FromUnsignedInt(uint source)
        {
            return FromInt((int)source);
        }
    }
}
