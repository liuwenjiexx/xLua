local pb = require 'pb'
local protoc = require 'protobuf/protoc'


---@class Proto
local Proto = {}

local protoCSCmd
local protoSCCmd
local decodeed = true;

local default_cache_tb = {}
local __decode = pb.decode

local __G_ERROR_TRACK = __G_ERROR_TRACK

local decode = function(typename, buffer)
    if nil ~= buffer then
        local ret = __decode(typename, buffer);
        if false == ret then
            print(string.format("<color = #FF0000>%s</color>",err.."\t"..typename));
            decodeed = false;
            return false;
        end
        return ret
    end
    --default data
    local def = default_cache_tb[typename];
    if nil == def then
        def = pb.default(typename, {});
        default_cache_tb[typename] = def;
    end
    return def;
end

function Proto:initialize(options)
    protoCSCmd = options.CSCmd
    protoSCCmd = options.SCCmd
    pb.load(options.bytes)

end

function Proto:encode( type,msg,data )
    local p = nil
    local msgId = nil
    local errorMsg = nil

    if type == "cs" then
        local name = protoCSCmd.msg[msg]
        if name == nil then
            error("serialize cmd is null > "..msg)
        else
            p, errorMsg = pb.encode(name,data)
            msgId = protoCSCmd.msgToId[msg]
        end
    elseif type == "sc" then
        local name = protoSCCmd.msg[msg]
        if name == nil then
            error("serialize cmd is null > "..msg)
        else
            p, errorMsg =pb.encode(name,data)
            msgId = protoCSCmd.msgToId[msg]
        end
    else
        error("serialize type is error > "..type)
    end
    if errorMsg then
        error("serialize type is error > "..msg .. "  " .. errorMsg)
    end
    return p,msgId
end

function Proto:decode( type,cmd,data )
    local p = nil
    local ok,err = xpcall(function()
        if type == "cs" then
            local name = protoCSCmd.id[cmd]
            if name == nil then
                error("serialize cmd is null > "..cmd)
            else
                p =  decode(protoCSCmd.id[cmd],data)
            end
        elseif type == "sc" then
            local name = protoSCCmd.id[cmd]
            if name == nil then
                error("serialize cmd is null > "..cmd)
            else
                p =  decode(name,data)
            end
        else
            error("serialize type is error > "..type)
        end
    end,function()
        if __G_ERROR_TRACK then
            __G_ERROR_TRACK(cmd)
        else
            print("ERROR",debug.traceback(cmd))
        end
    end,33)
    if not ok then
        if __G_ERROR_TRACK then
            __G_ERROR_TRACK(cmd)
        else
            print("ERROR",debug.traceback(cmd))
        end
    else
        return p
    end
end

function Proto:getCSCmd( msg )
    return protoCSCmd.msgToId[msg]
end

function Proto:getSCCmd(msg)
    return protoSCCmd.msgToId[msg]
end

return Proto