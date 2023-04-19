using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using ToioAI.Networking;
using ToioAI.Networking.Model;

namespace ToioAI
{
    public class CubeLuaGenerator
    {
        // TODO: 履歴をキャッシュ
        // TODO: CancellationToken伝搬
        public async UniTask<string> GenerateAsync(string message)
        {
            var request = new CubesLuaRequest(new List<CubesLuaRequestMessage>()
            {
                new CubesLuaRequestMessage("system", "あなたはluaのコードジェネレータです。"),
                new CubesLuaRequestMessage("user", message),
            });
            var api = new API();
            var res = await api.CubesLua(request); // いったんエラーハンドリングはせず、そのままエラー投げちゃう。
            return res.Data.Content;
        }
    }
}
