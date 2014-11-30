// This file is part of the TA.NetMF.MotorControl project
// 
// Copyright © 2014-2014 Tigra Astronomy, all rights reserved.
// This source code is licensed under the MIT License, see http://opensource.org/licenses/MIT
// 
// File: OutputPortExtensions.cs  Created: 2014-06-05@02:27
// Last modified: 2014-11-30@13:57 by Tim
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
