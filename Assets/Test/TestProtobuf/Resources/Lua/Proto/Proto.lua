-- ***该文件为自动生成的***
local package = "Example"
local p= {
[1] = package..".LoginRequest",
[2] = package..".LoginResponse"
}
return {
  cs = {
    id = {
[10001] = p[1]
},
    msg = {
["LoginRequest"] = p[1]
},
  msgToId = {
["LoginRequest"] = 10001
}
},
sc ={
    id = {
[10002] = p[2]
},
    msg = {
["LoginResponse"] = p[2]
},
  msgToId = {
["LoginResponse"] = 10002
}
}
}
