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
            public Dictionary<string, int> cubeIndexMap = new Dictionary<string, int>();

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

            public IEnumerator Navi2TargetCoroutine(string id, double x, double y, int rotateTime = 250, float timeout = 5f)
            {
                while (!cm.synced)
                {
                    yield return null;
                }

                float startTime = Time.time;
                var navigator = cm.navigators[cubeIndexMap[id]];

                Movement movement = navigator.Navi2Target(x, y, rotateTime).Exec();
                while (!movement.reached && Time.time - startTime < timeout)
                {
                    yield return null;
                    if (cm.synced)
                    {
                        movement = navigator.Navi2Target(x, y, rotateTime).Exec();
                    }
                }
                yield return null; // すぐに別の処理が入らないように1フレーム待つ
            }

            public IEnumerator Rotate2DegCoroutine(string id, double deg, int rotateTime = 250, float timeout = 5f)
            {
                while (!cm.synced)
                {
                    yield return null;
                }

                float startTime = Time.time;
                var handle = cm.handles[cubeIndexMap[id]];

                Movement movement = handle.Rotate2Deg(deg, rotateTime).Exec();
                while (!movement.reached && Time.time - startTime < timeout)
                {
                    yield return null;
                    if (cm.synced)
                    {
                        movement = handle.Rotate2Deg(deg, rotateTime).Exec();
                    }
                }
                yield return null; // すぐに別の処理が入らないように1フレーム待つ
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
        CubeCommandHandler cubeCommandHandler;
        CubeLuaGenerator luaGenerator;
        LuaEnv luaEnv;
        bool isProcessing = false;

        async void Start()
        {
            cm = new CubeManager(connectType);
            var cube = await cm.SingleConnect();
            label.text = "connected";
            cube.Move(1, 1, 10); // 初回Controllableがfalse返るので、一度動かしておく

            luaEnv = new LuaEnv();
            cubeCommandHandler = new CubeCommandHandler(cm);
            cubeCommandHandler.cubes["cube1"] = cube;
            cubeCommandHandler.cubeIndexMap["cube1"] = 0;
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

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                // Cubeの現在地を取得
                var cube = cm.cubes[0];
                UnityEngine.Debug.Log($"x: {cube.x}, y: {cube.y}");
                UnityEngine.Debug.Log(cube.isGrounded);

                // https://toio.github.io/toio-spec/docs/hardware_position_id/
                // 対象位置をランダムに決める x: 45~455, y: 45~455
                // example) left top: 45, 45
                var x = UnityEngine.Random.Range(45, 455);
                var y = UnityEngine.Random.Range(45, 455);
                UnityEngine.Debug.Log($"x: {x}, y: {y}");
            }

            if (Input.GetKeyDown(KeyCode.S))
            {
                luaEnv.DoString(@"
                    function routine()
                        cubeCommand:ShowMessage('Go to start position (=center) and look forward')
                        coroutine.yield(cubeCommand:Navi2TargetCoroutine('cube1', 250, 250))
                        coroutine.yield(cubeCommand:Rotate2DegCoroutine('cube1', -90))
                        cubeCommand:ShowMessage('Ready!')
                    end
                ");
                RunLuaAsync().Forget();
            }
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
            if (isProcessing || string.IsNullOrEmpty(inputField.text))
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
