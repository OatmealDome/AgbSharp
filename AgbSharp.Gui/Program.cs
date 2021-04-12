using System;
#if DEBUG
using System.Diagnostics;
#endif
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;
using AgbSharp.Core;
using System.IO;
using AgbSharp.Core.Controller;

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

                InputSnapshot snapshot = window.PumpEvents();
                foreach (KeyEvent keyEvent in snapshot.KeyEvents)
                {
                    switch (keyEvent.Key)
                    {
                        case Key.W:
                            agbDevice.Controller.UpdateKeyState(ControllerKey.Up, keyEvent.Down);
                            break;
                        case Key.S:
                            agbDevice.Controller.UpdateKeyState(ControllerKey.Down, keyEvent.Down);
                            break;
                        case Key.A:
                            agbDevice.Controller.UpdateKeyState(ControllerKey.Left, keyEvent.Down);
                            break;
                        case Key.D:
                            agbDevice.Controller.UpdateKeyState(ControllerKey.Right, keyEvent.Down);
                            break;
                        case Key.Minus:
                            agbDevice.Controller.UpdateKeyState(ControllerKey.Select, keyEvent.Down);
                            break;
                        case Key.Plus:
                            agbDevice.Controller.UpdateKeyState(ControllerKey.Start, keyEvent.Down);
                            break;
                        case Key.Comma:
                            agbDevice.Controller.UpdateKeyState(ControllerKey.B, keyEvent.Down);
                            break;
                        case Key.Period:
                            agbDevice.Controller.UpdateKeyState(ControllerKey.A, keyEvent.Down);
                            break;
                        case Key.K:
                            agbDevice.Controller.UpdateKeyState(ControllerKey.L, keyEvent.Down);
                            break;
                        case Key.L:
                            agbDevice.Controller.UpdateKeyState(ControllerKey.R, keyEvent.Down);
                            break;
                    }
                }

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
