using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System;
using System.Threading;
using System.Text;
using ToioAI.Networking;
using ToioAI.Networking.Model;

namespace ToioAITests
{
    public class APITests
    {
        [UnityTest]
        public IEnumerator POST_toio_cubes_lua() => UniTask.ToCoroutine(async () =>
        {
            var request = new CubesLuaRequest(new List<CubesLuaRequestMessage>()
            {
                new CubesLuaRequestMessage("system", "あなたはluaのコードジェネレータです。"),
                new CubesLuaRequestMessage("user", "toio動かすコードください"),
            });
            var api = new API();
            var res = await api.CubesLua(request);
            Debug.Log(res.Data.Content);
            Assert.That(res.Data.Content, Is.Not.Null);
        });

    }
}
