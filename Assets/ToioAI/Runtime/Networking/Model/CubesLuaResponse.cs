using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace ToioAI.Networking.Model
{
    [Serializable]
    public class CubesLuaResponse
    {
        public CubesLuaResponse()
        {
        }

        [SerializeField] CubesLuaResponseData data;
        public CubesLuaResponseData Data { get => data; set => data = value; }
    }

    [Serializable]
    public class CubesLuaResponseData
    {
        public CubesLuaResponseData()
        {
        }

        public string content;
        public string Content { get => content; set => content = value; }
    }
}
