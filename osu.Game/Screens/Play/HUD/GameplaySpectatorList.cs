// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Localisation;
using osu.Game.Users;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Play.HUD
{
    public partial class GameplaySpectatorList : CompositeDrawable
    {
        private readonly Bindable<bool> configVisibility = new Bindable<bool>();

        // This is going to dynamically change depending on the amount of spectators
        protected readonly FillFlowContainer<GameplaySpectatorUser> Flow;
        protected readonly FillFlowContainer OtherSpectators;
        protected OsuSpriteText SpectatorText;
        private const int fade_time = 100;
        private int spectators => Flow.Count;

        public readonly BindableInt MaxNames = new BindableInt(10);

        public GameplaySpectatorList()
        {
            Width = GameplaySpectatorUser.EXTENDED_WIDTH + GameplaySpectatorUser.SHEAR_WIDTH;

            InternalChildren = new Drawable[]
            {
                OtherSpectators = new FillFlowContainer
                {
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopLeft,
                    RelativeSizeAxes = Axes.Both,
                    Direction = FillDirection.Horizontal,
                    Child = new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Anchor = Anchor.TopLeft,
                        Origin = Anchor.TopLeft,
                        Child = SpectatorText = new OsuSpriteText
                        {
                            RelativeSizeAxes = Axes.X,
                            Anchor = Anchor.TopLeft,
                            Origin = Anchor.TopLeft,
                            Colour = Color4.White,
                            Font = OsuFont.Torus.With(size: 14, weight: FontWeight.SemiBold),
                            Truncate = false,
                            Shadow = false,
                            Margin = new MarginPadding { Left = GameplaySpectatorUser.SHEAR_WIDTH },
                        },
                    },
                },
                Flow = new FillFlowContainer<GameplaySpectatorUser>
                {
                    RelativeSizeAxes = Axes.X,
                    X = GameplaySpectatorUser.SHEAR_WIDTH,
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopLeft,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(2.5f),
                    LayoutDuration = 450,
                    LayoutEasing = Easing.OutQuint,
                    // Make the margin a bit higher than the text height
                    Margin = new MarginPadding { Top = OsuFont.Torus.Size * 1.5f },
                },
            };

            // Hide by default
            Flow.Alpha = 0.0f;
            OtherSpectators.Alpha = 0.0f;

            // When the bindable changes, update the display
            MaxNames.ValueChanged += _ => updateDisplay();
            configVisibility.ValueChanged += _ => updateDisplay();
        }

        public GameplaySpectatorUser Add(IUser? user)
        {
            var drawable = CreateSpectatorUserDrawable(user);

            Flow.Add(drawable);

            updateDisplay();
            return drawable;
        }

        public void Remove(IUser? user)
        {
            Flow.RemoveAll(u => u.User?.Username == user?.Username, true);

            updateDisplay();
        }

        private void updateDisplay()
        {
            // We removed everything, fade out everything
            if (spectators == 0 || configVisibility.Value == false)
            {
                Flow.FadeOut(fade_time);
                OtherSpectators.FadeOut(fade_time);
                return;
            }

            // Fade in the Text if needed
            if (OtherSpectators.Alpha == 0.0f)
            {
                OtherSpectators.FadeIn(fade_time);
            }

            // always adjust text
            SpectatorText.Text = LocalisableString.Format("Spectators ({0})", spectators);

            // We exceeded the maximum number
            if (spectators > MaxNames.Value && Flow.Alpha == 1.0f)
            {
                Flow.FadeOut(fade_time);
            }

            if (spectators <= MaxNames.Value && Flow.Alpha == 0.0f)
            {
                Flow.FadeIn(fade_time);
            }
        }

        public void Clear()
        {
            Flow.Clear();
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            config.BindWith(OsuSetting.DisplaySpectatorList, configVisibility);
        }

        protected GameplaySpectatorUser CreateSpectatorUserDrawable(IUser? user) =>
            new GameplaySpectatorUser(user);
    }
}
