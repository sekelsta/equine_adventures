using System;
﻿using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace EquineAdventures
{
    public class EquineAdventures : ModSystem
    {
        // Called on server and client
        public override void Start(ICoreAPI api)
        {
            api.RegisterEntityBehaviorClass("playerbondable", typeof(PlayerBondable));
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
