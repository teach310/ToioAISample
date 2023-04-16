using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ToioAI.CommandInterfaces;
using XLua;

namespace ToioAI
{
    public class ToioAIScene : MonoBehaviour
    {
        class CubeCommandHandler : ICubeCommand
        {
            public void ShowMessage(string message)
            {
                Debug.Log(message);
            }

            public XLua.Custom.LuaCubeCommandAdapter GetAdapter()
            {
                return new XLua.Custom.LuaCubeCommandAdapter(this);
            }
        }

        public void Start()
        {
            var luaEnv = new LuaEnv();
            var cubeCommand = new CubeCommandHandler();
            luaEnv.Global.Set("cube", cubeCommand.GetAdapter());
            luaEnv.DoString(@"
                cube:ShowMessage('Hello World!')
            ");
            luaEnv.Dispose();
        }
    }
}
