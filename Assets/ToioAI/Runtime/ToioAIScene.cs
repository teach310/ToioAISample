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

            public int GetCubePosX(string id)
            {
                var cube = cubes[id];
                if (cube == null)
                {
                    Debug.LogError($"Cube {id} is not found.");
                    return 0;
                }
                return cube.x;
            }

            public int GetCubePosY(string id)
            {
                var cube = cubes[id];
                if (cube == null)
                {
                    Debug.LogError($"Cube {id} is not found.");
                    return 0;
                }
                return (int)ReversePosY(cube.y);
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
                
                var convertedY = ReversePosY(y);

                Movement movement = navigator.Navi2Target(x, convertedY, rotateTime).Exec();
                while (!movement.reached && Time.time - startTime < timeout)
                {
                    yield return null;
                    if (cm.synced)
                    {
                        movement = navigator.Navi2Target(x, convertedY, rotateTime).Exec();
                    }
                }
                yield return null; // すぐに別の処理が入らないように1フレーム待つ
            }

            // NOTE: 左上原点の座標系だとchatgptの精度落ちるため左下原点の座標系での点をもらう。
            double ReversePosY(double y)
            {
                return 500 - y;
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
        public int cubeCount = 2;
        CubeManager cm;
        CubeCommandHandler cubeCommandHandler;
        CubeLuaGenerator luaGenerator;
        LuaEnv luaEnv;
        bool isProcessing = false;

        async void Start()
        {
            cm = new CubeManager(connectType);
            cubeCommandHandler = new CubeCommandHandler(cm);
            luaGenerator = new CubeLuaGenerator();

            var cubes = await cm.MultiConnect(cubeCount);
            label.text = "connected";

            for (int i = 0; i < cubes.Length; i++)
            {
                string id = $"cube{i + 1}";
                cubeCommandHandler.cubes[id] = cubes[i];
                cubeCommandHandler.cubeIndexMap[id] = i;
                cubes[i].Move(1, 1, 10); // 初回Controllableがfalse返るので、一度動かしておく
                luaGenerator.AddCubeId(id);
            }

            luaEnv = new LuaEnv();
            

            luaEnv.Global.Set("cubeCommand", cubeCommandHandler.GetAdapter());
            Coroutine InvokeStartCoroutine(IEnumerator routine) => StartCoroutine(routine);
            luaEnv.Global.Set("csStartCoroutine", (Func<IEnumerator, Coroutine>)InvokeStartCoroutine);

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
            bool isDebugMode = false;
            if (isDebugMode)
            {
                // if (Input.GetKeyDown(KeyCode.R))
                // {
                //     // Cubeの現在地を取得
                //     var cube = cm.cubes[0];
                //     UnityEngine.Debug.Log($"x: {cube.x}, y: {cube.y}");
                //     UnityEngine.Debug.Log(cube.isGrounded);

                //     // https://toio.github.io/toio-spec/docs/hardware_position_id/
                //     // 対象位置をランダムに決める x: 45~455, y: 45~455
                //     // example) left top: 45, 45
                //     var x = UnityEngine.Random.Range(45, 455);
                //     var y = UnityEngine.Random.Range(45, 455);
                //     UnityEngine.Debug.Log($"x: {x}, y: {y}");
                // }

                if (Input.GetKeyDown(KeyCode.Q))
                {
                    luaEnv.DoString(GetSampleLuaScript());
                    RunLuaAsync().Forget();
                }

                if (Input.GetKeyDown(KeyCode.S))
                {
                    luaEnv.DoString(@"
                        function routine()
                            local positions = {
                                {x = 100, y = 100},
                                {x = 100, y = 400},
                                {x = 400, y = 400},
                                {x = 400, y = 100},
                            }

                            cubeCommand:ShowMessage('set cube1 and cube2 start position')
                            coroutine1 = csStartCoroutine(cubeCommand:Navi2TargetCoroutine('cube1', positions[2].x, positions[2].y))
                            coroutine2 = csStartCoroutine(cubeCommand:Navi2TargetCoroutine('cube2', positions[1].x, positions[1].y))
                            coroutine.yield(coroutine1)
                            coroutine.yield(cubeCommand:Rotate2DegCoroutine('cube1', 0))
                            coroutine.yield(coroutine2)
                            coroutine.yield(cubeCommand:Rotate2DegCoroutine('cube2', -90))
                            coroutine.yield(CS.UnityEngine.WaitForSeconds(0.5))

                            cubeCommand:ShowMessage('move each cube')
                            coroutine1 = csStartCoroutine(cubeCommand:Navi2TargetCoroutine('cube1', positions[3].x, positions[3].y))
                            coroutine.yield(CS.UnityEngine.WaitForSeconds(0.2))
                            coroutine2 = csStartCoroutine(cubeCommand:Navi2TargetCoroutine('cube2', positions[2].x, positions[2].y))
                            coroutine.yield(coroutine1)
                            coroutine.yield(cubeCommand:Rotate2DegCoroutine('cube1', 90))
                            coroutine.yield(coroutine2)
                            coroutine.yield(cubeCommand:Rotate2DegCoroutine('cube2', 0))
                            coroutine.yield(CS.UnityEngine.WaitForSeconds(0.5))

                            coroutine1 = csStartCoroutine(cubeCommand:Navi2TargetCoroutine('cube1', positions[4].x, positions[4].y))
                            coroutine.yield(CS.UnityEngine.WaitForSeconds(0.2))
                            coroutine2 = csStartCoroutine(cubeCommand:Navi2TargetCoroutine('cube2', positions[3].x, positions[3].y))
                            coroutine.yield(coroutine1)
                            coroutine.yield(cubeCommand:Rotate2DegCoroutine('cube1', 180))
                            coroutine.yield(coroutine2)
                            coroutine.yield(cubeCommand:Rotate2DegCoroutine('cube2', 90))

                            cubeCommand:ShowMessage('Finish!')
                        end
                    ");
                    RunLuaAsync().Forget();
                }
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
                await LuaCoroutine().ToUniTask(this);
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
                await LuaCoroutine().ToUniTask(this);
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

        string GetSampleLuaScript()
        {
            return @"
                local util = require 'xlua.util'

                function startCoroutine(coroutine, ...)
                    if type(coroutine) == 'function' then
                        return csStartCoroutine(util.cs_generator(coroutine, ...))
                    end
                    return csStartCoroutine(coroutine)
                end

                function routine()
                    cubeCommand:ShowMessage('Start!')
                    csCoroutine = startCoroutine(showIdLater, 'Hello')
                    coroutine.yield(csCoroutine)
                    coroutine1 = startCoroutine(cubeCommand:Navi2TargetCoroutine('cube1', 100, 100))
                    coroutine.yield(coroutine1)
                    cubeCommand:ShowMessage('Finished!')
                end

                function showIdLater(id)
                    coroutine.yield(CS.UnityEngine.WaitForSeconds(0.5))
                    cubeCommand:ShowMessage(id)
                end
            ";
        }
    }
}
