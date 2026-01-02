// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Utils;
using osu.Game.Tournament.Models;
using osuTK.Graphics;

namespace osu.Game.Tournament.Components
{
    public partial class TournamentProtectIcon : Container
    {
        private SpriteIcon protectIcon = null!;
        private Box background = null!;

        private ColourInfo backgroundColour;

        private TeamColour? teamColour;

        public TeamColour? TeamColour
        {
            get => teamColour;
            set
            {
                if (value == teamColour)
                    return;

                teamColour = value;

                if (IsLoaded)
                    updateColour();
            }
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.CentreRight,
                Origin = Anchor.CentreRight,
                Name = "main content",
                Masking = true,
                Children = new Drawable[]
                {
                    background = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Anchor = Anchor.TopRight,
                        Origin = Anchor.Centre,
                        Rotation = 45,
                    },
                    protectIcon = new SpriteIcon
                    {
                        RelativeSizeAxes = Axes.Both,
                        RelativePositionAxes = Axes.Both,
                        Origin = Anchor.BottomLeft,
                        Anchor = Anchor.Centre,
                        Width = 0.3f,
                        Height = 0.3f,
                        X = 0.14f,
                        Y = -0.14f,
                        Icon = FontAwesome.Solid.ShieldAlt,
                    },
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            updateColour();
        }

        private void updateColour()
        {
            if (TeamColour == null)
            {
                Alpha = 0;
                return;
            }

            backgroundColour = TournamentGame.GetTeamColour(TeamColour.Value);

            protectIcon.Colour = Interpolation.ValueAt<Color4>(0.1f, Color4.Black, backgroundColour, 0, 1);
            background.Colour = backgroundColour;
            Alpha = 1;
        }
    }
}
