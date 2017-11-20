﻿namespace PKHeX.WinForms
{
    partial class SAV_HallOfFame7
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
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SAV_HallOfFame7));
            this.B_Close = new System.Windows.Forms.Button();
            this.B_Cancel = new System.Windows.Forms.Button();
            this.CHK_Flag = new System.Windows.Forms.CheckBox();
            this.NUD_Count = new System.Windows.Forms.NumericUpDown();
            this.L_Count = new System.Windows.Forms.Label();
            this.L_F1 = new System.Windows.Forms.Label();
            this.CB_F1 = new System.Windows.Forms.ComboBox();
            this.CB_F2 = new System.Windows.Forms.ComboBox();
            this.L_F2 = new System.Windows.Forms.Label();
            this.CB_F3 = new System.Windows.Forms.ComboBox();
            this.L_F3 = new System.Windows.Forms.Label();
            this.CB_F4 = new System.Windows.Forms.ComboBox();
            this.L_F4 = new System.Windows.Forms.Label();
            this.CB_F5 = new System.Windows.Forms.ComboBox();
            this.L_F5 = new System.Windows.Forms.Label();
            this.CB_F6 = new System.Windows.Forms.ComboBox();
            this.L_F6 = new System.Windows.Forms.Label();
            this.CB_C6 = new System.Windows.Forms.ComboBox();
            this.L_C6 = new System.Windows.Forms.Label();
            this.CB_C5 = new System.Windows.Forms.ComboBox();
            this.L_C5 = new System.Windows.Forms.Label();
            this.CB_C4 = new System.Windows.Forms.ComboBox();
            this.L_C4 = new System.Windows.Forms.Label();
            this.CB_C3 = new System.Windows.Forms.ComboBox();
            this.L_C3 = new System.Windows.Forms.Label();
            this.CB_C2 = new System.Windows.Forms.ComboBox();
            this.L_C2 = new System.Windows.Forms.Label();
            this.CB_C1 = new System.Windows.Forms.ComboBox();
            this.L_C1 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.NUD_Count)).BeginInit();
            this.SuspendLayout();
            // 
            // B_Close
            // 
            this.B_Close.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.B_Close.Location = new System.Drawing.Point(306, 166);
            this.B_Close.Name = "B_Close";
            this.B_Close.Size = new System.Drawing.Size(76, 23);
            this.B_Close.TabIndex = 29;
            this.B_Close.Text = "Save";
            this.B_Close.UseVisualStyleBackColor = true;
            this.B_Close.Click += new System.EventHandler(this.B_Close_Click);
            // 
            // B_Cancel
            // 
            this.B_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.B_Cancel.Location = new System.Drawing.Point(224, 166);
            this.B_Cancel.Name = "B_Cancel";
            this.B_Cancel.Size = new System.Drawing.Size(76, 23);
            this.B_Cancel.TabIndex = 28;
            this.B_Cancel.Text = "Cancel";
            this.B_Cancel.UseVisualStyleBackColor = true;
            this.B_Cancel.Click += new System.EventHandler(this.B_Cancel_Click);
            // 
            // CHK_Flag
            // 
            this.CHK_Flag.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.CHK_Flag.AutoSize = true;
            this.CHK_Flag.Location = new System.Drawing.Point(12, 170);
            this.CHK_Flag.Name = "CHK_Flag";
            this.CHK_Flag.Size = new System.Drawing.Size(46, 17);
            this.CHK_Flag.TabIndex = 25;
            this.CHK_Flag.Text = "Flag";
            this.CHK_Flag.UseVisualStyleBackColor = true;
            this.CHK_Flag.Visible = false;
            // 
            // NUD_Count
            // 
            this.NUD_Count.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.NUD_Count.Location = new System.Drawing.Point(163, 169);
            this.NUD_Count.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.NUD_Count.Name = "NUD_Count";
            this.NUD_Count.Size = new System.Drawing.Size(55, 20);
            this.NUD_Count.TabIndex = 27;
            this.NUD_Count.Value = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.NUD_Count.Visible = false;
            // 
            // L_Count
            // 
            this.L_Count.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.L_Count.Location = new System.Drawing.Point(57, 166);
            this.L_Count.Name = "L_Count";
            this.L_Count.Size = new System.Drawing.Size(100, 23);
            this.L_Count.TabIndex = 26;
            this.L_Count.Text = "Count:";
            this.L_Count.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.L_Count.Visible = false;
            // 
            // L_F1
            // 
            this.L_F1.Location = new System.Drawing.Point(12, 25);
            this.L_F1.Name = "L_F1";
            this.L_F1.Size = new System.Drawing.Size(50, 23);
            this.L_F1.TabIndex = 1;
            this.L_F1.Text = "PKM 1:";
            this.L_F1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // CB_F1
            // 
            this.CB_F1.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.CB_F1.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.CB_F1.FormattingEnabled = true;
            this.CB_F1.Location = new System.Drawing.Point(68, 27);
            this.CB_F1.Name = "CB_F1";
            this.CB_F1.Size = new System.Drawing.Size(121, 21);
            this.CB_F1.TabIndex = 2;
            // 
            // CB_F2
            // 
            this.CB_F2.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.CB_F2.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.CB_F2.FormattingEnabled = true;
            this.CB_F2.Location = new System.Drawing.Point(68, 49);
            this.CB_F2.Name = "CB_F2";
            this.CB_F2.Size = new System.Drawing.Size(121, 21);
            this.CB_F2.TabIndex = 4;
            // 
            // L_F2
            // 
            this.L_F2.Location = new System.Drawing.Point(12, 47);
            this.L_F2.Name = "L_F2";
            this.L_F2.Size = new System.Drawing.Size(50, 23);
            this.L_F2.TabIndex = 3;
            this.L_F2.Text = "PKM 2:";
            this.L_F2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // CB_F3
            // 
            this.CB_F3.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.CB_F3.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.CB_F3.FormattingEnabled = true;
            this.CB_F3.Location = new System.Drawing.Point(68, 71);
            this.CB_F3.Name = "CB_F3";
            this.CB_F3.Size = new System.Drawing.Size(121, 21);
            this.CB_F3.TabIndex = 6;
            // 
            // L_F3
            // 
            this.L_F3.Location = new System.Drawing.Point(12, 69);
            this.L_F3.Name = "L_F3";
            this.L_F3.Size = new System.Drawing.Size(50, 23);
            this.L_F3.TabIndex = 5;
            this.L_F3.Text = "PKM 3:";
            this.L_F3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // CB_F4
            // 
            this.CB_F4.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.CB_F4.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.CB_F4.FormattingEnabled = true;
            this.CB_F4.Location = new System.Drawing.Point(68, 93);
            this.CB_F4.Name = "CB_F4";
            this.CB_F4.Size = new System.Drawing.Size(121, 21);
            this.CB_F4.TabIndex = 8;
            // 
            // L_F4
            // 
            this.L_F4.Location = new System.Drawing.Point(12, 91);
            this.L_F4.Name = "L_F4";
            this.L_F4.Size = new System.Drawing.Size(50, 23);
            this.L_F4.TabIndex = 7;
            this.L_F4.Text = "PKM 4:";
            this.L_F4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // CB_F5
            // 
            this.CB_F5.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.CB_F5.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.CB_F5.FormattingEnabled = true;
            this.CB_F5.Location = new System.Drawing.Point(68, 115);
            this.CB_F5.Name = "CB_F5";
            this.CB_F5.Size = new System.Drawing.Size(121, 21);
            this.CB_F5.TabIndex = 10;
            // 
            // L_F5
            // 
            this.L_F5.Location = new System.Drawing.Point(12, 113);
            this.L_F5.Name = "L_F5";
            this.L_F5.Size = new System.Drawing.Size(50, 23);
            this.L_F5.TabIndex = 9;
            this.L_F5.Text = "PKM 5:";
            this.L_F5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // CB_F6
            // 
            this.CB_F6.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.CB_F6.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.CB_F6.FormattingEnabled = true;
            this.CB_F6.Location = new System.Drawing.Point(68, 137);
            this.CB_F6.Name = "CB_F6";
            this.CB_F6.Size = new System.Drawing.Size(121, 21);
            this.CB_F6.TabIndex = 12;
            // 
            // L_F6
            // 
            this.L_F6.Location = new System.Drawing.Point(12, 135);
            this.L_F6.Name = "L_F6";
            this.L_F6.Size = new System.Drawing.Size(50, 23);
            this.L_F6.TabIndex = 11;
            this.L_F6.Text = "PKM 6:";
            this.L_F6.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // CB_C6
            // 
            this.CB_C6.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.CB_C6.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.CB_C6.FormattingEnabled = true;
            this.CB_C6.Location = new System.Drawing.Point(261, 137);
            this.CB_C6.Name = "CB_C6";
            this.CB_C6.Size = new System.Drawing.Size(121, 21);
            this.CB_C6.TabIndex = 24;
            // 
            // L_C6
            // 
            this.L_C6.Location = new System.Drawing.Point(205, 135);
            this.L_C6.Name = "L_C6";
            this.L_C6.Size = new System.Drawing.Size(50, 23);
            this.L_C6.TabIndex = 23;
            this.L_C6.Text = "PKM 6:";
            this.L_C6.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // CB_C5
            // 
            this.CB_C5.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.CB_C5.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.CB_C5.FormattingEnabled = true;
            this.CB_C5.Location = new System.Drawing.Point(261, 115);
            this.CB_C5.Name = "CB_C5";
            this.CB_C5.Size = new System.Drawing.Size(121, 21);
            this.CB_C5.TabIndex = 22;
            // 
            // L_C5
            // 
            this.L_C5.Location = new System.Drawing.Point(205, 113);
            this.L_C5.Name = "L_C5";
            this.L_C5.Size = new System.Drawing.Size(50, 23);
            this.L_C5.TabIndex = 21;
            this.L_C5.Text = "PKM 5:";
            this.L_C5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // CB_C4
            // 
            this.CB_C4.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.CB_C4.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.CB_C4.FormattingEnabled = true;
            this.CB_C4.Location = new System.Drawing.Point(261, 93);
            this.CB_C4.Name = "CB_C4";
            this.CB_C4.Size = new System.Drawing.Size(121, 21);
            this.CB_C4.TabIndex = 20;
            // 
            // L_C4
            // 
            this.L_C4.Location = new System.Drawing.Point(205, 91);
            this.L_C4.Name = "L_C4";
            this.L_C4.Size = new System.Drawing.Size(50, 23);
            this.L_C4.TabIndex = 19;
            this.L_C4.Text = "PKM 4:";
            this.L_C4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // CB_C3
            // 
            this.CB_C3.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.CB_C3.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.CB_C3.FormattingEnabled = true;
            this.CB_C3.Location = new System.Drawing.Point(261, 71);
            this.CB_C3.Name = "CB_C3";
            this.CB_C3.Size = new System.Drawing.Size(121, 21);
            this.CB_C3.TabIndex = 18;
            // 
            // L_C3
            // 
            this.L_C3.Location = new System.Drawing.Point(205, 69);
            this.L_C3.Name = "L_C3";
            this.L_C3.Size = new System.Drawing.Size(50, 23);
            this.L_C3.TabIndex = 17;
            this.L_C3.Text = "PKM 3:";
            this.L_C3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // CB_C2
            // 
            this.CB_C2.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.CB_C2.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.CB_C2.FormattingEnabled = true;
            this.CB_C2.Location = new System.Drawing.Point(261, 49);
            this.CB_C2.Name = "CB_C2";
            this.CB_C2.Size = new System.Drawing.Size(121, 21);
            this.CB_C2.TabIndex = 16;
            // 
            // L_C2
            // 
            this.L_C2.Location = new System.Drawing.Point(205, 47);
            this.L_C2.Name = "L_C2";
            this.L_C2.Size = new System.Drawing.Size(50, 23);
            this.L_C2.TabIndex = 15;
            this.L_C2.Text = "PKM 2:";
            this.L_C2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // CB_C1
            // 
            this.CB_C1.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.CB_C1.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.CB_C1.FormattingEnabled = true;
            this.CB_C1.Location = new System.Drawing.Point(261, 27);
            this.CB_C1.Name = "CB_C1";
            this.CB_C1.Size = new System.Drawing.Size(121, 21);
            this.CB_C1.TabIndex = 14;
            // 
            // L_C1
            // 
            this.L_C1.Location = new System.Drawing.Point(205, 25);
            this.L_C1.Name = "L_C1";
            this.L_C1.Size = new System.Drawing.Size(50, 23);
            this.L_C1.TabIndex = 13;
            this.L_C1.Text = "PKM 1:";
            this.L_C1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(65, 1);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(124, 23);
            this.label1.TabIndex = 0;
            this.label1.Text = "First";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(258, 1);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(124, 23);
            this.label2.TabIndex = 0;
            this.label2.Text = "Current";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // SAV_HallOfFame7
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(394, 201);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.CB_C6);
            this.Controls.Add(this.L_C6);
            this.Controls.Add(this.CB_C5);
            this.Controls.Add(this.L_C5);
            this.Controls.Add(this.CB_C4);
            this.Controls.Add(this.L_C4);
            this.Controls.Add(this.CB_C3);
            this.Controls.Add(this.L_C3);
            this.Controls.Add(this.CB_C2);
            this.Controls.Add(this.L_C2);
            this.Controls.Add(this.CB_C1);
            this.Controls.Add(this.L_C1);
            this.Controls.Add(this.CB_F6);
            this.Controls.Add(this.L_F6);
            this.Controls.Add(this.CB_F5);
            this.Controls.Add(this.L_F5);
            this.Controls.Add(this.CB_F4);
            this.Controls.Add(this.L_F4);
            this.Controls.Add(this.CB_F3);
            this.Controls.Add(this.L_F3);
            this.Controls.Add(this.CB_F2);
            this.Controls.Add(this.L_F2);
            this.Controls.Add(this.CB_F1);
            this.Controls.Add(this.L_F1);
            this.Controls.Add(this.L_Count);
            this.Controls.Add(this.NUD_Count);
            this.Controls.Add(this.CHK_Flag);
            this.Controls.Add(this.B_Cancel);
            this.Controls.Add(this.B_Close);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SAV_HallOfFame7";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Hall of Fame Viewer";
            ((System.ComponentModel.ISupportInitialize)(this.NUD_Count)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button B_Close;
        private System.Windows.Forms.Button B_Cancel;
        private System.Windows.Forms.CheckBox CHK_Flag;
        private System.Windows.Forms.NumericUpDown NUD_Count;
        private System.Windows.Forms.Label L_Count;
        private System.Windows.Forms.Label L_F1;
        private System.Windows.Forms.ComboBox CB_F1;
        private System.Windows.Forms.ComboBox CB_F2;
        private System.Windows.Forms.Label L_F2;
        private System.Windows.Forms.ComboBox CB_F3;
        private System.Windows.Forms.Label L_F3;
        private System.Windows.Forms.ComboBox CB_F4;
        private System.Windows.Forms.Label L_F4;
        private System.Windows.Forms.ComboBox CB_F5;
        private System.Windows.Forms.Label L_F5;
        private System.Windows.Forms.ComboBox CB_F6;
        private System.Windows.Forms.Label L_F6;
        private System.Windows.Forms.ComboBox CB_C6;
        private System.Windows.Forms.Label L_C6;
        private System.Windows.Forms.ComboBox CB_C5;
        private System.Windows.Forms.Label L_C5;
        private System.Windows.Forms.ComboBox CB_C4;
        private System.Windows.Forms.Label L_C4;
        private System.Windows.Forms.ComboBox CB_C3;
        private System.Windows.Forms.Label L_C3;
        private System.Windows.Forms.ComboBox CB_C2;
        private System.Windows.Forms.Label L_C2;
        private System.Windows.Forms.ComboBox CB_C1;
        private System.Windows.Forms.Label L_C1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
    }
}