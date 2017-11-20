﻿using System;
using System.Linq;
using System.Windows.Forms;
using PKHeX.Core;

namespace PKHeX.WinForms
{
    public partial class SAV_SuperTrain : Form
    {
        private readonly SaveFile Origin;
        private readonly SAV6 SAV;
        public SAV_SuperTrain(SaveFile sav)
        {
            SAV = (SAV6)(Origin = sav).Clone();
            trba = GameInfo.Strings.trainingbags;
            trba[0] = "---";
            offsetTime = SAV.SuperTrain + 0x08;
            offsetSpec = SAV.SuperTrain + 0x188;
            offsetVal = SAV.SuperTrain + 0x18A;
            InitializeComponent();
            WinFormsUtil.TranslateInterface(this, Main.CurrentLanguage);
            string[] stages = GameInfo.Strings.trainingstage;
            listBox1.Items.Clear();
            for (int i = 0; i < 30; i++)
                listBox1.Items.Add($"{(i + 1):00} - {stages[i + 2]}");

            Setup();
        }

        private readonly string[] trba;
        private readonly int offsetVal;
        private readonly int offsetTime;
        private readonly int offsetSpec;
        private void Setup()
        {
            dataGridView1.Rows.Clear();
            dataGridView1.Columns.Clear();
            {
                CB_Species.DisplayMember = "Text";
                CB_Species.ValueMember = "Value";
                CB_Species.DataSource = new BindingSource(GameInfo.SpeciesDataSource.Where(s => s.Value <= SAV.MaxSpeciesID).ToList(), null);

                CB_S2.DisplayMember = "Text";
                CB_S2.ValueMember = "Value";
                CB_S2.DataSource = new BindingSource(GameInfo.SpeciesDataSource.Where(s => s.Value <= SAV.MaxSpeciesID).ToList(), null);
            }
            listBox1.SelectedIndex = 0;
            FillTrainingBags();

            CB_S2.SelectedValue = (int)BitConverter.ToUInt16(SAV.Data, offsetSpec + 4 * 30);
            TB_Time1.Text = BitConverter.ToSingle(SAV.Data, offsetTime + 4 * 30).ToString();
            TB_Time2.Text = BitConverter.ToSingle(SAV.Data, offsetTime + 4 * 31).ToString();
        }
        private void FillTrainingBags()
        {
            DataGridViewColumn dgvIndex = new DataGridViewTextBoxColumn();
            {
                dgvIndex.HeaderText = "Slot";
                dgvIndex.DisplayIndex = 0;
                dgvIndex.Width = 25;
                dgvIndex.ReadOnly = true;
                dgvIndex.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }
            DataGridViewComboBoxColumn dgvBag = new DataGridViewComboBoxColumn
            {
                DisplayStyle = DataGridViewComboBoxDisplayStyle.Nothing
            };
            {
                foreach (string t in trba)
                    if (t.Length > 0)
                        dgvBag.Items.Add(t);

                dgvBag.DisplayIndex = 1;
                dgvBag.Width = 135;
                dgvBag.FlatStyle = FlatStyle.Flat;
            }
            dataGridView1.Columns.Add(dgvIndex);
            dataGridView1.Columns.Add(dgvBag);

            dataGridView1.Rows.Add(12);
            int offset = SAV.SuperTrain + 0x308;
            for (int i = 0; i < 12; i++)
            {
                dataGridView1.Rows[i].Cells[0].Value = (i + 1).ToString();
                dataGridView1.Rows[i].Cells[1].Value = trba[SAV.Data[offset + i]];
            }
        }        
        private void DropClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.ColumnIndex != 1) return;
                ComboBox comboBox = (ComboBox)dataGridView1.EditingControl;
                comboBox.DroppedDown = true;
            }
            catch { }
        }
        private void ChangeListRecordSelection(object sender, EventArgs e)
        {
            int index = listBox1.SelectedIndex;
            if (index < 0) return;
            TB_Time.Text = BitConverter.ToSingle(SAV.Data, offsetTime + 4 * index).ToString();
            TB_Unk.Text = BitConverter.ToUInt16(SAV.Data, offsetVal + 4 * index).ToString();
            CB_Species.SelectedValue = (int)BitConverter.ToUInt16(SAV.Data, offsetSpec + 4 * index);
        }
        private void B_Save_Click(object sender, EventArgs e)
        {
            // Copy Bags
            byte[] bagarray = new byte[12];
            int emptyslots = 0;
            for (int i = 0; i < 12; i++)
            {
                string bag = dataGridView1.Rows[i].Cells[1].Value.ToString();
                if (Array.IndexOf(trba, bag) == 0)
                {
                    emptyslots++;
                    continue;
                }
                bagarray[i - emptyslots] = (byte)Array.IndexOf(trba, bag);
            }
            try { BitConverter.GetBytes(float.Parse(TB_Time1.Text)).CopyTo(SAV.Data, offsetTime + 4 * 30); } catch { }
            try { BitConverter.GetBytes(float.Parse(TB_Time2.Text)).CopyTo(SAV.Data, offsetTime + 4 * 31); } catch { }
            BitConverter.GetBytes((ushort)WinFormsUtil.GetIndex(CB_S2)).CopyTo(SAV.Data, offsetSpec + 4 * 30);
            bagarray.CopyTo(SAV.Data, SAV.SuperTrain + 0x308);
            Origin.SetData(SAV.Data, 0);
            Close();
        }
        private void B_Cancel_Click(object sender, EventArgs e)
        {
            Close();
        }
        private void ChangeRecordSpecies(object sender, EventArgs e)
        {
            int index = listBox1.SelectedIndex;
            if (index < 0) return;
            BitConverter.GetBytes(WinFormsUtil.GetIndex(CB_Species)).CopyTo(SAV.Data, offsetSpec + 4 * index);
        }
        private void ChangeRecordVal(object sender, EventArgs e)
        {
            int index = listBox1.SelectedIndex;
            if (index < 0) return;
            try { BitConverter.GetBytes(ushort.Parse(TB_Unk.Text)).CopyTo(SAV.Data, offsetVal + 4 * index); } catch { }
        }
        private void ChangeRecordTime(object sender, EventArgs e)
        {
            int index = listBox1.SelectedIndex;
            if (index < 0) return;
            try { BitConverter.GetBytes(float.Parse(TB_Time.Text)).CopyTo(SAV.Data, offsetTime + 4 * index); } catch { }
        }
    }
}

