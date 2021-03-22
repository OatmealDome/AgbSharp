using AgbSharp.Core.Cpu.Status;

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
        // Interpreter must implement these
        //

        public abstract int Step();

    }
}