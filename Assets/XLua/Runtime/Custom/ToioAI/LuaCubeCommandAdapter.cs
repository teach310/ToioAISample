using System.Collections;
using System.Collections.Generic;
using ToioAI.CommandInterfaces;
using UnityEngine;

namespace XLua.Custom
{
    [LuaCallCSharp]
    public class LuaCubeCommandAdapter
    {
        ICubeCommand command;

        public LuaCubeCommandAdapter(ICubeCommand command)
        {
            this.command = command;
        }

        public void ShowMessage(string message) => command.ShowMessage(message);
        public void Move(string id, int left, int right, int durationMs) => command.Move(id, left, right, durationMs);
    }
}
