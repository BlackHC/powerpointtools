﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Office.Interop.PowerPoint;

namespace PowerPointLaTeX
{
    public partial class TagsGrid : UserControl
    {
        public TagsGrid(string name, Tags tags)
        {
            InitializeComponent();

            itemName.Text = name;
            for (int i = 1; i <= tags.Count; i++)
            {
                tagsGridView.Rows.Add(new string[] { tags.Name(i), tags.Value(i) });
            }
        }
    }
}
