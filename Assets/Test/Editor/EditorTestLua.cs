using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;

public static  class EditorTestLua
{

    [CSharpCallLua]
    public static List<Type> s
    {
        get
        {
            List<Type> list = new List<Type>();
            list.AddRange(new Type[] {
                typeof(Action<string,int>)
            });
            return list;
        }
    }
}
