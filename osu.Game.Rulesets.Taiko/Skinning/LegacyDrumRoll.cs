// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Skinning;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Taiko.Skinning
{
    public class LegacyDrumRoll : CompositeDrawable, IHasAccentColour
    {
        private LegacyCirclePiece headCircle;

        private Sprite body;

        private Sprite end;

        public LegacyDrumRoll()
        {
            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load(ISkinSource skin, OsuColour colours)
        {
            InternalChildren = new Drawable[]
            {
                end = new Sprite
                {
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreLeft,
                    RelativeSizeAxes = Axes.Both,
                    Texture = skin.GetTexture("taiko-roll-end"),
                    FillMode = FillMode.Fit,
                },
                body = new Sprite
                {
                    RelativeSizeAxes = Axes.Both,
                    Texture = skin.GetTexture("taiko-roll-middle"),
                },
                headCircle = new LegacyCirclePiece
                {
                    RelativeSizeAxes = Axes.Y,
                },
            };

            AccentColour = colours.YellowDark;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            updateAccentColour();
        }

        private Color4 accentColour;

        public Color4 AccentColour
        {
            get => accentColour;
            set
            {
                if (value == accentColour)
                    return;

                accentColour = value;
                if (IsLoaded)
                    updateAccentColour();
            }
        }

        private void updateAccentColour()
        {
            headCircle.AccentColour = accentColour;
            body.Colour = accentColour;
            end.Colour = accentColour;
        }
    }
}
