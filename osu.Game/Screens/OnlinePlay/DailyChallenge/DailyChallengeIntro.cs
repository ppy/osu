// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Screens;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Extensions;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.Rooms;
using osu.Game.Overlays;
using osu.Game.Rulesets;
using osu.Game.Screens.OnlinePlay.Match;
using osu.Game.Screens.Play.HUD;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.OnlinePlay.DailyChallenge
{
    public partial class DailyChallengeIntro : OsuScreen
    {
        private readonly Room room;
        private readonly PlaylistItem item;

        private Container introContent = null!;
        private Container topTitleDisplay = null!;
        private Container bottomDateDisplay = null!;
        private Container beatmapBackground = null!;
        private Box flash = null!;

        private FillFlowContainer beatmapContent = null!;

        private bool beatmapBackgroundLoaded;

        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Plum);

        public DailyChallengeIntro(Room room)
        {
            this.room = room;
            item = room.Playlist.Single();

            ValidForResume = false;
        }

        protected override BackgroundScreen CreateBackground() => new DailyChallengeIntroBackgroundScreen(colourProvider);

        [BackgroundDependencyLoader]
        private void load()
        {
            Ruleset ruleset = Ruleset.Value.CreateInstance();

            InternalChildren = new Drawable[]
            {
                introContent = new Container
                {
                    Alpha = 0f,
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Shear = new Vector2(OsuGame.SHEAR, 0f),
                    Children = new Drawable[]
                    {
                        beatmapContent = new FillFlowContainer
                        {
                            AlwaysPresent = true, // so we can get the size ahead of time
                            Direction = FillDirection.Vertical,
                            AutoSizeAxes = Axes.Both,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Alpha = 0,
                            Scale = new Vector2(0.001f),
                            Spacing = new Vector2(10),
                            Children = new Drawable[]
                            {
                                beatmapBackground = new Container
                                {
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopCentre,
                                    Size = new Vector2(500f, 150f),
                                    CornerRadius = 20f,
                                    BorderColour = colourProvider.Content2,
                                    BorderThickness = 3f,
                                    Masking = true,
                                    Children = new Drawable[]
                                    {
                                        new Box
                                        {
                                            Colour = colourProvider.Background3,
                                            RelativeSizeAxes = Axes.Both,
                                        },
                                        flash = new Box
                                        {
                                            Colour = Color4.White,
                                            Blending = BlendingParameters.Additive,
                                            RelativeSizeAxes = Axes.Both,
                                            Depth = float.MinValue,
                                        }
                                    }
                                },
                                new Container
                                {
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopCentre,
                                    Width = 500f,
                                    AutoSizeAxes = Axes.Y,
                                    CornerRadius = 10f,
                                    Masking = true,
                                    Children = new Drawable[]
                                    {
                                        new Box
                                        {
                                            Colour = colourProvider.Background3,
                                            RelativeSizeAxes = Axes.Both,
                                        },
                                        new TruncatingSpriteText
                                        {
                                            Anchor = Anchor.Centre,
                                            Origin = Anchor.Centre,
                                            RelativeSizeAxes = Axes.X,
                                            Shear = new Vector2(-OsuGame.SHEAR, 0f),
                                            Text = item.Beatmap.GetDisplayString(),
                                            Padding = new MarginPadding { Vertical = 5f, Horizontal = 5f },
                                            Font = OsuFont.GetFont(size: 24),
                                        },
                                    }
                                },
                                new ModFlowDisplay
                                {
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopCentre,
                                    AutoSizeAxes = Axes.Both,
                                    Shear = new Vector2(-OsuGame.SHEAR, 0f),
                                    Current =
                                    {
                                        Value = item.RequiredMods.Select(m => m.ToMod(ruleset)).ToArray()
                                    },
                                }
                            }
                        },
                        topTitleDisplay = new Container
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.CentreRight,
                            AutoSizeAxes = Axes.Both,
                            CornerRadius = 10f,
                            Masking = true,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    Colour = colourProvider.Background3,
                                    RelativeSizeAxes = Axes.Both,
                                },
                                new OsuSpriteText
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Text = "Today's Challenge",
                                    Margin = new MarginPadding { Horizontal = 10f, Vertical = 5f },
                                    Shear = new Vector2(-OsuGame.SHEAR, 0f),
                                    Font = OsuFont.GetFont(size: 32, weight: FontWeight.Light, typeface: Typeface.TorusAlternate),
                                },
                            }
                        },
                        bottomDateDisplay = new Container
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.CentreLeft,
                            AutoSizeAxes = Axes.Both,
                            CornerRadius = 10f,
                            Masking = true,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    Colour = colourProvider.Background3,
                                    RelativeSizeAxes = Axes.Both,
                                },
                                new OsuSpriteText
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Text = room.Name.Value.Split(':', StringSplitOptions.TrimEntries).Last(),
                                    Margin = new MarginPadding { Horizontal = 10f, Vertical = 5f },
                                    Shear = new Vector2(-OsuGame.SHEAR, 0f),
                                    Font = OsuFont.GetFont(size: 32, weight: FontWeight.Light, typeface: Typeface.TorusAlternate),
                                },
                            }
                        },
                    }
                }
            };

            LoadComponentAsync(new OnlineBeatmapSetCover(item.Beatmap.BeatmapSet as IBeatmapSetOnlineInfo)
            {
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                FillMode = FillMode.Fit,
                Scale = new Vector2(1.2f),
                Shear = new Vector2(-OsuGame.SHEAR, 0f),
            }, c =>
            {
                beatmapBackground.Add(c);

                beatmapBackgroundLoaded = true;
                updateAnimationState();
            });
        }

        private bool animationBegan;
        private bool trackContent;

        private const float initial_v_shift = 32;
        private const float final_v_shift = 340;

        protected override void Update()
        {
            base.Update();

            if (trackContent)
            {
                float vShift = initial_v_shift + (beatmapContent.DrawHeight * beatmapContent.Scale.Y) / 2;

                float yPos = (float)Interpolation.DampContinuously(bottomDateDisplay.Y, vShift, 16, Clock.ElapsedFrameTime);
                float xPos = (float)Interpolation.DampContinuously(bottomDateDisplay.X, getShearForY(vShift) + final_v_shift, 16, Clock.ElapsedFrameTime);

                topTitleDisplay.Position = new Vector2(-xPos, -yPos);
                bottomDateDisplay.Position = new Vector2(xPos, yPos);
            }
        }

        public override void OnEntering(ScreenTransitionEvent e)
        {
            base.OnEntering(e);
            this.FadeInFromZero(400, Easing.OutQuint);
            updateAnimationState();
        }

        public override void OnSuspending(ScreenTransitionEvent e)
        {
            this.FadeOut(200, Easing.OutQuint);
            base.OnSuspending(e);
        }

        private void updateAnimationState()
        {
            if (!beatmapBackgroundLoaded || !this.IsCurrentScreen())
                return;

            if (animationBegan)
                return;

            beginAnimation();
            animationBegan = true;
        }

        private void beginAnimation()
        {
            const float v_spacing = 0;

            using (BeginDelayedSequence(200))
            {
                introContent.Show();

                topTitleDisplay.MoveToOffset(new Vector2(getShearForY(-initial_v_shift), -initial_v_shift));
                bottomDateDisplay.MoveToOffset(new Vector2(getShearForY(initial_v_shift), initial_v_shift));

                topTitleDisplay.MoveToX(getShearForY(topTitleDisplay.Y) - 500)
                               .MoveToX(getShearForY(topTitleDisplay.Y) - v_spacing, 300, Easing.OutQuint)
                               .FadeInFromZero(400, Easing.OutQuint);

                bottomDateDisplay.MoveToX(getShearForY(bottomDateDisplay.Y) + 500)
                                 .MoveToX(getShearForY(bottomDateDisplay.Y) + v_spacing, 300, Easing.OutQuint)
                                 .FadeInFromZero(400, Easing.OutQuint);

                using (BeginDelayedSequence(500))
                {
                    Schedule(() => trackContent = true);

                    beatmapContent
                        .ScaleTo(1f, 500, Easing.InQuint)
                        .Then()
                        .ScaleTo(1.1f, 3000);

                    using (BeginDelayedSequence(240))
                    {
                        beatmapContent.FadeInFromZero(280, Easing.InQuad);

                        flash
                            .Delay(400)
                            .FadeOutFromOne(5000, Easing.OutQuint);

                        ApplyToBackground(bs => ((RoomBackgroundScreen)bs).SelectedItem.Value = item);

                        using (BeginDelayedSequence(2600))
                        {
                            introContent.FadeOut(200, Easing.OutQuint).OnComplete(_ =>
                            {
                                if (this.IsCurrentScreen())
                                    this.Push(new DailyChallenge(room));
                            });
                        }
                    }
                }
            }
        }

        private static float getShearForY(float yPos) => yPos * -OsuGame.SHEAR * 2;

        private partial class DailyChallengeIntroBackgroundScreen : RoomBackgroundScreen
        {
            private readonly OverlayColourProvider colourProvider;

            public DailyChallengeIntroBackgroundScreen(OverlayColourProvider colourProvider)
                : base(null)
            {
                this.colourProvider = colourProvider;
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                AddInternal(new Box
                {
                    Depth = float.MinValue,
                    RelativeSizeAxes = Axes.Both,
                    Colour = colourProvider.Background5.Opacity(0.6f),
                });
            }
        }
    }
}
