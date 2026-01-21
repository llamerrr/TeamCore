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

            // Filter for actual active players
            int activePlayerCount = 0;
            int deadPlayerCount = 0;

            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player p = Main.player[i];
                if (p.active)
                {
                    activePlayerCount++;
                    if (p.dead || p.ghost)
                    {
                        deadPlayerCount++;
                    }
                }
            }

            // Check for Game Over condition
            // Only trigger if not already triggered (Latching logic)
            if (!gameOverTriggered)
            {
                 // If there are players and ALL of them are dead
                 if (activePlayerCount > 0 && activePlayerCount == deadPlayerCount)
                 {
                     gameOverTriggered = true;
                     TriggerGameOverMessage();
                 }
            }
            
            // Once Game Over is triggered, we continually enforce the state
            if (gameOverTriggered)
            {
                EnforceHardcoreDeath();
            }
        }

        private void TriggerGameOverMessage()
        {
            string message = "Everyone has died! Team Hardcore Game Over.";
			string message1 = "===========================================";
            if (Main.netMode == NetmodeID.SinglePlayer)
            {
				Main.NewText(message1, Color.Red);
				Main.NewText(message, Color.Red);
            }
            else if (Main.netMode == NetmodeID.Server)
            {

				System.Console.WriteLine(message1);
                Terraria.Chat.ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral(message1), Color.Red);

                System.Console.WriteLine(message);
                Terraria.Chat.ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral(message), Color.Red);

				System.Console.WriteLine(message1);
                Terraria.Chat.ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral(message1), Color.Red);
            }
        }

        private void EnforceHardcoreDeath()
        {
            // On Client, we mostly care about enforcing our own local player State
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                Player p = Main.LocalPlayer;
                if (p.active)
                {
                    p.difficulty = 2; 
                    p.ghost = true;
                    p.dead = true;
                    p.respawnTimer = 3600; 
                }
                return; // Client doesn't need to iterate others or sync
            }

            // Server Logic
             for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player p = Main.player[i];
                if (p.active)
                {
                    // Force Hardcore difficulty (2)
                    p.difficulty = 2; 
                    p.ghost = true;
                    p.dead = true;
                    // Keep respawn timer high (wait time) prevents respawn
                    p.respawnTimer = 3600; 

                    // Periodically sync to ensure clients stay in ghost mode
                    if (Main.netMode == NetmodeID.Server && Main.time % 60 == 0)
                    {
                        NetMessage.SendData(MessageID.SyncPlayer, -1, -1, null, i);
                    }
                }
            }
        }
    }
}
