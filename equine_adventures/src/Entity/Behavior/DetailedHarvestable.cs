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
        private long listenerID;

        public DetailedHarvestable(Entity entity)
          : base(entity)
        {
        }

        public override void Initialize(EntityProperties properties, JsonObject typeAttributes) {
            JsonObject editedTypeAttributes = typeAttributes.Clone();
            editedTypeAttributes.Token["fixedweight"] = true;
            base.Initialize(properties, editedTypeAttributes);
            int frequency = 3000;
            listenerID = entity.World.RegisterGameTickListener(new Action<float>(SlowTick), frequency);
        }

        public override void AfterInitialized(bool onFirstSpawn) {
            if (onFirstSpawn) {
                AnimalWeight = 0.85f
                    + 0.1f * (float)entity.World.Rand.NextDouble()
                    + 0.1f * (float)entity.World.Rand.NextDouble();
                LastWeightUpdateTotalHours = entity.World.Calendar.TotalHours;
            }
        }

        public override void OnGameTick(float deltaTime) {
            // Do nothing. Don't call base method. Don't reset AnimalWeight to 1.
        }

        protected void SlowTick(float deltaTime) {
            // TODO: Weight change logic here
        }

        public override void GetInfoText(StringBuilder infotext) {
            base.GetInfoText(infotext);
            double[] conditionBoundaries = new double[] {-0.18, -0.12, -0.072, -0.036, 0.036, 0.072, 0.12, 0.18};
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
            // TODO
            double weightKilograms = 503.45;
            double weightPounds = weightKilograms * 2.20462;
            // Also TODO: This should depend on age

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
