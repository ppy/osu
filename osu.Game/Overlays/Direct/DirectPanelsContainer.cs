// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Overlays.SearchableList;
using OpenTK;

namespace osu.Game.Overlays.Direct
{
    public class DirectPanelsContainer : Container
    {
        private const float panel_padding = 10f;

        private FillFlowContainer<DirectPanel> flowContainer;
        private BeatmapManager beatmaps;

        public DirectPanel CurrentPreview;

        private List<BeatmapSetInfo> beatmapSets = new List<BeatmapSetInfo>();
        public List<BeatmapSetInfo> BeatmapSets
        {
            get => beatmapSets;
            set
            {
                if (value.Equals(beatmapSets))
                    return;

                beatmapSets = value;

                recreatePanels();
            }
        }

        private PanelDisplayStyle displayStyle;
        public PanelDisplayStyle DisplayStyle
        {
            get => displayStyle;
            set
            {
                if (value == displayStyle)
                    return;

                displayStyle = value;

                recreatePanels();
            }
        }

        public DirectPanelsContainer()
        {
            Child = flowContainer = new FillFlowContainer<DirectPanel>
            {
                Anchor = Anchor.BottomRight,
                Origin = Anchor.BottomRight,
                Width = 810f,
                Spacing = new Vector2(panel_padding),
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Full,
            };
        }

        [BackgroundDependencyLoader]
        private void load(BeatmapManager beatmaps)
        {
            this.beatmaps = beatmaps;

            if (beatmaps != null)
            {
                beatmaps.ItemAdded += setAdded;
                beatmaps.ItemRemoved += setRemoved;
            }
        }

        /// <summary>
        /// Adds a new panel to the end of the existing list of panels.
        /// </summary>
        /// <param name="set">The <see cref="BeatmapSetInfo"/> to add.</param>
        public void AddPanel(BeatmapSetInfo set)
        {
            var existing = flowContainer.Children.FirstOrDefault(p => p.SetInfo.OnlineBeatmapSetID == set.OnlineBeatmapSetID);

            if (existing != null)
                return;

            beatmapSets.Add(set);

            DirectPanel newPanel;

            if (displayStyle == PanelDisplayStyle.Grid)
                newPanel = new DirectGridPanel(set);
            else
                newPanel = new DirectListPanel(set);

            newPanel.DownloadIndicatorsVisible = beatmaps.QueryBeatmapSet(s => s.OnlineBeatmapSetID == set.OnlineBeatmapSetID) == null;
            newPanel.Alpha = 0;

            flowContainer.Add(newPanel);
            newPanel.FadeIn(200);

            newPanel.PreviewPlaying.ValueChanged += newValue => previewPlayingChanged(newPanel, newValue);
        }

        /// <summary>
        /// Removes a panel from the existing list of panels.
        /// </summary>
        /// <param name="set">The <see cref="BeatmapSetInfo"/> to remove.</param>
        public void RemovePanel(BeatmapSetInfo set)
        {
            var panel = flowContainer.Children.FirstOrDefault(p => p.SetInfo.OnlineBeatmapSetID == set.OnlineBeatmapSetID);

            if (panel != null)
            {
                panel.FadeOut(200).Expire();
                beatmapSets.Remove(set);
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (beatmaps != null)
            {
                beatmaps.ItemAdded -= setAdded;
                beatmaps.ItemRemoved -= setRemoved;
            }
        }

        private void setAdded(BeatmapSetInfo set)
        {
            var panel = flowContainer.Children.FirstOrDefault(p => p.SetInfo.OnlineBeatmapSetID == set.OnlineBeatmapSetID);

            if (panel != null)
                panel.DownloadIndicatorsVisible = false;
        }

        private void setRemoved(BeatmapSetInfo set)
        {
            var panel = flowContainer.Children.FirstOrDefault(p => p.SetInfo.OnlineBeatmapSetID == set.OnlineBeatmapSetID);

            if (panel != null)
                panel.DownloadIndicatorsVisible = true;
        }

        private void previewPlayingChanged(DirectPanel panel, bool newValue)
        {
            if (!newValue)
                return;

            if (CurrentPreview != null && CurrentPreview != panel)
                CurrentPreview.PreviewPlaying.Value = false;

            CurrentPreview = panel;
        }

        private void recreatePanels()
        {
            if (!IsLoaded)
                return;

            if (flowContainer != null)
            {
                flowContainer.FadeOut(200);
                flowContainer.Expire();
                flowContainer = null;

                if (CurrentPreview != null)
                {
                    CurrentPreview.PreviewPlaying.Value = false;
                    CurrentPreview = null;
                }
            }

            if (BeatmapSets == null) return;

            var newPanels = new FillFlowContainer<DirectPanel>
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Spacing = new Vector2(panel_padding),
                Margin = new MarginPadding { Top = 10 },
                ChildrenEnumerable = BeatmapSets.Select(b =>
                {
                    DirectPanel panel;

                    switch (DisplayStyle)
                    {
                        case PanelDisplayStyle.Grid:
                            panel =  new DirectGridPanel(b)
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                            };
                            break;

                        default:
                            panel = new DirectListPanel(b);
                            break;
                    }

                    panel.DownloadIndicatorsVisible = beatmaps.QueryBeatmapSet(s => s.OnlineBeatmapSetID == b.OnlineBeatmapSetID) == null;

                    return panel;
                }),
            };

            LoadComponentAsync(newPanels, p =>
            {
                if (flowContainer != null)
                    Remove(flowContainer);

                Add(flowContainer = newPanels);

                foreach (DirectPanel panel in p.Children)
                    panel.PreviewPlaying.ValueChanged += newValue => previewPlayingChanged(panel, newValue);
            });
        }
    }
}
