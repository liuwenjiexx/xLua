print("hello TestEvent")  

stringEventtHandle=function(str)
    print("string event: ",str)
end
stringIntEventHandle=function(str,n)
    print("string int event: ",str,n)
end
BindEvent=function()
    print("Bind Event")
    CS.TestEvent.StringEvent("+",stringEventtHandle)
    CS.TestEvent.StringIntEvent("+",stringIntEventHandle)
end

UnbindEvent=function()
    print("Unbind Event")
    CS.TestEvent.StringEvent("-",stringEventtHandle)
    CS.TestEvent.StringIntEvent("-",stringIntEventHandle)
end