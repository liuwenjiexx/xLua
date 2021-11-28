using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;
using XLuaTest;

public class TestEvent : MonoBehaviour
{
    [CSharpCallLua]
    public static event Action<string> StringEvent;
    [CSharpCallLua]
    public static event Action<string, int> StringIntEvent;
    LuaFunction bindEvent;
    LuaFunction unbindEvent;

     

    // Start is called before the first frame update
    IEnumerator Start()
    {
        yield return null;
        var luaEnv = GetComponent<LuaBehaviour>().luaEnv;
        bindEvent = luaEnv.Global.Get<LuaFunction>("BindEvent");
        unbindEvent = luaEnv.Global.Get<LuaFunction>("UnbindEvent");
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnGUI()
    {
        if (GUILayout.Button("Trigger event"))
        {
            Debug.Log("Trigger event");
            StringEvent?.Invoke("abc");
            StringIntEvent?.Invoke("abc", 123);
        }

        using (new GUILayout.HorizontalScope())
        {
            if (GUILayout.Button("+"))
            {
                Debug.Log("Bind event");
                bindEvent.Call();
            }
            if (GUILayout.Button("-"))
            {
                Debug.Log("Unbind event");
                unbindEvent.Call();
            }
        }
    }
}
