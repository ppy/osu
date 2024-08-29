using osu.Framework.Graphics;
using osu.Framework.Input;
using osu.Framework.Allocation;
using osu.Framework.Platform;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Input.Bindings;
using osu.Game.Overlays;
using osu.Framework.Logging;

namespace osu.Desktop.Platform
{
    internal partial class BossKeyManager : Component, IKeyBindingHandler<GlobalAction>, IHandleGlobalKeyboardInput
    {
        [Resolved]
        private GameHost host { get; set; } = null!;

        [Resolved]
        private VolumeOverlay volumeOverlay { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
        }

        public bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
        {
            if (e.Action == GlobalAction.BossKey && !e.Repeat)
            {
                host.Window.Hide();
                bool previousState = volumeOverlay.IsMuted.Value;
                
                host.Window.CreateNotificationTrayIcon("osu!", () => Schedule(() => onShow(previousState)));
                Logger.Log($"Created notification tray icon");
                volumeOverlay.IsMuted.Value = true;

                return true;
            }

            return false;
        }

        private void onShow(bool previousState)
        {
            Logger.Log($"Notification tray icon clicked");
            host.Window.Show();
            host.Window.Raise();
            host.Window.RemoveNotificationTrayIcon();
            Logger.Log($"Notification tray icon removed");
            volumeOverlay.IsMuted.Value = previousState;
        }

        public void OnReleased(KeyBindingReleaseEvent<GlobalAction> e)
        {
        }
    }
}
