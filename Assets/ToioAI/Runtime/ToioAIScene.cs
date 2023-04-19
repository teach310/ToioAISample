using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ToioAI.CommandInterfaces;
using XLua;
using toio;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using System;

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
        [SerializeField] Button sendButton;
        [SerializeField] InputField inputField;
        public ConnectType connectType;
        CubeManager cm;
        CubeLuaGenerator luaGenerator;
        LuaEnv luaEnv;
        bool isProcessing = false;

        async void Start()
        {
            cm = new CubeManager(connectType);
            var cube = await cm.SingleConnect();
            label.text = "connected";
            cube.Move(1,1, 10); // 初回Controllableがfalse返るので、一度動かしておく

            luaEnv = new LuaEnv();
            var cubeCommandHandler = new CubeCommandHandler(cm);
            cubeCommandHandler.cubes["cube1"] = cube;
            luaGenerator = new CubeLuaGenerator();

            luaEnv.Global.Set("cubeCommand", cubeCommandHandler.GetAdapter());

            // routineは上書きする
            luaEnv.DoString(@"
                local util = require 'xlua.util'

                function getCsRoutine()
                    return util.cs_generator(routine);
                end

                function routine()
                    cubeCommand:ShowMessage('2')
                    coroutine.yield(CS.UnityEngine.WaitForSeconds(1))
                    cubeCommand:ShowMessage('1')
                    coroutine.yield(CS.UnityEngine.WaitForSeconds(1))
                    cubeCommand:ShowMessage('Start!')
                end
            ");
            inputField.onSubmit.AddListener((message) =>
            {
                TryStartProcess();
            });
        }

        void OnDestroy()
        {
            luaEnv?.Dispose();
        }

        public void OnClickSend()
        {
            TryStartProcess();
        }

        void TryStartProcess()
        {
            if(isProcessing || string.IsNullOrEmpty(inputField.text))
            {
                return;
            }

            ProcessAsync(inputField.text).Forget();
        }

        async UniTaskVoid ProcessAsync(string message)
        {
            isProcessing = true;
            label.text = "processing...";
            inputField.interactable = false;
            sendButton.interactable = false;
            try
            {
                var content = await luaGenerator.GenerateAsync(message);
                Debug.Log(content);
                luaEnv.DoString(content);
                await LuaCoroutine();
            }
            finally
            {
                inputField.text = string.Empty;
                inputField.interactable = true;
                sendButton.interactable = true;
                isProcessing = false;
                label.text = "connected";
            }
        }

        async UniTaskVoid RunLuaAsync()
        {
            isProcessing = true;
            label.text = "processing...";
            try
            {
                await LuaCoroutine();
            }
            finally
            {
                isProcessing = false;
                label.text = "connected";
            }
        }

        IEnumerator LuaCoroutine()
        {
            var routine = luaEnv.Global.Get<Func<IEnumerator>>("getCsRoutine").Invoke();
            yield return routine;
        }
    }
}
