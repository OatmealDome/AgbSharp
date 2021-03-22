namespace AgbSharp.Core.Cpu
{
    [System.Serializable]
    public class CpuException : System.Exception
    {
        public CpuException(string message) : base(message)
        {

        }

        public CpuException(string message, System.Exception inner) : base(message, inner)
        {
            
        }

    }
}