using GraphicsPlayground.Graphics.Render;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphicsPlayground.Scripts.InternalScripts;

public class TestScript : IScript
{
    void IScript.OnLoad(Engine engine)
    {
        Console.WriteLine("Loaded TestScript");
    }

    void IScript.OnUnload()
    {
        Console.WriteLine("Unloaded TestScript");
    }

    void IScript.Run()
    {
        Console.WriteLine("TestScript Run");
    }

    bool IScript.ShouldUpdate => false;

    void IScript.Update()
    {
        Console.WriteLine("TestScript Update");
    }
}
