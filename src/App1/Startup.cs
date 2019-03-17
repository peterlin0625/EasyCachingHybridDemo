﻿namespace App1
{
    using EasyCaching.Bus.CSRedis;
    using EasyCaching.Bus.Redis;
    using EasyCaching.Bus.RabbitMQ;
    using EasyCaching.Core;
    using EasyCaching.Core.Configurations;
    using EasyCaching.HybridCache;
    using EasyCaching.InMemory;
    using EasyCaching.Redis;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddEasyCaching(option =>
            {
                // local
                option.UseInMemory("m1");
                // distributed
                option.UseRedis(config =>
                {
                    config.DBConfig.Endpoints.Add(new ServerEndPoint("127.0.0.1", 6379));
                    config.DBConfig.Database = 5;
                }, "myredis");

                // combine local and distributed
                option.UseHybrid(config =>
                {
                    config.TopicName = "test-topic";
                    ////rabbitmq bus should use route key
                    //config.TopicName = "rmq.queue.undurable.easycaching.subscriber.*";

                    config.EnableLogging = true;

                    // specify the local cache provider name after v0.5.4
                    config.LocalCacheProviderName = "m1";
                    // specify the distributed cache provider name after v0.5.4
                    config.DistributedCacheProviderName = "myredis";
                })
                //// use redis bus
                // .WithRedisBus(busConf =>
                // {
                //     busConf.Endpoints.Add(new ServerEndPoint("127.0.0.1", 6379));
                // });
                //// use csredis bus
                //.WithCSRedisBus(busConf =>
                //{
                //    busConf.ConnectionStrings = new System.Collections.Generic.List<string>
                //    {
                //        "127.0.0.1:6379,defaultDatabase=13,poolsize=10"
                //    };
                //})
                //use rabbitmq bus
                .WithRabbitMQBus(busConf => 
                {
                    busConf = new RabbitMQBusOptions();
                });
            });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }
    }
}
