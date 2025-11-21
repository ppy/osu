// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Toolkit.HighPerformance;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Transforms;
using osu.Framework.Utils;
using osu.Game.Graphics.Containers;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Rooms;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.Match.BeatmapSelect
{
    public partial class BeatmapSelectGrid : CompositeDrawable
    {
        public const double ARRANGE_DELAY = 200;

        private const double hide_duration = 800;
        private const double arrange_duration = 1000;
        private const double roll_duration = 4000;
        private const double present_beatmap_delay = 1200;
        private const float panel_spacing = 4;

        public event Action<MultiplayerPlaylistItem>? ItemSelected;

        private readonly Dictionary<long, MatchmakingSelectPanel> panelLookup = new Dictionary<long, MatchmakingSelectPanel>();
        private readonly Dictionary<long, MatchmakingPlaylistItem> playlistItems = new Dictionary<long, MatchmakingPlaylistItem>();
        private MatchmakingSelectPanelRandom randomPanel = null!;

        private readonly PanelGridContainer panelGridContainer;
        private readonly Container<MatchmakingSelectPanel> rollContainer;
        private readonly OsuScrollContainer scroll;

        private bool allowSelection = true;

        private readonly Sample?[] spinSamples = new Sample?[5];
        private static readonly int[] spin_sample_sequence = [0, 1, 2, 3, 4, 2, 3, 4];
        private Sample? randomRevealSample;
        private Sample? resultSample;
        private Sample? swooshSample;
        private double? lastSamplePlayback;

        public BeatmapSelectGrid()
        {
            InternalChildren = new Drawable[]
            {
                scroll = new OsuScrollContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    ScrollbarVisible = false,
                    Child = panelGridContainer = new PanelGridContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Padding = new MarginPadding(20),
                        Spacing = new Vector2(panel_spacing)
                    },
                },
                rollContainer = new Container<MatchmakingSelectPanel>
                {
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                },
            };
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            for (int i = 0; i < spinSamples.Length; i++)
                spinSamples[i] = audio.Samples.Get($@"Multiplayer/Matchmaking/Selection/roulette-{i}");

            randomRevealSample = audio.Samples.Get(@"Multiplayer/Matchmaking/Selection/random-reveal");
            resultSample = audio.Samples.Get(@"Multiplayer/Matchmaking/Selection/roulette-result");
            swooshSample = audio.Samples.Get(@"SongSelect/options-pop-out");
        }

        public void AddItems(IEnumerable<MatchmakingPlaylistItem> items)
        {
            foreach (var item in items)
            {
                playlistItems[item.ID] = item;

                var panel = panelLookup[item.ID] = new MatchmakingSelectPanelBeatmap(item)
                {
                    AllowSelection = allowSelection,
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Action = i => ItemSelected?.Invoke(i),
                };

                panelGridContainer.Add(panel);
                panelGridContainer.SetLayoutPosition(panel, (float)panel.Item.StarRating);
            }

            panelLookup[-1] = randomPanel = new MatchmakingSelectPanelRandom(new MultiplayerPlaylistItem { ID = -1 })
            {
                AllowSelection = allowSelection,
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                Action = i => ItemSelected?.Invoke(i),
            };
            panelGridContainer.Add(randomPanel);
            panelGridContainer.SetLayoutPosition(randomPanel, float.MinValue);

            const double enter_duration = 500;

            // the scroll container has a 1 frame delay until it receives the correct height for the scrollable area which leads to the scrollbar resizing awkwardly
            // if we wait until the panels have entered we get to avoid having to see that and the scrollbar it will appear synchronized with the rest of the content as a bonus
            Scheduler.AddDelayed(() => scroll.ScrollbarVisible = true, enter_duration);

            SchedulerAfterChildren.Add(() =>
            {
                foreach (var panel in panelGridContainer)
                {
                    double delay = panel.Y / 3;

                    panel.FadeInAndEnterFromBelow(duration: enter_duration, delay: delay);
                }

                panelsLoaded.SetResult();
            });
        }

        public void SetUserSelection(APIUser user, long itemId, bool selected) => whenPanelsLoaded(() =>
        {
            if (!panelLookup.TryGetValue(itemId, out var panel))
                return;

            if (selected)
                panel.AddUser(user);
            else
                panel.RemoveUser(user);
        });

        public void RevealRandomItem(MultiplayerPlaylistItem item) => whenPanelsLoaded(() =>
        {
            playlistItems.TryGetValue(item.ID, out var playlistItem);

            Debug.Assert(playlistItem != null);

            randomRevealSample?.Play();
            randomPanel.RevealBeatmap(playlistItem.Beatmap, playlistItem.Mods);
        });

        public void RollAndDisplayFinalBeatmap(long[] candidateItemIds, long finalItemId) => whenPanelsLoaded(() =>
        {
            Debug.Assert(candidateItemIds.Length >= 1);
            Debug.Assert(candidateItemIds.Contains(finalItemId));
            Debug.Assert(panelLookup.ContainsKey(finalItemId));
            Debug.Assert(candidateItemIds.All(id => panelLookup.ContainsKey(id)));

            allowSelection = false;

            TransferCandidatePanelsToRollContainer(candidateItemIds);

            if (candidateItemIds.Length == 1)
            {
                this.Delay(ARRANGE_DELAY)
                    .Schedule(() => ArrangeItemsForRollAnimation())
                    .Delay(arrange_duration + present_beatmap_delay)
                    .Schedule(() => PresentUnanimouslyChosenBeatmap(finalItemId));
            }
            else
            {
                this.Delay(ARRANGE_DELAY)
                    .Schedule(() => ArrangeItemsForRollAnimation())
                    .Delay(arrange_duration)
                    .Schedule(() => PlayRollAnimation(finalItemId, roll_duration))
                    .Delay(roll_duration + present_beatmap_delay)
                    .Schedule(() => PresentRolledBeatmap(finalItemId));
            }
        });

        internal void TransferCandidatePanelsToRollContainer(long[] candidateItemIds, double duration = hide_duration)
        {
            scroll.ScrollbarVisible = false;
            panelGridContainer.LayoutDisabled = true;

            var rng = new Random();

            var remainingPanels = new List<MatchmakingSelectPanel>();

            foreach (var panel in panelGridContainer.Children.ToArray())
            {
                panel.AllowSelection = false;

                if (!candidateItemIds.Contains(panel.Item.ID))
                {
                    panel.PopOutAndExpire(duration: duration / 2, delay: rng.NextDouble() * duration / 2);
                    continue;
                }

                remainingPanels.Add(panel);
            }

            rng.Shuffle(remainingPanels.AsSpan());

            foreach (var panel in remainingPanels)
            {
                var position = panel.ScreenSpaceDrawQuad.Centre;

                panelGridContainer.Remove(panel, false);

                panel.Anchor = panel.Origin = Anchor.Centre;
                panel.Position = rollContainer.ToLocalSpace(position) - rollContainer.ChildSize / 2;

                rollContainer.Add(panel);
            }
        }

        internal void ArrangeItemsForRollAnimation(double duration = arrange_duration, double stagger = 30)
        {
            var positions = calculateLayoutPositionsForRollAnimation(rollContainer.Children.Count);

            Debug.Assert(positions.Length == rollContainer.Children.Count);

            for (int i = 0; i < positions.Length; i++)
            {
                var panel = rollContainer.Children[i];

                var position = positions[i] * (MatchmakingSelectPanel.SIZE + new Vector2(panel_spacing));

                panel.MoveTo(position, duration + stagger * i, new SplitEasingFunction(Easing.InCubic, Easing.OutExpo, 0.3f));

                Scheduler.AddDelayed(() =>
                {
                    var chan = swooshSample?.GetChannel();
                    if (chan == null) return;

                    chan.Frequency.Value = 1.25f - RNG.NextDouble(0.5f);
                    chan.Play();
                }, stagger * i);
            }
        }

        private static Vector2[] calculateLayoutPositionsForRollAnimation(int panelCount)
        {
            if (panelCount == 1)
                return new[] { Vector2.Zero };

            // goal is to get the positions arranged in clockwise order, with the top-left position being the first one
            // to keep things simple the positions are first inserted in the order: right row, optional bottom center panel, left row backwards
            // then the positions get shifted by 1 to move the top-left position into the first spot

            bool hasCenterPanel = panelCount % 2 == 1;
            int rowCount = (panelCount + 1) / 2;
            int outerRowCount = hasCenterPanel ? rowCount - 1 : rowCount;

            float yOffset = -(rowCount - 1f) / 2;

            var positions = new Vector2[panelCount];

            for (int row = 0; row < outerRowCount; row++)
            {
                positions[row] = new Vector2(0.5f, row + yOffset);
            }

            if (hasCenterPanel)
            {
                int centerIndex = panelCount / 2;

                positions[centerIndex] = new Vector2(0, outerRowCount + yOffset);
            }

            for (int row = 0; row < outerRowCount; row++)
            {
                int index = positions.Length - 1 - row;

                positions[index] = new Vector2(-0.5f, row + yOffset);
            }

            return positions.TakeLast(1).Concat(positions.SkipLast(1)).ToArray();
        }

        internal void PlayRollAnimation(long finalItem, double duration = roll_duration)
        {
            const int minimum_steps = 20;

            int finalItemIndex = rollContainer.Children
                                              .Select(it => it.Item.ID)
                                              .ToImmutableList()
                                              .IndexOf(finalItem);

            Debug.Assert(finalItemIndex >= 0);

            int numSteps = minimum_steps;
            while ((numSteps - 1) % rollContainer.Children.Count != finalItemIndex)
                numSteps++;

            MatchmakingSelectPanel? lastPanel = null;

            for (int i = 0; i < numSteps; i++)
            {
                float progress = ((float)i) / (numSteps - 1);

                double delay = Math.Pow(progress, 2.5) * duration;
                var panel = rollContainer.Children[i % rollContainer.Children.Count];

                int ii = i;
                Scheduler.AddDelayed(() =>
                {
                    lastPanel?.HideBorder();
                    panel.ShowBorder();

                    if (lastSamplePlayback == null || Time.Current - lastSamplePlayback > OsuGameBase.SAMPLE_DEBOUNCE_TIME)
                    {
                        int sequenceIdx = ii % spin_sample_sequence.Length;
                        spinSamples[spin_sample_sequence[sequenceIdx]]?.Play();
                        lastSamplePlayback = Time.Current;
                    }

                    lastPanel = panel;
                }, delay);
            }
        }

        internal void PresentRolledBeatmap(long finalItem)
        {
            Debug.Assert(rollContainer.Children.Any(it => it.Item.ID == finalItem));

            foreach (var panel in rollContainer.Children)
            {
                if (panel.Item.ID != finalItem)
                {
                    panel.FadeOut(200);
                    panel.PopOutAndExpire(easing: Easing.InQuad);
                    continue;
                }

                // if we changed child depth without scheduling we'd change the order of the panels while iterating
                Schedule(() =>
                {
                    rollContainer.ChangeChildDepth(panel, float.MinValue);

                    panel.ShowChosenBorder();
                    panel.MoveTo(Vector2.Zero, 1000, Easing.OutExpo)
                         .ScaleTo(1.5f, 1000, Easing.OutExpo);

                    resultSample?.Play();
                });
            }
        }

        internal void PresentUnanimouslyChosenBeatmap(long finalItem)
        {
            // TODO: display special animation in this case

            PresentRolledBeatmap(finalItem);
        }

        private readonly TaskCompletionSource panelsLoaded = new TaskCompletionSource();

        private void whenPanelsLoaded(Action action) => Task.Run(async () =>
        {
            await panelsLoaded.Task.ConfigureAwait(false);
            Schedule(action);
        });

        private partial class PanelGridContainer : FillFlowContainer<MatchmakingSelectPanel>
        {
            public bool LayoutDisabled;

            protected override IEnumerable<Vector2> ComputeLayoutPositions()
            {
                if (LayoutDisabled)
                    return FlowingChildren.Select(c => c.Position);

                return base.ComputeLayoutPositions();
            }
        }

        private readonly struct SplitEasingFunction(DefaultEasingFunction easeIn, DefaultEasingFunction easeOut, float ratio) : IEasingFunction
        {
            public SplitEasingFunction(Easing easeIn, Easing easeOut, float ratio = 0.5f)
                : this(new DefaultEasingFunction(easeIn), new DefaultEasingFunction(easeOut), ratio)
            {
            }

            public double ApplyEasing(double time)
            {
                if (time < ratio)
                    return easeIn.ApplyEasing(time / ratio) * ratio;

                return double.Lerp(ratio, 1, easeOut.ApplyEasing((time - ratio) / (1 - ratio)));
            }
        }
    }
}
