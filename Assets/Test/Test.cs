using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;

public class Test : LuaBehaviour
{

    // Start is called before the first frame update
    void Start()
    {

        //Action<int, ref int> call;
        //call = Add;

        luaEnv.Global.SetInPath("array", new int[] { 1, 2, 3 });
        luaEnv.DoString("require 'Test'");
    }

    static int Add(int a,ref int b)
    {
        return a + b;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
