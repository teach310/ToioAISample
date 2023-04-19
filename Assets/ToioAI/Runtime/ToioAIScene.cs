using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ToioAI.CommandInterfaces;
using XLua;
using toio;
using UnityEngine.UI;

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

        [SerializeField] Text label;
        public ConnectType connectType;
        CubeManager cm;
        LuaEnv luaEnv;

        async void Start()
        {
            cm = new CubeManager(connectType);
            var cube = await cm.SingleConnect();
            label.text = "connected";

            luaEnv = new LuaEnv();
            var cubeCommand = new CubeCommandHandler();
            luaEnv.Global.Set("cube", cubeCommand.GetAdapter());
            luaEnv.DoString(@"
                cube:ShowMessage('Hello World!')
            ");
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                foreach(var cube in cm.syncCubes)
                {
                    cube.Move(50, -50, 100);
                }
            }
        }

        void OnDestroy()
        {
            luaEnv?.Dispose();
        }
    }
}
