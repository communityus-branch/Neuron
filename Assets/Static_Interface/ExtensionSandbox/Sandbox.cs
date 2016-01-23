using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.Security.Policy;
namespace Static_Interface.ExtensionSandbox
{
    public class Sandbox
    {
        private readonly List<SecurityPermissionFlag> _permissions = new List<SecurityPermissionFlag>
        {
            SecurityPermissionFlag.Execution
        }; 

        private static Sandbox _instance;
        public static Sandbox Instance => _instance ?? (_instance = new Sandbox());
        private readonly List<string> _loadedFiles = new List<string>(); 
        private readonly Dictionary<Assembly, AppDomain> _domains = new Dictionary<Assembly, AppDomain>();  
        public AppDomain CreateAppDomain(string path)
        {
            Evidence evidence = new Evidence();
            evidence.AddHost(new Zone(SecurityZone.Untrusted));
            evidence.AddHost(new Url(path));
            PermissionSet permSet = new PermissionSet(PermissionState.None);
            foreach (SecurityPermissionFlag perm in _permissions)
            {
                permSet.AddPermission(new SecurityPermission(perm));
            }
            permSet.AddPermission(new UIPermission(PermissionState.Unrestricted));

            //Todo: loop trough all drives
            //Todo: whitelist for plugin directory
            permSet.AddPermission(new FileIOPermission(FileIOPermissionAccess.NoAccess, "C:\\"));
            
            AppDomainSetup ads = new AppDomainSetup();
            ads.ApplicationBase = path;
            return AppDomain.CreateDomain("ExtensionSandbox_" + path, 
                evidence, 
                ads);

            //Todo: Update to .Net 4 to support better sandboxing...
        }

        public bool LoadAssembly(string path, out Assembly loadedAssembly, out AppDomain domain)
        {
            loadedAssembly = null;
            domain = null;
            if (!File.Exists(path) || _loadedFiles.Contains(path)) return false;
            Type type = typeof(Proxy);
            domain = CreateAppDomain(Directory.GetParent(path).FullName);
            var value = (Proxy)domain.CreateInstanceAndUnwrap(
                type.Assembly.FullName,
                type.FullName);

            loadedAssembly= value.GetAssembly(path);
            _loadedFiles.Add(path);
            _domains.Add(loadedAssembly, domain);
            return true;
        }

        public bool UnloadAssembly(Assembly asm)
        {
            if (!_domains.ContainsKey(asm)) return false;
            AppDomain domain = _domains[asm];
            AppDomain.Unload(domain);
            return true;
        }

        private class Proxy : MarshalByRefObject
        {
            public Assembly GetAssembly(string assemblyPath)
            {
                try
                {
                    return Assembly.LoadFile(assemblyPath);
                }
                catch (Exception)
                {
                    return null;
                    // throw new InvalidOperationException(ex);
                }
            }
        }
    }
}