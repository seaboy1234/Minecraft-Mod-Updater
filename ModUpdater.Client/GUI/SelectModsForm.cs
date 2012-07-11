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
        private List<Mod> selected, unselected;

        public SelectModsForm()
        {
            InitializeComponent();
            selected = new List<Mod>();
            unselected = new List<Mod>();
        }

        private void btnSelect_Click(object sender, EventArgs e)
        {
            Mod m = unselected.Find(((Mod)lsUnselected.SelectedItem).Identifier);
            if (m == null)
            {
                MessageBox.Show("Please select a mod.");
            }
            SelectMod(m);
        }

        private void btnSelectAll_Click(object sender, EventArgs e)
        {

        }

        private void btnUnselect_Click(object sender, EventArgs e)
        {
            Mod m = selected.Find(((Mod)lsSelected.SelectedItem).Identifier);
            if (m == null)
            {
                MessageBox.Show("Please select a mod.");
            }
            UnselectMod(m);
        }

        private void btnUnselectAll_Click(object sender, EventArgs e)
        {

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
    }
}
