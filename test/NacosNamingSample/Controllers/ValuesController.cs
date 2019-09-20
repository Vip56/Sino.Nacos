using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Sino.Nacos.Naming;
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
        }

        // GET api/values
        [HttpGet]
        public async Task<ActionResult<IEnumerable<string>>> Get()
        {
            //var t = Dns.GetHostEntry(Dns.GetHostName()).AddressList;
            var instance = new Instance()
            {
                Ip = "192.168.2.2",
                Port = 5000,
                Weight = 5,
                ClusterName = "tms",
                ServiceName = "tms_order_v1"
            };
            instance.Metadata.Add("k1", "v1");
            instance.Metadata.Add("k2", "v2");
            
            await _namingService.RegisterInstance("tms_order_v1", instance);
            await _namingService.RegisterInstance("tms_order_v1", "192.168.2.50", 5000);
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
