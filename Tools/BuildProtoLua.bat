@echo off
cd BuildProtobuf
BuildProtobuf.exe -Protoc="..\protoc.exe" -Source="..\..\Assets\Test\TestProtobuf\Proto" -PB="..\..\Assets\Test\TestProtobuf\Resources\Lua\Proto\Proto.pb.bytes" -Lua="..\..\Assets\Test\TestProtobuf\Resources\Lua\Proto\Proto.lua"