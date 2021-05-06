// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.IO;
using System.Linq;
using Newtonsoft.Json;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Extensions;
using osu.Game.Rulesets.Edit;
using osu.Game.Screens.Edit.Compose.Components;

namespace osu.Game.Skinning.Editor
{
    public class SkinBlueprintContainer : BlueprintContainer<ISkinnableComponent>
    {
        private readonly Drawable target;

        public SkinBlueprintContainer(Drawable target)
        {
            this.target = target;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            checkForComponents();
        }

        private void checkForComponents()
        {
            ISkinnableComponent[] skinnableComponents = target.ChildrenOfType<ISkinnableComponent>().ToArray();

            // todo: the OfType() call can be removed with better IDrawable support.
            string json = JsonConvert.SerializeObject(skinnableComponents.OfType<Drawable>().Select(d => d.CreateSerialisedInformation()), new JsonSerializerSettings { Formatting = Formatting.Indented });

            File.WriteAllText("/Users/Dean/json-out.json", json);

            foreach (var c in skinnableComponents)
                AddBlueprintFor(c);

            // We'd hope to eventually be running this in a more sensible way, but this handles situations where new drawables become present (ie. during ongoing gameplay)
            // or when drawables in the target are loaded asynchronously and may not be immediately available when this BlueprintContainer is loaded.
            Scheduler.AddDelayed(checkForComponents, 1000);
        }

        protected override SelectionHandler<ISkinnableComponent> CreateSelectionHandler() => new SkinSelectionHandler();

        protected override SelectionBlueprint<ISkinnableComponent> CreateBlueprintFor(ISkinnableComponent component)
            => new SkinBlueprint(component);
    }
}
