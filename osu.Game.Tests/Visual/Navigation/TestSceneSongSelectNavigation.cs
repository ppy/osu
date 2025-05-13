// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Screens;
using osu.Game.Database;
using osu.Game.Overlays;
using osu.Game.Overlays.Toolbar;
using osu.Game.Screens;
using osu.Game.Screens.Footer;
using osu.Game.Screens.Menu;
using osu.Game.Screens.SelectV2;

namespace osu.Game.Tests.Visual.Navigation
{
    [Explicit]
    public partial class TestSceneSongSelectNavigation : ScreenTestScene
    {
        [Cached]
        private readonly ScreenFooter screenFooter;

        [Cached]
        private readonly OsuLogo logo;

        [Cached(typeof(INotificationOverlay))]
        private readonly INotificationOverlay notificationOverlay = new NotificationOverlay();

        protected override bool UseOnlineAPI => true;

        public TestSceneSongSelectNavigation()
        {
            Children = new Drawable[]
            {
                new PopoverContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        new Toolbar
                        {
                            State = { Value = Visibility.Visible },
                        },
                        screenFooter = new ScreenFooter
                        {
                            OnBack = () => Stack.CurrentScreen.Exit(),
                        },
                        logo = new OsuLogo
                        {
                            Alpha = 0f,
                        },
                    },
                },
            };

            Stack.Padding = new MarginPadding { Top = Toolbar.HEIGHT };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            RealmDetachedBeatmapStore beatmapStore;
            Dependencies.CacheAs<BeatmapStore>(beatmapStore = new RealmDetachedBeatmapStore());
            Add(beatmapStore);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Stack.ScreenPushed += updateFooter;
            Stack.ScreenExited += updateFooter;
        }

        public override void SetUpSteps()
        {
            base.SetUpSteps();
            AddStep("load screen", () => Stack.Push(new SoloSongSelect()));
            AddUntilStep("wait for load", () => Stack.CurrentScreen is SoloSongSelect songSelect && songSelect.IsLoaded);
        }

        private void updateFooter(IScreen? _, IScreen? newScreen)
        {
            if (newScreen is IOsuScreen osuScreen && osuScreen.ShowFooter)
            {
                screenFooter.Show();
                screenFooter.SetButtons(osuScreen.CreateFooterButtons());
            }
            else
            {
                screenFooter.Hide();
                screenFooter.SetButtons(Array.Empty<ScreenFooterButton>());
            }
        }
    }
}
