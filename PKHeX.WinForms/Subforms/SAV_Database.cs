﻿#define LOADALL
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using PKHeX.Core;
using PKHeX.WinForms.Controls;

namespace PKHeX.WinForms
{
    public partial class SAV_Database : Form
    {
        private readonly SaveFile SAV;
        private readonly SAVEditor BoxView;
        private readonly PKMEditor PKME_Tabs;
        public SAV_Database(PKMEditor f1, SAVEditor saveditor)
        {
            SAV = saveditor.SAV;
            BoxView = saveditor;
            PKME_Tabs = f1;
            InitializeComponent();

            // Preset Filters to only show PKM available for loaded save
            CB_FormatComparator.SelectedIndex = 3; // <=

            PKXBOXES = new[]
            {
                bpkx1, bpkx2, bpkx3, bpkx4, bpkx5, bpkx6,
                bpkx7, bpkx8, bpkx9, bpkx10,bpkx11,bpkx12,
                bpkx13,bpkx14,bpkx15,bpkx16,bpkx17,bpkx18,
                bpkx19,bpkx20,bpkx21,bpkx22,bpkx23,bpkx24,
                bpkx25,bpkx26,bpkx27,bpkx28,bpkx29,bpkx30,

                bpkx31,bpkx32,bpkx33,bpkx34,bpkx35,bpkx36,
                bpkx37,bpkx38,bpkx39,bpkx40,bpkx41,bpkx42,
                bpkx43,bpkx44,bpkx45,bpkx46,bpkx47,bpkx48,
                bpkx49,bpkx50,bpkx51,bpkx52,bpkx53,bpkx54,
                bpkx55,bpkx56,bpkx57,bpkx58,bpkx59,bpkx60,
                bpkx61,bpkx62,bpkx63,bpkx64,bpkx65,bpkx66,
            };

            // Enable Scrolling when hovered over
            foreach (var slot in PKXBOXES)
            {
                // Enable Click
                slot.MouseClick += (sender, e) =>
                {
                    if (ModifierKeys == Keys.Control)
                        ClickView(sender, e);
                    else if (ModifierKeys == Keys.Alt)
                        ClickDelete(sender, e);
                    else if (ModifierKeys == Keys.Shift)
                        ClickSet(sender, e);
                };
            }
            
            Counter = L_Count.Text;
            Viewed = L_Viewed.Text;
            L_Viewed.Text = ""; // invis for now
            var hover = new ToolTip();
            L_Viewed.MouseEnter += (sender, e) => hover.SetToolTip(L_Viewed, L_Viewed.Text);
            PopulateComboBoxes();

            ContextMenuStrip mnu = new ContextMenuStrip();
            ToolStripMenuItem mnuView = new ToolStripMenuItem("View");
            ToolStripMenuItem mnuDelete = new ToolStripMenuItem("Delete");

            // Assign event handlers
            mnuView.Click += ClickView;
            mnuDelete.Click += ClickDelete;

            // Add to main context menu
            mnu.Items.AddRange(new ToolStripItem[] { mnuView, mnuDelete });

            // Assign to datagridview
            foreach (PictureBox p in PKXBOXES)
                p.ContextMenuStrip = mnu;

            // Load Data
            B_Search.Enabled = false;
            L_Count.Text = "Loading...";
            new Task(LoadDatabase).Start();

            Menu_SearchSettings.DropDown.Closing += (sender, e) =>
            {
                if (e.CloseReason == ToolStripDropDownCloseReason.ItemClicked)
                    e.Cancel = true;
            };
            CenterToParent();
        }

        private readonly PictureBox[] PKXBOXES;
        private readonly string DatabasePath = Main.DatabasePath;
        private List<PKM> Results;
        private List<PKM> RawDB;
        private int slotSelected = -1; // = null;
        private Image slotColor;
        private const int RES_MAX = 66;
        private const int RES_MIN = 6;
        private readonly string Counter;
        private readonly string Viewed;
        private const int MAXFORMAT = 7;
        private readonly string EXTERNAL_SAV = new DirectoryInfo(Main.BackupPath).Name + Path.DirectorySeparatorChar;
        private static string Hash(PKM pk)
        {
            switch (pk.Format)
            {
                case 1: return $"{pk.Species:000}{((PK1) pk).DV16:X4}";
                case 2: return $"{pk.Species:000}{((PK2) pk).DV16:X4}";
                default: return $"{pk.Species:000}{pk.PID:X8}{string.Join(" ", pk.IVs)}{pk.AltForm:00}";
            }
        }

