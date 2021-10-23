// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osuTK;

namespace osu.Game.Beatmaps.Drawables.Cards.Buttons
{
    public class PlayButton : OsuHoverContainer
    {
        public BindableBool Playing { get; } = new BindableBool();

        private readonly BeatmapSetInfo beatmapSetInfo;

        private readonly SpriteIcon icon;

        public PlayButton(BeatmapSetInfo beatmapSetInfo)
        {
            this.beatmapSetInfo = beatmapSetInfo;

            Anchor = Origin = Anchor.Centre;

            Children = new Drawable[]
            {
                icon = new SpriteIcon
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Icon = FontAwesome.Solid.Play,
                    Size = new Vector2(14)
                },
            };

            Action = () => Playing.Toggle();
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            HoverColour = colours.Yellow;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Playing.BindValueChanged(_ => updateState(), true);
        }

        private void updateState()
        {
            icon.Icon = Playing.Value ? FontAwesome.Solid.Stop : FontAwesome.Solid.Play;
        }
    }
}
