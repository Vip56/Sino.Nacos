namespace Sino.Nacos.Naming
{
    /// <summary>
    /// 提供实例附加数据
    /// </summary>
    public class PreservedMetadataKeys
    {
        /// <summary>
        /// 注册来源
        /// </summary>
        public const string REGISTER_SOURCE = "preserved.register.source";

        /// <summary>
        /// 心跳超时时间
        /// </summary>
        public const string HEART_BEAT_TIMEOUT = "preserved.heart.beat.timeout";

        /// <summary>
        /// 实例移除超时时间
        /// </summary>
        public const string IP_DELETE_TIMEOUT = "preserved.ip.delete.timeout";

        /// <summary>
        /// 心跳间隔时间
        /// </summary>
        public const string HEART_BEAT_INTERVAL = "preserved.heart.beat.interval";
    }
}
