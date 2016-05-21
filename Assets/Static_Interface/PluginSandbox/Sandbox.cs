using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using Static_Interface.API.Utils;

namespace Static_Interface.PluginSandbox
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
        private readonly List<AppDomain> _domains = new List<AppDomain>();
        public AppDomain CreateAppDomain(string file)
        {
            PermissionSet permSet = new PermissionSet(PermissionState.None);
            foreach (SecurityPermissionFlag perm in _permissions)
            {
                permSet.AddPermission(new SecurityPermission(perm));
            }

            var parentDir = Directory.GetParent(file).FullName;

            string[] allowedFiles = { parentDir, IOUtil.GetPluginsDir() };
            permSet.AddPermission(new FileIOPermission(FileIOPermissionAccess.Read, allowedFiles));
            permSet.AddPermission(new FileIOPermission(FileIOPermissionAccess.Write, allowedFiles));
            permSet.AddPermission(new FileIOPermission(FileIOPermissionAccess.PathDiscovery, allowedFiles));

            permSet.AddPermission(new FileIOPermission(FileIOPermissionAccess.Read, GetType().Assembly.Location));

#if !UNITY_EDITOR
            AppDomainSetup setup = new AppDomainSetup();

            setup.ApplicationBase =
                          AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            var ad = AppDomain.CreateDomain("PluginSandbox_" + file, 
                null, setup, permSet);
            return ad;
#else
            return AppDomain.CurrentDomain;
#endif



            //Todo: Update this when updating to .NET 4.0 or higher
        }

        public void LoadAssembly(string path, Action<bool, Assembly, AppDomain> callback)
        {
            if (!File.Exists(path) || _loadedFiles.Contains(path))
            {
                callback.Invoke(_loadedFiles.Contains(path), null, null);
                return;
            }

            //TODO: DEBUG CODE: REMOVE BEFORE RELEASE!!!!!
            //Todo: Disabled app domains for development purporses
            //var domain = CreateAppDomain(path);

            //var proxy = (SandboxProxy)
            //  domain.CreateInstanceAndUnwrap(
            //      typeof(SandboxProxy).Assembly.FullName,
            //      typeof(SandboxProxy).FullName);


            //AssemblyName[] trustedAssemblies = GetType().Assembly.GetReferencedAssemblies();
            //foreach (AssemblyName asm in trustedAssemblies)
            //{
            //    proxy.LoadAssemby(asm);
            //}

            //proxy.LoadAssemby(GetType().Assembly.GetName());
            //var loadedAssembly = proxy.LoadAssemby(path);
            //callback.Invoke(true, loadedAssembly, domain);


            callback.Invoke(true, Assembly.LoadFrom(path), AppDomain.CurrentDomain);
            _loadedFiles.Add(path);
            //_domains.Add(domain);
        }

        [Serializable]
        public class SandboxProxy : MarshalByRefObject
        {
            public Assembly LoadAssemby(AssemblyName asm)
            {
                return Assembly.Load(asm);
            }

            public Assembly LoadAssemby(string file)
            {
                return Assembly.LoadFrom(file);
            }
        }

        public bool UnloadAssembly(Assembly asm)
        {
            LogUtils.Debug("Unloading assembly: " + asm.Location);
            foreach (AppDomain domain in _domains)
            {
                if (domain.GetAssemblies().Contains(asm))
                {
                    AppDomain.Unload(domain);
                    _domains.Remove(domain);
                    break;
                }
            }

            /* This is only for development enviroment: Unlock related files, otherwise the files will remain locked even 
             * after stopping the game from the editor, which would require to restart the editor everytime someone wants
             * to update the plugins            
             */
            foreach (var f in asm.GetFiles())
                try
                {
                    f.Unlock(0, f.Length);
                }
                catch (IOException)
                {
                    // ignore
                }
            return true;
        }

        public interface ISandboxInitCallback
        {
            void OnSandboxInited(Assembly asm, AppDomain domain);
        }
    }
}