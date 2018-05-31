using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Media;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using PKHeX.Core;
using PKHeX.WinForms.Controls;
using PKHeX.WinForms.Properties;

namespace PKHeX.WinForms
{
    public partial class Main
    {
        private List<Pokemon> pkms;
        private string extractPath;
        private bool singleFiles;
        private string cyberSavPath;
        private bool savExtraction;
        private string minBox;
        private string maxBox;
        private bool bak;
        private bool gen7;

        public Main(List<Pokemon> pkms, string extractPath, bool singleFiles, string cyberSavPath, bool savExtraction, string minBox, string maxBox, bool bak, bool gen7)
        {
            this.pkms = pkms;
            this.extractPath = extractPath;
            this.singleFiles = singleFiles;
            this.cyberSavPath = cyberSavPath;
            this.savExtraction = savExtraction;
            this.minBox = minBox;
            this.maxBox = maxBox;
            this.bak = bak;
            this.gen7 = gen7;

            new Task(RefreshMGDB).Start();
            InitializeComponent();

            FormLoadCheckForUpdates();
            FormLoadAddEvents();

            string[] args = Environment.GetCommandLineArgs();
            FormLoadInitialSettings(args, out bool showChangelog, out bool BAKprompt);
            FormLoadInitialFiles(args);

            IsInitialized = true; // Splash Screen closes on its own.
            PKME_Tabs_UpdatePreviewSprite(null, null);
        }

