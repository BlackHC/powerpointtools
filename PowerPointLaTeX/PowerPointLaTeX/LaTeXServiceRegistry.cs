﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using PowerPointLaTeX.Properties;

namespace PowerPointLaTeX
{
    class LaTeXRenderingServiceRegistry
    {
        private Dictionary<string, ILaTeXRenderingService> services = new Dictionary<string, ILaTeXRenderingService>();
        private string[] serviceNames;

        public LaTeXRenderingServiceRegistry() {
            Initialize();
        }
        
        private void Initialize() {
            // from: http://www.codeproject.com/KB/architecture/CSharpClassFactory.aspx
            // Get the assembly that contains this code

            Assembly asm = Assembly.GetCallingAssembly();
            
            // Get a list of all the types in the assembly

            Type[] allTypes = asm.GetTypes();
            foreach (Type type in allTypes)
            {
                // Only scan classes that arn't abstract
                if (type.IsClass && !type.IsAbstract && type.GetInterface( typeof(ILaTeXRenderingService).Name ) != null)
                {
                    ILaTeXRenderingService service = (ILaTeXRenderingService)asm.CreateInstance(type.FullName);
                    services.Add(service.SeriveName, service);
                }
            }

            var keys = services.Keys;
            serviceNames = new string[keys.Count];
            keys.CopyTo(serviceNames, 0);
        }

        public ILaTeXRenderingService GetService(string serviceName) {
            return services[serviceName];
        }

        public string[] ServiceNames {
            get {
                return serviceNames;
            }
        }

        public ILaTeXRenderingService Service {
            get {
                ILaTeXRenderingService service;
                if( !services.TryGetValue( Settings.Default.LatexService, out service ) ) {
                    var keyValuePair = services.First();
                    Settings.Default.LatexService = keyValuePair.Key;
                    service = keyValuePair.Value;
                }
                return service;
            }
        }
        
    }
}
