﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:2.0.50727.4927
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace PowerPointLaTeX.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "9.0.0.0")]
    internal sealed partial class MiKTexSettings : global::System.Configuration.ApplicationSettingsBase {
        
        private static MiKTexSettings defaultInstance = ((MiKTexSettings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new MiKTexSettings())));
        
        public static MiKTexSettings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("d:\\LaTeX\\MiKTeX Portable")]
        public string MikTexPath {
            get {
                return ((string)(this["MikTexPath"]));
            }
            set {
                this["MikTexPath"] = value;
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public string Default_LatexRelPath {
            get {
                return ((string)(this["Default_LatexRelPath"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public string Default_DVIPNGRelPath {
            get {
                return ((string)(this["Default_DVIPNGRelPath"]));
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("d:\\LaTeX\\MiKTeX Portable\\miktex\\bin\\latex.exe")]
        public string LatexPath {
            get {
                return ((string)(this["LatexPath"]));
            }
            set {
                this["LatexPath"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("d:\\LaTeX\\MiKTeX Portable\\miktex\\bin\\dvipng.exe")]
        public string DVIPNGPath {
            get {
                return ((string)(this["DVIPNGPath"]));
            }
            set {
                this["DVIPNGPath"] = value;
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public string MikTexTemplate {
            get {
                return ((string)(this["MikTexTemplate"]));
            }
        }
    }
}
