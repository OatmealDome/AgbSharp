using System;
#if DEBUG
using System.Diagnostics;
#endif
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;

namespace GbSharp.Gui
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("dotnet AgbSharp.Gui.dll <bootrom> <rom>");
                //return;
            }

            // Create a window to render to using Veldrid
            WindowCreateInfo windowCreateInfo = new WindowCreateInfo()
            {
                X = 100,
                Y = 100,
                WindowWidth = 240 * 2,
                WindowHeight = 160 * 2,
                WindowTitle = "AgbSharp.Gui"
            };

            // Create a window to render to using Veldrid
            Sdl2Window window;
            GraphicsDevice graphicsDevice;

            VeldridStartup.CreateWindowAndGraphicsDevice(windowCreateInfo, out window, out graphicsDevice);

            Renderer renderer = new Renderer(graphicsDevice);

#if DEBUG
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
#endif

            // Run emulation
            while (window.Exists)
            {
#if DEBUG
                double newElapsed = stopwatch.Elapsed.TotalMilliseconds;
#endif

                // TODO
                // Run frame

#if DEBUG
                // Choppy audio, but we shouldn't need this for debugging anyway
                while (stopwatch.Elapsed.TotalMilliseconds - newElapsed < 16.7)
                {
                    ;
                }
#endif
                
                Sdl2Events.ProcessEvents();
                
                // TODO
                // Draw frame using pixel output

                byte[] framebuffer = new byte[4 * 240 * 160];
                for (int i = 0; i < (240 * 160); i++)
                {
                    int fbPos = i * 4;

                    framebuffer[fbPos] = 0xff;
                    framebuffer[fbPos + 1] = 0;
                    framebuffer[fbPos + 2] = 0;
                    framebuffer[fbPos + 3] = 0xff;
                }

                renderer.Draw(framebuffer);

                // TODO
                // Update controllers
            }

#if DEBUG
            stopwatch.Stop();
#endif

            renderer.DisposeResources();
        }

    }
}
