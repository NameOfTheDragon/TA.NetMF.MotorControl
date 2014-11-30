// This file is part of the TA.NetMF.MotorControl project
// 
// Copyright © 2014-2014 Tigra Astronomy, all rights reserved.
// This source code is licensed under the MIT License, see http://opensource.org/licenses/MIT
// 
// File: Octet.cs  Created: 2014-06-05@02:27
// Last modified: 2014-11-30@13:57 by Tim
namespace TA.NetMF.Motor
    {
    /// <summary>
    ///   Struct Octet - an immutable representation of an 8-bit integer, with each bit individually addressable.
    /// </summary>
    public struct Octet
        {
        static readonly Octet max = FromInt(0xFF);
        static readonly Octet zero = FromInt(0);
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
        }
    }
