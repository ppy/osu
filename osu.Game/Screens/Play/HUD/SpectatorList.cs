// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Specialized;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Pooling;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Localisation.HUD;
using osu.Game.Online.Chat;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Spectator;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Play.HUD
{
    public partial class SpectatorList : CompositeDrawable, ISerialisableDrawable
    {
        private const int max_spectators_displayed = 10;

        public Bindable<Typeface> HeaderFont { get; } = new Bindable<Typeface>(Typeface.Torus);
        public BindableColour4 HeaderColour { get; } = new BindableColour4(Colour4.White);

        private IBindableList<SpectatorUser> watchingUsers { get; } = new BindableList<SpectatorUser>();
        private IBindableList<int> multiplayerPlayers { get; } = new BindableList<int>();
        private BindableList<SpectatorUser> actualSpectators { get; } = new BindableList<SpectatorUser>();

        private Bindable<LocalUserPlayingState> userPlayingState { get; } = new Bindable<LocalUserPlayingState>();

        private OsuSpriteText header = null!;
        private FillFlowContainer mainFlow = null!;
        private FillFlowContainer<SpectatorListEntry> spectatorsFlow = null!;
        private DrawablePool<SpectatorListEntry> pool = null!;

        [Resolved]
        private SpectatorClient client { get; set; } = null!;

        [Resolved]
        private GameplayState gameplayState { get; set; } = null!;

        [Resolved]
        private MultiplayerClient multiplayerClient { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            AutoSizeAxes = Axes.Y;

            InternalChildren = new[]
            {
                Empty().With(t => t.Size = new Vector2(100, 50)),
                mainFlow = new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    Children = new Drawable[]
                    {
                        header = new OsuSpriteText
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

            HeaderColour.Value = header.Colour;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            ((IBindable<LocalUserPlayingState>)userPlayingState).BindTo(gameplayState.PlayingState);

            multiplayerPlayers.BindTo(multiplayerClient.CurrentMatchPlayingUserIds);
            multiplayerPlayers.BindCollectionChanged((_, _) => removePlayersFromMultiplayerRoom());

            watchingUsers.BindTo(client.WatchingUsers);
            watchingUsers.BindCollectionChanged(onWatchingUsersChanged, true);

            actualSpectators.BindCollectionChanged(onSpectatorsChanged, true);
            userPlayingState.BindValueChanged(_ => updateVisibility());

            HeaderFont.BindValueChanged(_ => updateAppearance());
            HeaderColour.BindValueChanged(_ => updateAppearance(), true);
            FinishTransforms(true);

            this.FadeInFromZero(200, Easing.OutQuint);
        }

        private void onWatchingUsersChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                {
                    for (int i = 0; i < e.NewItems!.Count; i++)
                        actualSpectators.Add((SpectatorUser)e.NewItems![i]!);

                    break;
                }

                case NotifyCollectionChangedAction.Remove:
                {
                    for (int i = 0; i < e.OldItems!.Count; i++)
                        actualSpectators.Remove((SpectatorUser)e.OldItems![i]!);

                    break;
                }

                case NotifyCollectionChangedAction.Reset:
                {
                    actualSpectators.Clear();
                    break;
                }

                default:
                    throw new NotSupportedException();
            }

            removePlayersFromMultiplayerRoom();
        }

        private void removePlayersFromMultiplayerRoom()
        {
            // the multiplayer gameplay leaderboard relies on calling `SpectatorClient.WatchUser()` to get updates on users' total scores.
            // this has an unfortunate side effect of other players showing up in `SpectatorClient.WatchingUsers`.
            //
            // we do not generally wish to display other players in the room as spectators due to that implementation detail,
            // therefore this code is intended to filter out those players on the client side.
            actualSpectators.RemoveAll(s => multiplayerPlayers.Contains(s.OnlineID));
        }

        private void onSpectatorsChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                {
                    for (int i = 0; i < e.NewItems!.Count; i++)
                    {
                        var spectator = (SpectatorUser)e.NewItems![i]!;
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

                    if (actualSpectators.Count >= max_spectators_displayed && spectatorsFlow.Count < max_spectators_displayed)
                    {
                        for (int i = spectatorsFlow.Count; i < max_spectators_displayed; i++)
                            addNewSpectatorToList(i, actualSpectators[i]);
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

            header.Text = SpectatorListStrings.SpectatorCount(actualSpectators.Count).ToUpper();
            updateVisibility();

            for (int i = 0; i < spectatorsFlow.Count; i++)
            {
                spectatorsFlow[i].Colour = i < max_spectators_displayed - 1
                    ? Color4.White
                    : ColourInfo.GradientVertical(Color4.White, Color4.White.Opacity(0));
            }
        }

        private void addNewSpectatorToList(int i, SpectatorUser spectator)
        {
            var entry = pool.Get(entry =>
            {
                entry.Current.Value = spectator;
                entry.UserPlayingState = userPlayingState;
            });

            spectatorsFlow.Insert(i, entry);
        }

        private void updateVisibility()
        {
            // We don't want to show spectators when we are watching a replay.
            mainFlow.FadeTo(actualSpectators.Count > 0 && userPlayingState.Value != LocalUserPlayingState.NotPlaying ? 1 : 0, 250, Easing.OutQuint);
        }

        private void updateAppearance()
        {
            header.Font = OsuFont.GetFont(HeaderFont.Value, 12, FontWeight.Bold);
            header.Colour = HeaderColour.Value;

            Width = header.DrawWidth;
        }

        private partial class SpectatorListEntry : PoolableDrawable
        {
            public Bindable<SpectatorUser> Current { get; } = new Bindable<SpectatorUser>();

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

        public bool UsesFixedAnchor { get; set; }
    }
}
