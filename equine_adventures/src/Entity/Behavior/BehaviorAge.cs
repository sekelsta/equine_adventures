// Based on PetAI's BehaviorRaisable (MIT licensed), which is based on Vintage Story's BehaviorGrow
// Options for code reuse limited by the majority of the logic in BehaviorGrow hiding in a private non-virtual method

using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;

namespace EquineAdventures {
    public class BehaviorAge : EntityBehavior {
        ITreeAttribute growTree;
        JsonObject typeAttributes;
        long callbackId;

        public float HoursToGrow { get; protected set; }

        public AssetLocation[] AdultEntityCodes {
            get { return AssetLocation.toLocations(typeAttributes["adultEntityCodes"].AsArray<string>(new string[0])); }
        }

        internal double TimeSpawned {
            get { return growTree.GetDouble("timeSpawned"); }
            set { growTree.SetDouble("timeSpawned", value); }
        }


        public BehaviorAge(Entity entity) : base(entity) { }

        public override void Initialize(EntityProperties properties, JsonObject typeAttributes) {
            base.Initialize(properties, typeAttributes);
            this.typeAttributes = typeAttributes;

            if (typeAttributes.KeyExists("monthsToGrow")) {
                HoursToGrow = typeAttributes["monthsToGrow"].AsFloat() 
                    * entity.World.Calendar.DaysPerMonth * entity.World.Calendar.HoursPerDay;
            }
            else {
                HoursToGrow = typeAttributes["hoursToGrow"].AsFloat(96);
            }

            growTree = entity.WatchedAttributes.GetTreeAttribute("grow");

            if (growTree == null) {
                entity.WatchedAttributes.SetAttribute("grow", growTree = new TreeAttribute());
                TimeSpawned = entity.World.Calendar.TotalHours;
            }

            callbackId = entity.World.RegisterCallback(CheckGrowth, 3000);
        }


        protected virtual void CheckGrowth(float dt) {
            if (!entity.Alive) {
                return;
            }

            if (entity.World.Calendar.TotalHours >= TimeSpawned + HoursToGrow) {
                AssetLocation[] entityCodes = AdultEntityCodes;
                if (entityCodes.Length == 0) return;
                AssetLocation code = entityCodes[entity.World.Rand.Next(entityCodes.Length)];

                EntityProperties adultType = entity.World.GetEntityType(code);

                if (adultType == null) {
                    entity.World.Logger.Error("Misconfigured entity. Entity with code '{0}' is configured (via Grow behavior) to grow into '{1}', but no such entity type was registered.", entity.Code, code);
                    return;
                }

                Cuboidf collisionBox = adultType.SpawnCollisionBox;

                // Delay adult spawning if we're colliding
                if (entity.World.CollisionTester.IsColliding(entity.World.BlockAccessor, collisionBox, entity.ServerPos.XYZ, false)) {
                    callbackId = entity.World.RegisterCallback(CheckGrowth, 3000);
                    return;
                }

                Entity adult = entity.World.ClassRegistry.CreateEntity(adultType);

                adult.ServerPos.SetFrom(entity.ServerPos);
                adult.Pos.SetFrom(adult.ServerPos);

                entity.Die(EnumDespawnReason.Expire, null);
                entity.World.SpawnEntity(adult);
                CopyAttributesTo(adult);
            }
            else {
                callbackId = entity.World.RegisterCallback(CheckGrowth, 3000);
                double age = entity.World.Calendar.TotalHours - TimeSpawned;
                if (age >= 0.1 * HoursToGrow) {
                    // Used for drawing the critter larger over time if SizeGrowthFactor is nonzero
                    float newAge = (float)(age / HoursToGrow - 0.1);
                    if (newAge >= 1.01f * growTree.GetFloat("age")) {
                        growTree.SetFloat("age", newAge);
                        entity.WatchedAttributes.MarkPathDirty("grow");
                    }
                }
            }

            entity.World.FrameProfiler.Mark("entity-checkgrowth");
        }

        protected virtual void CopyAttributesTo(Entity adult) {
            bool keepTexture = adult.WatchedAttributes.HasAttribute("textureIndex") 
                && entity.Properties.Client?.FirstTexture?.Alternates != null 
                && adult.Properties.Client?.FirstTexture?.Alternates != null 
                && entity.Properties.Client.FirstTexture.Alternates.Length == adult.Properties.Client.FirstTexture.Alternates.Length;

            if (entity.WatchedAttributes.HasAttribute("domesticationstatus")) {
                adult.WatchedAttributes.SetAttribute("domesticationstatus", entity.WatchedAttributes.GetAttribute("domesticationstatus"));
            }

            adult.GetBehavior<EntityBehaviorNameTag>()?.SetName(entity.GetBehavior<EntityBehaviorNameTag>()?.DisplayName);

            if (keepTexture) {
                //Attempt to not change the texture during growing up
                adult.WatchedAttributes.SetInt("textureIndex", entity.WatchedAttributes.GetInt("textureIndex", 0));
            }/* TODO: Add this back if it can be done without creating a hard dependency on PetAI
            if (entity is EntityPet childPet && adult is EntityPet adultPet) {
                for (int i = 0; i < childPet.GearInventory.Count; i++) {
                    childPet.GearInventory[i].TryPutInto(entity.World, adultPet.GearInventory[i]);
                }
            }*/
        }

        public override void OnEntityDespawn(EntityDespawnData despawn) {
            entity.World.UnregisterCallback(callbackId);
        }


        public override string PropertyName() {
            return "age";
        }
    }
}
