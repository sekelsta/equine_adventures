using System;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;


using System.Collections.Generic;
using Vintagestory.API.Server;
using Vintagestory.API.Util;

namespace EquineAdventures {
    public class DetailedHarvestable : EntityBehaviorHarvestable {
        private float baseWeightKg; // TODO: Make final weight depend on age and pregnancy status

        public DetailedHarvestable(Entity entity)
          : base(entity)
        {
        }

        public override void Initialize(EntityProperties properties, JsonObject typeAttributes) {
            JsonObject editedTypeAttributes = typeAttributes.Clone();
            editedTypeAttributes.Token["fixedweight"] = true;
            base.Initialize(properties, editedTypeAttributes);
            baseWeightKg = typeAttributes["weightKg"].AsFloat(200f);
        }

        public override void AfterInitialized(bool onFirstSpawn) {
            if (onFirstSpawn) {
                AnimalWeight = 0.85f
                    + 0.1f * (float)entity.World.Rand.NextDouble()
                    + 0.1f * (float)entity.World.Rand.NextDouble();
                LastWeightUpdateTotalHours = entity.World.Calendar.TotalHours;
            }
        }

        private void shiftWeight(float delta) {
            if (entity.WatchedAttributes.GetTreeAttribute("hunger") == null) {
                entity.WatchedAttributes["hunger"] = new TreeAttribute();
            }
            ITreeAttribute hungerTree = entity.WatchedAttributes.GetTreeAttribute("hunger");
            hungerTree.SetFloat("saturation", hungerTree.GetFloat("saturation", 0f) - delta);
            entity.WatchedAttributes.MarkPathDirty("hunger");
            float conversion = 0.2f; // TODO: Set a reasonable value
            // Inefficiency
            conversion *= delta > 0 ? 0.99f : 1.01f;
            AnimalWeight = (float)Math.Clamp(AnimalWeight + delta * conversion, 0.5f, 2f);
        }

        public override void OnGameTick(float deltaTime) {
            // Don't call base method. Don't reset AnimalWeight to 1.

            double updateTime = LastWeightUpdateTotalHours;
            double updateFrequencyHours = 2;
            while (updateTime + updateFrequencyHours < entity.World.Calendar.TotalHours) {
                updateTime += updateFrequencyHours;
                ITreeAttribute hungerTree = entity.WatchedAttributes.GetTreeAttribute("hunger");
                float saturation = hungerTree == null ? 0f : hungerTree.GetFloat("saturation");
                float maxSaturation = 16f;
                if (hungerTree != null) {
                    maxSaturation = hungerTree.GetFloat("maxsaturation", maxSaturation);
                }
                float fullness = saturation / maxSaturation;
                float s = 0.05f * maxSaturation;
                if (fullness < 0.1f) {
                    shiftWeight(-1 * s);
                }
                else if (fullness > 0.9f) {
                    shiftWeight(s);
                }
                else if (AnimalWeight < 1 && fullness > 0.2f) {
                    shiftWeight(s);
                }
                else if (AnimalWeight > 1 && fullness < 0.8f) {
                    shiftWeight(-1 * s);
                }
            }
            LastWeightUpdateTotalHours = updateTime;
        }

        public override void GetInfoText(StringBuilder infotext) {
            base.GetInfoText(infotext);
            double[] conditionBoundaries = new double[] {-0.3, -0.15, -0.072, -0.036, 0.036, 0.072, 0.15, 0.3};
            int bodyScore = 1;
            foreach (double b in conditionBoundaries) {
                if (AnimalWeight > 1 + b) {
                    bodyScore += 1;
                }
                else {
                    break;
                }
            }
            Debug.Assert(bodyScore >= 1);
            Debug.Assert(bodyScore <= 9);

            double weightKilograms = AnimalWeight * baseWeightKg;
            double weightPounds = weightKilograms * 2.20462;

            int kilogramsRounded = 5 * (int)Math.Round(weightKilograms / 5);
            int poundsRounded = 10 * (int)Math.Round(weightPounds / 10);
            string unitsSuffix = EquineAdventures.Config.WeightSuffix();
            string conditionKey = "equine_adventures:infotext-bodycondition" + bodyScore.ToString() + "-male";//TODO. See entity.Properties.Variant
            string text = Lang.GetUnformatted("equine_adventures:infotext-conditionweight" + unitsSuffix)
                .Replace("{condition}", Lang.Get(conditionKey))
                .Replace("{pounds}", poundsRounded.ToString())
                .Replace("{kilograms}", kilogramsRounded.ToString());
            infotext.AppendLine(text);
        }

        public override string PropertyName() => "detailedharvestable";
    }
}
