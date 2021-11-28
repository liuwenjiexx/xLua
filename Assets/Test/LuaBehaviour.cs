using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using XLua;

public class LuaBehaviour : MonoBehaviour
{
    public LuaEnv luaEnv = new LuaEnv();

    public string mainLua;

    #region Protobuf


#if (UNITY_IPHONE || UNITY_TVOS || UNITY_WEBGL || UNITY_SWITCH) && !UNITY_EDITOR
        const string LUADLL = "__Internal";
#else
    const string LUADLL = "xlua";
#endif


    [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
    public static extern int luaopen_pb(System.IntPtr L);

    [MonoPInvokeCallback(typeof(XLua.LuaDLL.lua_CSFunction))]
    public static int LoadPb(System.IntPtr L)
    {
        return luaopen_pb(L);
    }



    #endregion

    void Awake()
    {
        #region Protobuf

        luaEnv.AddBuildin("pb", LoadPb);

        #endregion


        luaEnv.AddLoader((ref string filePath) =>
        {
            var txt = Resources.Load<TextAsset>("Lua/" + filePath);
            if (txt)
            {
                return txt.bytes;
            }
            return null;
        });
    }
    protected virtual void Start()
    {
        luaEnv.DoString($"require '{mainLua}'");
        
    }

}
