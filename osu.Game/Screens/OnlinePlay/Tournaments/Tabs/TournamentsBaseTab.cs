// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Screens.OnlinePlay.Tournaments.Tabs
{
    public abstract partial class TournamentsBaseTab : CompositeDrawable
    {
        [Resolved]
        protected TournamentInfo TournamentInfo { get; private set; } = null!;

        public abstract TournamentsTab TabType { get; }

        protected TournamentsBaseTab()
        {
            RelativeSizeAxes = Axes.Both;
            Alpha = 0.0f;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.Both;
            Alpha = 0.0f;
            InternalChild = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Child = new OsuSpriteText
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Text = TabType.ToString()
                }
            };
        }
    }
}