        // Important Events
        private void ClickView(object sender, EventArgs e)
        {
            sender = ((sender as ToolStripItem)?.Owner as ContextMenuStrip)?.SourceControl ?? sender as PictureBox;
            int index = Array.IndexOf(PKXBOXES, sender);
            if (index >= RES_MAX)
            {
                System.Media.SystemSounds.Exclamation.Play();
                return;
            }
            index += SCR_Box.Value * RES_MIN;
            if (index >= Results.Count)
            {
                System.Media.SystemSounds.Exclamation.Play();
                return;
            }
            
            PKME_Tabs.PopulateFields(Results[index], false);
            slotSelected = index;
            slotColor = Properties.Resources.slotView;
            FillPKXBoxes(SCR_Box.Value);
            L_Viewed.Text = string.Format(Viewed, Results[index].Identifier);
        }
        private void ClickDelete(object sender, EventArgs e)
        {
            sender = ((sender as ToolStripItem)?.Owner as ContextMenuStrip)?.SourceControl ?? sender as PictureBox;
            int index = Array.IndexOf(PKXBOXES, sender);
            if (index >= RES_MAX)
            {
                System.Media.SystemSounds.Exclamation.Play();
                return;
            }
            index += SCR_Box.Value * RES_MIN;
            if (index >= Results.Count)
            {
                System.Media.SystemSounds.Exclamation.Play();
                return;
            }

            var pk = Results[index];
            string path = pk.Identifier;

#if LOADALL
            if (path.StartsWith(EXTERNAL_SAV))
            {
                WinFormsUtil.Alert("Can't delete from a backup save.");
                return;
            }
#endif
            if (path.Contains(Path.DirectorySeparatorChar))
            {
                // Data from Database: Delete file from disk
                if (File.Exists(path))
                    File.Delete(path);
            }
            else
            {
                // Data from Box: Delete from save file
                int box = pk.Box-1;
                int slot = pk.Slot-1;
                int offset = SAV.GetBoxOffset(box) + slot*SAV.SIZE_STORED;
                PKM pkSAV = SAV.GetStoredSlot(offset);

                if (!pkSAV.Data.SequenceEqual(pk.Data)) // data still exists in SAV, unmodified
                {
                    WinFormsUtil.Error("Database slot data does not match save data!", "Don't move Pokémon after initializing the Database, please re-open the Database viewer.");
                    return;
                }
                var change = new SlotChange {Box = box, Offset = offset, Slot = slot};
                BoxView.M.SetPKM(BoxView.SAV.BlankPKM, change, true, Properties.Resources.slotDel);
            }
            // Remove from database.
            RawDB.Remove(pk);
            Results.Remove(pk);
            // Refresh database view.
            L_Count.Text = string.Format(Counter, Results.Count);
            slotSelected = -1;
            FillPKXBoxes(SCR_Box.Value);
            System.Media.SystemSounds.Asterisk.Play();
        }
        private void ClickSet(object sender, EventArgs e)
        {
            // Don't care what slot was clicked, just add it to the database
            if (!PKME_Tabs.VerifiedPKM())
                return;

            PKM pk = PKME_Tabs.PreparePKM();
            Directory.CreateDirectory(DatabasePath);

            string path = Path.Combine(DatabasePath, Util.CleanFileName(pk.FileName));

            if (RawDB.Any(p => p.Identifier == path))
            {
                WinFormsUtil.Alert("File already exists in database!");
                return;
            }

            File.WriteAllBytes(path, pk.Data.Take(pk.SIZE_STORED).ToArray());
            pk.Identifier = path;

            int pre = RawDB.Count;
            RawDB.Add(pk);
            RawDB = new List<PKM>(RawDB);
            int post = RawDB.Count;
            if (pre == post)
            { WinFormsUtil.Alert("Pokémon already exists in database."); return; }
            Results.Add(pk);

            // Refresh database view.
            L_Count.Text = string.Format(Counter, Results.Count);
            slotSelected = Results.Count - 1;
            slotColor = Properties.Resources.slotSet;
            if ((SCR_Box.Maximum+1)*6 < Results.Count)
                SCR_Box.Maximum += 1;
            SCR_Box.Value = Math.Max(0, SCR_Box.Maximum - PKXBOXES.Length/6 + 1);
            FillPKXBoxes(SCR_Box.Value);
            WinFormsUtil.Alert("Added Pokémon from tabs to database.");
        }
        private void PopulateComboBoxes()
        {
            // Set the Text
            CB_HeldItem.DisplayMember =
            CB_Species.DisplayMember =
            CB_Ability.DisplayMember =
            CB_Nature.DisplayMember =
            CB_GameOrigin.DisplayMember =
            CB_HPType.DisplayMember = "Text";

            // Set the Value
            CB_HeldItem.ValueMember =
            CB_Species.ValueMember =
            CB_Ability.ValueMember =
            CB_Nature.ValueMember =
            CB_GameOrigin.ValueMember =
            CB_HPType.ValueMember = "Value";

            var Any = new ComboItem {Text = "Any", Value = -1};

            var DS_Species = new List<ComboItem>(GameInfo.SpeciesDataSource);
            DS_Species.RemoveAt(0); DS_Species.Insert(0, Any); CB_Species.DataSource = DS_Species;

            var DS_Item = new List<ComboItem>(GameInfo.ItemDataSource);
            DS_Item.Insert(0, Any); CB_HeldItem.DataSource = DS_Item;

            var DS_Nature = new List<ComboItem>(GameInfo.NatureDataSource);
            DS_Nature.Insert(0, Any); CB_Nature.DataSource = DS_Nature;

            var DS_Ability = new List<ComboItem>(GameInfo.AbilityDataSource);
            DS_Ability.Insert(0, Any); CB_Ability.DataSource = DS_Ability;

            var DS_Version = new List<ComboItem>(GameInfo.VersionDataSource);
            DS_Version.Insert(0, Any); CB_GameOrigin.DataSource = DS_Version;
            
            string[] hptypes = new string[GameInfo.Strings.types.Length - 2]; Array.Copy(GameInfo.Strings.types, 1, hptypes, 0, hptypes.Length);
            var DS_Type = Util.GetCBList(hptypes, null); 
            DS_Type.Insert(0, Any); CB_HPType.DataSource = DS_Type;

            // Set the Move ComboBoxes too..
            var DS_Move = new List<ComboItem>(GameInfo.MoveDataSource);
            DS_Move.RemoveAt(0); DS_Move.Insert(0, Any);
            {
                foreach (ComboBox cb in new[] { CB_Move1, CB_Move2, CB_Move3, CB_Move4 })
                {
                    cb.DisplayMember = "Text"; cb.ValueMember = "Value";
                    cb.DataSource = new BindingSource(DS_Move, null);
                }
            }

            // Trigger a Reset
            ResetFilters(null, null);
        }
        private void ResetFilters(object sender, EventArgs e)
        {
            CHK_Shiny.Checked = CHK_IsEgg.Checked = true;
            CHK_Shiny.CheckState = CHK_IsEgg.CheckState = CheckState.Indeterminate;
            MT_ESV.Text = "";
            CB_HeldItem.SelectedIndex = 0;
            CB_Species.SelectedIndex = 0;
            CB_Ability.SelectedIndex = 0;
            CB_Nature.SelectedIndex = 0;
            CB_HPType.SelectedIndex = 0;

            CB_Level.SelectedIndex = 0;
            TB_Level.Text = "";
            CB_EVTrain.SelectedIndex = 0;
            CB_IV.SelectedIndex = 0;

            CB_Move1.SelectedIndex = CB_Move2.SelectedIndex = CB_Move3.SelectedIndex = CB_Move4.SelectedIndex = 0;

            CB_GameOrigin.SelectedIndex = 0;
            CB_Generation.SelectedIndex = 0;

            MT_ESV.Visible = L_ESV.Visible = false;
            RTB_Instructions.Clear();

            if (sender != null)
                System.Media.SystemSounds.Asterisk.Play();
        }
        private void GenerateDBReport(object sender, EventArgs e)
        {
            if (WinFormsUtil.Prompt(MessageBoxButtons.YesNo, "Generate a Report on all data?", "This may take a while...")
                != DialogResult.Yes)
                return;

            ReportGrid reportGrid = new ReportGrid();
            reportGrid.Show();
            reportGrid.PopulateData(Results.ToArray());
        }

