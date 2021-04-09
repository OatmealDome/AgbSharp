using System.Collections.Generic;
using System.Linq;
using AgbSharp.Core.Cpu.Status;
using AgbSharp.Core.Util;

namespace AgbSharp.Core.Cpu.Interpreter
{
    abstract class InstructionSetInterpreter
    {
        protected AgbCpu Cpu;

        // Easy accessors for properties in AgbCpu
        protected ProgramStatus CurrentStatus
        {
            get
            {
                return Cpu.CurrentStatus;
            }
        }

        protected ProgramStatus SavedStatus
        {
            get
            {
                return Cpu.CurrentSavedStatus;
            }
        }

        // Constants to use with Reg()
        protected const int SP = 13;
        protected const int LR = 14;
        protected const int PC = 15;

        protected InstructionSetInterpreter(AgbCpu cpu)
        {
            Cpu = cpu;
        }

        //
        // Helper functions
        //

        protected ref uint Reg(int reg)
        {
            return ref Cpu.CurrentRegisterSet.GetRegister(reg);
        }

        protected void InterpreterAssert(string message)
        {
            throw new CpuException(message);
        }

        protected void InterpreterAssert(bool expression, string message)
        {
            if (!expression)
            {
                throw new CpuException(message);
            }
        }

        //
        // Shift
        //

        protected uint PerformShift(ShiftType shiftType, uint operand, int shift, bool isZeroSpecialCase, bool setConditionCodes)
        {
            switch (shiftType)
            {
                case ShiftType.LogicalLeft: // LSL
                    if (!isZeroSpecialCase)
                    {
                        if (setConditionCodes)
                        {
                            CurrentStatus.Carry = BitUtil.IsBitSet(operand, 32 - shift);
                        }

                        // C# takes the shift amount as (right-hand operand & 0x1F), so any value 32 or higher will be treated as 0.
                        // https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/bitwise-and-shift-operators
                        if (shift >= 32)
                        {
                            operand = 0;
                        }
                        else
                        {
                            operand <<= shift;
                        }
                    }

                    break;
                case ShiftType.LogicalRight: // LSR
                    if (isZeroSpecialCase)
                    {
                        if (setConditionCodes)
                        {
                            CurrentStatus.Carry = BitUtil.IsBitSet(operand, 31);
                        }

                        operand = 0;
                    }
                    else
                    {
                        if (setConditionCodes)
                        {
                            CurrentStatus.Carry = BitUtil.IsBitSet(operand, shift - 1);
                        }

                        if (shift >= 32)
                        {
                            operand = 0;
                        }
                        else
                        {
                            operand >>= shift;
                        }
                    }

                    break;
                case ShiftType.ArithmaticRight: // ASR
                    if (isZeroSpecialCase || shift >= 32)
                    {
                        bool msb = BitUtil.IsBitSet(operand, 31);

                        if (msb)
                        {
                            operand = 0xFFFFFFFF;
                        }
                        else
                        {
                            operand = 0x00000000;
                        }

                        if (setConditionCodes)
                        {
                            CurrentStatus.Carry = msb;
                        }
                    }
                    else
                    {
                        if (setConditionCodes)
                        {
                            CurrentStatus.Carry = BitUtil.IsBitSet(operand, shift - 1);
                        }

                        // C# will do an ASR if the left operand is an int
                        operand = (uint)((int)operand >> shift);
                    }

                    break;
                case ShiftType.RotateRight: // ROR
                    if (isZeroSpecialCase)
                    {
                        // This operation is like RRX#1. See here for a diagram:
                        // http://www-mdp.eng.cam.ac.uk/web/library/enginfo/mdp_micro/lecture4/lecture4-3-4.html

                        bool oldCarry = CurrentStatus.Carry;

                        CurrentStatus.Carry = BitUtil.IsBitSet(operand, 0);

                        operand >>= 1;

                        if (oldCarry)
                        {
                            BitUtil.SetBit(ref operand, 31);
                        }
                        else
                        {
                            BitUtil.ClearBit(ref operand, 31);
                        }
                    }
                    else
                    {
                        if (setConditionCodes)
                        {
                            CurrentStatus.Carry = BitUtil.IsBitSet(operand, shift - 1);
                        }

                        operand = BitUtil.RotateRight(operand, shift);
                    }

                    break;
            }

            return operand;
        }

