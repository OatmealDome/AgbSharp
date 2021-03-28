using AgbSharp.Core.Cpu;
using AgbSharp.Core.Memory.Ram;
using Xunit;

namespace AgbSharp.Core.Tests.Cpu.Thumb
{
    public class ThumbInterpreter_FormThirteenAddSpOffset_Tests
    {
        [Fact]
        public void AddToSp_WithPositiveOffset_SpCorrect()
        {
            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.CurrentRegisterSet.GetRegister(CpuUtil.SP) = InternalWramRegion.REGION_START;

            CpuUtil.RunCpu(cpu, new ushort[]
            {
                0xB07F // ADD SP, #0x1FC
            }, true);

            Assert.Equal(InternalWramRegion.REGION_START + 0x1FC, cpu.CurrentRegisterSet.GetRegister(CpuUtil.SP));
        }

        [Fact]
        public void AddToSp_WithNegativeOffset_SpCorrect()
        {
            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.CurrentRegisterSet.GetRegister(CpuUtil.SP) = InternalWramRegion.REGION_START;

            CpuUtil.RunCpu(cpu, new ushort[]
            {
                0xB0FF // ADD SP, #-0x1FC
            }, true);

            Assert.Equal(InternalWramRegion.REGION_START - 0x1FC, cpu.CurrentRegisterSet.GetRegister(CpuUtil.SP));
        }
    }
}