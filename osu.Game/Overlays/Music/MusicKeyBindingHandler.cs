// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Input.Bindings;
using osu.Game.Localisation;
using osu.Game.Overlays.OSD;

namespace osu.Game.Overlays.Music
{
    /// <summary>
    /// Handles <see cref="GlobalAction"/>s related to music playback, and displays <see cref="Toast"/>s via the global <see cref="OnScreenDisplay"/> accordingly.
    /// </summary>
    public class MusicKeyBindingHandler : Component, IKeyBindingHandler<GlobalAction>
    {
        [Resolved]
        private IBindable<WorkingBeatmap> beatmap { get; set; }

        [Resolved]
        private MusicController musicController { get; set; }

        [Resolved(canBeNull: true)]
        private OnScreenDisplay onScreenDisplay { get; set; }

        public bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
        {
            if (beatmap.Disabled)
                return false;

            switch (e.Action)
            {
                case GlobalAction.MusicPlay:
                    // use previous state as TogglePause may not update the track's state immediately (state update is run on the audio thread see https://github.com/ppy/osu/issues/9880#issuecomment-674668842)
                    bool wasPlaying = musicController.IsPlaying;

                    if (musicController.TogglePause())
                        onScreenDisplay?.Display(new MusicActionToast(wasPlaying ? ToastStrings.PauseTrack : ToastStrings.PlayTrack, e.Action));
                    return true;

                case GlobalAction.MusicNext:
                    musicController.NextTrack(() => onScreenDisplay?.Display(new MusicActionToast(GlobalActionKeyBindingStrings.MusicNext, e.Action)));

                    return true;

                case GlobalAction.MusicPrev:
                    musicController.PreviousTrack(res =>
                    {
                        switch (res)
                        {
                            case PreviousTrackResult.Restart:
                                onScreenDisplay?.Display(new MusicActionToast(ToastStrings.RestartTrack, e.Action));
                                break;

                            case PreviousTrackResult.Previous:
                                onScreenDisplay?.Display(new MusicActionToast(GlobalActionKeyBindingStrings.MusicPrev, e.Action));
                                break;
                        }
                    });

                    return true;
            }

            return false;
        }

        public void OnReleased(KeyBindingReleaseEvent<GlobalAction> e)
        {
        }

        private class MusicActionToast : Toast
        {
            private readonly GlobalAction action;

            public MusicActionToast(LocalisableString value, GlobalAction action)
                : base(ToastStrings.MusicPlayback, value, string.Empty)
            {
                this.action = action;
            }

            [BackgroundDependencyLoader]
            private void load(OsuConfigManager config)
            {
                ShortcutText.Text = config.LookupKeyBindings(action).ToUpper();
            }
        }
    }
}
