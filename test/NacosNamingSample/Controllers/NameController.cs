using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Sino.Nacos.Config;

namespace NacosNamingSample.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NameController : Controller
    {
        private IConfigService _configService;

        public NameController(IConfigService service)
        {
            _configService = service;
        }

        [HttpGet("config")]
        public async Task<ActionResult<string>> GetConfig(string name)
        {
            if (string.IsNullOrEmpty(name))
                name = "default";

            string content = await _configService.GetConfig(name, "tms");

            if (string.IsNullOrEmpty(content))
                content = "NotFound";

            return content;
        }

        [HttpGet("publish")]
        public async Task<ActionResult<string>> Publish(string name, string value)
        {
            if (string.IsNullOrEmpty(name))
                name = "default";
            if (string.IsNullOrEmpty(value))
                value = "value";

            bool result = await _configService.PublishConfig(name, "tms", value);

            if (result)
            {
                return "OK";
            }
            else
            {
                return "Failed";
            }
        }

        [HttpGet("listener")]
        public async Task<ActionResult<string>> Listener(string name)
        {
            if (string.IsNullOrEmpty(name))
                name = "default";

            await _configService.AddListener(name, "tms", x =>
            {
                Console.WriteLine($"Config is change, value is {x}");
            });

            return "OK";
        }

        [HttpGet("delete")]
        public async Task<ActionResult<string>> Remove(string name)
        {
            if (string.IsNullOrEmpty(name))
                name = "default";

            bool result = await _configService.RemoveConfig(name, "tms");

            if (result)
            {
                return "OK";
            }
            else
            {
                return "Failed";
            }
        }

        [HttpGet("status")]
        public ActionResult<string> GetServerStatus()
        {
            return _configService.GetServerStatus();
        }

        private static Action<string> _listener;

        [HttpGet("addlistener")]
        public async Task<ActionResult<string>> AddListener()
        {
            if (_listener == null)
            {
                _listener = x =>
                {
                    Console.WriteLine($"[Config change the value is {x}]");
                };
                await _configService.AddListener("lis", "tms", _listener);
            }
            return "OK";
        }

        [HttpGet("deletelistener")]
        public ActionResult<string> DeleteListener()
        {
            if (_listener != null)
            {
                _configService.RemoveListener("lis", "tms", _listener);
            }

            return "OK";
        }
    }
}