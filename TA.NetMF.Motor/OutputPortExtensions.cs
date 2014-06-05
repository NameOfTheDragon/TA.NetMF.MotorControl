// This file is part of the TA.NetMF.MotorControl project
// 
// Copyright © 2014 Tigra Astronomy, all rights reserved.
// This source code is licensed under Creative Commons Attribution International 4.0 license
// http://creativecommons.org/licenses/by/4.0/
// 
// File: OutputPortExtensions.cs  Created: 2014-06-05@02:27
// Last modified: 2014-06-05@12:23 by Tim

using Microsoft.SPOT.Hardware;

namespace TA.NetMF.Motor
    {
    public static class OutputPortExtensions
        {
        public static void High(this OutputPort port)
            {
            port.Write(true);
            }

        public static void Low(this OutputPort port)
            {
            port.Write(false);
            }
        }
    }
