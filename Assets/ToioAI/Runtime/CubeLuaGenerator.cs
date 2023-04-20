using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using ToioAI.Networking;
using ToioAI.Networking.Model;
using System.Text;

namespace ToioAI
{
    public class CubeLuaGenerator
    {
        List<string> cubeIds = new List<string>();

        // TODO: 履歴をキャッシュ
        // TODO: CancellationToken伝搬
        public async UniTask<string> GenerateAsync(string message)
        {
            var request = new CubesLuaRequest(new List<CubesLuaRequestMessage>()
            {
                new CubesLuaRequestMessage("system", CubeIdsSystemMessage()),
                new CubesLuaRequestMessage("user", message),
            });
            var api = new API();
            var res = await api.CubesLua(request); // いったんエラーハンドリングはせず、そのままエラー投げちゃう。
            return GetLua(res.Data.Content);
        }

        public void AddCubeId(string id)
        {
            cubeIds.Add(id);
        }

        string CubeIdsSystemMessage()
        {
            var sb = new StringBuilder();
            sb.AppendLine("## Cube IDs\n");
            foreach (var id in cubeIds)
            {
                sb.AppendLine(id);
            }
            return sb.ToString();
        }

        string GetLua(string resContent)
        {
            return @$"
local util = require 'xlua.util'

function startCoroutine(coroutine, ...)
    if type(coroutine) == 'function' then
        return csStartCoroutine(util.cs_generator(coroutine, ...))
    end
    return csStartCoroutine(coroutine)
end

{resContent}
            ";
        }
    }
}
