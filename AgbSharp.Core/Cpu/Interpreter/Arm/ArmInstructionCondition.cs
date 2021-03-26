namespace AgbSharp.Core.Cpu.Interpreter.Arm
{
    public enum ArmInstructionCondition : int
    {
        Equal = 0, // EQ
        NotEqual = 1, // NE
        CarrySet = 2, // CS / HS
        CarryClear = 3, // CC / LO
        Negative = 4, // MI (minus)
        Positive = 5, // PL (plus)
        Overflow = 6, // VS (V set)
        OverflowClear = 7, // VC (V clear),
        UnsignedHigher = 8, // HI
        UnsignedLowerOrSame = 9, // LS
        GreaterThanOrEqual = 0xA, // GE
        LessThan = 0xB, // LT
        GreaterThan = 0xC, // GT
        LessThanOrEqual = 0xD, // LE
        Always = 0xE, // AL
        Reserved = 0xF // never (ARMv1, ARMv2) or reserved (ARMv3+)
    }
}