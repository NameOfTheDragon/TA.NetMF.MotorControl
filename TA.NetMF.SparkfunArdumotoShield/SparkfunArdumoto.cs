// This file is part of the TA.NetMF.MotorControl project
// 
// Copyright © 2014-2015 Tigra Astronomy, all rights reserved.
// This source code is licensed under the MIT License, see http://opensource.org/licenses/MIT
// 
// File: SparkfunArdumoto.cs  Created: 2015-01-13@13:45
// Last modified: 2015-02-02@18:05 by Tim

using System;
using SecretLabs.NETMF.Hardware.NetduinoPlus;
using TA.NetMF.Motor;

namespace TA.NetMF.ShieldDriver
    {
    public sealed class SparkfunArdumoto
        {
        /// <summary>
        ///   Gets an <see cref="HBridge" /> instance configured for the specified platform and
        ///   hardware.
        /// </summary>
        /// <param name="winding">
        ///   The motor winding number as shown on the shield's silk screen.
        /// </param>
        /// <param name="targetDevice">The target device.</param>
        /// <returns>HBridge.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">targetDevice</exception>
        /// <remarks>
        ///   Netduino 1 did not have a PWM channel in the right place for this shield. Therefore a
        ///   cut-and-strap is necessary. Cut pins marked PWMA and PWMB on the shield, and strap them
        ///   to digital outputs D5 and D6, respectively. On Netduino 2, all the PWM signals are
        ///   brought out to the connector so the shield should work as-is. The driver needs to be
        ///   configured appropriately, and that is the purpose of the optional
        ///   <paramref name="targetDevice" /> parameter.
        /// </remarks>
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
        ///   Creates a bridge configuration for a Sparkfun shield modified to work with Netduino 1.
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
        /// <returns>TA.NetMF.Utils.IStepSequencer.</returns>
        public IStepSequencer GetMicrosteppingStepperMotor(int microsteps, HBridge phase1, HBridge phase2)
            {
            return new TwoPhaseMicrosteppingSequencer(phase1, phase2, microsteps);
            }

        public void InitializeShield() {}

        /// <summary>
        ///   Gets an <see cref="HBridge" /> instance configured for the specified motor connector and
        ///   target hardware. This is a convenent alias for <see cref="GetHBridge" />.
        /// </summary>
        /// <param name="connector">The connector, as indicated on the shield's silk screen.</param>
        /// <param name="targetPlatform">The target platform.</param>
        /// <returns>HBridge.</returns>
        /// <remarks>
        ///   Netduino 1 did not have a PWM channel in the right place for this shield. Therefore a
        ///   cut-and-strap is necessary. Cut pins marked PWMA and PWMB on the shield, and strap them
        ///   to digital outputs D5 and D6, respectively. On Netduino 2, all the PWM signals are
        ///   brought out to the connector so the shield should work as-is. The driver needs to be
        ///   configured appropriately, and that is the purpose of the optional
        ///   <paramref name="targetDevice" /> parameter.
        /// </remarks>
        public HBridge GetDcMotor(Connector connector, TargetDevice targetPlatform = TargetDevice.Netduino2)
            {
            return GetHBridge(connector, targetPlatform);
            }
        }
    }