        public void Form1_Load(object sender, EventArgs e)
        {

            string pokegenfolder = "Gen Files\\";

            PictureBox[] pba = {
                                    C_SAV.Box.bpkx1, C_SAV.Box.bpkx2, C_SAV.Box.bpkx3, C_SAV.Box.bpkx4, C_SAV.Box.bpkx5, C_SAV.Box.bpkx6,
                                    C_SAV.Box.bpkx7, C_SAV.Box.bpkx8, C_SAV.Box.bpkx9, C_SAV.Box.bpkx10,C_SAV.Box.bpkx11,C_SAV.Box.bpkx12,
                                    C_SAV.Box.bpkx13,C_SAV.Box.bpkx14,C_SAV.Box.bpkx15,C_SAV.Box.bpkx16,C_SAV.Box.bpkx17,C_SAV.Box.bpkx18,
                                    C_SAV.Box.bpkx19,C_SAV.Box.bpkx20,C_SAV.Box.bpkx21,C_SAV.Box.bpkx22,C_SAV.Box.bpkx23,C_SAV.Box.bpkx24,
                                    C_SAV.Box.bpkx25,C_SAV.Box.bpkx26,C_SAV.Box.bpkx27,C_SAV.Box.bpkx28,C_SAV.Box.bpkx29,C_SAV.Box.bpkx30
                                };

            Image standardImage = pba[0].Image;

            int minB = 1;
            int maxB = 31;

            if (!singleFiles)
            {
            }
            if (savExtraction)
            {
                openQuick(cyberSavPath);
                minB = int.Parse(minBox);
                maxB = int.Parse(maxBox);
                C_SAV.Box.CB_BoxSelect.SelectedIndex = minB - 1;
            }

            Dictionary<string, string> metDict = new Dictionary<string, string>();
            DirectoryInfo d = new DirectoryInfo(pokegenfolder + "gold");
            foreach (FileInfo f in d.GetFiles())
            {
                if (f.Name.Contains("Ho-Oh"))
                {
                    if (!metDict.ContainsKey("ho-oh"))
                    {
                        metDict.Add("ho-oh", f.FullName);
                    }
                }
                else
                {
                    string filename = f.Name.Substring(f.Name.IndexOf("-") + 1);
                    filename = filename.Substring(0, filename.IndexOf("-")).Trim();
                    if (!metDict.ContainsKey(filename.ToLower()))
                    {
                        metDict.Add(filename.ToLower(), f.FullName);
                    }
                }
            }

            int count = 0;
            foreach (Pokemon p in pkms)
            {
                ComboItem i = new ComboItem();
                i.Text = null;
                string poke = p.name;
                poke = poke.Replace("'", "’");
                poke = poke.Replace("-Mega", "");

                if (savExtraction)
                {
                    if (count == 30)
                    {
                        if (C_SAV.Box.CB_BoxSelect.SelectedIndex == maxB - 1)
                        {
                            MessageBox.Show("Stopped at " + poke + ", since the boxes are fully stretched!");
                            break;
                        }
                        count = 0;
                        C_SAV.Box.CB_BoxSelect.SelectedIndex++;
                    }
                    if (!ContextMenuSAV.ClickViewChange(pba[count], null))
                    {
                    }
                    else
                    {
                        bool stop = false;
                        while (ContextMenuSAV.ClickViewChange(pba[count], null))
                        {
                            count++;
                            if (count == 30)
                            {
                                if (C_SAV.Box.CB_BoxSelect.SelectedIndex == maxB - 1)
                                {
                                    MessageBox.Show("Stopped at " + poke + ", since the boxes are fully stretched!");
                                    stop = true;
                                    break;
                                }
                                count = 0;
                                C_SAV.Box.CB_BoxSelect.SelectedIndex++;
                            }
                        }
                        if (stop)
                        {
                            break;
                        }
                    }
                }

                string item = p.item;
                if (item.Equals("BlackGlasses"))
                {
                    item = "Black Glasses";
                }
                if (item.Equals("NeverMeltIce"))
                {
                    item = "Never-Melt Ice";
                }
                if (item.Equals("BrightPowder"))
                {
                    item = "Bright Powder";
                }
                item = item.Replace("'", "’");
                string ability = p.ability;
                if (ability.Equals("Lightningrod"))
                {
                    ability = "Lightning Rod";
                }
                string nature = p.nature;
                string hptype = null;
                string move1 = (p.moves.Count > 0) ? p.moves[0] : "(None)";
                if (move1.Contains("Hidden Power"))
                {
                    hptype = move1.Substring(13);
                    move1 = "Hidden Power";
                }
                string move2 = (p.moves.Count > 1) ? p.moves[1] : "(None)";
                if (move2.Contains("Hidden Power"))
                {
                    hptype = move2.Substring(13);
                    move2 = "Hidden Power";
                }
                string move3 = (p.moves.Count > 2) ? p.moves[2] : "(None)";
                if (move3.Contains("Hidden Power"))
                {
                    hptype = move3.Substring(13);
                    move3 = "Hidden Power";
                }
                string move4 = (p.moves.Count > 3) ? p.moves[3] : "(None)";
                if (move4.Contains("Hidden Power"))
                {
                    hptype = move4.Substring(13);
                    move4 = "Hidden Power";
                }

                //fixing double moves
                if ((move1 != "(None)" && (move1.Equals(move2) || move1.Equals(move3) || move1.Equals(move4))) || (move2 != "(None)" && (move2.Equals(move3) || move2.Equals(move4))) || (move3 != "(None)" && move3.Equals(move4)))
                {
                    continue;
                }

                //fixing King's Shield (King’s Shield)
                move1 = move1.Replace("'", "’");
                move2 = move2.Replace("'", "’");
                move3 = move3.Replace("'", "’");
                move4 = move4.Replace("'", "’");

                string pokemonlower = poke.ToLower();
                bool special = false;

                //fixing Mewtwo + Unnerve and all other Event Mons
                if (poke.Equals("Mewtwo") && ability.Equals("Unnerve"))
                {
                    special = true;
                    mainMenuOpen(metDict["mewtwo (unnerve)"]);
                }
                else if (poke.Equals("Jirachi") && (move1.Contains("Heart Stamp") || move2.Contains("Heart Stamp") || move3.Contains("Heart Stamp") || move4.Contains("Heart Stamp")))
                {
                    special = true;
                    mainMenuOpen(metDict["jirachi (heart stamp)"]);
                }
                else if (poke.Equals("Jirachi") && (move1.Contains("Play Rough") || move2.Contains("Play Rough") || move3.Contains("Play Rough") || move4.Contains("Play Rough")))
                {
                    special = true;
                    mainMenuOpen(metDict["jirachi (heart stamp)"]);
                }
                else if (poke.Equals("Zapdos") && ability.Equals("Static"))
                {
                    special = true;
                    mainMenuOpen(metDict["zapdos (static)"]);
                }
                else if (poke.Equals("Thundurus") && ability.Equals("Defiant"))
                {
                    special = true;
                    mainMenuOpen(metDict["thundurus (defiant)"]);
                }
                else if (poke.Equals("Entei") && (move1.Contains("Extreme Speed") || move2.Contains("Extreme Speed") || move3.Contains("Extreme Speed") || move4.Contains("Extreme Speed")))
                {
                    special = true;
                    mainMenuOpen(metDict["entei (espeed)"]);
                }
                else if (poke.Equals("Tauros") && (move1.Contains("Rock Climb") || move2.Contains("Rock Climb") || move3.Contains("Rock Climb") || move4.Contains("Rock Climb")))
                {
                    special = true;
                    mainMenuOpen(metDict["tauros (rock climb)"]);
                }
                else if (poke.Equals("Tauros") && (move1.Contains("Body Slam") || move2.Contains("Body Slam") || move3.Contains("Body Slam") || move4.Contains("Body Slam")))
                {
                    special = true;
                    mainMenuOpen(metDict["tauros (body slam)"]);
                }
                else if (poke.Equals("Celebi") && (move1.Contains("Nasty Plot") || move2.Contains("Nasty Plot") || move3.Contains("Nasty Plot") || move4.Contains("Nasty Plot")))
                {
                    special = true;
                    mainMenuOpen(metDict["celebi (nasty plot)"]);
                }
                else if (poke.Equals("Heatran") && (move1.Contains("Eruption") || move2.Contains("Eruption") || move3.Contains("Eruption") || move4.Contains("Eruption")))
                {
                    special = true;
                    mainMenuOpen(metDict["heatran (eruption)"]);
                }
                else if (poke.Equals("Jirachi") && (move1.Contains("Follow Me") || move2.Contains("Follow Me") || move3.Contains("Follow Me") || move4.Contains("Follow Me")))
                {
                    special = true;
                    mainMenuOpen(metDict["jirachi (follow me)"]);
                }
                else if (poke.Equals("Raikou") && (move1.Contains("Aura Sphere") || move2.Contains("Aura Sphere") || move3.Contains("Aura Sphere") || move4.Contains("Aura Sphere")))
                {
                    special = true;
                    mainMenuOpen(metDict["raikou (aura sphere)"]);
                }
                else if (poke.Contains("Dusk") && (move1.Contains("Helping Hand") || move2.Contains("Helping Hand") || move3.Contains("Helping Hand") || move4.Contains("Helping Hand")))
                {
                    special = true;
                    mainMenuOpen(metDict["duskull (helping hand)"]);
                }
                else if (poke.Equals("Lunatone") && (move1.Contains("Baton Pass") || move2.Contains("Baton Pass") || move3.Contains("Baton Pass") || move4.Contains("Baton Pass")))
                {
                    special = true;
                    mainMenuOpen(metDict["lunatone (baton pass)"]);
                }
                else if (poke.Contains("Toge") && (move1.Contains("Helping Hand") || move2.Contains("Helping Hand") || move3.Contains("Helping Hand") || move4.Contains("Helping Hand")))
                {
                    special = true;
                    mainMenuOpen(metDict["togepi (helping hand)"]);
                }
                else if (poke.Equals("Rapidash") && (move1.Contains("Baton Pass") || move2.Contains("Baton Pass") || move3.Contains("Baton Pass") || move4.Contains("Baton Pass")))
                {
                    special = true;
                    mainMenuOpen(metDict["rapidash (baton pass)"]);
                }
                else if (poke.Equals("Salamence") && (move1.Contains("Refresh") || move2.Contains("Refresh") || move3.Contains("Refresh") || move4.Contains("Refresh")))
                {
                    special = true;
                    mainMenuOpen(metDict["salamence (refresh)"]);
                }
                else if (poke.Equals("Moltres") && (move1.Contains("Morning Sun") || move2.Contains("Morning Sun") || move3.Contains("Morning Sun") || move4.Contains("Morning Sun")))
                {
                    special = true;
                    mainMenuOpen(metDict["moltres (morning sun)"]);
                }
                else if (poke.Equals("Moltres") && ability.Contains("Flame Body"))
                {
                    special = true;
                    mainMenuOpen(metDict["moltres (flame body)"]);
                }
                else if (poke.Equals("Blastoise") && (move1.Contains("Celebate") || move2.Contains("Celebate") || move3.Contains("Celebate") || move4.Contains("Celebate")))
                {
                    special = true;
                    mainMenuOpen(metDict["blastoise (celebrate)"]);
                }
                else if (poke.Equals("Gengar") && (move1.Contains("Sludge Wave") || move2.Contains("Sludge Wave") || move3.Contains("Sludge Wave") || move4.Contains("Sludge Wave")))
                {
                    special = true;
                    mainMenuOpen(metDict["gengar"]);
                }
                else if (poke.Equals("Lycanroc") && ability.Contains("Tough Claws"))
                {
                    special = true;
                    mainMenuOpen(metDict["lycanroc (dusk)"]);
                }
                else if (poke.Equals("Rockruff") && ability.Contains("Own Tempo"))
                {
                    special = true;
                    mainMenuOpen(metDict["rockruff (dusk)"]);
                }
                else if (poke.Equals("Zygarde-10%"))
                {
                    special = true;
                    mainMenuOpen(metDict["zygarde (10%)"]);
                }
                else if (poke.Equals("Type: Null"))
                {
                    special = true;
                    mainMenuOpen(metDict["type null"]);
                }
                else if (poke.Equals("Gengar"))
                {

                }
                else if (poke.Contains("Totem"))
                {
                    special = true;
                    mainMenuOpen(pokegenfolder + "totem.pk7");
                }
                else
                {
                    foreach (KeyValuePair<string, string> met in metDict)
                    {
                        if (pokemonlower.Contains(met.Key))
                        {
                            special = true;
                            mainMenuOpen(met.Value);
                            break;
                        }
                    }
                }

                if (!special)
                {
                    if (move1.Equals("Soft-Boiled") || move2.Equals("Soft-Boiled") || move3.Equals("Soft-Boiled") || move4.Equals("Soft-Boiled"))
                    {
                        mainMenuOpen(pokegenfolder + "softboiled.ek6");
                    }
                    else if (((move1.Equals("Wish") || move2.Equals("Wish") || move3.Equals("Wish") || move4.Equals("Wish"))) && (poke.Contains("Chansey") || poke.Contains("Blissey") || poke.Contains("Hypno") || poke.Contains("Lickilicky")))
                    {
                        mainMenuOpen(pokegenfolder + "softboiled.ek6");
                    }
                    else if ((move1.Equals("Double-Edge") || move2.Equals("Double-Edge") || move3.Equals("Double-Edge") || move4.Equals("Double-Edge")) && (poke.Contains("Doduo") || poke.Contains("Dodrio") || poke.Contains("Glalie") || poke.Contains("Tauros")))
                    {
                        mainMenuOpen(pokegenfolder + "softboiled.ek6");
                    }
                    else if ((move1.Equals("Counter") || move2.Equals("Counter") || move3.Equals("Counter") || move4.Equals("Counter")) && (poke.Contains("Geodude") || poke.Contains("Golem")))
                    {
                        mainMenuOpen(pokegenfolder + "softboiled.ek6");
                    }
                    else if ((move1.Equals("Body Slam") || move2.Equals("Body Slam") || move3.Equals("Body Slam") || move4.Equals("Body Slam")) && (poke.Contains("Dunsparce")))
                    {
                        mainMenuOpen(pokegenfolder + "softboiled.ek6");
                    }
                    else if (!gen7 && (move1.Equals("Defog") || move2.Equals("Defog") || move3.Equals("Defog") || move4.Equals("Defog")) && !poke.Contains("Mandibuzz") && !poke.Contains("Vullaby") && !poke.Contains("Braviary") && !poke.Contains("Golbat") && !poke.Contains("Crobat") && !poke.Contains("Archeops") && !poke.Contains("Archen") && !poke.Contains("Swanna") && !poke.Contains("Ducklett") && !poke.Contains("Zubat") && !poke.Contains("Pidgey") && !poke.Contains("Scizor") && !poke.Contains("Dartrix") && !poke.Contains("Decidueye") && !poke.Contains("Fomantis") && !poke.Contains("Lurantis") && !poke.Contains("Rowlet"))
                    {
                        mainMenuOpen(pokegenfolder + "defog.ek6");
                    }
                    else if ((move1.Equals("Tailwind") || move2.Equals("Tailwind") || move3.Equals("Tailwind") || move4.Equals("Tailwind")) && poke.Contains("Lumineon"))
                    {
                        mainMenuOpen(pokegenfolder + "defog.ek6");
                    }
                    else if ((move1.Equals("Bullet Seed") || move2.Equals("Bullet Seed") || move3.Equals("Bullet Seed") || move4.Equals("Bullet Seed")) && (poke.Contains("Cacturne") || poke.Contains("Ludicolo") || poke.Contains("Torterra")))
                    {
                        mainMenuOpen(pokegenfolder + "defog.ek6");
                    }
                    else if ((move1.Equals("Sucker Punch") || move2.Equals("Sucker Punch") || move3.Equals("Sucker Punch") || move4.Equals("Sucker Punch")) && (poke.Contains("Geodude") || poke.Contains("Golem") || poke.Contains("Hitmonlee") || poke.Contains("Hitmontop") || poke.Contains("Shiftry") || poke.Contains("Victreebel")))
                    {
                        mainMenuOpen(pokegenfolder + "defog.ek6");
                    }
                    else if ((move1.Equals("Vacuum Wave") || move2.Equals("Vacuum Wave") || move3.Equals("Vacuum Wave") || move4.Equals("Vacuum Wave")) && (poke.Contains("Infernape") || poke.Contains("Poliwrath")))
                    {
                        mainMenuOpen(pokegenfolder + "defog.ek6");
                    }
                    else if ((move1.Equals("Whirlpool") || move2.Equals("Whirlpool") || move3.Equals("Whirlpool") || move4.Equals("Whirlpool")) && (poke.Contains("Seel")))
                    {
                        mainMenuOpen(pokegenfolder + "defog.ek6");
                    }
                    else if ((move1.Equals("Sucker Punch") || move2.Equals("Sucker Punch") || move3.Equals("Sucker Punch") || move4.Equals("Sucker Punch")) && (poke.Contains("Lanturn")))
                    {
                        mainMenuOpen(pokegenfolder + "defog.ek6");
                    }
                    else if ((move1.Equals("Work Up") || move2.Equals("Work Up") || move3.Equals("Work Up") || move4.Equals("Work Up")) && (poke.Contains("Togepi") || poke.Contains("Exploud")))
                    {
                        mainMenuOpen(pokegenfolder + "workup.ek6");
                    }
                    else if ((move1.Equals("Refresh") || move2.Equals("Refresh") || move3.Equals("Refresh") || move4.Equals("Refresh")) && (poke.Contains("Pidgeot") || poke.Contains("Salamence")))
                    {
                        mainMenuOpen(pokegenfolder + "refresh.ek6");
                    }
                    else
                    {
                        if (gen7)
                        {
                            // Gen 7
                            mainMenuOpen(pokegenfolder + "sunmoon.pk7");
                        }
                        else
                        {
                            // Gen 6
                            mainMenuOpen(pokegenfolder + "oras.ekx.ek6");
                        }
                    }
                }

                if (p.level != null && int.Parse(PKME_Tabs.TB_MetLevel.Text) > int.Parse(p.level))
                {
                    PKME_Tabs.TB_MetLevel.Text = p.level;
                }

                if (PKME_Tabs.CB_Form.Items.Count != 0)
                {
                    PKME_Tabs.CB_Form.SelectedIndex = 0;
                }

                string[] evs = p.evs.Split('/');
                foreach (ComboItem items in PKME_Tabs.CB_Species.Items)
                {
                    if (poke.Contains(items.Text))
                    {
                        if (i.Text != null)
                        {
                            if (i.Text.Length < items.Text.Length)
                            {
                                i = items;
                            }
                        }
                        else
                        {
                            i = items;
                        }
                    }
                }
                PKME_Tabs.CB_Species.SelectedItem = i;
                PKME_Tabs.TB_Nickname.Text = i.Text;
                if (p.level != null)
                {
                    PKME_Tabs.TB_Level.Text = p.level;
                }
                else
                {
                    PKME_Tabs.TB_Level.Text = "100";
                }

                if (poke.Contains("-"))
                {
                    string extension = ((poke.Split('-'))[1]).Trim();
                    if (poke.Contains("-Totem"))
                    {
                        extension = "Large";
                    }
                    bool b = false;

                    while (PKME_Tabs.CB_Form.SelectedIndex < PKME_Tabs.CB_Form.Items.Count - 1)
                    {
                        PKME_Tabs.CB_Form.SelectedIndex++;
                        string text = PKME_Tabs.CB_Form.SelectedItem.ToString().Substring(PKME_Tabs.CB_Form.SelectedItem.ToString().IndexOf("=") + 1);
                        if (text.Contains(","))
                        {
                            text = text.Substring(0, text.IndexOf(","));
                        }
                        text = text.Trim();
                        if (text.Contains(extension))
                        {
                            b = true;
                            break;
                        }
                    }
                    if (!b)
                    {
                        PKME_Tabs.CB_Form.SelectedIndex = 0;
                    }
                }

                i.Text = null;
                foreach (ComboItem items in PKME_Tabs.CB_HeldItem.Items)
                {
                    if (item.Contains(items.Text))
                    {
                        if (i.Text != null)
                        {
                            if (i.Text.Length < items.Text.Length)
                            {
                                i = items;
                            }
                        }
                        else
                        {
                            i = items;
                        }
                    }
                }
                PKME_Tabs.CB_HeldItem.SelectedItem = i;

                i.Text = null;
                foreach (ComboItem items in PKME_Tabs.CB_Ability.Items)
                {
                    if (items.Text.Contains(ability))
                    {
                        if (i.Text != null)
                        {
                            if (i.Text.Length < items.Text.Length)
                            {
                                i = items;
                            }
                        }
                        else
                        {
                            i = items;
                        }
                    }
                }
                if (i.Text == null)
                {
                    if (!ability.Equals(""))
                    {
                        while (true)
                        {
                            bool b = false;
                            foreach (ComboItem items in PKME_Tabs.CB_Ability.Items)
                            {
                                if (items.Text.Contains(ability))
                                {
                                    PKME_Tabs.CB_Ability.SelectedItem = items;
                                    b = true;
                                    break;
                                }
                            }
                            if (b)
                            {
                                break;
                            }
                            else
                            {
                                int species = WinFormsUtil.GetIndex(PKME_Tabs.CB_Species);
                                int[] mspec = {     // XY
                                        003, 009, 065, 094, 115, 127, 130, 142, 181, 212, 214, 229, 248, 257, 282, 303, 306, 308, 310, 354, 359, 380, 381, 445, 448, 460, 
                                        // ORAS
                                        015, 018, 080, 208, 254, 260, 302, 319, 323, 334, 362, 373, 376, 384, 428, 475, 531, 719,
                                    };
                                if (Array.IndexOf(mspec, species) > -1 || ((species == 6) || (species == 150)))
                                {
                                    break;
                                }
                                PKME_Tabs.CB_Form.SelectedIndex++;
                            }
                        }
                    }
                }
                else
                {
                    if (poke.Equals("Registeel"))
                    {
                        PKME_Tabs.CB_Ability.SelectedIndex = 0;
                    }
                    else if (poke.Contains("Kyurem"))
                    {
                        PKME_Tabs.CB_Ability.SelectedIndex = 1;
                    }
                    else
                    {
                        PKME_Tabs.CB_Ability.SelectedItem = i;
                    }
                }

                if (nature != null)
                {
                    i.Text = null;
                    foreach (ComboItem items in PKME_Tabs.CB_Nature.Items)
                    {
                        if (items.Text.Contains(nature))
                        {
                            if (i.Text != null)
                            {
                                if (i.Text.Length < items.Text.Length)
                                {
                                    i = items;
                                }
                            }
                            else
                            {
                                i = items;
                            }
                        }
                    }
                    PKME_Tabs.CB_Nature.SelectedItem = i;
                }

                i.Text = null;
                foreach (ComboItem items in PKME_Tabs.CB_Move1.Items)
                {
                    if (items.Text.Equals(move1))
                    {
                        if (i.Text != null)
                        {
                            if (i.Text.Length < items.Text.Length)
                            {
                                i = items;
                            }
                        }
                        else
                        {
                            i = items;
                        }
                    }
                }
                PKME_Tabs.CB_Move1.SelectedItem = i;

                i.Text = null;
                foreach (ComboItem items in PKME_Tabs.CB_Move2.Items)
                {
                    if (items.Text.Equals(move2))
                    {
                        if (i.Text != null)
                        {
                            if (i.Text.Length < items.Text.Length)
                            {
                                i = items;
                            }
                        }
                        else
                        {
                            i = items;
                        }
                    }
                }
                PKME_Tabs.CB_Move2.SelectedItem = i;

                i.Text = null;
                foreach (ComboItem items in PKME_Tabs.CB_Move3.Items)
                {
                    if (items.Text.Equals(move3))
                    {
                        if (i.Text != null)
                        {
                            if (i.Text.Length < items.Text.Length)
                            {
                                i = items;
                            }
                        }
                        else
                        {
                            i = items;
                        }
                    }
                }
                PKME_Tabs.CB_Move3.SelectedItem = i;

                i.Text = null;
                foreach (ComboItem items in PKME_Tabs.CB_Move4.Items)
                {
                    if (items.Text.Equals(move4))
                    {
                        if (i.Text != null)
                        {
                            if (i.Text.Length < items.Text.Length)
                            {
                                i = items;
                            }
                        }
                        else
                        {
                            i = items;
                        }
                    }
                }
                PKME_Tabs.CB_Move4.SelectedItem = i;

                // Make PP Max
                PKME_Tabs.CB_PPu1.SelectedIndex = 3;
                PKME_Tabs.CB_PPu2.SelectedIndex = 3;
                PKME_Tabs.CB_PPu3.SelectedIndex = 3;
                PKME_Tabs.CB_PPu4.SelectedIndex = 3;

                // IVs
                PKME_Tabs.TB_HPIV.Text = "31";
                PKME_Tabs.TB_ATKIV.Text = "31";
                PKME_Tabs.TB_DEFIV.Text = "31";
                PKME_Tabs.TB_SPAIV.Text = "31";
                PKME_Tabs.TB_SPDIV.Text = "31";
                PKME_Tabs.TB_SPEIV.Text = "31";

                if (hptype != null)
                {
                    if (hptype.Contains("Fighting"))
                    {
                        PKME_Tabs.TB_DEFIV.Text = "30";
                        PKME_Tabs.TB_SPAIV.Text = "30";
                        PKME_Tabs.TB_SPDIV.Text = "30";
                        PKME_Tabs.TB_SPEIV.Text = "30";
                    }
                    else if (hptype.Contains("Flying"))
                    {
                        PKME_Tabs.TB_SPAIV.Text = "30";
                        PKME_Tabs.TB_SPDIV.Text = "30";
                        PKME_Tabs.TB_HPIV.Text = "30";
                        PKME_Tabs.TB_ATKIV.Text = "30";
                        PKME_Tabs.TB_DEFIV.Text = "30";
                    }
                    else if (hptype.Contains("Poison"))
                    {
                        PKME_Tabs.TB_DEFIV.Text = "30";
                        PKME_Tabs.TB_SPDIV.Text = "30";
                        PKME_Tabs.TB_SPAIV.Text = "30";
                    }
                    else if (hptype.Contains("Ground"))
                    {
                        PKME_Tabs.TB_SPDIV.Text = "30";
                        PKME_Tabs.TB_SPAIV.Text = "30";
                    }
                    else if (hptype.Contains("Rock"))
                    {
                        PKME_Tabs.TB_SPDIV.Text = "30";
                        PKME_Tabs.TB_SPEIV.Text = "30";
                        PKME_Tabs.TB_DEFIV.Text = "30";
                    }
                    else if (hptype.Contains("Bug"))
                    {
                        PKME_Tabs.TB_SPDIV.Text = "30";
                        PKME_Tabs.TB_ATKIV.Text = "30";
                        PKME_Tabs.TB_DEFIV.Text = "30";
                    }
                    else if (hptype.Contains("Ghost"))
                    {
                        PKME_Tabs.TB_DEFIV.Text = "30";
                        PKME_Tabs.TB_SPDIV.Text = "30";
                    }
                    else if (hptype.Contains("Steel"))
                    {
                        PKME_Tabs.TB_SPDIV.Text = "30";
                    }
                    else if (hptype.Contains("Fire"))
                    {
                        PKME_Tabs.TB_SPAIV.Text = "30";
                        PKME_Tabs.TB_SPEIV.Text = "30";
                        PKME_Tabs.TB_ATKIV.Text = "30";
                    }
                    else if (hptype.Contains("Water"))
                    {
                        PKME_Tabs.TB_SPAIV.Text = "30";
                        PKME_Tabs.TB_ATKIV.Text = "30";
                        PKME_Tabs.TB_DEFIV.Text = "30";
                    }
                    else if (hptype.Contains("Grass"))
                    {
                        PKME_Tabs.TB_SPAIV.Text = "30";
                        PKME_Tabs.TB_HPIV.Text = "30";
                    }
                    else if (hptype.Contains("Electric"))
                    {
                        PKME_Tabs.TB_SPAIV.Text = "30";
                    }
                    else if (hptype.Contains("Psychic"))
                    {
                        PKME_Tabs.TB_SPEIV.Text = "30";
                        PKME_Tabs.TB_ATKIV.Text = "30";
                    }
                    else if (hptype.Contains("Ice"))
                    {
                        PKME_Tabs.TB_DEFIV.Text = "30";
                        PKME_Tabs.TB_ATKIV.Text = "30";
                    }
                    else if (hptype.Contains("Dragon"))
                    {
                        PKME_Tabs.TB_DEFIV.Text = "30";
                    }
                }

                if (int.Parse(PKME_Tabs.TB_Level.Text) == 100)
                {
                    PKME_Tabs.pkm.HT_HP = (PKME_Tabs.TB_HPIV.Text != "31");
                    PKME_Tabs.UpdateIVs(PKME_Tabs.TB_HPIV, e);
                    PKME_Tabs.pkm.HT_ATK = (PKME_Tabs.TB_ATKIV.Text != "31");
                    PKME_Tabs.UpdateIVs(PKME_Tabs.TB_ATKIV, e);
                    PKME_Tabs.pkm.HT_DEF = (PKME_Tabs.TB_DEFIV.Text != "31");
                    PKME_Tabs.UpdateIVs(PKME_Tabs.TB_DEFIV, e);
                    PKME_Tabs.pkm.HT_SPA = (PKME_Tabs.TB_SPAIV.Text != "31");
                    PKME_Tabs.UpdateIVs(PKME_Tabs.TB_SPAIV, e);
                    PKME_Tabs.pkm.HT_SPD = (PKME_Tabs.TB_SPDIV.Text != "31");
                    PKME_Tabs.UpdateIVs(PKME_Tabs.TB_SPDIV, e);
                    PKME_Tabs.pkm.HT_SPE = (PKME_Tabs.TB_SPEIV.Text != "31");
                    PKME_Tabs.UpdateIVs(PKME_Tabs.TB_SPEIV, e);

                    if (p.ivs != null)
                    {
                        string[] ivs = p.ivs.Split('/');

                        foreach (string iv in ivs)
                        {
                            if (iv.Contains("HP"))
                            {
                                PKME_Tabs.pkm.HT_HP = false;
                                PKME_Tabs.UpdateIVs(PKME_Tabs.TB_HPIV, e);
                                PKME_Tabs.TB_HPIV.Text = iv.Substring(0, iv.IndexOf("HP")).Trim();
                            }
                            else if (iv.Contains("Atk"))
                            {
                                PKME_Tabs.pkm.HT_ATK = false;
                                PKME_Tabs.UpdateIVs(PKME_Tabs.TB_ATKIV, e);
                                PKME_Tabs.TB_ATKIV.Text = iv.Substring(0, iv.IndexOf("Atk")).Trim();
                            }
                            else if (iv.Contains("Def"))
                            {
                                PKME_Tabs.pkm.HT_DEF = false;
                                PKME_Tabs.UpdateIVs(PKME_Tabs.TB_DEFIV, e);
                                PKME_Tabs.TB_DEFIV.Text = iv.Substring(0, iv.IndexOf("Def")).Trim();
                            }
                            else if (iv.Contains("SpA"))
                            {
                                PKME_Tabs.pkm.HT_SPA = false;
                                PKME_Tabs.UpdateIVs(PKME_Tabs.TB_SPAIV, e);
                                PKME_Tabs.TB_SPAIV.Text = iv.Substring(0, iv.IndexOf("SpA")).Trim();
                            }
                            else if (iv.Contains("SpD"))
                            {
                                PKME_Tabs.pkm.HT_SPD = false;
                                PKME_Tabs.UpdateIVs(PKME_Tabs.TB_SPDIV, e);
                                PKME_Tabs.TB_SPDIV.Text = iv.Substring(0, iv.IndexOf("SpD")).Trim();
                            }
                            else if (iv.Contains("Spe"))
                            {
                                PKME_Tabs.pkm.HT_SPE = false;
                                PKME_Tabs.UpdateIVs(PKME_Tabs.TB_SPEIV, e);
                                PKME_Tabs.TB_SPEIV.Text = iv.Substring(0, iv.IndexOf("Spe")).Trim();
                            }
                        }

                    }
                }
                else
                {
                    if (p.ivs != null)
                    {
                        string[] ivs = p.ivs.Split('/');

                        foreach (string iv in ivs)
                        {
                            if (iv.Contains("HP"))
                            {
                                PKME_Tabs.TB_HPIV.Text = iv.Substring(0, iv.IndexOf("HP")).Trim();
                            }
                            else if (iv.Contains("Atk"))
                            {
                                PKME_Tabs.TB_ATKIV.Text = iv.Substring(0, iv.IndexOf("Atk")).Trim();
                            }
                            else if (iv.Contains("Def"))
                            {
                                PKME_Tabs.TB_DEFIV.Text = iv.Substring(0, iv.IndexOf("Def")).Trim();
                            }
                            else if (iv.Contains("SpA"))
                            {
                                PKME_Tabs.TB_SPAIV.Text = iv.Substring(0, iv.IndexOf("SpA")).Trim();
                            }
                            else if (iv.Contains("SpD"))
                            {
                                PKME_Tabs.TB_SPDIV.Text = iv.Substring(0, iv.IndexOf("SpD")).Trim();
                            }
                            else if (iv.Contains("Spe"))
                            {
                                PKME_Tabs.TB_SPEIV.Text = iv.Substring(0, iv.IndexOf("Spe")).Trim();
                            }
                        }

                    }
                }

                // EVs
                PKME_Tabs.TB_HPEV.Text = "0";
                PKME_Tabs.TB_ATKEV.Text = "0";
                PKME_Tabs.TB_DEFEV.Text = "0";
                PKME_Tabs.TB_SPAEV.Text = "0";
                PKME_Tabs.TB_SPDEV.Text = "0";
                PKME_Tabs.TB_SPEEV.Text = "0";

                foreach (string ev in evs)
                {
                    if (ev.Contains("HP"))
                    {
                        PKME_Tabs.TB_HPEV.Text = ev.Substring(0, ev.IndexOf("HP")).Trim();
                    }
                    else if (ev.Contains("Atk"))
                    {
                        PKME_Tabs.TB_ATKEV.Text = ev.Substring(0, ev.IndexOf("Atk")).Trim();
                    }
                    else if (ev.Contains("Def"))
                    {
                        PKME_Tabs.TB_DEFEV.Text = ev.Substring(0, ev.IndexOf("Def")).Trim();
                    }
                    else if (ev.Contains("SpA"))
                    {
                        PKME_Tabs.TB_SPAEV.Text = ev.Substring(0, ev.IndexOf("SpA")).Trim();
                    }
                    else if (ev.Contains("SpD"))
                    {
                        PKME_Tabs.TB_SPDEV.Text = ev.Substring(0, ev.IndexOf("SpD")).Trim();
                    }
                    else if (ev.Contains("Spe"))
                    {
                        PKME_Tabs.TB_SPEEV.Text = ev.Substring(0, ev.IndexOf("Spe")).Trim();
                    }
                }


                if (p.gender != null)
                {
                    if (p.gender == "m")
                    {
                        PKME_Tabs.Label_Gender.Text = PKME_Tabs.gendersymbols[0];
                    }
                    else
                    {
                        PKME_Tabs.Label_Gender.Text = PKME_Tabs.gendersymbols[1];
                    }
                }

                if (p.happiness != null)
                {
                    PKME_Tabs.TB_Friendship.Text = p.happiness;
                }
                else
                {
                    PKME_Tabs.TB_Friendship.Text = "255";
                }

                if (p.nickname != null)
                {
                    PKME_Tabs.CHK_Nicknamed.Checked = true;
                    PKME_Tabs.TB_Nickname.Text = p.nickname;
                }

                PKME_Tabs.ClickMoves(PKME_Tabs.GB_RelearnMoves, null);

                if ((!poke.Contains("Genesect") && !(move1.Contains("Extreme Speed") || move2.Contains("Extreme Speed") || move3.Contains("Extreme Speed") || move4.Contains("Extreme Speed"))) && (!poke.Contains("Entei") && !(move1.Contains("Extreme Speed") || move2.Contains("Extreme Speed") || move3.Contains("Extreme Speed") || move4.Contains("Extreme Speed"))) && (!poke.Contains("Raikou") && !(move1.Contains("Aura Sphere") || move2.Contains("Aura Sphere") || move3.Contains("Aura Sphere") || move4.Contains("Aura Sphere"))))
                {
                    PKME_Tabs.UpdateRandomPID(PKME_Tabs.BTN_RerollPID, null);
                }
                if (p.shiny)
                {
                    PKME_Tabs.UpdateShinyPID(null, null);
                }

                if (singleFiles)
                {
                    mainMenuSave(p);
                }
                else if ((!singleFiles && !savExtraction) || savExtraction)
                {
                    ContextMenuSAV.ClickSet(pba[count], null);
                    ContextMenuSAV.ClickView(pba[count], null);
                    ContextMenuSAV.ClickSet(pba[count], null);
                }
                count++;
            }

            if (!singleFiles && !savExtraction)
            {
                if (!extractPath.Trim().EndsWith("\\") && !extractPath.Trim().EndsWith("/"))
                {
                    extractPath = extractPath + "\\";
                }
                File.WriteAllBytes(extractPath + "boxdata.bin", C_SAV.SAV.GetBoxBinary(C_SAV.Box.CB_BoxSelect.SelectedIndex));
            }
            else if (savExtraction)
            {
                clickExportSAVCustomized(cyberSavPath);
            }

            this.Close();
        }