        private void LoadDatabase()
        {
            var dbTemp = new ConcurrentBag<PKM>();
            var files = Directory.EnumerateFiles(DatabasePath, "*", SearchOption.AllDirectories);
            Parallel.ForEach(files, file =>
            {
                FileInfo fi = new FileInfo(file);
                if (!fi.Extension.Contains(".pk") || !PKX.IsPKM(fi.Length)) return;
                var pk = PKMConverter.GetPKMfromBytes(File.ReadAllBytes(file), file, prefer: (fi.Extension.Last() - '0') & 0xF);
                if (pk != null)
                    dbTemp.Add(pk);
            });

#if LOADALL
            if (SaveUtil.GetSavesFromFolder(Main.BackupPath, false, out IEnumerable<string> result))
            {
                Parallel.ForEach(result, file =>
                {
                    var sav = SaveUtil.GetVariantSAV(File.ReadAllBytes(file));
                    var path = EXTERNAL_SAV + new FileInfo(file).Name;
                    if (sav.HasBox)
                        foreach (var pk in sav.BoxData)
                            addPKM(pk);

                    void addPKM(PKM pk)
                    {
                        pk.Identifier = Path.Combine(path, pk.Identifier);
                        dbTemp.Add(pk);
                    }
                });
            }
#endif

            // Prepare Database
            RawDB = new List<PKM>(dbTemp.OrderBy(pk => pk.Identifier)
                .Concat(SAV.BoxData.Where(pk => pk.Species != 0)) // Fetch from save file
                .Where(pk => pk.ChecksumValid && pk.Species != 0 && pk.Sanity == 0)
                .Distinct());
            try
            {
                BeginInvoke(new MethodInvoker(() => SetResults(RawDB)));
            }
            catch { /* Window Closed? */ }
        }

