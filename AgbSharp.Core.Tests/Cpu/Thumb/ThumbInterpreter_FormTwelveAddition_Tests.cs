using AgbSharp.Core.Cpu;
using AgbSharp.Core.Memory.Ram;
using Xunit;

namespace AgbSharp.Core.Tests.Cpu.Thumb
{
    public class ThumbInterpreter_FormTwelveAddition_Tests
    {
        [Fact]
        public void GetRelativeAddress_LoadRegSevenWithPcRelativeAddress_RegSevenCorrect()
        {
            const uint targetAddress = InternalWramRegion.REGION_START + 0x4 + 0x3FC;

            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.CurrentRegisterSet.GetRegister(7) = 0xDEADBEEF;

            CpuUtil.RunCpu(cpu, new ushort[]
            {
                0xA7FF // ADD r7, PC, #0x3FC
            }, true);

            Assert.Equal(targetAddress, cpu.CurrentRegisterSet.GetRegister(7));
        }

        [Fact]
        public void GetRelativeAddress_LoadRegSevenWithSpRelativeAddress_RegSevenCorrect()
        {
            const uint targetAddress = InternalWramRegion.REGION_START + 0x3FC;

            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.CurrentRegisterSet.GetRegister(7) = 0xDEADBEEF;
            cpu.CurrentRegisterSet.GetRegister(CpuUtil.SP) = InternalWramRegion.REGION_START;

            CpuUtil.RunCpu(cpu, new ushort[]
            {
                0xAFFF // ADD r7, SP, #0x3FC
            }, true);

            Assert.Equal(targetAddress, cpu.CurrentRegisterSet.GetRegister(7));
        }
        
    }
}