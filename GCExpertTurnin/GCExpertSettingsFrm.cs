﻿using System;
using System.Windows.Forms;

namespace LlamaBotBases.GCExpertTurnin
{
    public partial class GCExpertSettingsFrm : Form
    {
        public GCExpertSettingsFrm()
        {
            InitializeComponent();
        }

        private void GCExpertSettingsFrm_Load(object sender, EventArgs e)
        {
            propertyGrid1.SelectedObject = GCExpertSettings.Instance;
        }
    }
}