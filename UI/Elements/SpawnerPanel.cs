using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using SquidTestingMod.Common.Configs;
using SquidTestingMod.Helpers;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace SquidTestingMod.UI.Elements
{
    /// <summary>
    /// A base class for panels for item and NPC spawner.
    /// This can be extended to add scrollbar, searchbox, itemcounts, itemgrid, etc.
    /// This class makes it easier to focus on only filter functions for the item/npc panels
    /// </summary>
    public abstract class SpawnerPanel : DraggablePanel
    {
        // Panel size
        private const int W = 400 + padding; // Width of the panel
        private const int H = 400; // Height of the panel

        // UI Elements
        protected CustomGrid ItemsGrid;
        protected UIScrollbar Scrollbar;
        protected CustomTextBox SearchTextBox;
        protected UIText ItemCountText;
        public CustomTextBox GetCustomTextBox() => SearchTextBox;

        // Resize button
        ResizeButton resizeButton;

        // Store item slots
        protected List<CustomItemSlot> allItemSlots = [];

        public SpawnerPanel(string header) : base(header)
        {
            // Set the panel properties
            Width.Set(W, 0f);
            Height.Set(H, 0f);
            HAlign = 0.0f;
            VAlign = 1.0f;
            Top.Set(-70, 0f);
            Left.Set(20, 0f);

            // Create all content in the panel
            ItemCountText = new UIText("0 Items", textScale: 0.4f, true)
            {
                HAlign = 0.5f,
                VAlign = 1f,
            };

            SearchTextBox = new("Search")
            {
                Width = { Pixels = 200 },
                Height = { Pixels = 35 },
                HAlign = 1f,
                Top = { Pixels = -padding + 35 + padding },
                BackgroundColor = Color.White,
                BorderColor = Color.Black
            };
            SearchTextBox.OnTextChanged += FilterItems;

            Scrollbar = new UIScrollbar()
            {
                HAlign = 1f,
                Height = { Pixels = H - 130 - 10 }, // -10 because of scrollbarPadding=5 on top and bottom
                Width = { Pixels = 20 },
                Top = { Pixels = -padding + 30 + padding + 35 + padding + 5 },
                Left = { Pixels = 0f },
            };

            ItemsGrid = new CustomGrid()
            {
                Height = { Pixels = H - 130 },
                Width = { Percent = 1f, Pixels = -20 },
                ListPadding = 0f, // distance between items
                Top = { Pixels = -padding + 30 + padding + 35 + padding },
                Left = { Pixels = 0f },
                OverflowHidden = true, // hide items outside the grid
            };
            ItemsGrid.ManualSortMethod = (listUIElement) => { };
            ItemsGrid.SetScrollbar(Scrollbar);

            // Resize
            resizeButton = new(Ass.Resize);
            resizeButton.OnDragY += offsetY =>
            {
                // Log.Info($"[BEFORE] height: {Height.Pixels}, Top: {Top.Pixels}, V Align: {VAlign}");

                float oldHeight = Height.Pixels;
                float newHeight = oldHeight + offsetY;
                float maxHeight = 180f;

                // Clamp max height
                if (newHeight > H || newHeight < maxHeight)
                {
                    return;
                }

                // Clamp min height
                // if (newHeight < 200f)
                // newHeight = 200f;


                // Set new heights
                Height.Set(newHeight, 0f);
                ItemsGrid.Height.Set(newHeight - 140, 0f);
                Scrollbar.Height.Set(newHeight - 140 - 10, 0f);

                // Set new top offsets
                float topOffset = newHeight - oldHeight;
                Top.Pixels += topOffset;
                // ItemsGrid.Top.Pixels -= topOffset;
                // Scrollbar.Top.Pixels -= topOffset;

                Recalculate();

                // Log.Info($"[AFTER] height: {Height.Pixels}, Top: {Top.Pixels}, V Align: {VAlign}");
            };

            // Add all content in the panel
            Append(ItemCountText);
            Append(SearchTextBox);
            Append(Scrollbar);
            Append(ItemsGrid);
            Append(resizeButton);
        }

        public override void LeftMouseDown(UIMouseEvent evt)
        {
            if (Scrollbar != null && Scrollbar.ContainsPoint(evt.MousePosition))
                return;

            if (!Active)
                return;

            mouseDownPos = evt.MousePosition;
            base.LeftMouseDown(evt);
            dragging = true;
            IsDragging = false;
            dragOffset = evt.MousePosition - new Vector2(Left.Pixels, Top.Pixels);

            base.LeftMouseDown(evt);
        }

        protected virtual void FilterItems()
        {
            // Implement this in child classes
        }

        #region Update
        public override void Update(GameTime gameTime)
        {
            // if (IsMouseHovering)
            // {
            //     Main.LocalPlayer.mouseInterface = true;
            // }

            if (!Active)
                return;

            if (ContainsPoint(Main.MouseScreen))
            {
                Main.LocalPlayer.mouseInterface = true;
            }

            base.Update(gameTime);

            if (resizeButton.draggingResize)
            {
                dragging = false;  // Prevent panel dragging while resizing
                IsDragging = false;
                return;
            }

            if (Draggable & dragging)
            {
                float dragDistance = Vector2.Distance(new Vector2(Main.mouseX, Main.mouseY), mouseDownPos);
                if (dragDistance > DragThreshold)
                {
                    IsDragging = true;
                    Left.Set(Main.mouseX - dragOffset.X, 0f);
                    Top.Set(Main.mouseY - dragOffset.Y, 0f);
                    Recalculate();
                }
            }
            else
            {
                IsDragging = false;
            }
        }
        #endregion

        #region Dragging
        public override bool ContainsPoint(Vector2 point)
        {
            if (!Active)
                return false;

            return base.ContainsPoint(point);
        }

        public override void LeftMouseUp(UIMouseEvent evt)
        {
            base.LeftMouseUp(evt);
            dragging = false;
            IsDragging = false;
            Recalculate();
        }

        public override void LeftClick(UIMouseEvent evt)
        {
            if (IsDragging)
                return;
            base.LeftClick(evt);
        }
        #endregion
    }
}