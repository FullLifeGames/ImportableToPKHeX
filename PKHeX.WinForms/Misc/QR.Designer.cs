﻿using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using PKHeX.Core;

namespace PKHeX.WinForms
{
    partial class QR
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

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
            ComponentResourceManager resources = new ComponentResourceManager(typeof(QR));
            this.PB_QR = new PictureBox();
            this.FontLabel = new Label();
            this.NUD_Box = new NumericUpDown();
            this.label1 = new Label();
            this.label2 = new Label();
            this.NUD_Slot = new NumericUpDown();
            this.label3 = new Label();
            this.NUD_Copies = new NumericUpDown();
            this.B_Refresh = new Button();
            ((ISupportInitialize)(this.PB_QR)).BeginInit();
            ((ISupportInitialize)(this.NUD_Box)).BeginInit();
            ((ISupportInitialize)(this.NUD_Slot)).BeginInit();
            ((ISupportInitialize)(this.NUD_Copies)).BeginInit();
            this.SuspendLayout();
            // 
            // PB_QR
            // 
            this.PB_QR.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Left) 
            | AnchorStyles.Right)));
            this.PB_QR.BackgroundImageLayout = ImageLayout.None;
            this.PB_QR.Location = new Point(2, 1);
            this.PB_QR.Name = "PB_QR";
            this.PB_QR.Size = new Size(405, 455);
            this.PB_QR.TabIndex = 0;
            this.PB_QR.TabStop = false;
            this.PB_QR.Click += new EventHandler(this.PB_QR_Click);
            // 
            // FontLabel
            // 
            this.FontLabel.AutoSize = true;
            this.FontLabel.Location = new Point(388, 393);
            this.FontLabel.Name = "FontLabel";
            this.FontLabel.Size = new Size(19, 13);
            this.FontLabel.TabIndex = 1;
            this.FontLabel.Text = "<3";
            this.FontLabel.Visible = false;
            // 
            // NUD_Box
            // 
            this.NUD_Box.Location = new Point(38, 465);
            this.NUD_Box.Maximum = new decimal(new int[] {
            32,
            0,
            0,
            0});
            this.NUD_Box.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.NUD_Box.Name = "NUD_Box";
            this.NUD_Box.Size = new Size(61, 20);
            this.NUD_Box.TabIndex = 2;
            this.NUD_Box.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new Point(4, 467);
            this.label1.Name = "label1";
            this.label1.Size = new Size(28, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Box:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new Point(105, 467);
            this.label2.Name = "label2";
            this.label2.Size = new Size(28, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Slot:";
            // 
            // NUD_Slot
            // 
            this.NUD_Slot.Location = new Point(139, 465);
            this.NUD_Slot.Maximum = new decimal(new int[] {
            30,
            0,
            0,
            0});
            this.NUD_Slot.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.NUD_Slot.Name = "NUD_Slot";
            this.NUD_Slot.Size = new Size(61, 20);
            this.NUD_Slot.TabIndex = 4;
            this.NUD_Slot.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new Point(211, 467);
            this.label3.Name = "label3";
            this.label3.Size = new Size(42, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "Copies:";
            // 
            // NUD_Copies
            // 
            this.NUD_Copies.Location = new Point(259, 465);
            this.NUD_Copies.Maximum = new decimal(new int[] {
            960,
            0,
            0,
            0});
            this.NUD_Copies.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.NUD_Copies.Name = "NUD_Copies";
            this.NUD_Copies.Size = new Size(52, 20);
            this.NUD_Copies.TabIndex = 6;
            this.NUD_Copies.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // B_Refresh
            // 
            this.B_Refresh.Location = new Point(317, 464);
            this.B_Refresh.Name = "B_Refresh";
            this.B_Refresh.Size = new Size(80, 23);
            this.B_Refresh.TabIndex = 8;
            this.B_Refresh.Text = "Refresh";
            this.B_Refresh.UseVisualStyleBackColor = true;
            this.B_Refresh.Click += new EventHandler(this.UpdateBoxSlotCopies);
            // 
            // QR
            // 
            this.AutoScaleDimensions = new SizeF(6F, 13F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(409, 407);
            this.Controls.Add(this.B_Refresh);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.NUD_Copies);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.NUD_Slot);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.NUD_Box);
            this.Controls.Add(this.FontLabel);
            this.Controls.Add(this.PB_QR);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.Icon = ((Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "QR";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "PKHeX QR Code (Click QR to Copy Image)";
            ((ISupportInitialize)(this.PB_QR)).EndInit();
            ((ISupportInitialize)(this.NUD_Box)).EndInit();
            ((ISupportInitialize)(this.NUD_Slot)).EndInit();
            ((ISupportInitialize)(this.NUD_Copies)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private PictureBox PB_QR;
        private Label FontLabel;
        private NumericUpDown NUD_Box;
        private Label label1;
        private Label label2;
        private NumericUpDown NUD_Slot;
        private Label label3;
        private NumericUpDown NUD_Copies;
        private Button B_Refresh;
    }
}