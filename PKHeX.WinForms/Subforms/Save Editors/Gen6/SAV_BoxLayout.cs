﻿using System;
using System.Linq;
using System.Windows.Forms;
using PKHeX.Core;

namespace PKHeX.WinForms
{
    public partial class SAV_BoxLayout : Form
    {
        private readonly SaveFile Origin;
        private readonly SaveFile SAV;
        public SAV_BoxLayout(SaveFile sav, int box)
        {
            SAV = (Origin = sav).Clone();
            InitializeComponent();
            WinFormsUtil.TranslateInterface(this, Main.CurrentLanguage);
            editing = true;
            
            // Repopulate Wallpaper names
            if (!LoadWallpaperNames())
                WinFormsUtil.Error("Box layout is not supported for this game.", "Please close the window.");
            LoadBoxNames();
            LoadFlags();
            LoadUnlockedCount();

            LB_BoxSelect.SelectedIndex = box;
        }

        private bool LoadWallpaperNames()
        {
            CB_BG.Items.Clear();
            switch (SAV.Generation)
            {
                case 3:
                    if (SAV.GameCube)
                        goto default;
                    CB_BG.Items.AddRange(GameInfo.Strings.wallpapernames.Take(16).ToArray());
                    return true;
                case 4:
                case 5:
                case 6:
                    CB_BG.Items.AddRange(GameInfo.Strings.wallpapernames);
                    return true;
                case 7:
                    CB_BG.Items.AddRange(GameInfo.Strings.wallpapernames.Take(16).ToArray());
                    return true;
                default:
                    return false;
            }
        }
        private void LoadBoxNames()
        {
            LB_BoxSelect.Items.Clear();
            for (int i = 0; i < SAV.BoxCount; i++)
                LB_BoxSelect.Items.Add(SAV.GetBoxName(i));
        }
        private void LoadUnlockedCount()
        {
            if (SAV.BoxesUnlocked <= 0)
            {
                FLP_Unlocked.Visible = L_Unlocked.Visible = CB_Unlocked.Visible = false;
                return;
            }
            CB_Unlocked.Items.Clear();
            int max = SAV.BoxCount;
            if (SAV.Generation == 6)
                max -= 1; // cover legendary captured unlocks final box, not governed by BoxesUnlocked
            for (int i = 0; i <= max; i++)
                CB_Unlocked.Items.Add(i);
            CB_Unlocked.SelectedIndex = Math.Min(max, SAV.BoxesUnlocked);
        }
        private void LoadFlags()
        {
            byte[] flags = SAV.BoxFlags;
            if (flags == null)
            {
                FLP_Flags.Visible = false;
                return;
            }

            flagArr = new NumericUpDown[flags.Length];
            for (int i = 0; i < flags.Length; i++)
            {
                flagArr[i] = new NumericUpDown
                {
                    Minimum = 0,
                    Maximum = 255,
                    Width = CB_Unlocked.Width - 5,
                    Hexadecimal = true,
                    Value = flags[i]
                };
                FLP_Flags.Controls.Add(flagArr[i]);
            }
        }

        private NumericUpDown[] flagArr = new NumericUpDown[0];
        private bool editing;
        private bool renamingBox;
        private void ChangeBox(object sender, EventArgs e)
        {
            if (renamingBox)
                return;
            editing = true;
            
            CB_BG.SelectedIndex = Math.Min(CB_BG.Items.Count - 1, SAV.GetBoxWallpaper(LB_BoxSelect.SelectedIndex));
            TB_BoxName.Text = SAV.GetBoxName(LB_BoxSelect.SelectedIndex);

            editing = false; 
        }
        private void ChangeBoxDetails(object sender, EventArgs e)
        {
            if (editing)
                return;

            renamingBox = true;
            SAV.SetBoxName(LB_BoxSelect.SelectedIndex, TB_BoxName.Text);
            LB_BoxSelect.Items[LB_BoxSelect.SelectedIndex] = TB_BoxName.Text;
            renamingBox = false;
        }
        private void B_Cancel_Click(object sender, EventArgs e)
        {
            Close();
        }
        private void B_Save_Click(object sender, EventArgs e)
        {
            if (flagArr.Length > 0)
                SAV.BoxFlags = flagArr.Select(i => (byte) i.Value).ToArray();
            if (CB_Unlocked.Visible)
                SAV.BoxesUnlocked = CB_Unlocked.SelectedIndex;

            Origin.SetData(SAV.Data, 0);
            Close();
        }

        private void ChangeBoxBackground(object sender, EventArgs e)
        {
            if (!editing)
                SAV.SetBoxWallpaper(LB_BoxSelect.SelectedIndex, CB_BG.SelectedIndex);

            PAN_BG.BackgroundImage = SAV.WallpaperImage(CB_BG.SelectedIndex);
        }
        
        private bool MoveItem(int direction)
        {
            // Checking selected item
            if (LB_BoxSelect.SelectedItem == null || LB_BoxSelect.SelectedIndex < 0)
                return false; // No selected item - nothing to do

            // Calculate new index using move direction
            int newIndex = LB_BoxSelect.SelectedIndex + direction;

            // Checking bounds of the range
            if (newIndex < 0 || newIndex >= LB_BoxSelect.Items.Count)
                return false; // Index out of range - nothing to do

            object selected = LB_BoxSelect.SelectedItem;

            // Removing removable element
            LB_BoxSelect.Items.Remove(selected);
            // Insert it in new position
            LB_BoxSelect.Items.Insert(newIndex, selected);
            // Restore selection
            LB_BoxSelect.SetSelected(newIndex, true);
            editing = renamingBox = false;

            return true;
        }

        private void MoveBox(object sender, EventArgs e)
        {
            int index = LB_BoxSelect.SelectedIndex;
            int dir = sender == B_Up ? -1 : +1;
            editing = renamingBox = true;
            if (!MoveItem(dir))
            {
                System.Media.SystemSounds.Asterisk.Play();
            }
            else if (!SAV.SwapBox(index, index + dir)) // valid but locked
            {
                MoveItem(-dir); // undo
                WinFormsUtil.Alert("Locked/Team slots prevent movement of box(es).");
            }
            else
                ChangeBox(null, null);
            editing = renamingBox = false;
        }
    }
}