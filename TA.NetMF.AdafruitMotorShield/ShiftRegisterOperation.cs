namespace TA.NetMF.AdafruitMotorShieldV1
    {
    /// <summary>
    ///   Struct ShiftRegisterOperation - represents a single bit operation on the parallel output register.
    /// </summary>
    public struct ShiftRegisterOperation
        {
        public ShiftRegisterOperation(ushort bitNumber, bool value) : this()
            {
            BitNumber = bitNumber;
            Value = value;
            }

        public ushort BitNumber { get; private set; }
        public bool Value { get; private set; }

        public override string ToString()
            {
            return "Bit "+BitNumber.ToString()+"="+Value.ToString();
            }
        }
    }