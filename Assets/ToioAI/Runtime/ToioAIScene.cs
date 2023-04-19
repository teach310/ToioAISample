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
            CubeManager cm;
            public Dictionary<string, Cube> cubes = new Dictionary<string, Cube>();

            public CubeCommandHandler(CubeManager cm)
            {
                this.cm = cm;
            }

            public void ShowMessage(string message)
            {
                Debug.Log(message);
            }

            public void Move(string id, int left, int right, int durationMs)
            {
                var cube = cubes[id];
                if (cube == null)
                {
                    Debug.LogError($"Cube {id} is not found.");
                    return;
                }

                if (!cm.IsControllable(cube))
                {
                    // 前回のCubeへの送信から45ミリ秒以上経過していない場合
                    Debug.Log($"Cube {id} is not controllable.");
                    return;
                }
                cube.Move(left, right, durationMs);
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
            var cubeCommandHandler = new CubeCommandHandler(cm);
            cubeCommandHandler.cubes["cube1"] = cube;

            luaEnv.Global.Set("cubeCommand", cubeCommandHandler.GetAdapter());
            luaEnv.DoString(@"
                cubeCommand:ShowMessage('Start!')
            ");
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                luaEnv.DoString(@"
                    cubeCommand:Move('cube1', 50, -50, 100)
                ");
            }
        }

        void OnDestroy()
        {
            luaEnv?.Dispose();
        }
    }
}
