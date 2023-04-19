using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace ToioAI.Networking.Model
{
    [Serializable]
    public class CubesLuaRequest
    {
        [SerializeField] List<CubesLuaRequestMessage> messages;

        public CubesLuaRequest(List<CubesLuaRequestMessage> messages)
        {
            this.messages = messages;
        }
    }

    [Serializable]
    public class CubesLuaRequestMessage
    {
        public string role;
        public string content;

        public CubesLuaRequestMessage(string role, string content)
        {
            this.role = role;
            this.content = content;
        }
    }
}
