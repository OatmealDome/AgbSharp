namespace AgbSharp.Core.Cpu.Interrupt
{
    public enum InterruptType : int
    {
        VBlank,
        HBlank,
        VCounterMatch,
        TimerZeroOverflow,
        TimerOneOverflow,
        TimerTwoOverflow,
        TimerThreeOverflow,
        Serial,
        DmaZero,
        DmaOne,
        DmaTwo,
        DmaThree,
        Key,
        GamePak,
        UnusedOne,
        UnusedTwo
    }
}