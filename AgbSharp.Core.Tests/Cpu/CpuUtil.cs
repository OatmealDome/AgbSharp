using AgbSharp.Core.Cpu;
using AgbSharp.Core.Memory;
using AgbSharp.Core.Memory.Ram;
using AgbSharp.Core.Util;

namespace AgbSharp.Core.Tests.Cpu
{
    class CpuUtil
    {
        public const int SP = 13;
        public const int LR = 14;
        public const int PC = 15;

        public static AgbCpu CreateCpu()
        {
            AgbMemoryMap map = new AgbMemoryMap();

            map.RegisterRegion(new InternalWramRegion());

            AgbCpu cpu = new AgbCpu(map);

            return cpu;
        }

        public static void RunCpu(AgbCpu cpu, uint[] instructions, bool littleEndian = false)
        {
            for (int i = 0; i < instructions.Length; i++)
            {
                uint instruction = instructions[i];

                if (!littleEndian)
                {
                    instruction = ByteUtil.SwapEndianness(instruction);
                }

                cpu.MemoryMap.WriteU32(InternalWramRegion.REGION_START + (uint)i * 4, instruction);
            }

            cpu.CurrentRegisterSet.GetRegister(PC) = InternalWramRegion.REGION_START;

            for (int i = 0; i < instructions.Length; i++)
            {
                cpu.Step();
            }
        }

        public static AgbCpu CreateAndRunCpu(uint[] instructions, bool littleEndian = false)
        {
            AgbCpu cpu = CreateCpu();

            RunCpu(cpu, instructions, littleEndian);

            return cpu;
        }
        
    }
}