using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.Localization;
using Microsoft.Xna.Framework;

namespace TeamCore
{
	public class TeamCore : Mod
	{
        // The main mod class. Logic is handled in the ModSystem below.
	}

    public class TeamHardcoreSystem : ModSystem
    {
        private bool gameOverTriggered = false;

        public override void PostUpdateEverything()
        {
            // We only care about checking this logic on the Server (for MP) or SinglePlayer
            // Clients just receive the result (disconnection or world exit)
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            // Filter for actual active players (ignore empty slots)
            int activePlayerCount = 0;
            int deadPlayerCount = 0;

            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player p = Main.player[i];
                if (p.active)
                {
                    activePlayerCount++;
                    // A player is considered "dead" for this mechanic if they are dead or a ghost
                    if (p.dead || p.ghost)
                    {
                        deadPlayerCount++;
                    }
                }
            }

            // If there are players and ALL of them are dead
            if (activePlayerCount > 0 && activePlayerCount == deadPlayerCount)
            {
                if (!gameOverTriggered)
                {
                    gameOverTriggered = true;
                    TriggerGameOver();
                }
            }
            else
            {
                // Reset flag if someone respawns (though usually Game Over stops this)
                // In a true hardcore, the game would end, but for safety/loops:
                if (activePlayerCount > deadPlayerCount)
                    gameOverTriggered = false;
            }
        }

        private void TriggerGameOver()
        {
            string message = "===========================================";
            string message1 = "Everyone has died! Team Hardcore Game Over.";
            string message2 = "===========================================";

            if (Main.netMode == NetmodeID.SinglePlayer)
            {
                Main.NewText(message, Color.Red);
                Main.NewText(message1, Color.Red);
                Main.NewText(message2, Color.Red);
            }
            else if (Main.netMode == NetmodeID.Server)
            {
                System.Console.WriteLine(message);
                Terraria.Chat.ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral(message), Color.Red);
            }

            // Apply Hardcore Ghost state to all active players
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player p = Main.player[i];
                if (p.active)
                {
                    // Force Hardcore difficulty (2)
                    p.difficulty = 2; // 0 = Softcore, 1 = Mediumcore, 2 = Hardcore, 3 = Journey
                    p.ghost = true;
                    p.dead = true;

                    if (Main.netMode == NetmodeID.Server)
                    {
                        // Sync player data (includes difficulty and ghost state)
                        NetMessage.SendData(MessageID.SyncPlayer, -1, -1, null, i);
                    }
                }
            }
        }
    }
}
