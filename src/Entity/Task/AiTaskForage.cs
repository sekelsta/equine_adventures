
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.GameContent;

namespace EquineAdventures {
    public class AiTaskForage: AiTaskSeekFoodAndEat {
        protected HerbivoreHunger hungerBehavior;

        public IAnimalFoodSource TargetPoi {
            get => (IAnimalFoodSource)this.GetType().GetField("targetPoi").GetValue(this);
            protected set => this.GetType().GetField("targetPoi").SetValue(this, value);
        }

        public AiTaskForage(EntityAgent entity) : base(entity) { }

        public override void AfterInitialize()
        {
            hungerBehavior = entity.GetBehavior<HerbivoreHunger>();
        }

        public override bool ShouldExecute()
        {
            if (hungerBehavior == null) {
                return base.ShouldExecute();
            }
            if (!IsSearchTime()) {
                return false;
            }
            if (hungerBehavior.ShouldNurse()) {
                SeekMilk();
            }
            if (hungerBehavior.ShouldEat()) {
                if (base.ShouldExecute()) {
                    return true;
                }
                SeekForage();
                if (TargetPoi != null) {
                    return true;
                }
            }
            if (hungerBehavior.ShouldDrink()) {
                SeekWater();
            }
            return TargetPoi != null;
        }

        protected bool IsSearchTime() {
            long lastPOISearchTotalMs = (long)this.GetType().GetField("lastPOISearchTotalMs").GetValue(this);
            return lastPOISearchTotalMs + 15000L <= entity.World.ElapsedMilliseconds
                && cooldownUntilMs <= entity.World.ElapsedMilliseconds
                && cooldownUntilTotalHours <= entity.World.Calendar.TotalHours
                && EmotionStatesSatisifed();
        }

        protected void SeekMilk() {
            // TODO
        }

        protected void SeekForage() {
            // TODO: Find a nice chunk of grass and nom nom
        }

        protected void SeekWater() {
            // TODO
        }
    }
}
