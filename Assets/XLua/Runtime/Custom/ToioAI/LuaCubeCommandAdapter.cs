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
        public IEnumerator Navi2TargetCoroutine(string id, double x, double y) => command.Navi2TargetCoroutine(id, x, y, 250, 5f);
        public IEnumerator Rotate2DegCoroutine(string id, double deg) => command.Rotate2DegCoroutine(id, deg, 250, 5f);
    }
}
