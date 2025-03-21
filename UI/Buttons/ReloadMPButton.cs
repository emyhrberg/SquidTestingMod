using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ReLogic.Content;
using SquidTestingMod.Helpers;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace SquidTestingMod.UI.Buttons
{
    public class ReloadMPButton : BaseButton
    {
        // Set custom animation dimensions
        private float _scale = 1.25f;
        protected override float Scale => _scale;
        protected override int FrameCount => 5;
        protected override int FrameSpeed => 12;
        protected override int FrameWidth => 65;
        protected override int FrameHeight => 65;

        // Constructor
        public ReloadMPButton(Asset<Texture2D> spritesheet, string buttonText, string hoverText, float textSize) : base(spritesheet, buttonText, hoverText, textSize)
        {
            // deactived by default since the SP button is active
            if (Main.netMode == NetmodeID.SinglePlayer)
            {
                Active = false;
                ButtonText.Active = false;
            }
            else if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                Active = true;
                ButtonText.Active = true;
            }
        }

        public async override void LeftClick(UIMouseEvent evt)
        {
            // If alt+click, toggle the mode and return
            bool altClick = Main.keyState.IsKeyDown(Keys.LeftAlt);
            if (altClick)
            {
                Active = false;
                ButtonText.Active = false;

                // set MP active
                MainSystem sys = ModContent.GetInstance<MainSystem>();
                foreach (var btn in sys?.mainState?.AllButtons)
                {
                    if (btn is ReloadSPButton spBtn)
                    {
                        spBtn.Active = true;
                        spBtn.ButtonText.Active = true;
                    }
                }
                return;
            }

            ReloadUtilities.PrepareClient(ClientModes.MPMain);

            // we must be in multiplayer for this to have an effect
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                await ReloadUtilities.ExitAndKillServer();

                int clientCount = Main.player.Where((p) => p.active).Count() - 1;
                List<NamedPipeServerStream> clients = new List<NamedPipeServerStream>();
                List<Task<string?>> clientResponses = new List<Task<string?>>();

                Log.Info($"Waiting for {clientCount} clients...");

                // Wait for conecting all clients
                for (int i = 0; i < clientCount; i++)
                {
                    var pipeServer = new NamedPipeServerStream(ReloadUtilities.pipeName, PipeDirection.InOut, clientCount);
                    await pipeServer.WaitForConnectionAsync();
                    clients.Add(pipeServer);
                    Log.Info($"Client {i + 1} connected!");
                }

                foreach (var client in clients)
                {
                    clientResponses.Add(ReloadUtilities.ReadPipeMessage(client));
                }

                // ������, ���� �� �볺��� �������� ��� �����������
                await Task.WhenAll(clientResponses);

                ReloadUtilities.BuildAndReloadMod();

                Log.Info("Mod Builded");
                // After building - reload all other clients
                for (int i = 0; i < clients.Count; i++)
                {
                    using StreamWriter writer = new StreamWriter(clients[i]) { AutoFlush = true };
                    await writer.WriteLineAsync($"yes, go reload yourself now");
                }

                foreach (var client in clients)
                {
                    client.Close();
                    client.Dispose();
                }
            }
            else if (Main.netMode == NetmodeID.SinglePlayer)
            {
                await ReloadUtilities.ExitWorldOrServer();
                ReloadUtilities.BuildAndReloadMod();
            }
        }

        public override void RightClick(UIMouseEvent evt)
        {
            // If right click, toggle the mode and return
            Active = false;
            ButtonText.Active = false;

            // set MP active
            MainSystem sys = ModContent.GetInstance<MainSystem>();
            foreach (var btn in sys?.mainState?.AllButtons)
            {
                if (btn is ReloadSPButton spBtn)
                {
                    spBtn.Active = true;
                    spBtn.ButtonText.Active = true;
                }
            }
            return;
        }
    }
}