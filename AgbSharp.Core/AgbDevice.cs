using System;
using AgbSharp.Core.Cpu;
using AgbSharp.Core.Memory;
using AgbSharp.Core.Memory.Bios;
using AgbSharp.Core.Memory.GamePak;
using AgbSharp.Core.Ppu;

namespace AgbSharp.Core
{
    public class AgbDevice
    {
        public AgbMemoryMap MemoryMap
        {
            get;
            private set;
        }

        public AgbCpu Cpu
        {
            get;
            private set;
        }

        public AgbPpu Ppu
        {
            get;
            private set;
        }

        private int ElapsedFrameCycles;
        private int CpuCyclesForDot;
        private const int CyclesPerFrame = 228 * 308 * 4; // 228 lines * 308 dots/line * 4 cycles/dot
        private const int CyclesPerDot = 4;

        public AgbDevice()
        {
            MemoryMap = new AgbMemoryMap();
            Cpu = new AgbCpu(MemoryMap);
            Ppu = new AgbPpu(MemoryMap, Cpu);
        }

        public void LoadBios(byte[] bios)
        {
            if (bios.Length != BiosRegion.REGION_SIZE)
            {
                throw new Exception("Invalid BIOS");
            }

            MemoryMap.RegisterRegion(new BiosRegion(bios));
        }

        public void LoadRom(byte[] rom)
        {
            MemoryMap.RegisterRegion(new RomRegion(rom));
        }

        public void RunFrame()
        {
            while (ElapsedFrameCycles < CyclesPerDot)
            {
                while (CpuCyclesForDot < 4)
                {
                    CpuCyclesForDot += Cpu.Step();
                }

                MemoryMap.FlushMmio();

                Ppu.Tick();

                MemoryMap.UpdateMmio();

                CpuCyclesForDot -= CyclesPerDot;
                ElapsedFrameCycles += CyclesPerDot;
            }

            ElapsedFrameCycles -= CyclesPerFrame;
        }
        
    }
}