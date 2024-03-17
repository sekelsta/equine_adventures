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

namespace EquineAdventures {
    public class PlayerBondable : EntityBehavior {
        public PlayerBondable(Entity entity)
          : base(entity)
        {
        }

        public static readonly int MAX_PLAYER_MEMORY = 8;

        public ITreeAttribute playerRelations
        {
            get
            {
                if (entity.WatchedAttributes.GetTreeAttribute("playerRelations") == null)
                {
                    entity.WatchedAttributes.SetAttribute("playerRelations", new TreeAttribute());
                }
                return entity.WatchedAttributes.GetTreeAttribute("playerRelations");
            }
            set
            {
                entity.WatchedAttributes.SetAttribute("playerRelations", value);
                entity.WatchedAttributes.MarkPathDirty("playerRelations");
            }
        }

        public override void Initialize(EntityProperties properties, JsonObject attributes) {
            // Read out initialization data from the Json
        }

        public ITreeAttribute RelationWith(IPlayer player) {
            return playerRelations.GetTreeAttribute(player.PlayerUID);
        }

        public void DropWeakestRelation() {
            Debug.Assert(playerRelations.Count > 0);
            string lowestId = null;
            float lowestFamiliarity = float.MaxValue;
            foreach (var pair in playerRelations) {
                float currentFamiliarity = (pair.Value as TreeAttribute).GetFloat("familiarity", 0);
                if (currentFamiliarity <= lowestFamiliarity) {
                    lowestId = pair.Key;
                    lowestFamiliarity = currentFamiliarity;
                }
            }
            playerRelations.RemoveAttribute(lowestId);
            entity.WatchedAttributes.MarkPathDirty("playerRelations");
        }

        public ITreeAttribute GetOrCreateRelation(IPlayer player) {
            string idString = player.PlayerUID;
            if (playerRelations.GetTreeAttribute(idString) == null) {
                if (playerRelations.Count >= MAX_PLAYER_MEMORY) {
                    DropWeakestRelation();
                }
                playerRelations.GetOrAddTreeAttribute(idString);
                entity.WatchedAttributes.MarkPathDirty("playerRelations");
            }
            return playerRelations.GetTreeAttribute(idString);
        }

        public float Familiarity(IPlayer player) {
            ITreeAttribute relation = RelationWith(player);
            if (relation == null) {
                return 0;
            }
            return relation.GetFloat("familiarity", 0);
        }

        public void Familiarity(IPlayer player, float val) {
            val = Math.Clamp(val, 0, 100);
            GetOrCreateRelation(player).SetFloat("familiarity", val);
            entity.WatchedAttributes.MarkPathDirty("playerRelations");
        }

        public void AddFamiliarity(IPlayer player, float val) {
            Familiarity(player, val + Familiarity(player));
        }

        public float Opinion(IPlayer player) {
            ITreeAttribute relation = RelationWith(player);
            if (relation == null) {
                return 0;
            }
            return relation.GetFloat("opinion", 0);
        }

        public void Opinion(IPlayer player, float val) {
            val = Math.Clamp(val, -100, 100);
            GetOrCreateRelation(player).SetFloat("opinion", val);
            entity.WatchedAttributes.MarkPathDirty("playerRelations");
        }

        public void AddOpinion(IPlayer player, float val) {
            Opinion(player, val + Opinion(player));
        }

        public override void OnEntityReceiveDamage(DamageSource damageSource, ref float damage) {
            base.OnEntityReceiveDamage(damageSource, ref damage);
            if (damage <= 0) {
                return;
            }
            Entity attacker = damageSource.GetCauseEntity();
            if (attacker is EntityPlayer) {
                AddOpinion((attacker as EntityPlayer).Player, -8 * damage);
            }
            // TODO: If being ridden, lower opinion of rider
        }

        public override void GetInfoText(StringBuilder infotext) {
            if (entity.World.Side == EnumAppSide.Client) {
                IPlayer player = (entity.World as IClientWorldAccessor).Player;
                int opinionInt = (int)Opinion(player);
                string opinionString = (opinionInt).ToString();
                if (opinionInt > 0) {
                    opinionString = "+" + opinionString;
                }
                if (player != null) {
                    string text = Lang.Get("equine_adventures:infotext-bond")
                        .Replace("{playerName}", player.PlayerName)
                        .Replace("{familiarity}", ((int)Familiarity(player)).ToString())
                        .Replace("{opinion}", opinionString);
                    infotext.AppendLine(text);
                }
            }
            // FOrmat: Bond (playername): familiarity: X%, opinion: +-Y, obedience: Z
            base.GetInfoText(infotext);
        }

        public override void OnInteract(EntityAgent byEntity, ItemSlot itemSlot, Vec3d hitPosition, EnumInteractMode mode, ref EnumHandling handled) {
            // TODO
        }

        public override string PropertyName() => "playerbondable";

        // TODO: Store bonding data, per player ID
        // Bonding data: familiarity, opinion, obedience
        // Familiarity: self-explanatory
        // Opinion: expected value of next interaction
        // Obedience: self-explanatory
        // TODO: Store bond amount with each player, plus the default. Raise and lower it as appropriate
        // Raise it every once in a while if the player is nearby
        // Raise it if fed a treat
        // Lower it over time, in some carefully managed way that never drops too low (maybe * 0.99f?)
        // OK to use player's EntityID as a key, because it will persist between sessions
        // Store data for max 8 players: 7 highest (by familiarity) + 1 newest
    }
}