        private bool isPokemonGen7(string poke)
        {
            ComboItem i = new ComboItem();
            i.Text = null;
            foreach (ComboItem items in PKME_Tabs.CB_Species.Items)
            {
                if (poke.Contains(items.Text))
                {
                    if (i.Text != null)
                    {
                        if (i.Text.Length < items.Text.Length)
                        {
                            i = items;
                        }
                    }
                    else
                    {
                        i = items;
                    }
                }
            }
            PKME_Tabs.CB_Species.SelectedItem = i;
            int index = WinFormsUtil.GetIndex(PKME_Tabs.CB_Species);
            if (index >= 722)
            {
                return true;
            }
            else
            {
                if (poke.ToLower().Contains("-alola"))
                {
                    if (index == 19 || index == 20
                        || index == 52 || index == 53
                        || index == 88 || index == 89
                        || index == 50 || index == 51
                        || index == 74 || index == 75 || index == 76
                        || index == 27 || index == 28
                        || index == 37 || index == 38)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        private void mainMenuSave(Pokemon p)
        {
            string pokemon = p.name;
            string item = p.item;
            string ability = p.ability;
            string nature = p.nature;
            string move1 = (p.moves.Count > 0) ? p.moves[0] : "(None)";
            string move2 = (p.moves.Count > 1) ? p.moves[1] : "(None)";
            string move3 = (p.moves.Count > 2) ? p.moves[2] : "(None)";
            string move4 = (p.moves.Count > 3) ? p.moves[3] : "(None)";

            PKM pkm = PreparePKM();

            string fileName = PKME_Tabs.TB_Nickname.Text + " - " + PKME_Tabs.TB_PID.Text;

            if (!extractPath.Trim().EndsWith("\\") && !extractPath.Trim().EndsWith("/"))
            {
                extractPath = extractPath + "\\";
            }

            string path = extractPath + pokemon + "_" + item + "_" + ability + "_" + nature + "_" + move1 + "_" + move2 + "_" + move3 + "_" + move4 + ".ekx";
            //Debug Help
            Console.WriteLine(pokemon + "_" + item + "_" + ability + "_" + nature + "_" + move1 + "_" + move2 + "_" + move3 + "_" + move4);

            string ext = ".ekx";

            File.WriteAllBytes(path, pkm.EncryptedPartyData);
        }

        private void mainMenuOpen(string s)
        {
            string path = s;
            openQuick(path);
        }

        private void clickExportSAVCustomized(string path)
        {
            if (C_SAV.SAV.HasBox)
                C_SAV.SAV.CurrentBox = C_SAV.Box.CB_BoxSelect.SelectedIndex;

            if (File.Exists(path) && bak)
            {
                // File already exists, save a .bak
                byte[] backupfile = File.ReadAllBytes(path);
                File.WriteAllBytes(path + ".bak", backupfile);
            }
            bool dsv = Path.GetExtension(path)?.ToLower() == ".dsv";
            bool gci = Path.GetExtension(path)?.ToLower() == ".gci";
            File.WriteAllBytes(path, C_SAV.SAV.Write(dsv, gci));
            C_SAV.SAV.Edited = false;
        }

        // Main Menu Subfunctions
        private void openQuick(string path, bool force = false)
        {
            // detect if it is a folder (load into boxes or not)
            if (Directory.Exists(path))
            { C_SAV.LoadBoxes(out string _, path); return; }

            string ext = Path.GetExtension(path);
            FileInfo fi = new FileInfo(path);
            if (!fi.Exists)
                return;
            if (fi.Length > 0x10009C && fi.Length != 0x380000 && !SAV3GCMemoryCard.IsMemoryCardSize(fi.Length))
                WinFormsUtil.Error("Input file is too large." + Environment.NewLine + $"Size: {fi.Length} bytes", path);
            else if (fi.Length < 32)
                WinFormsUtil.Error("Input file is too small." + Environment.NewLine + $"Size: {fi.Length} bytes", path);
            else
            {
                byte[] input; try { input = File.ReadAllBytes(path); }
                catch (Exception e) { WinFormsUtil.Error("Unable to load file.  It could be in use by another program.\nPath: " + path, e); return; }

#if DEBUG
                OpenFile(input, path, ext, C_SAV.SAV);
#else
                try { OpenFile(input, path, ext, C_SAV.SAV); }
                catch (Exception e) { WinFormsUtil.Error("Unable to load file.\nPath: " + path, e); }
#endif
            }
        }
    }
}