        //
        // Addition/Subtraction helpers
        //
        // Thanks to mGBA for easy shortcuts.
        // include/mgba/internal/arm/isa-inlines.h
        //
        // Additional notes on Overflow flag:
        // http://teaching.idallen.com/dat2343/10f/notes/040_overflow.txt
        //

        protected void SetCarryAndOverflowOnAddition(uint first, uint second, uint result)
        {
            CurrentStatus.Carry = (BitUtil.GetBit(first, 31) + BitUtil.GetBit(second, 31)) > BitUtil.GetBit(result, 31);
            CurrentStatus.Overflow = !BitUtil.IsBitSet(first ^ second, 31) && BitUtil.IsBitSet(first ^ result, 31);
        }

        protected void SetCarryAndOverflowOnSubtraction(uint first, uint second, uint result, bool carry)
        {
            if (carry)
            {
                CurrentStatus.Carry = first >= (second + (CurrentStatus.Carry ? 0 : 1));
            }
            else
            {
                CurrentStatus.Carry = first >= second;
            }

            CurrentStatus.Overflow = BitUtil.IsBitSet(first ^ second, 31) && BitUtil.IsBitSet(first ^ result, 31);
        }

        //
        // Multiply helpers
        //

        protected int GetMBasedOnAllOnesOrZerosForMultiply(uint reg)
        {
            if ((reg & 0xFFFFFF00) == 0xFFFFFF00 || (reg & 0xFFFFFF00) == 0x00000000)
            {
                return 1;
            }
            else if ((reg & 0xFFFF0000) == 0xFFFF0000 || (reg & 0xFFFF0000) == 0x00000000)
            {
                return 2;
            }
            else if ((reg & 0xFF000000) == 0xFF000000 || (reg & 0xFF000000) == 0x00000000)
            {
                return 3;
            }
            else
            {
                return 4;
            }
        }

        protected int GetMBasedOnAllZerosForMultiply(uint reg)
        {
            if ((reg & 0xFFFFFF00) == 0x00000000)
            {
                return 1;
            }
            else if ((reg & 0xFFFF0000) == 0x00000000)
            {
                return 2;
            }
            else if ((reg & 0xFF000000) == 0x00000000)
            {
                return 3;
            }
            else
            {
                return 4;
            }
        }

        //
        // Data Block Transfer (LDM / STM) Helpers
        //

