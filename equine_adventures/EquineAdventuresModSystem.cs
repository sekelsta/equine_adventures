using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;

namespace EquineAdventures
{
    public class EquineAdventuresModSystem : ModSystem
    {
        // Called on server and client
        public override void Start(ICoreAPI api)
        {
            api.Logger.Notification("Hello from equine adventures mod: " + Lang.Get("mymodid:creaturegroup-equine"));
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            api.Logger.Notification("Hello from equine adventures mod server side");
        }

        public override void StartClientSide(ICoreClientAPI api)
        {
            api.Logger.Notification("Hello from equine adventures mod client side");
        }
    }
}
