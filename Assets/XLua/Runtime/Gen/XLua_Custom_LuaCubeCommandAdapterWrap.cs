#if USE_UNI_LUA
using LuaAPI = UniLua.Lua;
using RealStatePtr = UniLua.ILuaState;
using LuaCSFunction = UniLua.CSharpFunctionDelegate;
#else
using LuaAPI = XLua.LuaDLL.Lua;
using RealStatePtr = System.IntPtr;
using LuaCSFunction = XLua.LuaDLL.lua_CSFunction;
#endif

using XLua;
using System.Collections.Generic;


namespace XLua.CSObjectWrap
{
    using Utils = XLua.Utils;
    public class XLuaCustomLuaCubeCommandAdapterWrap 
    {
        public static void __Register(RealStatePtr L)
        {
			ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			System.Type type = typeof(XLua.Custom.LuaCubeCommandAdapter);
			Utils.BeginObjectRegister(type, L, translator, 0, 4, 0, 0);
			
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "ShowMessage", _m_ShowMessage);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "Move", _m_Move);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "Navi2TargetCoroutine", _m_Navi2TargetCoroutine);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "Rotate2DegCoroutine", _m_Rotate2DegCoroutine);
			
			
			
			
			
			Utils.EndObjectRegister(type, L, translator, null, null,
			    null, null, null);

		    Utils.BeginClassRegister(type, L, __CreateInstance, 1, 0, 0);
			
			
            
			
			
			
			Utils.EndClassRegister(type, L, translator);
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int __CreateInstance(RealStatePtr L)
        {
            
			try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
				if(LuaAPI.lua_gettop(L) == 2 && translator.Assignable<ToioAI.CommandInterfaces.ICubeCommand>(L, 2))
				{
					ToioAI.CommandInterfaces.ICubeCommand _command = (ToioAI.CommandInterfaces.ICubeCommand)translator.GetObject(L, 2, typeof(ToioAI.CommandInterfaces.ICubeCommand));
					
					var gen_ret = new XLua.Custom.LuaCubeCommandAdapter(_command);
					translator.Push(L, gen_ret);
                    
					return 1;
				}
				
			}
			catch(System.Exception gen_e) {
				return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
			}
            return LuaAPI.luaL_error(L, "invalid arguments to XLua.Custom.LuaCubeCommandAdapter constructor!");
            
        }
        
		
        
		
        
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_ShowMessage(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                XLua.Custom.LuaCubeCommandAdapter gen_to_be_invoked = (XLua.Custom.LuaCubeCommandAdapter)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    string _message = LuaAPI.lua_tostring(L, 2);
                    
                    gen_to_be_invoked.ShowMessage( _message );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_Move(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                XLua.Custom.LuaCubeCommandAdapter gen_to_be_invoked = (XLua.Custom.LuaCubeCommandAdapter)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    string _id = LuaAPI.lua_tostring(L, 2);
                    int _left = LuaAPI.xlua_tointeger(L, 3);
                    int _right = LuaAPI.xlua_tointeger(L, 4);
                    int _durationMs = LuaAPI.xlua_tointeger(L, 5);
                    
                    gen_to_be_invoked.Move( _id, _left, _right, _durationMs );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_Navi2TargetCoroutine(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                XLua.Custom.LuaCubeCommandAdapter gen_to_be_invoked = (XLua.Custom.LuaCubeCommandAdapter)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    string _id = LuaAPI.lua_tostring(L, 2);
                    double _x = LuaAPI.lua_tonumber(L, 3);
                    double _y = LuaAPI.lua_tonumber(L, 4);
                    
                        var gen_ret = gen_to_be_invoked.Navi2TargetCoroutine( _id, _x, _y );
                        translator.PushAny(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_Rotate2DegCoroutine(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                XLua.Custom.LuaCubeCommandAdapter gen_to_be_invoked = (XLua.Custom.LuaCubeCommandAdapter)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    string _id = LuaAPI.lua_tostring(L, 2);
                    double _deg = LuaAPI.lua_tonumber(L, 3);
                    
                        var gen_ret = gen_to_be_invoked.Rotate2DegCoroutine( _id, _deg );
                        translator.PushAny(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        
        
        
        
        
		
		
		
		
    }
}
