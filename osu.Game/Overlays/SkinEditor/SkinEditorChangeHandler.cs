// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Screens.Edit;
using osu.Game.Skinning;

namespace osu.Game.Overlays.SkinEditor
{
    public partial class SkinEditorChangeHandler : EditorChangeHandler
    {
        private readonly ISerialisableDrawableContainer? firstTarget;

        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly BindableList<ISerialisableDrawable>? components;

        public SkinEditorChangeHandler(Drawable targetScreen)
        {
            // To keep things simple, we are currently only handling the current target screen for undo / redo.
            // In the future we'll want this to cover all changes, even to skin's `InstantiationInfo`.
            // We'll also need to consider cases where multiple targets are on screen at the same time.

            firstTarget = targetScreen.ChildrenOfType<ISerialisableDrawableContainer>().FirstOrDefault();

            if (firstTarget == null)
                return;

            components = new BindableList<ISerialisableDrawable> { BindTarget = firstTarget.Components };
            components.BindCollectionChanged((_, _) => SaveState());
        }

        protected override void WriteCurrentStateToStream(MemoryStream stream)
        {
            if (firstTarget == null)
                return;

            var skinnableInfos = firstTarget.CreateSerialisedInfo().ToArray();
            string json = JsonConvert.SerializeObject(skinnableInfos, new JsonSerializerSettings { Formatting = Formatting.Indented });
            stream.Write(Encoding.UTF8.GetBytes(json));
        }

        protected override void ApplyStateChange(byte[] previousState, byte[] newState)
        {
            if (firstTarget == null)
                return;

            var deserializedContent = JsonConvert.DeserializeObject<IEnumerable<SerialisedDrawableInfo>>(Encoding.UTF8.GetString(newState));

            if (deserializedContent == null)
                return;

            SerialisedDrawableInfo[] skinnableInfos = deserializedContent.ToArray();
            ISerialisableDrawable[] targetComponents = firstTarget.Components.ToArray();

            // Store components based on type for later reuse
            var componentsPerTypeLookup = new Dictionary<Type, Queue<Drawable>>();

            foreach (ISerialisableDrawable component in targetComponents)
            {
                Type lookup = component.GetType();

                if (!componentsPerTypeLookup.TryGetValue(lookup, out Queue<Drawable>? componentsOfSameType))
                    componentsPerTypeLookup.Add(lookup, componentsOfSameType = new Queue<Drawable>());

                componentsOfSameType.Enqueue((Drawable)component);
            }

            for (int i = targetComponents.Length - 1; i >= 0; i--)
                firstTarget.Remove(targetComponents[i], false);

            foreach (var skinnableInfo in skinnableInfos)
            {
                Type lookup = skinnableInfo.Type;

                if (!componentsPerTypeLookup.TryGetValue(lookup, out Queue<Drawable>? componentsOfSameType))
                {
                    firstTarget.Add((ISerialisableDrawable)skinnableInfo.CreateInstance());
                    continue;
                }

                // Wherever possible, attempt to reuse existing component instances.
                if (componentsOfSameType.TryDequeue(out Drawable? component))
                {
                    component.ApplySerialisedInfo(skinnableInfo);
                }
                else
                {
                    component = skinnableInfo.CreateInstance();
                }

                firstTarget.Add((ISerialisableDrawable)component);
            }

            // Dispose components which were not reused.
            foreach ((Type _, Queue<Drawable> typeComponents) in componentsPerTypeLookup)
            {
                foreach (var component in typeComponents)
                    component.Dispose();
            }
        }
    }
}
