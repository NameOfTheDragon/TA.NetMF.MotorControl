using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

namespace TA.NetMF.AdafruitMotorShieldV2
{
    public class MotorShield
    {
        Pca9685PwmController PwmController;

        /// <summary>
        /// Initializes a new instance of the <see cref="MotorShield"/> class at the specified I2C address.
        /// </summary>
        /// <param name="address">The address.</param>
        public MotorShield(ushort address)
            {
            PwmController = new Pca9685PwmController(address);
            }

        void InitializeShield()
            {
            PwmController.Reset();
            }

    }
}
