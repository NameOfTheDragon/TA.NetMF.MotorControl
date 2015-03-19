// This file is part of the TA.NetMF.MotorControl project
// 
// Copyright © 2014-2015 Tigra Astronomy, all rights reserved.
// This source code is licensed under the MIT License, see http://opensource.org/licenses/MIT
// 
// File: Octet.cs  Created: 2015-01-13@13:45
// Last modified: 2015-01-17@22:33 by Tim

using System.Text;

namespace TA.NetMF.Motor
    {
    /// <summary>
    ///   Struct Octet - an immutable representation of an 8-bit integer, with each bit individually addressable.
    /// </summary>
    public struct Octet
        {
        readonly bool[] bits;

        /// <summary>
        ///   Initializes a new instance of the <see cref="Octet" /> struct from a ready-made bit array.
        /// </summary>
        /// <param name="bits">The bits.</param>
        Octet(bool[] bits)
            {
            this.bits = bits;
            }

        /// <summary>
        ///   Gets an Octet with all the bits set to zero.
        /// </summary>
        public static Octet Zero { get { return zero; } }

        /// <summary>
        ///   Gets an Octet set to the maximum value (i.e. all the bits set to one).
        /// </summary>
        public static Octet Max { get { return max; } }

        public bool this[int bit] { get { return bits[bit]; } }

        /// <summary>
        ///   Factory method: create an Octet from an integer.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns>Octet.</returns>
        public static Octet FromInt(int source)
            {
            var bits = new bool[8];
            for (var i = 0; i < 8; i++)
                {
                var bit = source & 0x01;
                bits[i] = bit == 0 ? false : true;
                source >>= 1;
                }
            return new Octet(bits);
            }

        /// <summary>
        ///   Factory method: create an Octet from an unisgned integer.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns>Octet.</returns>
        public static Octet FromUnsignedInt(uint source)
            {
            return FromInt((int)source);
            }

        /// <summary>
        ///   Returns a new octet with the specified bit number set to the specified value.
        ///   Other bits are duplicated.
        /// </summary>
        /// <param name="bit">The bit number to be modified.</param>
        /// <param name="value">The value of the specified bit number.</param>
        /// <returns>A new octet instance with the specified bit number set to the specified value.</returns>
        public Octet WithBitSetTo(ushort bit, bool value)
            {
            var newBits = new bool[8];
            bits.CopyTo(newBits, 0);
            newBits[bit] = value;
            return new Octet(newBits);
            }

        public override string ToString()
            {
            var builder = new StringBuilder();
            for (int i = 7; i >= 0; i--)
                {
                builder.Append(bits[i] ? '1' : '0');
                builder.Append(' ');
                }
            builder.Length -= 1;
            return builder.ToString();
            }

        static readonly Octet max = FromInt(0xFF);
        static readonly Octet zero = FromInt(0);
        }
    }
