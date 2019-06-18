// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Settings;
using osu.Game.Tournament.Components;
using osuTK;

namespace osu.Game.Tournament.Screens.Editors
{
    public abstract class TournamentEditorScreen<TDrawable, TModel> : TournamentScreen, IProvideVideo
        where TDrawable : Drawable, IModelBacked<TModel>
        where TModel : class, new()
    {
        protected abstract BindableList<TModel> Storage { get; }

        private FillFlowContainer<TDrawable> flow;

        protected ControlPanel ControlPanel;

        [BackgroundDependencyLoader]
        private void load()
        {
            AddRangeInternal(new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = OsuColour.Gray(0.2f),
                },
                new OsuScrollContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Width = 0.9f,
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Child = flow = new FillFlowContainer<TDrawable>
                    {
                        Direction = FillDirection.Vertical,
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        LayoutDuration = 200,
                        LayoutEasing = Easing.OutQuint,
                        Spacing = new Vector2(20)
                    },
                },
                ControlPanel = new ControlPanel
                {
                    Children = new Drawable[]
                    {
                        new OsuButton
                        {
                            RelativeSizeAxes = Axes.X,
                            Text = "Add new",
                            Action = () => Storage.Add(new TModel())
                        },
                        new DangerousSettingsButton
                        {
                            RelativeSizeAxes = Axes.X,
                            Text = "Clear all",
                            Action = Storage.Clear
                        },
                    }
                }
            });

            Storage.ItemsAdded += items => items.ForEach(i => flow.Add(CreateDrawable(i)));
            Storage.ItemsRemoved += items => items.ForEach(i => flow.RemoveAll(d => d.Model == i));

            foreach (var model in Storage)
                flow.Add(CreateDrawable(model));
        }

        protected abstract TDrawable CreateDrawable(TModel model);
    }
}