        // IO Usage
        private void OpenDB(object sender, EventArgs e)
        {
            if (Directory.Exists(DatabasePath))
                Process.Start("explorer.exe", DatabasePath);
        }
        private void Menu_Export_Click(object sender, EventArgs e)
        {
            if (Results == null || Results.Count == 0)
            { WinFormsUtil.Alert("No results to export."); return; }

            if (DialogResult.Yes != WinFormsUtil.Prompt(MessageBoxButtons.YesNo, "Export to a folder?"))
                return;

            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (DialogResult.OK != fbd.ShowDialog())
                return;

            string path = fbd.SelectedPath;
            Directory.CreateDirectory(path);

            foreach (PKM pkm in Results)
                File.WriteAllBytes(Path.Combine(path, Util.CleanFileName(pkm.FileName)), pkm.DecryptedBoxData);
        }

        // View Updates
        private IEnumerable<PKM> SearchDatabase()
        {
            // Populate Search Query Result
            IEnumerable<PKM> res = RawDB;

            // Filter for Selected Source
            if (!Menu_SearchBoxes.Checked)
                res = res.Where(pk => pk.Identifier.StartsWith(DatabasePath + Path.DirectorySeparatorChar, StringComparison.Ordinal));
            if (!Menu_SearchDatabase.Checked)
            {
                res = res.Where(pk => !pk.Identifier.StartsWith(DatabasePath + Path.DirectorySeparatorChar, StringComparison.Ordinal));
#if LOADALL
                res = res.Where(pk => !pk.Identifier.StartsWith(EXTERNAL_SAV, StringComparison.Ordinal));
#endif
            }

            int format = MAXFORMAT + 1 - CB_Format.SelectedIndex;
            switch (CB_FormatComparator.SelectedIndex)
            {
                case 0: /* Do nothing */                            break;
                case 1: res = res.Where(pk => pk.Format >= format); break;
                case 2: res = res.Where(pk => pk.Format == format); break;
                case 3: res = res.Where(pk => pk.Format <= format); break;
            }
            if (CB_FormatComparator.SelectedIndex != 0)
            {
                if (format <= 2) // 1-2
                    res = res.Where(pk => pk.Format <= 2);
                if (format >= 3 && format <= 6) // 3-6
                    res = res.Where(pk => pk.Format >= 3);
                if (format >= 7) // 1,3-6,7
                    res = res.Where(pk => pk.Format != 2);
            }

            switch (CB_Generation.SelectedIndex)
            {
                case 0: /* Do nothing */                break;
                case 1: res = res.Where(pk => pk.Gen7); break;
                case 2: res = res.Where(pk => pk.Gen6); break;
                case 3: res = res.Where(pk => pk.Gen5); break;
                case 4: res = res.Where(pk => pk.Gen4); break;
                case 5: res = res.Where(pk => pk.Gen3); break;
            }

            // Primary Searchables
            int species = WinFormsUtil.GetIndex(CB_Species);
            int ability = WinFormsUtil.GetIndex(CB_Ability);
            int nature = WinFormsUtil.GetIndex(CB_Nature);
            int item = WinFormsUtil.GetIndex(CB_HeldItem);
            if (species != -1) res = res.Where(pk => pk.Species == species);
            if (ability != -1) res = res.Where(pk => pk.Ability == ability);
            if (nature != -1) res = res.Where(pk => pk.Nature == nature);
            if (item != -1) res = res.Where(pk => pk.HeldItem == item);

            // Secondary Searchables
            int move1 = WinFormsUtil.GetIndex(CB_Move1);
            int move2 = WinFormsUtil.GetIndex(CB_Move2);
            int move3 = WinFormsUtil.GetIndex(CB_Move3);
            int move4 = WinFormsUtil.GetIndex(CB_Move4);
            var moves = new[] {move1, move2, move3, move4}.Where(z => z > 0).ToList();
            int count = moves.Count;
            if (count > 0) res = res.Where(pk => pk.Moves.Intersect(moves).Count() == count);
            int vers = WinFormsUtil.GetIndex(CB_GameOrigin);
            if (vers != -1) res = res.Where(pk => pk.Version == vers);
            int hptype = WinFormsUtil.GetIndex(CB_HPType);
            if (hptype != -1) res = res.Where(pk => pk.HPType == hptype);
            if (CHK_Shiny.CheckState == CheckState.Checked) res = res.Where(pk => pk.IsShiny);
            if (CHK_Shiny.CheckState == CheckState.Unchecked) res = res.Where(pk => !pk.IsShiny);
            if (CHK_IsEgg.CheckState == CheckState.Checked) res = res.Where(pk => pk.IsEgg);
            if (CHK_IsEgg.CheckState == CheckState.Unchecked) res = res.Where(pk => !pk.IsEgg);
            if (CHK_IsEgg.CheckState == CheckState.Checked && string.IsNullOrWhiteSpace(MT_ESV.Text))
                res = res.Where(pk => pk.PSV == Convert.ToInt16(MT_ESV.Text));

            // Tertiary Searchables
            res = FilterByLVL(res, CB_Level.SelectedIndex, TB_Level.Text);
            res = FilterByIVs(res, CB_IV.SelectedIndex);
            res = FilterByEVs(res, CB_EVTrain.SelectedIndex);

            slotSelected = -1; // reset the slot last viewed

            if (Menu_SearchLegal.Checked && !Menu_SearchIllegal.Checked)
                res = res.Where(pk => new LegalityAnalysis(pk).ParsedValid);
            if (!Menu_SearchLegal.Checked && Menu_SearchIllegal.Checked)
                res = res.Where(pk => new LegalityAnalysis(pk).ParsedInvalid);

            if (RTB_Instructions.Lines.Any(line => line.Length > 0))
            {
                var filters = BatchEditor.StringInstruction.GetFilters(RTB_Instructions.Lines).ToArray();
                BatchEditor.ScreenStrings(filters);
                res = res.Where(pkm => IsPKMFiltered(pkm, filters)); // Compare across all filters
            }

            if (Menu_SearchClones.Checked)
                res = res.GroupBy(Hash).Where(group => group.Count() > 1).SelectMany(z => z);

            return res;
        }

