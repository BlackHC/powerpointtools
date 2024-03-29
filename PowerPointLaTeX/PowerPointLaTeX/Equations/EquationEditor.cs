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
using Microsoft.Office.Interop.PowerPoint;
using System.Diagnostics;

namespace PowerPointLaTeX {
    partial class EquationEditor : Form {
        private System.Timers.Timer updatePreviewTimer;
        LocalCache localCache;

        public String LaTeXCode {
            get { return formulaText.Text; }
        }

        public int FontSize {
            get { return (int) fontSizeUpDown.Value; }
        }

        public EquationEditor(ICacheStorage masterStorage, String latexCode, int initialFontSize) {
            InitializeComponent();

            localCache = new LocalCache(masterStorage);

            updatePreviewTimer = new System.Timers.Timer();
            updatePreviewTimer.Interval = 0.5 * 1000;
            updatePreviewTimer.AutoReset = false;
            updatePreviewTimer.Elapsed += new System.Timers.ElapsedEventHandler(updatePreviewTimer_Elapsed);

            formulaText.Text = latexCode;
            fontSizeUpDown.Value = initialFontSize;

            //updatePreview();
        }

        void updatePreviewTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e) {
            formulaPreview.Invoke( new MethodInvoker(updatePreview) );
        }

        private void updatePreview() {
            UseWaitCursor = true;

            formulaPreview.Image = null;

            if( LaTeXCode != "" ) {
                Image previewImage;
                int unusedBaselineOffset;
                float wantedPixelsPerEmHeight = DPIHelper.FontSizeToPixelsPerEmHeight((float)fontSizeUpDown.Value);
                float actualPixelsPerEmHeight = wantedPixelsPerEmHeight;
                previewImage = LaTeXRendering.GetImageForLaTeXCode( localCache, LaTeXCode, ref actualPixelsPerEmHeight, out unusedBaselineOffset );
                formulaPreview.Image = previewImage;

                formulaPreview.Height = (int) (previewImage.Height * wantedPixelsPerEmHeight / actualPixelsPerEmHeight);
                formulaPreview.Width = (int) (previewImage.Width * wantedPixelsPerEmHeight / actualPixelsPerEmHeight);
                
                formulaPreview.Top = 0;
                formulaPreview.Left = (tableLayoutPanel2.Width - formulaPreview.Width) / 2;
           }

            UseWaitCursor = false;
        }

        private void formulaText_TextChanged(object sender, EventArgs e) {
            updatePreviewTimer.Stop();
            updatePreviewTimer.Start();
        }

        private void fontSizeUpDown_ValueChanged( object sender, EventArgs e ) {
            updatePreviewTimer.Stop();
            updatePreviewTimer.Start();
        }

        private void fontSizeUpDown_Click( object sender, EventArgs e ) {
            updatePreviewTimer.Stop();
            updatePreviewTimer.Start();
        }

        private void applyButton_Click(object sender, EventArgs e)
        {
            if( localCache.MasterStorage != null ) {
                localCache.MasterStorage.Set(LaTeXCode, localCache.Get(LaTeXCode)); 
            }
        }
    }
}
