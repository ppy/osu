// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Configuration;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Input.Handlers.Mouse;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Game.Graphics;
using osu.Game.Graphics.Cursor;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osu.Game.Tournament.Models;
using osuTK.Graphics;

namespace osu.Game.Tournament
{
    [Cached]
    public partial class TournamentGame : TournamentGameBase
    {
        public static ColourInfo GetTeamColour(TeamColour teamColour) => teamColour == TeamColour.Red ? COLOUR_RED : COLOUR_BLUE;

        public static readonly Color4 COLOUR_RED = new OsuColour().TeamColourRed;
        public static readonly Color4 COLOUR_BLUE = new OsuColour().TeamColourBlue;

        public static readonly Color4 ELEMENT_BACKGROUND_COLOUR = Color4Extensions.FromHex("#fff");
        public static readonly Color4 ELEMENT_FOREGROUND_COLOUR = Color4Extensions.FromHex("#000");

        public static readonly Color4 TEXT_COLOUR = Color4Extensions.FromHex("#fff");
        private Drawable heightWarning = null!;

        private Bindable<WindowMode> windowMode = null!;
        private readonly BindableSize windowSize = new BindableSize();

        private LoadingSpinner loadingSpinner = null!;

        [Cached(typeof(IDialogOverlay))]
        private readonly DialogOverlay dialogOverlay = new DialogOverlay();

        [BackgroundDependencyLoader]
        private void load(FrameworkConfigManager frameworkConfig, GameHost host)
        {
            frameworkConfig.BindWith(FrameworkSetting.WindowedSize, windowSize);

            windowMode = frameworkConfig.GetBindable<WindowMode>(FrameworkSetting.WindowMode);

            Add(loadingSpinner = new LoadingSpinner(true, true)
            {
                Anchor = Anchor.BottomRight,
                Origin = Anchor.BottomRight,
                Margin = new MarginPadding(40),
            });

            // in order to have the OS mouse cursor visible, relative mode needs to be disabled.
            // can potentially be removed when https://github.com/ppy/osu-framework/issues/4309 is resolved.
            var mouseHandler = host.AvailableInputHandlers.OfType<MouseHandler>().FirstOrDefault();

            if (mouseHandler != null)
                mouseHandler.UseRelativeMode.Value = false;

            loadingSpinner.Show();

            BracketLoadTask.ContinueWith(t => Schedule(() =>
            {
                if (t.IsFaulted)
                {
                    loadingSpinner.Hide();
                    loadingSpinner.Expire();

                    Logger.Error(t.Exception, "Couldn't load bracket with error");
                    Add(new WarningBox($"Your {BRACKET_FILENAME} file could not be parsed. Please check runtime.log for more details."));

                    return;
                }

                LoadComponentsAsync(new[]
                {
                    new SaveChangesOverlay
                    {
                        Depth = float.MinValue,
                    },
                    heightWarning = new WarningBox("Please make the window wider")
                    {
                        Anchor = Anchor.BottomCentre,
                        Origin = Anchor.BottomCentre,
                        Margin = new MarginPadding(20),
                    },
                    new OsuContextMenuContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        Child = new TournamentSceneManager()
                    },
                    dialogOverlay
                }, drawables =>
                {
                    loadingSpinner.Hide();
                    loadingSpinner.Expire();
                    AddRange(drawables);

                    windowSize.BindValueChanged(size => ScheduleAfterChildren(() =>
                    {
                        int minWidth = (int)(size.NewValue.Height / 768f * TournamentSceneManager.REQUIRED_WIDTH) - 1;
                        heightWarning.Alpha = size.NewValue.Width < minWidth ? 1 : 0;
                    }), true);

                    windowMode.BindValueChanged(_ => ScheduleAfterChildren(() =>
                    {
                        windowMode.Value = WindowMode.Windowed;
                    }), true);
                });
            }));
        }
    }
}