        private static bool IsPKMFiltered(PKM pkm, IEnumerable<BatchEditor.StringInstruction> filters)
        {
            foreach (var cmd in filters)
            {
                if (cmd.PropertyName == nameof(PKM.Identifier) + "Contains")
                {
                    bool result = pkm.Identifier.Contains(cmd.PropertyValue);
                    if (result != cmd.Evaluator)
                        return false;
                    continue;
                }
                if (!pkm.GetType().HasPropertyAll(cmd.PropertyName))
                    return false;
                try { if (pkm.GetType().IsValueEqual(pkm, cmd.PropertyName, cmd.PropertyValue) == cmd.Evaluator) continue; }
                catch { Debug.WriteLine($"Unable to compare {cmd.PropertyName} to {cmd.PropertyValue}."); }
                return false;
            }
            return true;
        }

        private static IEnumerable<PKM> FilterByLVL(IEnumerable<PKM> res, int option, string lvl)
        {
            if (string.IsNullOrWhiteSpace(lvl))
                return res;
            if (!int.TryParse(lvl, out int level))
                return res;
            if (level > 100)
                return res;

            switch (option)
            {
                case 0: break; // Any (Do nothing)
                case 1: // <=
                    return res.Where(pk => pk.Stat_Level <= level);
                case 2: // == 
                    return res.Where(pk => pk.Stat_Level == level);
                case 3: // >=
                    return res.Where(pk => pk.Stat_Level >= level);
            }
            return res;
        }
        private static IEnumerable<PKM> FilterByEVs(IEnumerable<PKM> res, int option)
        {
            switch (option)
            {
                case 0: break; // Any (Do nothing)
                case 1: // None (0)
                    return res.Where(pk => pk.EVs.Sum() == 0);
                case 2: // Some (127-0)
                    return res.Where(pk => pk.EVs.Sum() < 128);
                case 3: // Half (128-507)
                    return res.Where(pk => pk.EVs.Sum() >= 128 && pk.EVs.Sum() < 508);
                case 4: // Full (508+)
                    return res.Where(pk => pk.EVs.Sum() >= 508);
            }
            return res;
        }
        private static IEnumerable<PKM> FilterByIVs(IEnumerable<PKM> res, int option)
        {
            switch (option)
            {
                case 0: break; // Do nothing
                case 1: // <= 90
                    return res.Where(pk => pk.IVs.Sum() <= 90);
                case 2: // 91-120
                    return res.Where(pk => pk.IVs.Sum() > 90 && pk.IVs.Sum() <= 120);
                case 3: // 121-150
                    return res.Where(pk => pk.IVs.Sum() > 120 && pk.IVs.Sum() <= 150);
                case 4: // 151-179
                    return res.Where(pk => pk.IVs.Sum() > 150 && pk.IVs.Sum() < 180);
                case 5: // 180+
                    return res.Where(pk => pk.IVs.Sum() >= 180);
                case 6: // == 186
                    return res.Where(pk => pk.IVs.Sum() == 186);
            }
            return res;
        }

