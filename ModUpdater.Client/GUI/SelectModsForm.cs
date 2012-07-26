using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ModUpdater.Client.Utility;

namespace ModUpdater.Client.GUI
{
    public partial class SelectModsForm : Form
    {
        public Mod[] SelectedMods { get { return selected.ToArray(); } }
        public Mod[] UnselectedMods { get { return unselected.ToArray(); } }
        private List<Mod> selected, unselected;

        public SelectModsForm(Mod[] optional, Mod[] selectedOptional)
        {
            InitializeComponent();
            
            selected = new List<Mod>(selectedOptional);
            unselected = new List<Mod>();
            foreach (Mod m in optional)
            {
                if (!selectedOptional.Contains(m))
                {
                    unselected.Add(m);
                }
            }
        }

        private void btnSelect_Click(object sender, EventArgs e)
        {
            if (lsUnselected.SelectedItem == null) return;
            Mod m = unselected.Find(((Mod)lsUnselected.SelectedItem).Identifier);
            if (m == null)
            {
                MessageBox.Show("Please select a mod.");
            }
            SelectMod(m);
        }

        private void btnSelectAll_Click(object sender, EventArgs e)
        {
            foreach (Mod m in unselected.ToArray())
            {
                if (unselected.Contains(m))
                {
                    SelectMod(m);
                }
            }
        }

        private void btnUnselect_Click(object sender, EventArgs e)
        {
            if (lsSelected.SelectedItem == null) return;
            Mod m = selected.Find(((Mod)lsSelected.SelectedItem).Identifier);
            if (m == null)
            {
                MessageBox.Show("Please select a mod.");
            }
            UnselectMod(m);
        }

        private void btnUnselectAll_Click(object sender, EventArgs e)
        {
            foreach (Mod m in selected.ToArray())
            {
                if (m.RequiredBy.Count > 0)
                {
                    foreach (Mod mod in m.RequiredBy)
                    {
                        UnselectMod(mod);
                    }
                }
                UnselectMod(m);
            }
        }

        private void SelectMod(Mod m)
        {
            foreach (Mod mod in unselected.ToArray())
            {
                if (m.Requires.Contains(mod.Identifier))
                {
                    SelectMod(mod);
                }
            }
            selected.Add(m);
            lsSelected.Items.Add(m);
            lsUnselected.Items.Remove(m);
            unselected.Remove(m);
        }

        private void UnselectMod(Mod m)
        {
            string requiredBy = "";
            foreach (Mod mod in m.RequiredBy.ToArray())
            {
                if (m.RequiredBy.Contains(mod))
                    requiredBy += mod.Name + ", ";
            }
            if (requiredBy != "")
            {
                MessageBox.Show("This mod is required by other mods! \nYou must remove " + requiredBy.Remove(requiredBy.Length - 2) + " before removing this one.");
            }
            selected.Remove(m);
            lsSelected.Items.Remove(m);
            lsUnselected.Items.Add(m);
            unselected.Add(m);
        }

        private void SelectModsForm_Load(object sender, EventArgs e)
        {
            foreach (Mod m in selected)
            {
                lsSelected.Items.Add(m);
            }
            foreach (Mod m in unselected)
            {
                if (selected.Contains(m)) continue;
                lsUnselected.Items.Add(m);
            }
            DialogResult = System.Windows.Forms.DialogResult.None;
        }

        private void btnDone_Click(object sender, EventArgs e)
        {
            DialogResult r = MessageBox.Show(string.Format("Are you sure you want to add {0} more mods to the download list?", selected.Count), "Confirm Action", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            if (r == System.Windows.Forms.DialogResult.No) return;
            DialogResult = System.Windows.Forms.DialogResult.Yes;
            Close();
        }
    }
}
