// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Audio.Effects;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osuTK;

namespace osu.Game.Collections
{
    public class ManageCollectionsDialog : OsuFocusedOverlayContainer
    {
        private const double enter_duration = 500;
        private const double exit_duration = 200;

        private AudioFilter lowPassFilter;

        [Resolved(CanBeNull = true)]
        private CollectionManager collectionManager { get; set; }

        public ManageCollectionsDialog()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            RelativeSizeAxes = Axes.Both;
            Size = new Vector2(0.5f, 0.8f);

            Masking = true;
            CornerRadius = 10;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours, AudioManager audio)
        {
            Children = new Drawable[]
            {
                new Box
                {
                    Colour = colours.GreySeaFoamDark,
                    RelativeSizeAxes = Axes.Both,
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Child = new GridContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        RowDimensions = new[]
                        {
                            new Dimension(GridSizeMode.AutoSize),
                        },
                        Content = new[]
                        {
                            new Drawable[]
                            {
                                new Container
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Children = new Drawable[]
                                    {
                                        new OsuSpriteText
                                        {
                                            Anchor = Anchor.Centre,
                                            Origin = Anchor.Centre,
                                            Text = "Manage collections",
                                            Font = OsuFont.GetFont(size: 30),
                                            Padding = new MarginPadding { Vertical = 10 },
                                        },
                                        new IconButton
                                        {
                                            Anchor = Anchor.CentreRight,
                                            Origin = Anchor.CentreRight,
                                            Icon = FontAwesome.Solid.Times,
                                            Colour = colours.GreySeaFoamDarker,
                                            Scale = new Vector2(0.8f),
                                            X = -10,
                                            Action = () => State.Value = Visibility.Hidden
                                        }
                                    }
                                }
                            },
                            new Drawable[]
                            {
                                new Container
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Children = new Drawable[]
                                    {
                                        new Box
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                            Colour = colours.GreySeaFoamDarker
                                        },
                                        new DrawableCollectionList
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                            Items = { BindTarget = collectionManager?.Collections ?? new BindableList<BeatmapCollection>() }
                                        }
                                    }
                                }
                            },
                        }
                    }
                },
                lowPassFilter = new AudioFilter(audio.TrackMixer)
            };
        }

        protected override void PopIn()
        {
            base.PopIn();

            lowPassFilter.CutoffTo(300, 100, Easing.OutCubic);
            this.FadeIn(enter_duration, Easing.OutQuint);
            this.ScaleTo(0.9f).Then().ScaleTo(1f, enter_duration, Easing.OutQuint);
        }

        protected override void PopOut()
        {
            base.PopOut();

            lowPassFilter.CutoffTo(AudioFilter.MAX_LOWPASS_CUTOFF, 100, Easing.InCubic);

            this.FadeOut(exit_duration, Easing.OutQuint);
            this.ScaleTo(0.9f, exit_duration);

            // Ensure that textboxes commit
            GetContainingInputManager()?.TriggerFocusContention(this);
        }
    }
}