        private async void B_Search_Click(object sender, EventArgs e)
        {
            B_Search.Enabled = false;
            var search = SearchDatabase();

            bool legalSearch = Menu_SearchLegal.Checked ^ Menu_SearchIllegal.Checked;
            if (legalSearch && WinFormsUtil.Prompt(MessageBoxButtons.YesNo, "Check wordfilter legality?") == DialogResult.No)
                Legal.CheckWordFilter = false;
            var results = await Task.Run(() => search.ToArray());
            Legal.CheckWordFilter = true;

            if (results.Length == 0)
            {
                if (!Menu_SearchBoxes.Checked && !Menu_SearchDatabase.Checked)
                    WinFormsUtil.Alert("No data source to search!", "No results found!");
                else
                    WinFormsUtil.Alert("No results found!");
            }
            SetResults(new List<PKM>(results)); // updates Count Label as well.
            System.Media.SystemSounds.Asterisk.Play();
            B_Search.Enabled = true;
        }
        private void UpdateScroll(object sender, ScrollEventArgs e)
        {
            if (e.OldValue != e.NewValue)
                FillPKXBoxes(e.NewValue);
        }
        private void SetResults(List<PKM> res)
        {
            Results = new List<PKM>(res);

            SCR_Box.Maximum = (int)Math.Ceiling((decimal)Results.Count / RES_MIN);
            if (SCR_Box.Maximum > 0) SCR_Box.Maximum -= 1;

            SCR_Box.Value = 0;
            FillPKXBoxes(0);

            L_Count.Text = string.Format(Counter, Results.Count);
            B_Search.Enabled = true;
        }
        private void FillPKXBoxes(int start)
        {
            if (Results == null)
            {
                for (int i = 0; i < RES_MAX; i++)
                    PKXBOXES[i].Image = null;
                return;
            }
            int begin = start*RES_MIN;
            int end = Math.Min(RES_MAX, Results.Count - start*RES_MIN);
            for (int i = 0; i < end; i++)
                PKXBOXES[i].Image = Results[i + begin].Sprite();
            for (int i = end; i < RES_MAX; i++)
                PKXBOXES[i].Image = null;

            for (int i = 0; i < RES_MAX; i++)
                PKXBOXES[i].BackgroundImage = Properties.Resources.slotTrans;
            if (slotSelected != -1 && slotSelected >= RES_MIN * start && slotSelected < RES_MIN * start + RES_MAX)
                PKXBOXES[slotSelected - start * RES_MIN].BackgroundImage = slotColor ?? Properties.Resources.slotView;
        }

