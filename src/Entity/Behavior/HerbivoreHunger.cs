using System;

using Vintagestory.API.Common.Entities;
using Vintagestory.GameContent;

namespace EquineAdventures {
    public class HerbivoreHunger : EntityBehaviorHunger {
        public HerbivoreHunger(Entity entity) : base(entity) { }

        // TODO: This is never set
        public float ForageLevel {
            get => entity.WatchedAttributes.GetTreeAttribute("hunger").GetFloat("forageLevel");
            set {
                entity.WatchedAttributes.GetTreeAttribute("hunger").SetFloat("forageLevel", value);
                entity.WatchedAttributes.MarkPathDirty("hunger");
            }
        }

        // TODO: This is never set
        public float HydrationLevel {
            get => entity.WatchedAttributes.GetTreeAttribute("hunger").GetFloat("hydrationLevel");
            set {
                entity.WatchedAttributes.GetTreeAttribute("hunger").SetFloat("hydrationLevel", value);
                entity.WatchedAttributes.MarkPathDirty("hunger");
            }
        }

        // TODO: Call this instead of UpdateNutrientHealthBoost, which I can't override because it's not virtual
        public void UpdateNutrientBoost() {
            float forage = ForageLevel / MaxSaturation;
            float fruit = FruitLevel / MaxSaturation;
            float grain = GrainLevel / MaxSaturation;
            float vegetable = VegetableLevel / MaxSaturation;
            float protein = ProteinLevel / MaxSaturation;
            float dairy = DairyLevel / MaxSaturation;
            float water = (float)Math.Min(HydrationLevel / MaxSaturation, 0.8);

            float variety = (float)(Math.Min(0.1, fruit) + Math.Min(0.25, vegetable) + Math.Min(0.25, grain));
            float sweets = fruit + 0.5f * vegetable;
            float healthMod = 8 * forage + 4 * water + 5 * variety + (float)Math.Min(0, 0.25 - sweets);

            EntityBehaviorHealth behavior = entity.GetBehavior<EntityBehaviorHealth>();
            behavior.MaxHealthModifiers["nutrientHealthMod"] = healthMod;
            behavior.MarkDirty();
        }

        public bool ShouldNurse() {
            // TODO
            return false;
        }

        public bool ShouldEat() {
            return Saturation / MaxSaturation < 0.8f && entity.WatchedAttributes.GetFloat("animalWeight", 1f) < 1.5f;
        }

        public bool ShouldDrink() {
            return HydrationLevel * 0.8 < MaxSaturation;
        }
    }
}
