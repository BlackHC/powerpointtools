﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using PowerPoint = Microsoft.Office.Interop.PowerPoint;
using Office = Microsoft.Office.Core;
using System.Windows.Forms;
using Microsoft.Office.Interop.PowerPoint;
using System.Diagnostics;
using Microsoft.Office.Tools;
using System.ComponentModel;

namespace PowerPointLaTeX
{
    public partial class ThisAddIn
    {
        internal LaTeXTool Tool
        {
            get;
            private set;
        }

        internal CustomTaskPane DeveloperTaskPane;


        private void ThisAddIn_Startup(object sender, System.EventArgs e)
        {
            Tool = new LaTeXTool();

            DeveloperTaskPane = CustomTaskPanes.Add(new DeveloperTaskPaneControl(), DeveloperTaskPaneControl.Title);
            DeveloperTaskPane.Visible = Properties.Settings.Default.ShowDeveloperTaskPane;

            // register events
            Application.PresentationSave += new EApplication_PresentationSaveEventHandler(Application_PresentationSave);
            Application.SlideShowBegin += new EApplication_SlideShowBeginEventHandler(Application_SlideShowBegin);
            Application.WindowBeforeDoubleClick += new EApplication_WindowBeforeDoubleClickEventHandler(Application_WindowBeforeDoubleClick);
            Application.WindowSelectionChange += new EApplication_WindowSelectionChangeEventHandler(Application_WindowSelectionChange);

            Properties.Settings.Default.PropertyChanged += new PropertyChangedEventHandler(Default_PropertyChanged);
            DeveloperTaskPane.VisibleChanged += new EventHandler(DeveloperTaskPane_VisibleChanged);
        }

        void DeveloperTaskPane_VisibleChanged(object sender, EventArgs e)
        {
            // TODO: we dont want to update the settings when PowerPoint is exiting [1/2/2009 Andreas]
            // I have no idea how to check for that case :-/
            
            Properties.Settings.Default.ShowDeveloperTaskPane = DeveloperTaskPane.Visible;
        }

        void Default_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ShowDeveloperTaskPane")
            {
                DeveloperTaskPane.Visible = (bool) ((Properties.Settings) sender)[e.PropertyName];
            }
        }

        private Dictionary<Presentation, Shape> oldTextShapeDict = new Dictionary<Presentation, Shape>();
        private Shape oldTextShape
        {
            get
            {
                if (!oldTextShapeDict.ContainsKey(Tool.ActivePresentation))
                {
                    return null;
                }
                return oldTextShapeDict[Tool.ActivePresentation];
            }
            set
            {
                oldTextShapeDict[Tool.ActivePresentation] = value;
            }
        }

        private void Application_WindowSelectionChange(Selection Sel)
        {
            // an exception is thrown otherwise >_>
            if (Tool.ActivePresentation.Final)
            {
                return;
            }

            // automatically select the parent (and thus all children) of a inline objects
            List<Shape> shapes = Sel.GetShapesFromShapeSelection();

            IEnumerable<Shape> parentShapes =
                from shape in shapes
                where shape.LaTeXTags().Type == EquationType.Inline
                select Tool.GetParentShape(shape);
            IEnumerable<Shape> shapeSuperset =
                from parentShape in parentShapes.Union(shapes)
                from inlineShape in Tool.GetInlineShapes(parentShape)
                select inlineShape;

            Sel.SelectShapes(parentShapes, false);
            Sel.SelectShapes(shapeSuperset, false);

            Shape textShape = Sel.GetShapeFromTextSelection();
            // recompile the old shape if necessary (do nothing if we click around in the same text shape though)
            if (!Tool.ActivePresentation.SettingsTags().ManualPreview)
            {
                // check if the old shape still exists
                try
                {
                    if (oldTextShape != null)
                    {
                        object testAccess = oldTextShape.Parent;
                    }
                }
                catch (System.Runtime.InteropServices.COMException ex)
                {
                    oldTextShape = null;
                }

                if (oldTextShape != null && oldTextShape != textShape)
                {
                    Tool.CompileShape(oldTextShape.GetSlide(), oldTextShape);
                }
                if (!Tool.ActivePresentation.SettingsTags().PresentationMode)
                {
                    if (textShape != null)
                    {
                        Tool.DecompileShape(Tool.ActiveSlide, textShape);
                    }
                }
            }
            if (Tool.ActivePresentation.SettingsTags().PresentationMode)
            {
                // deselect shapes that contain formulas, etc.
                if (textShape.LaTeXTags().Type.value.ContainsLaTeXCode())
                {
                    textShape = null;
                    Sel.Unselect();
                }
            }
            oldTextShape = textShape;
        }

        void Application_WindowBeforeDoubleClick(Selection Sel, ref bool Cancel)
        {
            // kill the MS engineers - kill them all and torture them slowly to death..
            // http://www.eggheadcafe.com/software/aspnet/33533167/ppt--windowbeforedoublec.aspx
        }

        void Application_SlideShowBegin(SlideShowWindow Wn)
        {
            // TODO: add a setting to enable or disable the behavior and code [1/2/2009 Andreas]
            // its going to be too slow atm imo

            // check whether anything still needs to be compiled and ask if necessary
            /*
            if( Tool.NeedsCompile( Wn.Presentation ) ) {
                            DialogResult result = MessageBox.Show("There are shapes that contain LaTeX code that hasn't been compiled yet. Do you want to compile everything now?", "PowerPointLaTeX", MessageBoxButtons.YesNo);
                            if( result == DialogResult.Yes) {
                                Tool.CompilePresentation(Wn.Presentation);
                            }
                        }*/
            
        }

        void Application_PresentationSave(Presentation presentation)
        {
            // compile everything in case the plugin isnt available elsewhere
            Tool.CompilePresentation(presentation);
            // purge unused items from the cache to keep it smaller (thats the idea)
            presentation.CacheTags().PurgeUnused();
        }

        private void ThisAddIn_Shutdown(object sender, System.EventArgs e)
        {
            Properties.Settings.Default.Save();
        }

        #region VSTO generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InternalStartup()
        {
            this.Startup += new System.EventHandler(ThisAddIn_Startup);
            this.Shutdown += new System.EventHandler(ThisAddIn_Shutdown);
        }

        #endregion
    }
}
