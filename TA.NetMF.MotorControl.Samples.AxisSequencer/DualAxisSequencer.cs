// This file is part of the TA.NetMF.MotorControl project
// 
// Copyright © 2014 Tigra Astronomy, all rights reserved.
// This source code is licensed under Creative Commons Attribution International 4.0 license
// http://creativecommons.org/licenses/by/4.0/
// 
// File: DualAxisSequencer.cs  Created: 2014-10-14@00:31
// Last modified: 2014-10-14@00:32 by Tim

using System.Threading;
using TA.NetMF.Motor;

namespace TA.NetMF.MotorControl.Samples.AxisSequencer
    {
    /// <summary>
    ///   Class DualAxisSequencer. Moves two axes in sequence and signals any waiting threads when the sequence is complete.
    /// </summary>
    internal class DualAxisSequencer
        {
        readonly AcceleratingStepperMotor firstAxis;
        readonly AcceleratingStepperMotor secondAxis;
        readonly ManualResetEvent sequenceComplete = new ManualResetEvent(true); // start signalled
        int firstTarget;
        int secondTarget;

        /// <summary>
        ///   Initializes a new instance of the <see cref="DualAxisSequencer" /> class.
        /// </summary>
        /// <param name="firstAxis">The first axis.</param>
        /// <param name="secondAxis">The second axis.</param>
        public DualAxisSequencer(AcceleratingStepperMotor firstAxis, AcceleratingStepperMotor secondAxis)
            {
            this.firstAxis = firstAxis;
            this.secondAxis = secondAxis;
            firstAxis.MotorStopped += FirstAxisMotorStopped;
            secondAxis.MotorStopped += SecondAxisMotorStopped;
            }

        /// <summary>
        ///   Clients may wait on this signal if they need to block until the sequence is complete.
        /// </summary>
        /// <value>A wait handle that is signalled when the dual axis sequence is complete.</value>
        protected ManualResetEvent SequenceComplete { get { return sequenceComplete; } }

        /// <summary>
        ///   Blocks the until sequence is complete. Once a sequence has been started using <see cref="RunInSequence" />,
        ///   clients may call this method to wait for the end of the sequence. If no sequence is in progress,
        ///   returns immediately; otherwise blocks the current thread until the sequence has run to completion.
        /// </summary>
        public void BlockUntilSequenceComplete()
            {
            SequenceComplete.WaitOne();
            }

        /// <summary>
        ///   Received the MotorStopped event from the first axis and starts the second axis.
        /// </summary>
        /// <param name="axis">The axis.</param>
        void FirstAxisMotorStopped(AcceleratingStepperMotor axis)
            {
            secondAxis.MoveToTargetPosition(secondTarget);
            }

        /// <summary>
        ///   Receives the MotorStopped event from the second axis and signals any waiting threads.
        /// </summary>
        /// <param name="axis">The axis.</param>
        void SecondAxisMotorStopped(AcceleratingStepperMotor axis)
            {
            SequenceComplete.Set(); // Unblock waiting threads
            }

        /// <summary>
        ///   Runs the two axes in sequence to the specified target positions.
        /// </summary>
        /// <param name="firstAxisTarget">The first axis' target position.</param>
        /// <param name="secondAxisTarget">The second axis' target position.</param>
        public void RunInSequence(int firstAxisTarget, int secondAxisTarget)
            {
            firstTarget = firstAxisTarget;
            secondTarget = secondAxisTarget;
            SequenceComplete.Reset(); // Start blocking waiters.
            firstAxis.MoveToTargetPosition(firstAxisTarget);
            // Does not block, returns immediately.
            }
        }
    }
