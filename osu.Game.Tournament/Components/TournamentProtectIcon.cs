// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Utils;
using osu.Game.Tournament.Models;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tournament.Components
{
    public partial class TournamentProtectIcon : Container
    {
        private SpriteIcon protectIcon = null!;
        private Sprite background = null!;

        private Color4 backgroundColour;

        private TeamColour? teamColour;

        public TeamColour? TeamColour
        {
            get => teamColour;
            set
            {
                if (value == teamColour)
                    return;

                teamColour = value;
                updateColour();
            }
        }

        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            Children = new Drawable[]
            {
                new Container
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Name = "main content",
                    Size = new Vector2(80), //to match mod icon size
                    Children = new Drawable[]
                    {
                        background = new Sprite
                        {
                            RelativeSizeAxes = Axes.Both,
                            FillMode = FillMode.Fit,
                            Texture = textures.Get("Icons/BeatmapDetails/mod-icon"),
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                        },
                        protectIcon = new SpriteIcon
                        {
                            Origin = Anchor.Centre,
                            Anchor = Anchor.Centre,
                            Size = new Vector2(40),
                            Icon = FontAwesome.Solid.ShieldAlt,
                        },
                    }
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            updateColour();
        }

        private void updateColour()
        {
            if (!IsLoaded)
                return;

            if (TeamColour == null)
            {
                Alpha = 0;
                protectIcon.Colour = Color4.Transparent;
                background.Colour = Color4.Transparent;
                return;
            }

            backgroundColour = TournamentGame.GetTeamColour((TeamColour)TeamColour);

            protectIcon.Colour = Interpolation.ValueAt<Colour4>(0.1f, Colour4.Black, backgroundColour, 0, 1);
            background.Colour = backgroundColour;
            Alpha = 1;
        }
    }
}
