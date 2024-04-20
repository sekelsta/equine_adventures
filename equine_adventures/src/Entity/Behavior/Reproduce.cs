using Vintagestory.API.Common.Entities;

namespace EquineAdventures {
    public class Reproduce : EntityBehavior {

        public Reproduce(Entity entity)
          : base(entity)
        {
        }

        // TODO?
        // Go in and out of heat
        // If in heat, not malnourished, and near a male, reproduce
        // Store info about the pregnancy depending on the male

        public override string PropertyName() => "reproduce";
    }
}
