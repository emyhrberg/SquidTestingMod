﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace SquidTestingMod.Helpers
{
    public enum ClientMode
    {
        FreshClient,
        SinglePlayer,
        MPMain,
        MPMinor,
    }

    internal static class ClientDataHandler
    {
        //Function that handles writing info that shoud survive modlreload
        static int _mode = (int)ClientMode.FreshClient;
        static int _playerId = 0;
        static int _worldId = 0;

        public static ClientMode Mode { get { return (ClientMode)_mode; } set { _mode = ((int)value); } }
        public static int PlayerId { get { return _playerId; } set { _playerId = value; } }
        public static int WorldId { get { return _worldId; } set { _worldId = value; } }
        public static void WriteData()
        {
            Log.Info("Writing Data");
            Main.instance.Window.Title = $"{_mode}, {_playerId}, {_worldId}";
        }

        public static void ReadData()
        {
            Log.Info("Reading Data");
            if (!string.IsNullOrEmpty(Main.instance.Window.Title))
            {
                string[] list = Main.instance.Window.Title.Split(", ");

                _mode = int.Parse(list[0]);
                _playerId = int.Parse(list[1]);
                _worldId = int.Parse(list[2]);
            }
            Main.instance.Window.Title = "SquidTestingMod: this is hard";

        }
    }
}