        protected int PerformDataBlockTransfer(int nRegNum, bool isPreIndex, bool isUp, bool isWriteBack, bool isLoad, bool useUserBank, uint regBitfield)
        {
            // An empty register bitfield is a special case which is handled separately
            
            if (regBitfield == 0)
            {
                return PerformDataBlockTransferEmptyBitfield(ref Reg(nRegNum), isPreIndex, isUp, isWriteBack, isLoad, useUserBank);
            }

            ref uint nReg = ref Reg(nRegNum);
            uint nRegInitial = nReg; // save for later in case its necessary

            // Find all the registers to transfer

            List<int> registersToTransfer = new List<int>();

            for (int i = 0; i < 16; i++)
            {
                if (BitUtil.IsBitSet(regBitfield, i))
                {
                    registersToTransfer.Add(i);
                }
            }

            registersToTransfer.Sort();

            // Find all the memory addresses to write those registers to

            List<uint> adddresses = new List<uint>();

            uint currentAddress = nReg;

            for (int i = 0; i < registersToTransfer.Count; i++)
            {
                uint address;

                if (isUp)
                {
                    address = currentAddress + 4;
                }
                else
                {
                    address = currentAddress - 4;
                }

                uint effectiveAddress;
                if (isPreIndex)
                {
                    effectiveAddress = address;
                }
                else
                {
                    effectiveAddress = currentAddress;
                }

                adddresses.Add(effectiveAddress);

                currentAddress = address;
            }

            adddresses.Sort();

            // Write back the final address if necessary

            if (isWriteBack)
            {
                nReg = currentAddress;
            }

            // Write out the registers to the calculated addresses 

            Dictionary<int, uint> zippedDict = registersToTransfer.Zip(adddresses, (k, v) => new { k, v })
                                                .ToDictionary(x => x.k, x => x.v);

            for (int i = 0; i < registersToTransfer.Count; i++)
            {
                int regNum = registersToTransfer[i];
                uint regAddress = adddresses[i];

                if (isLoad)
                {
                    uint value = Cpu.MemoryMap.ReadU32(regAddress);

                    if (useUserBank)
                    {
                        Cpu.RegUser(regNum) = value;
                    }
                    else
                    {
                        Reg(regNum) = value;
                    }
                }
                else
                {
                    uint regValue;

                    if (regNum == nRegNum)
                    {
                        // If the base register is in the register bitfield in an STM, weird things happen.
                        // If the first register transferred is the base register, the initial value is
                        // written back, which will overwrite the write-back address (if it was enabled).
                        // Otherwise, the last address in the list of addresses is written to the register.
                        if (registersToTransfer[0] != nRegNum)
                        {
                            regValue = currentAddress;
                        }
                        else
                        {
                            regValue = nRegInitial;
                        }
                    }
                    else
                    {
                        if (useUserBank)
                        {
                            regValue = Cpu.RegUser(regNum);
                        }
                        else
                        {
                            regValue = Reg(regNum);
                        }

                        if (regNum == PC)
                        {
                            regValue += 8;
                        }
                    }

                    Cpu.MemoryMap.WriteU32(regAddress, regValue);
                }
            }

            return registersToTransfer.Count;
        }

        protected int PerformDataBlockTransferEmptyBitfield(ref uint nReg, bool isPreIndex, bool isUp, bool isWriteBack, bool isLoad, bool useUserBank)
        {
            // TODO: Is this behaviour even correct?
            // It matches what the 5xx series of ARM tests from gba-tests expects, so maybe it really is?

            // When LDM / STM have no registers specified in the bitfield, PC will be loaded / stored
            // and the written back address (if any) will be nReg +/- 0x40. The effective address where
            // PC is loaded / stored depends on the values of the increment flag and the pre-index flag.

            uint effectiveAddress;

            if (isUp)
            {
                if (isPreIndex)
                {
                    effectiveAddress = nReg + 0x4;
                }
                else
                {
                    effectiveAddress = nReg;
                }
            }
            else
            {
                if (isPreIndex)
                {
                    effectiveAddress = nReg - 0x40;
                }
                else
                {
                    effectiveAddress = nReg - 0x3c;
                }
            }

            uint regValue;

            if (isLoad)
            {
                regValue = Cpu.MemoryMap.ReadU32(effectiveAddress);

                if (useUserBank)
                {
                    Cpu.RegUser(PC) = regValue;
                }
                else
                {
                    Reg(PC) = regValue;
                }
            }
            else
            {
                if (useUserBank)
                {
                    regValue = Cpu.RegUser(PC);
                }
                else
                {
                    regValue = Reg(PC);
                }

                regValue += 0x8;

                Cpu.MemoryMap.WriteU32(effectiveAddress, regValue);
            }

            if (isWriteBack)
            {
                if (isUp)
                {
                    nReg += 0x40;
                }
                else
                {
                    nReg -= 0x40;
                }
            }

            return 1;
        }

        //
        // SWI helper
        //

        protected void PerformSwi()
        {
            uint previousPsr = CurrentStatus.RegisterValue;

            // Enter Supervisor now so that we can access banked registers
            CurrentStatus.Mode = CpuMode.Supervisor;

            // SPSR_svc = CPSR (old)
            SavedStatus.RegisterValue = previousPsr;

            // LR_svc = PC
            Reg(LR) = Reg(PC);

            // Modify CPSR
            CurrentStatus.Thumb = false;
            CurrentStatus.IrqDisable = true;
            CurrentStatus.FastIrqDisable = true;

            Reg(PC) = 0x00000008;
        }

        //
        // Interpreter must implement these
        //

        public abstract int Step();

    }
}