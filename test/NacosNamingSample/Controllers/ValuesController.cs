using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Sino.Nacos.Naming;
using Sino.Nacos.Naming.Listener;
using Sino.Nacos.Naming.Model;

namespace NacosNamingSample.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private INamingService _namingService;

        public ValuesController(INamingService namingService)
        {
            _namingService = namingService;
            _namingService.RegisterInstance("tms_order", "192.168.2.1", 5000).Wait();
        }

        [HttpGet("register")]
        public async Task<ActionResult<string>> Register()
        {
            //var t = Dns.GetHostEntry(Dns.GetHostName()).AddressList;

            await _namingService.RegisterInstance("tms_order", new Instance()
            {
                Ip = "192.168.2.1",
                Port = 5000,
                Weight = 1,
                ClusterName = "hz",
                ServiceName = "tms_order_v1"
            });
            await _namingService.RegisterInstance("tms_order", "tms", new Instance()
            {
                Ip = "192.168.2.2",
                Port = 5001,
                Weight = 2,
                ClusterName = "hz",
                ServiceName = "tms_order_v2"
            });
            await _namingService.RegisterInstance("tms_order", "192.168.2.3", 5002);
            await _namingService.RegisterInstance("tms_order", "192.168.2.4", 5003, "hz");
            await _namingService.RegisterInstance("tms_order", "tms", "192.168.2.5", 5004);
            await _namingService.RegisterInstance("tms_order", "tms", "192.168.2.6", 5005, "hz");

            return "register";
        }

        [HttpGet("deregister")]
        public async Task<ActionResult<string>> Deregister()
        {
            await _namingService.DeregisterInstance("tms_order_v1", new Instance()
            {
                Ip = "192.168.2.1",
                Port = 5000,
                Weight = 1,
                ClusterName = "hz",
                ServiceName = "tms_order_v1"
            });

            await _namingService.DeregisterInstance("tms_order_v2", "tms", new Instance()
            {
                Ip = "192.168.2.2",
                Port = 5001,
                Weight = 2,
                ClusterName = "hz",
                ServiceName = "tms_order_v2"
            });

            await _namingService.DeregisterInstance("tms_order_v3", "192.168.2.3", 5002);
            await _namingService.DeregisterInstance("tms_order_v4", "192.168.2.4", 5003, "hz");
            await _namingService.DeregisterInstance("tms_order_v5", "tms", "192.168.2.5", 5004);
            await _namingService.DeregisterInstance("tms_order_v6", "tms", "192.168.2.6", 5005, "hz");

            return "deregisster";
        }

        // POST api/values
        [HttpGet("getAllInstance")]
        public async Task<ActionResult<string>> GetAllInstance()
        {
            await _namingService.RegisterInstance("tms_order", "192.168.2.1", 5000);
            await _namingService.RegisterInstance("tms_order", "192.168.2.2", 5001);
            await _namingService.RegisterInstance("tms_order", "192.168.2.3", 5002);
            await _namingService.RegisterInstance("tms_order", "tms", "192.168.2.4", 5003);

            var instance = await _namingService.GetAllInstances("tms_order");
            instance = await _namingService.GetAllInstances("tms_inquiry");
            instance = await _namingService.GetAllInstances("tms_order", "tms");
            instance = await _namingService.GetAllInstances("tms_order", "test");
            instance = await _namingService.GetAllInstances("tms_order", true);
            //instance = await _namingService.GetAllInstances("tms_order", false);
            instance = await _namingService.GetAllInstances("tms_order", "tms", false);
            //instance = await _namingService.GetAllInstances("tms_order", "tms", true);
            instance = await _namingService.GetAllInstances("tms_order", new List<string>() { "test" }); ;
            instance = await _namingService.GetAllInstances("tms_order", new List<string>() { "test" }, false);
            //instance = await _namingService.GetAllInstances("tms_order", "tms", new List<string>() { "test" }, true);
            instance = await _namingService.GetAllInstances("tms_order", "tms", new List<string>() { "test" }, false);

            return "getAllInstance";
        }

        [HttpGet("selectInstance")]
        public async Task<ActionResult<string>> SelectInstance()
        {
            await _namingService.RegisterInstance("tms_order", "192.168.2.1", 5000);
            await _namingService.RegisterInstance("tms_order", "192.168.2.2", 5001);
            await _namingService.RegisterInstance("tms_order", "192.168.2.3", 5002);
            await _namingService.RegisterInstance("tms_order", "tms", "192.168.2.4", 5003);

            var instance = await _namingService.SelectInstances("tms_order", true);
            instance = await _namingService.SelectInstances("tms_order", false);
            //instance = await _namingService.SelectInstances("tms_inquiry", true);
            //instance = await _namingService.SelectInstances("tms_inquiry", false);
            //instance = await _namingService.SelectInstances("tms_order", "tms", true);
            //instance = await _namingService.SelectInstances("tms_order", "tms", false);
            //instance = await _namingService.SelectInstances("tms_inquiry", "tms", true);
            //instance = await _namingService.SelectInstances("tms_order", true, false);
            //instance = await _namingService.SelectInstances("tms_order", "tms", true, false);
            //instance = await _namingService.SelectInstances("tms_order", new List<string>() { "test" }, true);
            //instance = await _namingService.SelectInstances("tms_order", "tms", new List<string>() { "test" }, true);
            //instance = await _namingService.SelectInstances("tms_order", new List<string>() { "test" }, true, false);
            //instance = await _namingService.SelectInstances("tms_order", "tms", new List<string>() { "test" }, true, false);

            return "selectInstance";
        }

        [HttpGet("selectOne")]
        public async Task<ActionResult<string>> SelectOne()
        {
            await _namingService.RegisterInstance("tms_order", "192.168.2.1", 5000);
            await _namingService.RegisterInstance("tms_order", "192.168.2.2", 5001);
            await _namingService.RegisterInstance("tms_order", "192.168.2.3", 5002);
            await _namingService.RegisterInstance("tms_order", "tms", "192.168.2.4", 5003);

            var instance = await _namingService.SelectOneHealthyInstance("tms_order");
            instance = await _namingService.SelectOneHealthyInstance("tms_order", "tms");
            instance = await _namingService.SelectOneHealthyInstance("tms_order", false);
            instance = await _namingService.SelectOneHealthyInstance("tms_order", "tms", false);
            //instance = await _namingService.SelectOneHealthyInstance("tms_order", new List<string>() { "test" });
            //instance = await _namingService.SelectOneHealthyInstance("tms_order", "tms", new List<string>() { "test" });
            //instance = await _namingService.SelectOneHealthyInstance("tms_inquiry");

            return "selectOne";
        }

        [HttpGet("subscribe")]
        public async Task<ActionResult<string>> Subscribe()
        {
            await _namingService.RegisterInstance("tms_order", "192.168.2.1", 5000);

            await _namingService.Subscribe("tms_order", OnUpdate);

            return "subscribe";
        }

        [HttpGet("update")]
        public async Task<ActionResult<string>> Update()
        {
            await _namingService.RegisterInstance("tms_order", "192.168.2.3", 5003);

            return "update";
        }

        [HttpGet("unsubscribe")]
        public async Task<ActionResult<string>> UnSubscribe()
        {
            _namingService.Unsubscribe("tms_order", OnUpdate);

            return "unsubscribe";
        }

        public void OnUpdate(IEvent ent)
        {
            var e = ent as NamingEvent;
        }

        [HttpGet("loadtest")]
        public async Task LoadTest()
        {
            Stopwatch watch = Stopwatch.StartNew();


            ParallelLoopResult result = Parallel.For(0, 100001, async x =>
             {
                 var instance = await _namingService.SelectOneHealthyInstance("tms_order");
                 if (x == 100000)
                 {
                     watch.Stop();
                     var t = watch.ElapsedMilliseconds;
                     Console.WriteLine($"Time is {t}");
                 }
             });
        }
    }
}
