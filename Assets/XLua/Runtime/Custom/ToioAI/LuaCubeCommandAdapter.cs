using System.Collections;
using System.Collections.Generic;
using ToioAI.CommandInterfaces;
using UnityEngine;

namespace XLua.Custom
{
    public class LuaCubeCommandAdapter
    {
        ICubeCommand command;

        public LuaCubeCommandAdapter(ICubeCommand command)
        {
            this.command = command;
        }

        public void ShowMessage(string message) => command.ShowMessage(message);
    }
}
