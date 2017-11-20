﻿using System;
using System.Collections.Generic;
using System.Windows.Forms;
using PKHeX.Core;

namespace PKHeX.WinForms
{
    public partial class SAV_Pokebean : Form
    {
        public SAV_Pokebean(SaveFile sav)
        {
            SAV = (SAV7)(Origin = sav).Clone();
            InitializeComponent();
            WinFormsUtil.TranslateInterface(this, Main.CurrentLanguage);

            var colors = new[] {"Red", "Blue", "Light Blue", "Green", "Yellow", "Purple", "Orange"};
            var beans = new List<string>();
            foreach (var color in colors)
            {
                beans.Add($"{color} Bean");
            }
            foreach (var color in colors)
            {
                beans.Add($"{color} Patterned Bean");
            }
            beans.Add("Rainbow Bean");
            beanlist = beans.ToArray();

            Setup();
        }

        private const int MaxBeanID = 14;

        private readonly SaveFile Origin;
        private readonly SAV7 SAV;
        private readonly string[] beanlist;
        private void Setup()
        {
            dgv.Rows.Clear();
            dgv.Columns.Clear();

            DataGridViewColumn dgvBean = new DataGridViewTextBoxColumn();
            {
                dgvBean.HeaderText = "Slot";
                dgvBean.DisplayIndex = 0;
                dgvBean.Width = 135;
                dgvBean.ReadOnly = true;
                dgvBean.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }
            DataGridViewComboBoxColumn dgvCount = new DataGridViewComboBoxColumn
            {
                DisplayStyle = DataGridViewComboBoxDisplayStyle.Nothing,
                DisplayIndex = 0,
                Width = 135,
                FlatStyle = FlatStyle.Flat,
                ValueType = typeof(int)
            };
            {
                for (var i = 0; i < 256; i++)
                    dgvCount.Items.Add(i);

                dgvCount.DisplayIndex = 1;
                dgvCount.Width = 45;
                dgvCount.FlatStyle = FlatStyle.Flat;
            }
            dgv.Columns.Add(dgvBean);
            dgv.Columns.Add(dgvCount);

            dgv.Rows.Add(MaxBeanID + 1);
            for (int i = 0; i <= MaxBeanID; i++)
            {
                dgv.Rows[i].Cells[0].Value = beanlist[i];
                dgv.Rows[i].Cells[1].Value = SAV.GetPokebeanCount(i);
            }
        }
        private static void DropClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex != 1) return;
            ((ComboBox)((DataGridView) sender).EditingControl).DroppedDown = true;
        }

        private void B_Cancel_Click(object sender, EventArgs e)
        {
            Close();
        }
        private void B_All_Click(object sender, EventArgs e)
        {
            for (int i = 0; i <= MaxBeanID; i++)
                SAV.SetPokebeanCount(i, 255);
            Setup();
        }
        private void B_None_Click(object sender, EventArgs e)
        {
            for (int i = 0; i <= MaxBeanID; i++)
                SAV.SetPokebeanCount(i, 0);
            Setup();
        }
    
        private void B_Save_Click(object sender, EventArgs e)
        {
            for (int i = 0; i <= MaxBeanID; i++)
                SAV.SetPokebeanCount(i, (int)dgv.Rows[i].Cells[1].Value);
            Origin.SetData(SAV.Data, 0);
            Close();
        }
    }
}
