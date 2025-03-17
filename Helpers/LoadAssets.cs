using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using SquidTestingMod.Common.Configs;
using SquidTestingMod.UI;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.ModLoader.UI.Elements;

namespace SquidTestingMod.Helpers
{
    /// <summary>
    /// To add a new asset, simply add a new field like:
    /// public static Asset<Texture2D> MyAsset;
    /// </summary>
    public class LoadAssets : ModSystem
    {
        public override void Load()
        {
            var ignored = Ass.Initialized;
        }
    }

    public static class Ass
    {
        // Buttons
        public static Asset<Texture2D> CollapseDown;
        public static Asset<Texture2D> CollapseUp;
        public static Asset<Texture2D> CollapseLeft;
        public static Asset<Texture2D> CollapseRight;
        public static Asset<Texture2D> Button;
        public static Asset<Texture2D> ButtonOnOff;
        public static Asset<Texture2D> ButtonConfig;
        public static Asset<Texture2D> ButtonItems;
        public static Asset<Texture2D> ButtonNPC;
        public static Asset<Texture2D> ButtonNPC_XMAS;
        public static Asset<Texture2D> ButtonPlayer;
        public static Asset<Texture2D> ButtonDebug;
        public static Asset<Texture2D> ButtonWorld;
        public static Asset<Texture2D> ButtonReloadSP;
        public static Asset<Texture2D> ButtonReloadMP;
        public static Asset<Texture2D> ButtonMods;

        // Filter buttons
        public static Asset<Texture2D> FilterBG;
        public static Asset<Texture2D> FilterBGActive;
        public static Asset<Texture2D> FilterAll;
        public static Asset<Texture2D> FilterMelee;
        public static Asset<Texture2D> FilterRanged;
        public static Asset<Texture2D> FilterMagic;
        public static Asset<Texture2D> FilterSummon;
        public static Asset<Texture2D> FilterArmor;
        public static Asset<Texture2D> FilterVanity;
        public static Asset<Texture2D> FilterAccessories;
        public static Asset<Texture2D> FilterPotion;
        public static Asset<Texture2D> FilterPlaceables;
        public static Asset<Texture2D> FilterTown;
        public static Asset<Texture2D> FilterMob;
        public static Asset<Texture2D> SortID;
        public static Asset<Texture2D> SortValue;
        public static Asset<Texture2D> SortRarity;
        public static Asset<Texture2D> SortName;
        public static Asset<Texture2D> SortDamage;
        public static Asset<Texture2D> SortDefense;
        public static Asset<Texture2D> Resize;

        // Misc
        public static Asset<Texture2D> Arrow;
        public static List<Asset<Texture2D>> ModIcons = new();

        // Bool for checking if assets are loaded
        public static bool Initialized { get; set; }

        // Constructor
        static Ass()
        {
            foreach (FieldInfo field in typeof(Ass).GetFields())
            {
                if (field.FieldType == typeof(Asset<Texture2D>))
                {
                    field.SetValue(null, RequestAsset(field.Name));
                }
            }

            // Load Mod Icons
            // foreach (var modFolderName in GetModFiles())
            // {
            // Asset<Texture2D> icon = GetIconImage(modFolderName);
            // ModIcons.Add(icon);
            // }
        }

        private static Asset<Texture2D> GetIconImage(string modPath)
        {
            try
            {
                string iconPath = Path.Combine(modPath, "icon.png");
                // Check if icon exists
                if (!File.Exists(iconPath))
                {
                    Log.Info("2 No icon found for mod: " + modPath + ". Using default icon.");
                    Asset<Texture2D> defaultIconTemp = Main.Assets.Request<Texture2D>("Images/UI/DefaultResourcePackIcon", AssetRequestMode.ImmediateLoad);
                    return defaultIconTemp;
                }

                Asset<Texture2D> iconAsset = Main.Assets.Request<Texture2D>(iconPath, AssetRequestMode.ImmediateLoad);
                return iconAsset;
            }
            catch (Exception)
            {
                Log.Info("No icon found for mod: " + modPath + ". Using default icon.");
                Asset<Texture2D> defaultIconTemp = Main.Assets.Request<Texture2D>("Images/UI/DefaultResourcePackIcon", AssetRequestMode.ImmediateLoad);
                return defaultIconTemp;
            }
        }

        private static Asset<Texture2D> RequestAsset(string path)
        {
            // string modName = typeof(Assets).Namespace;
            string modName = "SquidTestingMod"; // Use this, in case above line doesnt work
            return ModContent.Request<Texture2D>($"{modName}/Assets/" + path, AssetRequestMode.AsyncLoad);
        }

        private static List<string> GetModFiles()
        {
            List<string> strings = [];

            // 1. Getting Assembly 
            Assembly tModLoaderAssembly = typeof(Main).Assembly;

            // 2. Gettig method for finding modSources paths
            Type modCompileType = tModLoaderAssembly.GetType("Terraria.ModLoader.Core.ModCompile");
            MethodInfo findModSourcesMethod = modCompileType.GetMethod("FindModSources", BindingFlags.NonPublic | BindingFlags.Static);
            string[] modSources = (string[])findModSourcesMethod.Invoke(null, null);

            for (int i = 0; i < modSources.Length; i++)
            {
                strings.Add(modSources[i]);
            }
            return strings;
        }
    }
}
