using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CSObjectWrapEditor;
using UnityEditor;

namespace XLuaCustomEditor
{
    // This class changes Generate Code Output Path From XLua/Gen to XLua/Runtime/Gen
    public class CustomGenerator
    {
#if XLUA_GENERAL
        public static string genPath = "./Gen/";
#else
        public static string genPath = Application.dataPath + "/XLua/Runtime/Gen/";
#endif

        [MenuItem("XLua/Generate Code", validate = true)]
        public static bool DisableOriginalGenAll()
        {
            return false;
        }

        [MenuItem("XLua/Clear Generated Code", validate = true)]
        public static bool DisableOriginalClearGen()
        {
            return false;
        }

        [MenuItem("XLua/Custom/Generate Code", false, 1)]
        public static void GenAll()
        {
            GeneratorConfig.common_path = genPath;
            Generator.GenAll();
        }

        [MenuItem("XLua/Custom/Clear Generated Code", false, 2)]
        public static void ClearGen()
        {
            GeneratorConfig.common_path = genPath;
            Generator.ClearAll();
        }
    }
}