        // Misc Update Methods
        private void ToggleESV(object sender, EventArgs e)
        {
            L_ESV.Visible = MT_ESV.Visible = CHK_IsEgg.CheckState == CheckState.Checked;
        }
        private void ChangeLevel(object sender, EventArgs e)
        {
            if (CB_Level.SelectedIndex == 0)
                TB_Level.Text = "";
        }
        private void ChangeGame(object sender, EventArgs e)
        {
            if (CB_GameOrigin.SelectedIndex != 0)
                CB_Generation.SelectedIndex = 0;
        }
        private void ChangeGeneration(object sender, EventArgs e)
        {
            if (CB_Generation.SelectedIndex != 0)
                CB_GameOrigin.SelectedIndex = 0;
        }

        private void Menu_SearchAdvanced_Click(object sender, EventArgs e)
        {
            if (!Menu_SearchAdvanced.Checked)
            { Size = MinimumSize; RTB_Instructions.Clear(); }
            else Size = MaximumSize;
        }

        private void Menu_Exit_Click(object sender, EventArgs e)
        {
            Close();
        }
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if (!PAN_Box.RectangleToScreen(PAN_Box.ClientRectangle).Contains(MousePosition))
                return;
            int oldval = SCR_Box.Value;
            int newval = oldval + (e.Delta < 0 ? 1 : -1);
            if (newval >= SCR_Box.Minimum && SCR_Box.Maximum >= newval)
                FillPKXBoxes(SCR_Box.Value = newval);
        }

        private void ChangeFormatFilter(object sender, EventArgs e)
        {
            if (CB_FormatComparator.SelectedIndex == 0)
            {
                CB_Format.Visible = false; // !any
                CB_Format.SelectedIndex = 0;
            }
            else
            {
                CB_Format.Visible = true;
                int index = MAXFORMAT - SAV.Generation + 1;
                CB_Format.SelectedIndex = index < CB_Format.Items.Count ? index : 0; // SAV generation (offset by 1 for "Any")
            }
        }

        private void Menu_DeleteClones_Click(object sender, EventArgs e)
        {
            var dr = WinFormsUtil.Prompt(MessageBoxButtons.YesNo,
                "Deleting clones from database is not reversible." + Environment.NewLine +
                "If a PKM is deemed a clone, only the newest file (date modified) will be kept.", "Continue?");

            if (dr != DialogResult.Yes)
                return;

            var deleted = 0;
            var db = RawDB.Where(pk => pk.Identifier.StartsWith(DatabasePath + Path.DirectorySeparatorChar, StringComparison.Ordinal))
                .OrderByDescending(file => new FileInfo(file.Identifier).LastWriteTime);
            var clones = db.GroupBy(Hash).Where(group => group.Count() > 1).SelectMany(z => z.Skip(1));
            foreach (var pk in clones)
            {
                try { File.Delete(pk.Identifier); ++deleted; }
                catch { WinFormsUtil.Error("Unable to delete clone:" + Environment.NewLine + pk.Identifier); }
            }

            if (deleted == 0)
            { WinFormsUtil.Alert("No clones detected or deleted."); return; }

            WinFormsUtil.Alert($"{deleted} files deleted.", "The form will now close.");
            Close();
        }
    }
}
