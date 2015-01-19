// This file is part of the TA.NetMF.MotorControl project
// 
// Copyright © 2014-2014 Tigra Astronomy, all rights reserved.
// This source code is licensed under the MIT License, see http://opensource.org/licenses/MIT
// 
// File: ArdumotoShield.cs  Created: 2014-06-05@02:27
// Last modified: 2014-11-30@13:57 by Tim

using System;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware.NetduinoPlus;
using TA.NetMF.Motor;


namespace TA.NetMF.SparkfunArdumotoShield
    {
    public sealed class ArdumotoShield
        {
        public HBridge GetHBridge(Connector winding, TargetDevice targetDevice = TargetDevice.Netduino2)
            {
            switch (targetDevice)
                {
                case TargetDevice.Netduino:
                case TargetDevice.NeduinoPlus:
                    return Netduino1BridgeConfiguration(winding);
                case TargetDevice.Netduino2:
                case TargetDevice.NetduinoPlus2:
                    return Netduino2BridgeConfiguration(winding);
                default:
                    throw new ArgumentOutOfRangeException("targetDevice");
                }
            }

        /// <summary>
        /// Creates a bridge configuration for a Sparkfun shield modified to work with Netduino 1.
        /// </summary>
        /// <param name="winding">The winding.</param>
        /// <returns>HBridge.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">winding</exception>
        HBridge Netduino1BridgeConfiguration(Connector winding)
            {
            switch (winding)
                {
                case Connector.A:
                    return new SimpleHBridge(PWMChannels.PWM_PIN_D5, Pins.GPIO_PIN_D12);
                case Connector.B:
                    return new SimpleHBridge(PWMChannels.PWM_PIN_D6, Pins.GPIO_PIN_D13);
                default:
                    throw new ArgumentOutOfRangeException("winding");
                }
            }

        static HBridge Netduino2BridgeConfiguration(Connector winding)
            {
            switch (winding)
                {
                case Connector.A:
                    return new SimpleHBridge(PWMChannels.PWM_PIN_D3, Pins.GPIO_PIN_D12);
                    break;
                case Connector.B:
                    return new SimpleHBridge(PWMChannels.PWM_PIN_D11, Pins.GPIO_PIN_D13);
                default:
                    throw new ArgumentOutOfRangeException("winding");
                }
            }

        /// <summary>
        ///   Gets a stepper motor configured for the specified number of microsteps.
        /// </summary>
        /// <param name="microsteps">The microsteps.</param>
        /// <returns>TA.NetMF.Utils.IStepperMotorControl.</returns>
        public IStepperMotorControl GetMicrosteppingStepperMotor(int microsteps, HBridge phase1, HBridge phase2)
            {
            return new MicrosteppingStepperMotor(phase1, phase2, microsteps);
            }

        public void InitializeShield()
            {
            }
        }

    public enum Connector
        {
        A,B
        }

    public enum TargetDevice
        {
        Netduino,
        NeduinoPlus,
        Netduino2,
        NetduinoPlus2
        }
    }
