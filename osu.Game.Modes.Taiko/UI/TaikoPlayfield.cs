// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Modes.UI;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Game.Modes.Objects.Drawables;

namespace osu.Game.Modes.Taiko.UI
{
    public class TaikoPlayfield : Container
    {
        public Container<DrawableHitObject> HitObjects;

        public TaikoPlayfield()
        {
            RelativeSizeAxes = Axes.Both;

            Children = new Drawable[]
            {
                // Base field
                new Container()
                {
                    RelativeSizeAxes = Axes.X,
                    Size = new Vector2(1, 106),

                    Children = new Drawable[]
                    {
                        // Right area
                        new Container()
                        {
                            RelativeSizeAxes = Axes.Both,
                            RelativePositionAxes = Axes.Both,
                            Position = new Vector2(0.25f, 0),
                            Size = new Vector2(0.75f, 1),

                            Children = new Drawable[]
                            {
                                // Background
                                new Box()
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = new Color4(0, 0, 0, 127)
                                },
                                // Hit area + notes
                                new Container()
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    RelativePositionAxes = Axes.Both,

                                    Position = new Vector2(0.1f, 0),

                                    Children = new Drawable[]
                                    {
                                        new HitTarget()
                                        {
                                            Anchor = Anchor.CentreLeft,
                                            Origin = Anchor.Centre
                                        },
                                        // Todo: Add hitobjects here:
                                        HitObjects = new Container<DrawableHitObject>()
                                        {
                                            RelativeSizeAxes = Axes.Both
                                        }
                                    }
                                },
                                // Barlines
                                new Container()
                                {
                                    RelativeSizeAxes = Axes.Both
                                },
                                // Notes
                                new Container()
                                {
                                    RelativeSizeAxes = Axes.Both
                                }
                            },

                            BorderColour = new Color4(17, 17, 17, 255),
                            BorderThickness = 2
                        },
                        // Left area
                        new Container()
                        {
                            RelativeSizeAxes = Axes.Both,
                            Size = new Vector2(0.25f, 1),

                            Masking = true,

                            BorderColour = Color4.Black,
                            BorderThickness = 1,

                            Children = new Drawable[]
                            {
                                // Background
                                new Box()
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = new Color4(17, 17, 17, 255)
                                },
                                new InputDrum()
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,

                                    RelativePositionAxes = Axes.X,
                                    Position = new Vector2(0.10f, 0)
                                }
                            }
                        }
                    }
                }
            };
        }
    }
}