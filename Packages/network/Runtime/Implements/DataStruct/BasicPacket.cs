namespace  Yoozoo.Managers.NetworkV2.DataStruct
{
    public class BasicPacket : IPacket
    {
        //主要用来装载，除了messageId意外其他的包头内容
        public object customData;
    }
}