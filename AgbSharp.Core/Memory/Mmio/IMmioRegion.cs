namespace AgbSharp.Core.Memory.Mmio
{
    interface IMmioRegion
    {
        void Update();

        void Flush();
        
    }
}