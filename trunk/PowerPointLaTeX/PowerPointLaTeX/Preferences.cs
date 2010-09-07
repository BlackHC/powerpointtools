﻿#region Copyright Notice
// This file is part of PowerPoint LaTeX.
// 
// Copyright (C) 2008/2009 Andreas Kirsch
// 
// PowerPoint LaTeX is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// PowerPoint LaTeX is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using PowerPointLaTeX.Properties;
using PowerPointLaTeX;
using System.IO;

namespace PowerPointLaTeX
{
    public partial class Preferences : Form
    {
        private LaTeXTool Tool {
            get {
                return Globals.ThisAddIn.Tool;
            }
        }

        private string oldMiktexPreamble;

        public Preferences() {
            InitializeComponent();

            // fill the serviceSelector
            serviceSelector.Items.Clear();
            serviceSelector.Items.AddRange( Globals.ThisAddIn.LaTeXServices.ServiceNames );

            miktexPreambleBox.Text = oldMiktexPreamble = Tool.ActivePresentation.SettingsTags().MiKTeXPreamble;

            Save();
            
            miktexPathBox.Text = global::PowerPointLaTeX.Properties.MiKTexSettings.Default.MikTexPath;

            string aboutServices = "";
            foreach(string serviceName in Globals.ThisAddIn.LaTeXServices.ServiceNames) {
                ILaTeXService service = Globals.ThisAddIn.LaTeXServices.GetService( serviceName );
                string aboutNotice = service.AboutNotice;

                aboutServices += serviceName + ":\n\n" + aboutNotice + "\n\n";
            }

            var assName = System.Reflection.Assembly.GetExecutingAssembly().GetName();
            string appInfo = assName.Name + " " + assName.Version;
            aboutBox.Text = aboutBox.Text.Replace( "INSERT_APP_INFO", appInfo ).Replace( "INSERT_ABOUT_SERVICES", aboutServices );
        }

        private void Save() {
            Settings.Default.Save();
            MiKTexSettings.Default.Save();

            Tool.ActivePresentation.SettingsTags().MiKTeXPreamble.value = miktexPreambleBox.Text;
        }

        private void Reload() {
            Settings.Default.Reload();
            MiKTexSettings.Default.Reload();
        }

        private void AbortButton_Click( object sender, EventArgs e ) {
            Reload();
        }

        private void OkButton_Click( object sender, EventArgs e ) {
            Save();
        }

        private void miktexPathBox_TextChanged( object sender, EventArgs e ) {
            MiKTexSettings settings = global::PowerPointLaTeX.Properties.MiKTexSettings.Default;
            settings.MikTexPath = miktexPathBox.Text;
            settings.LatexPath = Path.Combine( settings.MikTexPath, settings.Default_LatexRelPath );
            settings.DVIPNGPath = Path.Combine( settings.MikTexPath, settings.Default_DVIPNGRelPath );
        }

        private void miktexPathBrowserButton_Click( object sender, EventArgs e ) {
            miktexPathBrowser.SelectedPath = miktexPathBox.Text;
            if( miktexPathBrowser.ShowDialog() == DialogResult.OK ) {
                miktexPathBox.Text = miktexPathBrowser.SelectedPath;
            }
        }

        private void Preferences_FormClosed( object sender, FormClosedEventArgs e ) {
            Reload();
        }
    }
}