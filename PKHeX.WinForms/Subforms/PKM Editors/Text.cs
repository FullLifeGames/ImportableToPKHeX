﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using PKHeX.Core;

namespace PKHeX.WinForms
{
    public partial class TrashEditor : Form
    {
        private readonly SaveFile SAV;
        public TrashEditor(TextBoxBase TB_NN, byte[] raw, SaveFile sav)
        {
            SAV = sav;
            InitializeComponent();
            bigendian = new[] { GameVersion.COLO, GameVersion.XD, GameVersion.BATREV, }.Contains(SAV.Version);
            WinFormsUtil.TranslateInterface(this, Main.CurrentLanguage);

            FinalString = TB_NN.Text;
            Raw = FinalBytes = raw;

            editing = true;
            if (raw != null)
                AddTrashEditing(raw.Length);

            AddCharEditing();
            TB_Text.MaxLength = TB_NN.MaxLength;
            TB_Text.Text = TB_NN.Text;
            TB_Text.Font = pkxFont;

            if (FLP_Characters.Controls.Count == 0)
            {
                FLP_Characters.Visible = false;
                FLP_Hex.Height *= 2;
            }
            else if (FLP_Hex.Controls.Count == 0)
            {
                FLP_Characters.Location = FLP_Hex.Location;
                FLP_Characters.Height *= 2;
            }

            editing = false;
            CenterToParent();
        }
        
        private readonly List<NumericUpDown> Bytes = new List<NumericUpDown>();
        private readonly Font pkxFont = FontUtil.GetPKXFont(12F);
        public string FinalString;
        public byte[] FinalBytes { get; private set; }
        private readonly byte[] Raw;
        private bool editing;
        private readonly bool bigendian;
        private void B_Cancel_Click(object sender, EventArgs e) => Close();
        private void B_Save_Click(object sender, EventArgs e)
        {
            FinalString = TB_Text.Text;
            if (FinalBytes != null)
                FinalBytes = Raw;
            Close();
        }

        private void AddCharEditing()
        {
            ushort[] chars = GetChars(SAV.Generation);
            if (chars.Length == 0)
                return;

            FLP_Characters.Visible = true;
            foreach (ushort c in chars)
            {
                var l = GetLabel((char)c+"");
                l.Font = pkxFont;
                l.AutoSize = false;
                l.Size = new Size(20, 20);
                l.Click += (s, e) => { if (TB_Text.Text.Length < TB_Text.MaxLength) TB_Text.AppendText(l.Text); };
                FLP_Characters.Controls.Add(l);
            }
        }
        private void AddTrashEditing(int count)
        {
            FLP_Hex.Visible = true;
            GB_Trash.Visible = true;
            NUD_Generation.Value = SAV.Generation;
            Font courier = new Font("Courier New", 8);
            for (int i = 0; i < count; i++)
            {
                var l = GetLabel($"${i:X2}");
                l.Font = courier;
                var n = GetNUD(hex: true, min: 0, max: 255);
                n.Click += (s, e) =>
                {
                    switch (ModifierKeys)
                    {
                        case Keys.Shift: n.Value = n.Maximum; break;
                        case Keys.Alt: n.Value = n.Minimum; break;
                    }
                };
                n.Value = Raw[i];
                n.ValueChanged += UpdateNUD;
                

                FLP_Hex.Controls.Add(l);
                FLP_Hex.Controls.Add(n);
                Bytes.Add(n);
            }
            TB_Text.TextChanged += UpdateString;

            CB_Species.DisplayMember = "Text";
            CB_Species.ValueMember = "Value";
            CB_Species.DataSource = new BindingSource(GameInfo.SpeciesDataSource, null);

            CB_Language.DisplayMember = "Text";
            CB_Language.ValueMember = "Value";
            var languages = Util.GetUnsortedCBList("languages");
            if (SAV.Generation < 7)
                languages = languages.Where(l => l.Value <= 8).ToList(); // Korean
            CB_Language.DataSource = languages;
        }

        private void UpdateNUD(object sender, EventArgs e)
        {
            if (editing)
                return;
            editing = true;
            // build bytes
            var nud = sender as NumericUpDown;
            int index = Bytes.IndexOf(nud);
            Raw[index] = (byte)nud.Value;

            string str = GetString();
            TB_Text.Text = str;
            editing = false;
        }
        private void UpdateString(object sender, EventArgs e)
        {
            if (editing)
                return;
            editing = true;
            // build bytes
            byte[] data = SetString(TB_Text.Text);
            Array.Copy(data, Raw, Math.Min(data.Length, Raw.Length));
            for (int i = 0; i < Raw.Length; i++)
                Bytes[i].Value = Raw[i];
            editing = false;
        }
        private void B_ApplyTrash_Click(object sender, EventArgs e)
        {
            string species = PKX.GetSpeciesNameGeneration(WinFormsUtil.GetIndex(CB_Species),
                WinFormsUtil.GetIndex(CB_Language), (int) NUD_Generation.Value);

            if (species == "") // no result
                species = CB_Species.Text;

            byte[] current = SetString(TB_Text.Text);
            byte[] data = SetString(species);
            if (data.Length <= current.Length)
            {
                WinFormsUtil.Alert("Trash byte layer is hidden by current text.",
                    $"Current Bytes: {current.Length}" + Environment.NewLine + $"Layer Bytes: {data.Length}");
                return;
            }
            if (data.Length > Bytes.Count)
            {
                WinFormsUtil.Alert("Trash byte layer is too long to apply.");
                return;
            }
            for (int i = current.Length; i < data.Length; i++)
                Bytes[i].Value = data[i];
        }
        private void B_ClearTrash_Click(object sender, EventArgs e)
        {
            byte[] current = SetString(TB_Text.Text);
            for (int i = current.Length; i < Bytes.Count; i++)
                Bytes[i].Value = 0;
        }
        private byte[] SetString(string text)
        {
            return SAV is SAV2 s && s.Korean
                ? StringConverter.SetString2KOR(text, Raw.Length)
                : StringConverter.SetString(text, SAV.Generation, SAV.Japanese, bigendian, Raw.Length, SAV.Language);
        }
        private string GetString()
        {
            return SAV is SAV2 s && s.Korean
                ? StringConverter.GetString2KOR(Raw, 0, Raw.Length)
                : StringConverter.GetString(Raw, SAV.Generation, SAV.Japanese, bigendian, Raw.Length);
        }

        // Helpers
        private static Label GetLabel(string str) => new Label {Text = str, AutoSize = true};
        private static NumericUpDown GetNUD(int min, int max, bool hex) => new NumericUpDown
        {
            Maximum = max,
            Minimum = min,
            Hexadecimal = hex,
            Width = 36,
            Padding = new Padding(0),
            Margin = new Padding(0),
        };

        private static ushort[] GetChars(int generation)
        {
            switch (generation)
            {
                case 6:
                case 7:
                    return chars67;
                default: return new ushort[0];
            }
        }
        private static readonly ushort[] chars67 =
        {
            0xE081, 0xE082, 0xE083, 0xE084, 0xE085, 0xE086, 0xE087, 0xE08D,
            0xE08E, 0xE08F, 0xE090, 0xE091, 0xE092, 0xE093, 0xE094, 0xE095,
            0xE096, 0xE097, 0xE098, 0xE099, 0xE09A, 0xE09B, 0xE09C, 0xE09D,
            0xE09E, 0xE09F, 0xE0A0, 0xE0A1, 0xE0A2, 0xE0A3, 0xE0A4, 0xE0A5,
        };
    }
}
