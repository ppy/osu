// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
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
        private const int fade_speed = 100;

        public int Spectators => Flow.Count;

        public int MaxNames = 10;

        public GameplaySpectatorList()
        {
            Width = GameplaySpectatorUser.EXTENDED_WIDTH + GameplaySpectatorUser.SHEAR_WIDTH;

            InternalChildren = new Drawable[]
            {
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
                },
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
                        },
                    },
                },
            };

            // Hide by default
            Flow.Alpha = 0.0f;
            OtherSpectators.Alpha = 0.0f;
        }

        public GameplaySpectatorUser Add([CanBeNull] IUser user)
        {
            var drawable = CreateSpectatorUserDrawable(user);

            Flow.Add(drawable);

            updateDisplay();
            return drawable;
        }

        public void Remove([CanBeNull] IUser user)
        {
            if (user == null) return;

            Flow.RemoveAll(u => u.User.Username == user.Username, true);

            updateDisplay();
        }

        private void updateDisplay()
        {
            if (Flow.Count > MaxNames)
            {
                // TODO: Just display text instead of the flow's members
                SpectatorText.Text = Flow.Count + " Spectators";

                Flow.FadeTo(0.0f, fade_speed);
                OtherSpectators.FadeTo(1, fade_speed, Easing.OutQuad);
            }
            else
            {
                OtherSpectators.FadeTo(0.0f, fade_speed);
                Flow.FadeTo(1, fade_speed, Easing.OutQuad);
            }
        }

        public void Clear()
        {
            Flow.Clear();
        }

        protected override void Update()
        {
            base.Update();
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            config.BindWith(OsuSetting.GameplaySpectatorList, configVisibility);
        }

        protected virtual GameplaySpectatorUser CreateSpectatorUserDrawable([CanBeNull] IUser user) =>
            new GameplaySpectatorUser(user);
    }
}
