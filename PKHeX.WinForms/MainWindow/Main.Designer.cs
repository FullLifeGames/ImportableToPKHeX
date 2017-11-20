﻿namespace PKHeX.WinForms
{
    partial class Main
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        
        public void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.Menu_File = new System.Windows.Forms.ToolStripMenuItem();
            this.Menu_Open = new System.Windows.Forms.ToolStripMenuItem();
            this.Menu_Save = new System.Windows.Forms.ToolStripMenuItem();
            this.Menu_ExportSAV = new System.Windows.Forms.ToolStripMenuItem();
            this.Menu_ExportMAIN = new System.Windows.Forms.ToolStripMenuItem();
            this.Menu_ExportBAK = new System.Windows.Forms.ToolStripMenuItem();
            this.Menu_Exit = new System.Windows.Forms.ToolStripMenuItem();
            this.Menu_Tools = new System.Windows.Forms.ToolStripMenuItem();
            this.Menu_Showdown = new System.Windows.Forms.ToolStripMenuItem();
            this.Menu_ShowdownImportPKM = new System.Windows.Forms.ToolStripMenuItem();
            this.Menu_ShowdownExportPKM = new System.Windows.Forms.ToolStripMenuItem();
            this.Menu_ShowdownExportParty = new System.Windows.Forms.ToolStripMenuItem();
            this.Menu_ShowdownExportBattleBox = new System.Windows.Forms.ToolStripMenuItem();
            this.Menu_Data = new System.Windows.Forms.ToolStripMenuItem();
            this.Menu_LoadBoxes = new System.Windows.Forms.ToolStripMenuItem();
            this.Menu_DumpBoxes = new System.Windows.Forms.ToolStripMenuItem();
            this.Menu_DumpBox = new System.Windows.Forms.ToolStripMenuItem();
            this.Menu_Report = new System.Windows.Forms.ToolStripMenuItem();
            this.Menu_Database = new System.Windows.Forms.ToolStripMenuItem();
            this.Menu_MGDatabase = new System.Windows.Forms.ToolStripMenuItem();
            this.Menu_BatchEditor = new System.Windows.Forms.ToolStripMenuItem();
            this.Menu_Folder = new System.Windows.Forms.ToolStripMenuItem();
            this.Menu_Options = new System.Windows.Forms.ToolStripMenuItem();
            this.Menu_Language = new System.Windows.Forms.ToolStripMenuItem();
            this.CB_MainLanguage = new System.Windows.Forms.ToolStripComboBox();
            this.Menu_Modify = new System.Windows.Forms.ToolStripMenuItem();
            this.Menu_ModifyDex = new System.Windows.Forms.ToolStripMenuItem();
            this.Menu_ModifyPKM = new System.Windows.Forms.ToolStripMenuItem();
            this.Menu_FlagIllegal = new System.Windows.Forms.ToolStripMenuItem();
            this.Menu_ModifyUnset = new System.Windows.Forms.ToolStripMenuItem();
            this.Menu_Undo = new System.Windows.Forms.ToolStripMenuItem();
            this.Menu_Redo = new System.Windows.Forms.ToolStripMenuItem();
            this.Menu_Unicode = new System.Windows.Forms.ToolStripMenuItem();
            this.Menu_About = new System.Windows.Forms.ToolStripMenuItem();
            this.dragout = new System.Windows.Forms.PictureBox();
            this.PB_Legal = new System.Windows.Forms.PictureBox();
            this.L_UpdateAvailable = new System.Windows.Forms.LinkLabel();
            this.PKME_Tabs = new PKHeX.WinForms.Controls.PKMEditor();
            this.C_SAV = new PKHeX.WinForms.Controls.SAVEditor();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dragout)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.PB_Legal)).BeginInit();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.BackColor = System.Drawing.Color.Transparent;
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Menu_File,
            this.Menu_Tools,
            this.Menu_Options});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(614, 24);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // Menu_File
            // 
            this.Menu_File.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Menu_Open,
            this.Menu_Save,
            this.Menu_ExportSAV,
            this.Menu_Exit});
            this.Menu_File.Name = "Menu_File";
            this.Menu_File.Size = new System.Drawing.Size(37, 20);
            this.Menu_File.Text = "File";
            // 
            // Menu_Open
            // 
            this.Menu_Open.Image = ((System.Drawing.Image)(resources.GetObject("Menu_Open.Image")));
            this.Menu_Open.Name = "Menu_Open";
            this.Menu_Open.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.Menu_Open.ShowShortcutKeys = false;
            this.Menu_Open.Size = new System.Drawing.Size(139, 22);
            this.Menu_Open.Text = "&Open...";
            this.Menu_Open.Click += new System.EventHandler(this.MainMenuOpen);
            // 
            // Menu_Save
            // 
            this.Menu_Save.Image = ((System.Drawing.Image)(resources.GetObject("Menu_Save.Image")));
            this.Menu_Save.Name = "Menu_Save";
            this.Menu_Save.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.Menu_Save.ShowShortcutKeys = false;
            this.Menu_Save.Size = new System.Drawing.Size(139, 22);
            this.Menu_Save.Text = "&Save PK6...";
            this.Menu_Save.Click += new System.EventHandler(this.MainMenuSave);
            // 
            // Menu_ExportSAV
            // 
            this.Menu_ExportSAV.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Menu_ExportMAIN,
            this.Menu_ExportBAK});
            this.Menu_ExportSAV.Enabled = false;
            this.Menu_ExportSAV.Image = ((System.Drawing.Image)(resources.GetObject("Menu_ExportSAV.Image")));
            this.Menu_ExportSAV.Name = "Menu_ExportSAV";
            this.Menu_ExportSAV.Size = new System.Drawing.Size(139, 22);
            this.Menu_ExportSAV.Text = "&Export SAV...";
            // 
            // Menu_ExportMAIN
            // 
            this.Menu_ExportMAIN.Image = ((System.Drawing.Image)(resources.GetObject("Menu_ExportMAIN.Image")));
            this.Menu_ExportMAIN.Name = "Menu_ExportMAIN";
            this.Menu_ExportMAIN.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.E)));
            this.Menu_ExportMAIN.ShowShortcutKeys = false;
            this.Menu_ExportMAIN.Size = new System.Drawing.Size(142, 22);
            this.Menu_ExportMAIN.Text = "&Export main";
            this.Menu_ExportMAIN.Click += new System.EventHandler(this.ClickExportSAV);
            // 
            // Menu_ExportBAK
            // 
            this.Menu_ExportBAK.Image = ((System.Drawing.Image)(resources.GetObject("Menu_ExportBAK.Image")));
            this.Menu_ExportBAK.Name = "Menu_ExportBAK";
            this.Menu_ExportBAK.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.B)));
            this.Menu_ExportBAK.ShowShortcutKeys = false;
            this.Menu_ExportBAK.Size = new System.Drawing.Size(142, 22);
            this.Menu_ExportBAK.Text = "Export &Backup";
            this.Menu_ExportBAK.Click += new System.EventHandler(this.ClickExportSAVBAK);
            // 
            // Menu_Exit
            // 
            this.Menu_Exit.Image = ((System.Drawing.Image)(resources.GetObject("Menu_Exit.Image")));
            this.Menu_Exit.Name = "Menu_Exit";
            this.Menu_Exit.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Q)));
            this.Menu_Exit.ShowShortcutKeys = false;
            this.Menu_Exit.Size = new System.Drawing.Size(139, 22);
            this.Menu_Exit.Text = "&Quit";
            this.Menu_Exit.Click += new System.EventHandler(this.MainMenuExit);
            // 
            // Menu_Tools
            // 
            this.Menu_Tools.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Menu_Showdown,
            this.Menu_Data,
            this.Menu_Folder});
            this.Menu_Tools.Name = "Menu_Tools";
            this.Menu_Tools.Size = new System.Drawing.Size(47, 20);
            this.Menu_Tools.Text = "Tools";
            // 
            // Menu_Showdown
            // 
            this.Menu_Showdown.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Menu_ShowdownImportPKM,
            this.Menu_ShowdownExportPKM,
            this.Menu_ShowdownExportParty,
            this.Menu_ShowdownExportBattleBox});
            this.Menu_Showdown.Image = ((System.Drawing.Image)(resources.GetObject("Menu_Showdown.Image")));
            this.Menu_Showdown.Name = "Menu_Showdown";
            this.Menu_Showdown.Size = new System.Drawing.Size(133, 22);
            this.Menu_Showdown.Text = "Showdown";
            // 
            // Menu_ShowdownImportPKM
            // 
            this.Menu_ShowdownImportPKM.Image = ((System.Drawing.Image)(resources.GetObject("Menu_ShowdownImportPKM.Image")));
            this.Menu_ShowdownImportPKM.Name = "Menu_ShowdownImportPKM";
            this.Menu_ShowdownImportPKM.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.T)));
            this.Menu_ShowdownImportPKM.ShowShortcutKeys = false;
            this.Menu_ShowdownImportPKM.Size = new System.Drawing.Size(231, 22);
            this.Menu_ShowdownImportPKM.Text = "Import Set from Clipboard";
            this.Menu_ShowdownImportPKM.Click += new System.EventHandler(this.ClickShowdownImportPKM);
            // 
            // Menu_ShowdownExportPKM
            // 
            this.Menu_ShowdownExportPKM.Image = ((System.Drawing.Image)(resources.GetObject("Menu_ShowdownExportPKM.Image")));
            this.Menu_ShowdownExportPKM.Name = "Menu_ShowdownExportPKM";
            this.Menu_ShowdownExportPKM.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.T)));
            this.Menu_ShowdownExportPKM.ShowShortcutKeys = false;
            this.Menu_ShowdownExportPKM.Size = new System.Drawing.Size(231, 22);
            this.Menu_ShowdownExportPKM.Text = "Export Set to Clipboard";
            this.Menu_ShowdownExportPKM.Click += new System.EventHandler(this.ClickShowdownExportPKM);
            // 
            // Menu_ShowdownExportParty
            // 
            this.Menu_ShowdownExportParty.Image = ((System.Drawing.Image)(resources.GetObject("Menu_ShowdownExportParty.Image")));
            this.Menu_ShowdownExportParty.Name = "Menu_ShowdownExportParty";
            this.Menu_ShowdownExportParty.Size = new System.Drawing.Size(231, 22);
            this.Menu_ShowdownExportParty.Text = "Export Party to Clipboard";
            this.Menu_ShowdownExportParty.Click += new System.EventHandler(this.ClickShowdownExportParty);
            // 
            // Menu_ShowdownExportBattleBox
            // 
            this.Menu_ShowdownExportBattleBox.Image = ((System.Drawing.Image)(resources.GetObject("Menu_ShowdownExportBattleBox.Image")));
            this.Menu_ShowdownExportBattleBox.Name = "Menu_ShowdownExportBattleBox";
            this.Menu_ShowdownExportBattleBox.Size = new System.Drawing.Size(231, 22);
            this.Menu_ShowdownExportBattleBox.Text = "Export Battle Box to Clipboard";
            this.Menu_ShowdownExportBattleBox.Click += new System.EventHandler(this.ClickShowdownExportBattleBox);
            // 
            // Menu_Data
            // 
            this.Menu_Data.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Menu_LoadBoxes,
            this.Menu_DumpBoxes,
            this.Menu_DumpBox,
            this.Menu_Report,
            this.Menu_Database,
            this.Menu_MGDatabase,
            this.Menu_BatchEditor});
            this.Menu_Data.Image = ((System.Drawing.Image)(resources.GetObject("Menu_Data.Image")));
            this.Menu_Data.Name = "Menu_Data";
            this.Menu_Data.Size = new System.Drawing.Size(133, 22);
            this.Menu_Data.Text = "Data";
            // 
            // Menu_LoadBoxes
            // 
            this.Menu_LoadBoxes.Image = ((System.Drawing.Image)(resources.GetObject("Menu_LoadBoxes.Image")));
            this.Menu_LoadBoxes.Name = "Menu_LoadBoxes";
            this.Menu_LoadBoxes.Size = new System.Drawing.Size(182, 22);
            this.Menu_LoadBoxes.Text = "Load Boxes";
            this.Menu_LoadBoxes.Click += new System.EventHandler(this.MainMenuBoxLoad);
            // 
            // Menu_DumpBoxes
            // 
            this.Menu_DumpBoxes.Image = ((System.Drawing.Image)(resources.GetObject("Menu_DumpBoxes.Image")));
            this.Menu_DumpBoxes.Name = "Menu_DumpBoxes";
            this.Menu_DumpBoxes.Size = new System.Drawing.Size(182, 22);
            this.Menu_DumpBoxes.Text = "Dump Boxes";
            this.Menu_DumpBoxes.Click += new System.EventHandler(this.MainMenuBoxDump);
            // 
            // Menu_DumpBox
            // 
            this.Menu_DumpBox.Image = ((System.Drawing.Image)(resources.GetObject("Menu_DumpBox.Image")));
            this.Menu_DumpBox.Name = "Menu_DumpBox";
            this.Menu_DumpBox.Size = new System.Drawing.Size(182, 22);
            this.Menu_DumpBox.Text = "Dump Box";
            this.Menu_DumpBox.Click += new System.EventHandler(this.MainMenuBoxDumpSingle);
            // 
            // Menu_Report
            // 
            this.Menu_Report.Image = ((System.Drawing.Image)(resources.GetObject("Menu_Report.Image")));
            this.Menu_Report.Name = "Menu_Report";
            this.Menu_Report.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.R)));
            this.Menu_Report.ShowShortcutKeys = false;
            this.Menu_Report.Size = new System.Drawing.Size(182, 22);
            this.Menu_Report.Text = "Box Data &Report";
            this.Menu_Report.Click += new System.EventHandler(this.MainMenuBoxReport);
            // 
            // Menu_Database
            // 
            this.Menu_Database.Image = ((System.Drawing.Image)(resources.GetObject("Menu_Database.Image")));
            this.Menu_Database.Name = "Menu_Database";
            this.Menu_Database.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.D)));
            this.Menu_Database.ShowShortcutKeys = false;
            this.Menu_Database.Size = new System.Drawing.Size(182, 22);
            this.Menu_Database.Text = "PKM &Database";
            this.Menu_Database.Click += new System.EventHandler(this.MainMenuDatabase);
            // 
            // Menu_MGDatabase
            // 
            this.Menu_MGDatabase.Image = ((System.Drawing.Image)(resources.GetObject("Menu_MGDatabase.Image")));
            this.Menu_MGDatabase.Name = "Menu_MGDatabase";
            this.Menu_MGDatabase.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.G)));
            this.Menu_MGDatabase.ShowShortcutKeys = false;
            this.Menu_MGDatabase.Size = new System.Drawing.Size(182, 22);
            this.Menu_MGDatabase.Text = "&Mystery Gift Database";
            this.Menu_MGDatabase.Click += new System.EventHandler(this.MainMenuMysteryDB);
            // 
            // Menu_BatchEditor
            // 
            this.Menu_BatchEditor.Image = ((System.Drawing.Image)(resources.GetObject("Menu_BatchEditor.Image")));
            this.Menu_BatchEditor.Name = "Menu_BatchEditor";
            this.Menu_BatchEditor.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.M)));
            this.Menu_BatchEditor.ShowShortcutKeys = false;
            this.Menu_BatchEditor.Size = new System.Drawing.Size(182, 22);
            this.Menu_BatchEditor.Text = "Batch Editor";
            this.Menu_BatchEditor.Click += new System.EventHandler(this.MainMenuBatchEditor);
            // 
            // Menu_Folder
            // 
            this.Menu_Folder.Image = global::PKHeX.WinForms.Properties.Resources.folder;
            this.Menu_Folder.Name = "Menu_Folder";
            this.Menu_Folder.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F)));
            this.Menu_Folder.ShowShortcutKeys = false;
            this.Menu_Folder.Size = new System.Drawing.Size(133, 22);
            this.Menu_Folder.Text = "Open Folder";
            this.Menu_Folder.Click += new System.EventHandler(this.MainMenuFolder);
            // 
            // Menu_Options
            // 
            this.Menu_Options.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Menu_Language,
            this.Menu_Modify,
            this.Menu_Unicode,
            this.Menu_About});
            this.Menu_Options.Name = "Menu_Options";
            this.Menu_Options.Size = new System.Drawing.Size(61, 20);
            this.Menu_Options.Text = "Options";
            // 
            // Menu_Language
            // 
            this.Menu_Language.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.CB_MainLanguage});
            this.Menu_Language.Image = ((System.Drawing.Image)(resources.GetObject("Menu_Language.Image")));
            this.Menu_Language.Name = "Menu_Language";
            this.Menu_Language.Size = new System.Drawing.Size(139, 22);
            this.Menu_Language.Text = "Language";
            // 
            // CB_MainLanguage
            // 
            this.CB_MainLanguage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CB_MainLanguage.Name = "CB_MainLanguage";
            this.CB_MainLanguage.Size = new System.Drawing.Size(121, 23);
            this.CB_MainLanguage.SelectedIndexChanged += new System.EventHandler(this.ChangeMainLanguage);
            // 
            // Menu_Modify
            // 
            this.Menu_Modify.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Menu_ModifyDex,
            this.Menu_ModifyPKM,
            this.Menu_FlagIllegal,
            this.Menu_ModifyUnset,
            this.Menu_Undo,
            this.Menu_Redo});
            this.Menu_Modify.Image = ((System.Drawing.Image)(resources.GetObject("Menu_Modify.Image")));
            this.Menu_Modify.Name = "Menu_Modify";
            this.Menu_Modify.Size = new System.Drawing.Size(139, 22);
            this.Menu_Modify.Text = "Set to SAV";
            // 
            // Menu_ModifyDex
            // 
            this.Menu_ModifyDex.Checked = true;
            this.Menu_ModifyDex.CheckOnClick = true;
            this.Menu_ModifyDex.CheckState = System.Windows.Forms.CheckState.Checked;
            this.Menu_ModifyDex.Name = "Menu_ModifyDex";
            this.Menu_ModifyDex.Size = new System.Drawing.Size(189, 22);
            this.Menu_ModifyDex.Text = "Modify Pokédex";
            this.Menu_ModifyDex.Click += new System.EventHandler(this.MainMenuModifyDex);
            // 
            // Menu_ModifyPKM
            // 
            this.Menu_ModifyPKM.Checked = true;
            this.Menu_ModifyPKM.CheckOnClick = true;
            this.Menu_ModifyPKM.CheckState = System.Windows.Forms.CheckState.Checked;
            this.Menu_ModifyPKM.Name = "Menu_ModifyPKM";
            this.Menu_ModifyPKM.Size = new System.Drawing.Size(189, 22);
            this.Menu_ModifyPKM.Text = "Modify PKM Info";
            this.Menu_ModifyPKM.Click += new System.EventHandler(this.MainMenuModifyPKM);
            // 
            // Menu_FlagIllegal
            // 
            this.Menu_FlagIllegal.CheckOnClick = true;
            this.Menu_FlagIllegal.Name = "Menu_FlagIllegal";
            this.Menu_FlagIllegal.Size = new System.Drawing.Size(189, 22);
            this.Menu_FlagIllegal.Text = "Flag Legality";
            this.Menu_FlagIllegal.Click += new System.EventHandler(this.MainMenuFlagIllegal);
            // 
            // Menu_ModifyUnset
            // 
            this.Menu_ModifyUnset.CheckOnClick = true;
            this.Menu_ModifyUnset.Name = "Menu_ModifyUnset";
            this.Menu_ModifyUnset.Size = new System.Drawing.Size(189, 22);
            this.Menu_ModifyUnset.Text = "Notify Unset Changes";
            this.Menu_ModifyUnset.Click += new System.EventHandler(this.MainMenuModifyUnset);
            // 
            // Menu_Undo
            // 
            this.Menu_Undo.Enabled = false;
            this.Menu_Undo.Name = "Menu_Undo";
            this.Menu_Undo.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.U)));
            this.Menu_Undo.ShowShortcutKeys = false;
            this.Menu_Undo.Size = new System.Drawing.Size(189, 22);
            this.Menu_Undo.Text = "Undo Last Change";
            this.Menu_Undo.Click += new System.EventHandler(this.ClickUndo);
            // 
            // Menu_Redo
            // 
            this.Menu_Redo.Enabled = false;
            this.Menu_Redo.Name = "Menu_Redo";
            this.Menu_Redo.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Y)));
            this.Menu_Redo.ShowShortcutKeys = false;
            this.Menu_Redo.Size = new System.Drawing.Size(189, 22);
            this.Menu_Redo.Text = "Redo Last Change";
            this.Menu_Redo.Click += new System.EventHandler(this.ClickRedo);
            // 
            // Menu_Unicode
            // 
            this.Menu_Unicode.Checked = true;
            this.Menu_Unicode.CheckOnClick = true;
            this.Menu_Unicode.CheckState = System.Windows.Forms.CheckState.Checked;
            this.Menu_Unicode.Name = "Menu_Unicode";
            this.Menu_Unicode.Size = new System.Drawing.Size(139, 22);
            this.Menu_Unicode.Text = "Unicode";
            this.Menu_Unicode.Click += new System.EventHandler(this.MainMenuUnicode);
            // 
            // Menu_About
            // 
            this.Menu_About.Image = ((System.Drawing.Image)(resources.GetObject("Menu_About.Image")));
            this.Menu_About.Name = "Menu_About";
            this.Menu_About.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.P)));
            this.Menu_About.ShowShortcutKeys = false;
            this.Menu_About.Size = new System.Drawing.Size(139, 22);
            this.Menu_About.Text = "About &PKHeX";
            this.Menu_About.Click += new System.EventHandler(this.MainMenuAbout);
            // 
            // dragout
            // 
            this.dragout.BackColor = System.Drawing.Color.Transparent;
            this.dragout.Location = new System.Drawing.Point(248, 0);
            this.dragout.Name = "dragout";
            this.dragout.Size = new System.Drawing.Size(40, 30);
            this.dragout.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.dragout.TabIndex = 60;
            this.dragout.TabStop = false;
            this.dragout.DragDrop += new System.Windows.Forms.DragEventHandler(this.DragoutDrop);
            this.dragout.DragOver += new System.Windows.Forms.DragEventHandler(Main.Dragout_DragOver);
            this.dragout.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Dragout_MouseDown);
            this.dragout.MouseEnter += new System.EventHandler(this.DragoutEnter);
            this.dragout.MouseLeave += new System.EventHandler(this.DragoutLeave);
            // 
            // PB_Legal
            // 
            this.PB_Legal.Image = ((System.Drawing.Image)(resources.GetObject("PB_Legal.Image")));
            this.PB_Legal.Location = new System.Drawing.Point(226, 5);
            this.PB_Legal.Name = "PB_Legal";
            this.PB_Legal.Size = new System.Drawing.Size(16, 16);
            this.PB_Legal.TabIndex = 101;
            this.PB_Legal.TabStop = false;
            this.PB_Legal.Click += new System.EventHandler(this.ClickLegality);
            // 
            // L_UpdateAvailable
            // 
            this.L_UpdateAvailable.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.L_UpdateAvailable.Location = new System.Drawing.Point(305, 5);
            this.L_UpdateAvailable.Name = "L_UpdateAvailable";
            this.L_UpdateAvailable.Size = new System.Drawing.Size(300, 13);
            this.L_UpdateAvailable.TabIndex = 102;
            this.L_UpdateAvailable.TabStop = true;
            this.L_UpdateAvailable.Text = "New Update Available!";
            this.L_UpdateAvailable.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.L_UpdateAvailable.Visible = false;
            // 
            // PKME_Tabs
            // 
            this.PKME_Tabs.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.PKME_Tabs.Location = new System.Drawing.Point(8, 27);
            this.PKME_Tabs.Name = "PKME_Tabs";
            this.PKME_Tabs.Size = new System.Drawing.Size(280, 325);
            this.PKME_Tabs.TabIndex = 103;
            this.PKME_Tabs.LegalityChanged += new System.EventHandler(this.PKME_Tabs_LegalityChanged);
            this.PKME_Tabs.UpdatePreviewSprite += new System.EventHandler(this.PKME_Tabs_UpdatePreviewSprite);
            this.PKME_Tabs.RequestShowdownImport += new System.EventHandler(this.PKME_Tabs_RequestShowdownImport);
            this.PKME_Tabs.RequestShowdownExport += new System.EventHandler(this.PKME_Tabs_RequestShowdownExport);
            this.PKME_Tabs.SaveFileRequested += new PKHeX.WinForms.Controls.PKMEditor.ReturnSAVEventHandler(this.PKME_Tabs_SaveFileRequested);
            // 
            // C_SAV
            // 
            this.C_SAV.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.C_SAV.Location = new System.Drawing.Point(292, 26);
            this.C_SAV.Name = "C_SAV";
            this.C_SAV.Size = new System.Drawing.Size(310, 326);
            this.C_SAV.TabIndex = 104;
            this.C_SAV.RequestReloadSave += new System.EventHandler(this.ClickSaveFileName);
            this.C_SAV.RequestCloneData += new System.EventHandler(this.ClickClone);
            // 
            // Main
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(614, 361);
            this.Controls.Add(this.C_SAV);
            this.Controls.Add(this.L_UpdateAvailable);
            this.Controls.Add(this.PB_Legal);
            this.Controls.Add(this.dragout);
            this.Controls.Add(this.menuStrip1);
            this.Controls.Add(this.PKME_Tabs);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.Name = "Main";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "PKHeX";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Main_FormClosing);
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.Main_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(Main.Main_DragEnter);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dragout)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.PB_Legal)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        public Controls.PKMEditor PKME_Tabs;
        private Controls.SAVEditor C_SAV;
        private System.Windows.Forms.LinkLabel L_UpdateAvailable;

        private System.Windows.Forms.PictureBox dragout;
        private System.Windows.Forms.PictureBox PB_Legal;

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem Menu_File;
        private System.Windows.Forms.ToolStripMenuItem Menu_Open;
        private System.Windows.Forms.ToolStripMenuItem Menu_Save;
        private System.Windows.Forms.ToolStripMenuItem Menu_Exit;
        private System.Windows.Forms.ToolStripMenuItem Menu_Tools;
        private System.Windows.Forms.ToolStripMenuItem Menu_Options;
        private System.Windows.Forms.ToolStripMenuItem Menu_Language;
        private System.Windows.Forms.ToolStripComboBox CB_MainLanguage;
        private System.Windows.Forms.ToolStripMenuItem Menu_About;
        private System.Windows.Forms.ToolStripMenuItem Menu_Unicode;
        private System.Windows.Forms.ToolStripMenuItem Menu_Modify;
        private System.Windows.Forms.ToolStripMenuItem Menu_ModifyDex;
        private System.Windows.Forms.ToolStripMenuItem Menu_ModifyPKM;
        private System.Windows.Forms.ToolStripMenuItem Menu_ExportSAV;
        private System.Windows.Forms.ToolStripMenuItem Menu_Showdown;
        private System.Windows.Forms.ToolStripMenuItem Menu_ShowdownExportPKM;
        private System.Windows.Forms.ToolStripMenuItem Menu_ShowdownImportPKM;
        private System.Windows.Forms.ToolStripMenuItem Menu_ShowdownExportParty;
        private System.Windows.Forms.ToolStripMenuItem Menu_ShowdownExportBattleBox;
        private System.Windows.Forms.ToolStripMenuItem Menu_Folder;
        private System.Windows.Forms.ToolStripMenuItem Menu_Data;
        private System.Windows.Forms.ToolStripMenuItem Menu_LoadBoxes;
        private System.Windows.Forms.ToolStripMenuItem Menu_Report;
        private System.Windows.Forms.ToolStripMenuItem Menu_Database;
        private System.Windows.Forms.ToolStripMenuItem Menu_DumpBoxes;
        private System.Windows.Forms.ToolStripMenuItem Menu_DumpBox;
        private System.Windows.Forms.ToolStripMenuItem Menu_ExportBAK;
        private System.Windows.Forms.ToolStripMenuItem Menu_ExportMAIN;
        private System.Windows.Forms.ToolStripMenuItem Menu_BatchEditor;
        private System.Windows.Forms.ToolStripMenuItem Menu_MGDatabase;
        private System.Windows.Forms.ToolStripMenuItem Menu_Undo;
        private System.Windows.Forms.ToolStripMenuItem Menu_Redo;
        private System.Windows.Forms.ToolStripMenuItem Menu_FlagIllegal;
        private System.Windows.Forms.ToolStripMenuItem Menu_ModifyUnset;
    }
}

