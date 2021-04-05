using System;
#if DEBUG
using System.Diagnostics;
#endif
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;
using AgbSharp.Core;
using System.IO;

namespace GbSharp.Gui
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("dotnet AgbSharp.Gui.dll <bootrom> <rom>");
                return;
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

            AgbDevice agbDevice = new AgbDevice();

            agbDevice.LoadBios(File.ReadAllBytes(args[0]));
            agbDevice.LoadRom(File.ReadAllBytes(args[1]));

            // Run emulation
            while (window.Exists)
            {
#if DEBUG
                double newElapsed = stopwatch.Elapsed.TotalMilliseconds;
#endif

                agbDevice.RunFrame();

#if DEBUG
                // Choppy audio, but we shouldn't need this for debugging anyway
                while (stopwatch.Elapsed.TotalMilliseconds - newElapsed < 16.7)
                {
                    ;
                }
#endif
                
                Sdl2Events.ProcessEvents();

                renderer.Draw(agbDevice.Ppu.Framebuffer);

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
