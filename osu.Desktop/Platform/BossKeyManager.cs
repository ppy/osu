using osu.Framework.Graphics;
using osu.Framework.Input;
using osu.Framework.Allocation;
using osu.Framework.Platform;
using osu.Framework.Platform.SDL3;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Input.Bindings;
using osu.Game.Overlays;
using osu.Framework.Logging;
using System;

namespace osu.Desktop.Platform
{
    internal partial class BossKeyManager : Component, IKeyBindingHandler<GlobalAction>, IHandleGlobalKeyboardInput
    {
        [Resolved]
        private GameHost host { get; set; } = null!;

        [Resolved]
        private VolumeOverlay volumeOverlay { get; set; } = null!;

        private IDisposable? activeTrayIcon = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
        }

        public bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
        {
            if (e.Action == GlobalAction.BossKey && !e.Repeat)
            {
                bool previousState = volumeOverlay.IsMuted.Value;

                if (host.Window is ISDLWindow window)
                {
                    // TODO: add icon
                    var icon = new TrayIcon
                    {
                        Label = "osu!",
                        Menu = new TrayMenuEntry[]
                        {
                            new TrayButton
                            {
                                Label = "Open osu!",
                                Action = () => Schedule(() => onShow(previousState)),
                            },
                        }
                    };
                
                    try 
                    {
                        Schedule(() => { activeTrayIcon = window.CreateTrayIcon(icon); });
                    } 
                    catch (PlatformNotSupportedException ex) 
                    {
                        Logger.Log($"aaaa");
                        return false;
                    }

                    host.Window.Hide();
                    Logger.Log($"Created notification tray icon");
                    volumeOverlay.IsMuted.Value = true;

                    return true;
                }
            }

            return false;
        }

        private void onShow(bool previousState)
        {
            Logger.Log($"Notification tray icon clicked");
            host.Window.Show();
            host.Window.Raise();

            activeTrayIcon.Dispose();
            Logger.Log($"Notification tray icon removed");
            volumeOverlay.IsMuted.Value = previousState;
        }

        public void OnReleased(KeyBindingReleaseEvent<GlobalAction> e)
        {
        }
    }
}
