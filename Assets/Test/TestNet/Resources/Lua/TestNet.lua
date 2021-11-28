print("hello TestNet") 
NetMgr = require 'Net/NetMgr'

Proto = require("protobuf/LuaProtoBuf")
local protodata=require("Proto/Proto")
Proto:initialize({
    bytes =CS.UnityEngine.Resources.Load("Lua/Proto/Proto.pb", typeof(CS.UnityEngine.TextAsset)).bytes,
    CSCmd=protodata.cs,
    SCCmd=protodata.sc,
    -- CSCmd = require("Proto/ProtoCSCmd"),
    -- SCCmd = require("Proto/ProtoSCCmd")
})

NetMgr:initialize(Proto)
NetMgr:addNetStateHandler(function(state) 
    if state==1 then
        print("connected") 

        --test CS
        NetMgr:sendMessage("LoginRequest",{ userName="xxx",userPwd="yyy" },function(data)
        print("Response: LoginRequest");
        --test CS
        end )
    end
end, true)
 
--test SC
NetMgr:addListener("LoginResponse",function(data) 
    print("Login response. accessToken: ".. data.accessToken) 
end)
--test SC

NetMgr:connect("127.0.0.1", "8080")

