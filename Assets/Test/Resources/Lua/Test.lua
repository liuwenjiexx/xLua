if not breakSocketHandle then
    breakSocketHandle,debugXpCall = require('LuaDebug')('localhost',7003)
end

local obj={
    arr={}
}
obj.arr[1]=1
obj.arr[2]=2
print("hello")
print(#obj.arr)

function  get_array( arr)

	print('array', arr:GetValue(0),arr:GetValue(1),arr:GetValue(2))    
    print('array', arr.Length)
end

get_array(array)