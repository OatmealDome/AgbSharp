using AgbSharp.Core.Cpu;
using AgbSharp.Core.Memory.Ram;
using Xunit;

namespace AgbSharp.Core.Tests.Cpu.Thumb
{
    public class ThumbInterpreter_FormFiveRegisterOperation_Tests
    {
        #region ADD

        [Fact]
        public void Add_AddRegNineToRegEight_RegEightCorrectAndFlagsUnaffected()
        {
            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.CurrentRegisterSet.GetRegister(8) = 0xFFFFFFFF;
            cpu.CurrentRegisterSet.GetRegister(9) = 0x00000002;

            CpuUtil.RunCpu(cpu, new ushort[]
            {
                0xc844 // ADD r8, r9
            });

            Assert.Equal((uint)0x00000001, cpu.CurrentRegisterSet.GetRegister(8));
            Assert.Equal((uint)0x00000002, cpu.CurrentRegisterSet.GetRegister(9));
            Assert.False(cpu.CurrentStatus.Negative);
            Assert.False(cpu.CurrentStatus.Zero);
            Assert.False(cpu.CurrentStatus.Carry);
            Assert.False(cpu.CurrentStatus.Overflow);
        }

        #endregion

        #region CMP

        [Fact]
        public void Compare_RegNineFromRegEight_FlagsSet()
        {
            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.CurrentRegisterSet.GetRegister(8) = 0x0000F000;
            cpu.CurrentRegisterSet.GetRegister(9) = 0x00001000;

            CpuUtil.RunCpu(cpu, new ushort[]
            {
                0xC845 // CMP r8, r9
            });

            Assert.Equal((uint)0x0000F000, cpu.CurrentRegisterSet.GetRegister(8));
            Assert.Equal((uint)0x00001000, cpu.CurrentRegisterSet.GetRegister(9));
            Assert.False(cpu.CurrentStatus.Negative);
            Assert.False(cpu.CurrentStatus.Zero);
            Assert.True(cpu.CurrentStatus.Carry);
            Assert.False(cpu.CurrentStatus.Overflow);
        }

        #endregion

        #region MOV

        [Fact]
        public void Move_MoveRegNineToRegEight_RegEightCorrect()
        {
            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.CurrentRegisterSet.GetRegister(8) = 0xCAFEBABE;
            cpu.CurrentRegisterSet.GetRegister(9) = 0xDEADBEEF;

            CpuUtil.RunCpu(cpu, new ushort[]
            {
                0xC846 // MOV r8, r9
            });

            Assert.Equal((uint)0xDEADBEEF, cpu.CurrentRegisterSet.GetRegister(8));
            Assert.Equal((uint)0xDEADBEEF, cpu.CurrentRegisterSet.GetRegister(9));
        }

        #endregion

        #region BX

        [Fact]
        public void BranchExchange_BranchToRegEightInArm_PcCorrectAndInArmMode()
        {
            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.CurrentRegisterSet.GetRegister(8) = InternalWramRegion.REGION_START;

            CpuUtil.RunCpu(cpu, new ushort[]
            {
                0xc747 // BX r8
            });

            Assert.Equal(InternalWramRegion.REGION_START, cpu.CurrentRegisterSet.GetRegister(CpuUtil.PC));
            Assert.Equal(InternalWramRegion.REGION_START, cpu.CurrentRegisterSet.GetRegister(8));
            Assert.False(cpu.CurrentStatus.Thumb);
        }

        [Fact]
        public void BranchExchange_BranchToRegEight_PcCorrectAndInThumbMode()
        {
            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.CurrentRegisterSet.GetRegister(8) = InternalWramRegion.REGION_START + 1;

            CpuUtil.RunCpu(cpu, new ushort[]
            {
                0xc747 // BX r8
            });

            Assert.Equal(InternalWramRegion.REGION_START, cpu.CurrentRegisterSet.GetRegister(CpuUtil.PC));
            Assert.Equal(InternalWramRegion.REGION_START + 1, cpu.CurrentRegisterSet.GetRegister(8));
            Assert.True(cpu.CurrentStatus.Thumb);
        }

        [Fact]
        public void BranchExchange_BranchToPcAtHalfWordBoundary_PcSetToNearestWordBoundaryAndInArmMode()
        {
            AgbCpu cpu = CpuUtil.CreateCpu();

            CpuUtil.RunCpu(cpu, new ushort[]
            {
                0xff47 // BX PC
            });

            Assert.Equal(InternalWramRegion.REGION_START + 0x4, cpu.CurrentRegisterSet.GetRegister(CpuUtil.PC));
            Assert.False(cpu.CurrentStatus.Thumb);
        }

        #endregion

    }
}