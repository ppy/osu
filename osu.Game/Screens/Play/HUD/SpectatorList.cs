// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Specialized;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Pooling;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.Chat;
using osu.Game.Users;
using osu.Game.Localisation.HUD;
using osu.Game.Localisation.SkinComponents;
using osuTK.Graphics;

namespace osu.Game.Screens.Play.HUD
{
    public partial class SpectatorList : CompositeDrawable
    {
        private const int max_spectators_displayed = 10;

        public BindableList<Spectator> Spectators { get; } = new BindableList<Spectator>();
        public Bindable<LocalUserPlayingState> UserPlayingState { get; } = new Bindable<LocalUserPlayingState>();

        [SettingSource(typeof(SkinnableComponentStrings), nameof(SkinnableComponentStrings.Font), nameof(SkinnableComponentStrings.FontDescription))]
        public Bindable<Typeface> Font { get; } = new Bindable<Typeface>(Typeface.Torus);

        [SettingSource(typeof(SkinnableComponentStrings), nameof(SkinnableComponentStrings.TextColour), nameof(SkinnableComponentStrings.TextColourDescription))]
        public BindableColour4 HeaderColour { get; } = new BindableColour4(Colour4.White);

        protected OsuSpriteText Header { get; private set; } = null!;

        private FillFlowContainer mainFlow = null!;
        private FillFlowContainer<SpectatorListEntry> spectatorsFlow = null!;
        private DrawablePool<SpectatorListEntry> pool = null!;

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            AutoSizeAxes = Axes.Y;

            InternalChildren = new Drawable[]
            {
                mainFlow = new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    Children = new Drawable[]
                    {
                        Header = new OsuSpriteText
                        {
                            Colour = colours.Blue0,
                            Font = OsuFont.GetFont(size: 12, weight: FontWeight.Bold),
                        },
                        spectatorsFlow = new FillFlowContainer<SpectatorListEntry>
                        {
                            AutoSizeAxes = Axes.Both,
                            Direction = FillDirection.Vertical,
                        }
                    }
                },
                pool = new DrawablePool<SpectatorListEntry>(max_spectators_displayed),
            };

            HeaderColour.Value = Header.Colour;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Spectators.BindCollectionChanged(onSpectatorsChanged, true);
            UserPlayingState.BindValueChanged(_ => updateVisibility());

            Font.BindValueChanged(_ => updateAppearance());
            HeaderColour.BindValueChanged(_ => updateAppearance(), true);
            FinishTransforms(true);

            this.FadeInFromZero(200, Easing.OutQuint);
        }

        private void onSpectatorsChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                {
                    for (int i = 0; i < e.NewItems!.Count; i++)
                    {
                        var spectator = (Spectator)e.NewItems![i]!;
                        int index = Math.Max(e.NewStartingIndex, 0) + i;

                        if (index >= max_spectators_displayed)
                            break;

                        addNewSpectatorToList(index, spectator);
                    }

                    break;
                }

                case NotifyCollectionChangedAction.Remove:
                {
                    spectatorsFlow.RemoveAll(entry => e.OldItems!.Contains(entry.Current.Value), false);

                    for (int i = 0; i < spectatorsFlow.Count; i++)
                        spectatorsFlow.SetLayoutPosition(spectatorsFlow[i], i);

                    if (Spectators.Count >= max_spectators_displayed && spectatorsFlow.Count < max_spectators_displayed)
                    {
                        for (int i = spectatorsFlow.Count; i < max_spectators_displayed; i++)
                            addNewSpectatorToList(i, Spectators[i]);
                    }

                    break;
                }

                case NotifyCollectionChangedAction.Reset:
                {
                    spectatorsFlow.Clear(false);
                    break;
                }

                default:
                    throw new NotSupportedException();
            }

            Header.Text = SpectatorListStrings.SpectatorCount(Spectators.Count).ToUpper();
            updateVisibility();

            for (int i = 0; i < spectatorsFlow.Count; i++)
            {
                spectatorsFlow[i].Colour = i < max_spectators_displayed - 1
                    ? Color4.White
                    : ColourInfo.GradientVertical(Color4.White, Color4.White.Opacity(0));
            }
        }

        private void addNewSpectatorToList(int i, Spectator spectator)
        {
            var entry = pool.Get(entry =>
            {
                entry.Current.Value = spectator;
                entry.UserPlayingState = UserPlayingState;
            });

            spectatorsFlow.Insert(i, entry);
        }

        private void updateVisibility()
        {
            mainFlow.FadeTo(Spectators.Count > 0 && UserPlayingState.Value != LocalUserPlayingState.NotPlaying ? 1 : 0, 250, Easing.OutQuint);
        }

        private void updateAppearance()
        {
            Header.Font = OsuFont.GetFont(Font.Value, 12, FontWeight.Bold);
            Header.Colour = HeaderColour.Value;

            Width = Header.DrawWidth;
        }

        private partial class SpectatorListEntry : PoolableDrawable
        {
            public Bindable<Spectator> Current { get; } = new Bindable<Spectator>();

            private readonly BindableWithCurrent<LocalUserPlayingState> current = new BindableWithCurrent<LocalUserPlayingState>();

            public Bindable<LocalUserPlayingState> UserPlayingState
            {
                get => current.Current;
                set => current.Current = value;
            }

            private OsuSpriteText username = null!;
            private DrawableLinkCompiler? linkCompiler;

            [Resolved]
            private OsuGame? game { get; set; }

            [BackgroundDependencyLoader]
            private void load()
            {
                AutoSizeAxes = Axes.Both;

                InternalChildren = new Drawable[]
                {
                    username = new OsuSpriteText(),
                };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                UserPlayingState.BindValueChanged(_ => updateEnabledState());
                Current.BindValueChanged(_ => updateState(), true);
            }

            protected override void PrepareForUse()
            {
                base.PrepareForUse();

                username.MoveToX(10)
                        .Then()
                        .MoveToX(0, 400, Easing.OutQuint);

                this.FadeInFromZero(400, Easing.OutQuint);
            }

            private void updateState()
            {
                username.Text = Current.Value.Username;
                linkCompiler?.Expire();
                AddInternal(linkCompiler = new DrawableLinkCompiler([username])
                {
                    IdleColour = Colour4.White,
                    Action = () => game?.HandleLink(new LinkDetails(LinkAction.OpenUserProfile, Current.Value)),
                });
                updateEnabledState();
            }

            private void updateEnabledState()
            {
                if (linkCompiler != null)
                    linkCompiler.Enabled.Value = UserPlayingState.Value != LocalUserPlayingState.Playing;
            }
        }

        public record Spectator(int OnlineID, string Username) : IUser
        {
            public CountryCode CountryCode => CountryCode.Unknown;
            public bool IsBot => false;
        }
    }
}
