using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PKHeX
{
    public partial class Converter : Form
    {
        public Converter()
        {
            InitializeComponent();
        }

        private void convert_Click(object sender, EventArgs e)
        {
            if (textBox2.Text == "" && (radioButton1.Checked || radioButton2.Checked))
            {
                MessageBox.Show("Please enter an Extract Path!");
            }
            else if (textBox3.Text == "" && radioButton3.Checked)
            {
                MessageBox.Show("Please enter an Cyber Sav File Path!");
            }
            else if ((maskedTextBox1.Text == "" || maskedTextBox2.Text == "") && radioButton3.Checked)
            {
                MessageBox.Show("Please enter valid Box numbers!");
            }
            else if (textBox1.Text == "") 
            {
                MessageBox.Show("Please enter an Importable!");
            }
            else
            {
                string importable = textBox1.Text;

                List<Pokemon> pokemon = extractImportable(importable);

                if (pokemon.Count > 30 && radioButton2.Checked)
                {
                    MessageBox.Show("Only 30 Pokemon fit into a Box!");
                }
                else
                {
                    Main form = new Main(pokemon, textBox2.Text, radioButton1.Checked, textBox3.Text, radioButton3.Checked, maskedTextBox1.Text, maskedTextBox2.Text, checkBox1.Checked);
                    form.Form1_Load(null, null);
                    form.Close();
                    MessageBox.Show("Done!");
                }
            }
        }

        private static List<Pokemon> extractImportable(string importable)
        {
            List<Pokemon> pokemon = new List<Pokemon>();
            Pokemon aktpkm = null;
            
            importable = importable.Replace("\r","");
            string[] splitArray = importable.Split('\n');
            int count = 0;
            foreach (string line in splitArray)
            {
                string s = line;

                if (s.Trim().StartsWith("==="))
                {
                    count++;
                    continue;
                }

                if((line.Contains("@") || (splitArray.Length > count + 1 && splitArray[count + 1].Contains("Ability:")) || (count - 1 >= 0 && splitArray[count - 1] == "") || (count - 1 < 0)) && line.Trim() != "")
                {
                    if (aktpkm != null)
                    {
                        pokemon.Add(aktpkm);
                    }
                    aktpkm = new Pokemon();

                    if (s.Contains("(M)"))
                    {
                        s = s.Replace("(M)", "");
                        aktpkm.gender = "m";   
                    }
                    else if (s.Contains("(F)"))
                    {
                        s = s.Replace("(F)", "");
                        aktpkm.gender = "f";
                    }

                    if (s.Contains("("))
                    {
                        aktpkm.nickname = s.Substring(0, s.IndexOf("(")).Trim();
                        aktpkm.name = s.Substring(s.LastIndexOf("(") + 1, s.LastIndexOf(")") - (s.LastIndexOf("(") + 1));
                        if (s.Contains("@"))
                        {
                            aktpkm.item = s.Substring(s.IndexOf("@") + 1).Trim();
                        }
                    }
                    else
                    {
                        if (s.Contains("@"))
                        {
                            aktpkm.name = s.Substring(0, s.IndexOf("@"));
                            aktpkm.item = s.Substring(s.IndexOf("@") + 1).Trim();
                        }
                        else
                        {
                            aktpkm.name = s;
                        }
                    }
                    aktpkm.name = aktpkm.name.Trim().Replace("-Primal", "");
                    if(aktpkm.name.Contains("-Mega"))
                    {
                        aktpkm.name = aktpkm.name.Substring(0, aktpkm.name.IndexOf("-Mega"));
                    }
                } 
                else if(s.Contains("Ability:"))
                {
                    aktpkm.ability = s.Substring(s.IndexOf(":") + 1).Trim();
                }
                else if (s.Contains("Level:"))
                {
                    aktpkm.level = s.Substring(s.IndexOf(":") + 1).Trim();
                }
                else if (s.Contains("EVs:"))
                {
                    aktpkm.evs = s.Substring(s.IndexOf(":") + 1).Trim();
                }
                else if (s.Contains("IVs:"))
                {
                    aktpkm.ivs = s.Substring(s.IndexOf(":") + 1).Trim();
                }                        
                else if (s.Contains("Happiness:"))
                {
                    aktpkm.happiness = s.Substring(s.IndexOf(":") + 1).Trim();
                }
                else if (s.Contains("Shiny:"))
                {
                    aktpkm.shiny = true;
                }
                else if (s.Trim().EndsWith("Nature"))
                {
                    aktpkm.nature = s.Substring(0, s.IndexOf("Nature")).Trim();
                }
                else if (s.Trim().StartsWith("-"))
                {
                    aktpkm.moves.Add(s.Substring(1).Trim());
                }
                count += 1;
            }
            pokemon.Add(aktpkm);
            return pokemon;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog objDialog = new FolderBrowserDialog();
            objDialog.Description = "Extract Path";
            objDialog.SelectedPath = Application.StartupPath; 
            DialogResult objResult = objDialog.ShowDialog(this);
            if (objResult == DialogResult.OK)
            {
                textBox2.Text = objDialog.SelectedPath;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = true;
            dialog.Title = "Basic Files";
            dialog.InitialDirectory = Application.StartupPath;
            DialogResult res = dialog.ShowDialog(this);
            if (res == DialogResult.OK)
            {
                foreach (string file in dialog.FileNames)
                {
                    FileInfo f = new FileInfo(file);
                    f.CopyTo(Application.StartupPath + "\\Gen Files\\" + f.Name);
                }
            }

            System.Windows.Forms.FolderBrowserDialog objDialog = new FolderBrowserDialog();
            objDialog.Description = "Gold Folder";
            objDialog.SelectedPath = Application.StartupPath;
            DialogResult objResult = objDialog.ShowDialog(this);
            if (objResult == DialogResult.OK)
            {
                DirectoryInfo d = new DirectoryInfo(objDialog.SelectedPath);
                Directory.CreateDirectory(Application.StartupPath + "\\Gen Files\\" + d.Name);
                foreach (FileInfo f in d.GetFiles())
                {
                    f.CopyTo(Application.StartupPath + "\\Gen Files\\" + d.Name + "\\" + f.Name);
                }
            }
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyValue == 65)
                textBox1.SelectAll();   
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = "Cyber Sav File Path";
            dialog.InitialDirectory = Application.StartupPath;
            DialogResult res = dialog.ShowDialog(this);
            if (res == DialogResult.OK)
            {
                textBox3.Text = dialog.FileName;
            }
        }

        private void maskedTextBox1_TextChanged(object sender, EventArgs e)
        {
            if (maskedTextBox1.Text != "" && maskedTextBox2.Text != "")
            {
                if (int.Parse(maskedTextBox1.Text) > 31)
                {
                    maskedTextBox1.Text = "31";
                }
                if (int.Parse(maskedTextBox1.Text) <= 0)
                {
                    maskedTextBox1.Text = "01";
                }
                if (int.Parse(maskedTextBox2.Text) < int.Parse(maskedTextBox1.Text))
                {
                    maskedTextBox1.Text = maskedTextBox2.Text;
                }
            }
        }

        private void maskedTextBox2_TextChanged(object sender, EventArgs e)
        {
            if (maskedTextBox2.Text != "" && maskedTextBox1.Text != "")
            {
                if (int.Parse(maskedTextBox2.Text) > 31)
                {
                    maskedTextBox2.Text = "31";
                }
                if (int.Parse(maskedTextBox2.Text) < int.Parse(maskedTextBox1.Text))
                {
                    maskedTextBox2.Text = maskedTextBox1.Text;
                }
            }
        }

    }
}
