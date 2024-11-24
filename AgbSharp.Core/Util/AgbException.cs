using System;

namespace AgbSharp.Core.Util;

public class AgbException : Exception
{
    public AgbException(string message) : base(message)
    {

    }

    public AgbException(string message, System.Exception inner) : base(message, inner)
    {
            
    }
}