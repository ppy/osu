// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;
using osu.Game.Users.Drawables;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay
{
    public partial class RankedPlayStageOverlay : CompositeDrawable
    {
        private readonly LocalisableString stageName;
        private readonly RankedPlayColourScheme colourScheme;

        public APIUser? PickingUser { get; init; }
        public double? Multiplier { get; init; }

        private FillFlowContainer displayContainer = null!;
        private FillFlowContainer detailsContainer = null!;
        private CircularContainer avatarContainer = null!;

        private Sample stageChangeSample = null!;

        [Resolved]
        private MusicController musicController { get; set; } = null!;

        public RankedPlayStageOverlay(LocalisableString stageName, RankedPlayColourScheme colourScheme)
        {
            this.stageName = stageName;
            this.colourScheme = colourScheme;
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            RelativeSizeAxes = Axes.Both;

            InternalChild = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Children = new Drawable[]
                {
                    new Box
                    {
                        Alpha = 0.4f,
                        RelativeSizeAxes = Axes.Both,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Colour = Color4Extensions.FromHex("#000"),
                    },
                    displayContainer = new FillFlowContainer
                    {
                        AutoSizeAxes = Axes.Both,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Direction = FillDirection.Vertical,
                        Spacing = new Vector2(0, 10),
                        Children = new Drawable[]
                        {
                            new Container
                            {
                                Width = 500,
                                AutoSizeAxes = Axes.Y,
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Shear = OsuGame.SHEAR,
                                Masking = true,
                                CornerRadius = 10,
                                Children = new Drawable[]
                                {
                                    new Box
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Colour = colourScheme.Surface.Darken(0.1f),
                                        Alpha = 0.8f,
                                    },
                                    new OsuSpriteText
                                    {
                                        Anchor = Anchor.Centre,
                                        Origin = Anchor.Centre,
                                        Shear = -OsuGame.SHEAR,
                                        Padding = new MarginPadding { Vertical = 20 },
                                        Font = OsuFont.TorusAlternate.With(size: 72),
                                        Shadow = false,
                                        Text = stageName,
                                    },
                                },
                            },
                            detailsContainer = new FillFlowContainer
                            {
                                AutoSizeAxes = Axes.Both,
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Direction = FillDirection.Horizontal,
                                Spacing = new Vector2(40, 0),
                            },
                        },
                    }
                },
            };

            if (PickingUser != null)
            {
                detailsContainer.Add(new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Horizontal,
                    Spacing = new Vector2(5, 0),
                    Children = new Drawable[]
                    {
                        avatarContainer = new CircularContainer
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Size = new Vector2(32),
                            Masking = true,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = colourScheme.Surface,
                                    Alpha = 0.5f,
                                },
                            },
                        },
                        new OsuSpriteText
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            UseFullGlyphHeight = false,
                            Font = OsuFont.Torus.With(size: 32),
                            Text = $"{PickingUser.Username}'s pick",
                            Colour = colourScheme.Primary,
                        },
                    },
                });
            }

            if (Multiplier != null)
            {
                detailsContainer.Add(new OsuSpriteText
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    UseFullGlyphHeight = false,
                    Font = OsuFont.Torus.With(size: 32),
                    Text = $"{Multiplier:N0}x damage",
                });
            }

            stageChangeSample = audio.Samples.Get(@"Multiplayer/Matchmaking/Ranked/stage-change");
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            if (PickingUser != null)
                LoadComponentAsync(new DrawableAvatar(PickingUser), a => avatarContainer.Add(a));

            const int duration = 500;
            const int time_visible = 1500;

            const Easing easing = Easing.OutQuint;

            this.FadeInFromZero(300, easing);

            displayContainer
                .ScaleTo(0.9f)
                .ScaleTo(1f, duration, easing);

            musicController.DuckMomentarily(time_visible, new DuckParameters
            {
                DuckDuration = 0,
                DuckVolumeTo = 0.5,
                DuckCutoffTo = 600,
            });
            stageChangeSample.Play();

            using (BeginDelayedSequence(time_visible))
            {
                this.FadeOut(duration, easing)
                    .Expire();

                displayContainer
                    .ScaleTo(0.9f, duration, easing);
            }
        }
    }
}
