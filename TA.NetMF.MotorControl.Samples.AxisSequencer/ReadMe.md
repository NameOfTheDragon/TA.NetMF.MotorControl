Sequenced Steppers Sample
=========================

This sample demonstrates how to use the `MotorStopped` event from `AcceleratingStepperMotor` to coordinate one or more motors. The sequencing is orchestrated by a class called `DualAxisSequencer`.

The sample picks a random target position then calls `DualAxisSequencer.RunInSequence()`. This method does not block, but returns as soon as the first motor is started. The two motors will run to their target positions one after the other, autonomously.

`DualAxisSequencer` provides a synchronization method called `BlockUntilSequenceComplete`. A client may call this method if it needs to block until the sequence is completed. The demo code uses this blocking method; once control is returned, it waits for a further 5 seconds then repeats.

The `AcceleratingStepperMotor` class is designed so that all axes can run independently and autonomously. Axis synchronization is a separate responsibility and is best performed by a wrapper class dedicated to that purpose, such as the `DualAxisSequencer` class in the sample.

Motor Shield Selection
======================

The projects are configured for Netduino Plus 2 hardware and .net MicroFramework 4.3.
If you use different hardware or a different framework version,
then you must manually re-target the projects to the correct device and framework.  

Each of the sample projects contains a region near the top of Program.cs where you can 
configure which of the supported motor shields you want to use. Uncomment exactly one of
the shield definitions and ensure the others are commented out. By default, a 'motor simulator' 
shield is used, which just flashes some LEDs and will not actually drive a motor.
