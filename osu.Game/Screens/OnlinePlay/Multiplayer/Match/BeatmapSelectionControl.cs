// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Screens;
using osu.Game.Online.API;
using osu.Game.Screens.OnlinePlay.Match.Components;

namespace osu.Game.Screens.OnlinePlay.Multiplayer.Match
{
    public class BeatmapSelectionControl : RoomSubScreenComposite
    {
        [Resolved]
        private MultiplayerMatchSubScreen matchSubScreen { get; set; }

        [Resolved]
        private IAPIProvider api { get; set; }

        private Container beatmapPanelContainer;
        private Button selectButton;

        public BeatmapSelectionControl()
        {
            AutoSizeAxes = Axes.Y;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Children = new Drawable[]
                {
                    beatmapPanelContainer = new Container
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y
                    },
                    selectButton = new PurpleTriangleButton
                    {
                        RelativeSizeAxes = Axes.X,
                        Height = 40,
                        Text = "Select beatmap",
                        Action = () => matchSubScreen.Push(new MultiplayerMatchSongSelect()),
                        Alpha = 0
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            SelectedItem.BindValueChanged(_ => updateBeatmap(), true);
            Host.BindValueChanged(host =>
            {
                if (RoomID.Value == null || host.NewValue?.Equals(api.LocalUser.Value) == true)
                    selectButton.Show();
                else
                    selectButton.Hide();
            }, true);
        }

        private void updateBeatmap()
        {
            if (SelectedItem.Value == null)
                beatmapPanelContainer.Clear();
            else
                beatmapPanelContainer.Child = new DrawableRoomPlaylistItem(SelectedItem.Value, false, false);
        }
    }
}
