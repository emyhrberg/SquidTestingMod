using Terraria;
using Terraria.ModLoader;

namespace ModHelper.Debug
{
    public class NoSpawns : GlobalNPC
    {
        public override void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns)
        {
#if DEBUG
            // Prevent division by zero
            spawnRate = 9999999; // Effectively stops enemy spawns
            maxSpawns = 0;
#endif

            // Call the base method to keep the original behavior when SpawnRateEnabled is false
            base.EditSpawnRate(player, ref spawnRate, ref maxSpawns);
        }
    }
}
