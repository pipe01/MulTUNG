using System;
using System.Collections.Generic;
using System.Reflection;
using MulTUNG.Properties;

public static class Preloader
{
    public static void Preload()
    {
        AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
    }
    
    private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
    {
        var dic = new Dictionary<string, byte[]>
        {
            ["Lidgren.Network"] = Resources.Lidgren_Network
        };

        var name = new AssemblyName(args.Name).Name;

        if (dic.TryGetValue(name, out var b))
        {
            return Assembly.Load(b);
        }

        return null;
    }
}