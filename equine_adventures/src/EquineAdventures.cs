using System;
﻿using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;

namespace EquineAdventures
{
    public class EquineAdventures : ModSystem
    {
        public static EquineConfig Config = null;

        // Called on server and client
        public override void Start(ICoreAPI api)
        {
            api.RegisterEntityBehaviorClass("playerbondable", typeof(PlayerBondable));
            api.RegisterEntityBehaviorClass("detailedharvestable", typeof(DetailedHarvestable));

            try {
                Config = api.LoadModConfig<EquineConfig>("equine_adventures_config.json");
            }
            catch (Exception e) {
                api.Logger.Error("Failed to load config file for Equine Adventures: " + e);
            }
            if (Config == null) {
                Config = new EquineConfig();
            }
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            // Server-side code goes here
        }

        public override void StartClientSide(ICoreClientAPI api)
        {
            // Client-side code goes here
        }
    }
}
