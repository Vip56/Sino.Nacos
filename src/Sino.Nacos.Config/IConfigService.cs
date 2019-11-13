using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Sino.Nacos.Config
{
    /// <summary>
    /// 配置服务接口
    /// </summary>
    public interface IConfigService
    {
        /// <summary>
        /// 获取配置
        /// </summary>
        /// <param name="dataId">数据编号</param>
        /// <param name="group">分组</param>
        Task<string> GetConfig(string dataId, string group);

        /// <summary>
        /// 获取并监听配置
        /// </summary>
        /// <param name="dataId">数据编号</param>
        /// <param name="group">分组</param>
        /// <param name="listener">监听回调</param>
        Task<string> GetConfigAndSignListener(string dataId, string group, Action<string> listener);

        /// <summary>
        /// 监听配置
        /// </summary>
        /// <param name="dataId">数据编号</param>
        /// <param name="group">分组</param>
        /// <param name="listener">监听回调</param>
        Task AddListener(string dataId, string group, Action<string> listener);

        /// <summary>
        /// 发布配置
        /// </summary>
        /// <param name="dataId">数据编号</param>
        /// <param name="group">分组</param>
        /// <param name="content">配置内容</param>
        Task<bool> PublishConfig(string dataId, string group, string content);

        /// <summary>
        /// 删除配置
        /// </summary>
        /// <param name="dataId">数据编号</param>
        /// <param name="group">分组</param>
        Task<bool> RemoveConfig(string dataId, string group);

        /// <summary>
        /// 删除监听
        /// </summary>
        /// <param name="dataId">数据编号</param>
        /// <param name="group">分组</param>
        /// <param name="listener">监听</param>
        void RemoveListener(string dataId, string group, Action<string> listener);

        /// <summary>
        /// 获取服务状态
        /// </summary>
        string GetServerStatus();
    }
}
