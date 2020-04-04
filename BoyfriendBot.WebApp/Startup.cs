using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AutoMapper;
using BoyfriendBot.Domain.AppSettings;
using BoyfriendBot.Domain.Commands;
using BoyfriendBot.Domain.Data.Context;
using BoyfriendBot.Domain.Data.Context.Interfaces;
using BoyfriendBot.Domain.Infrastructure.Mapping;
using BoyfriendBot.Domain.Services;
using BoyfriendBot.Domain.Services.Hosted;
using BoyfriendBot.Domain.Services.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Telegram.Bot;

namespace BoyfriendBot.WebApp
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Error)
                .WriteTo.RollingFile("C:/Logs/BoyfriendBot/boyfriend-bot.log",
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}")
                .CreateLogger();

            services
                // Configuration
                .Configure<DatabaseAppSettings>(Configuration.GetSection("Database"))
                .Configure<MessageTextProviderAppSettings>(Configuration.GetSection("MessageTextProvider"))
                .Configure<ScheduledMessageServiceAppSettings>(Configuration.GetSection("ScheduledMessageService"))
                .Configure<ListeningServiceAppSettings>(Configuration.GetSection("ListeningService"))
                .Configure<DateTimeGeneratorAppSettings>(Configuration.GetSection("DateTimeGenerator"))
                .Configure<InlineKeyboardMenuParserAppSettings>(Configuration.GetSection("InlineKeyboardMenuParser"))
                
                .AddSingleton(Configuration)

                // Hosted
                .AddHostedService<ScheduledMessageService>()
                .AddHostedService<ListeningService>()

                // Services
                .AddSingleton<IUserStorage, DoubleUserStorage>()
                .AddTransient<IBotMessageProvider, BotMessageProvider>()
                .AddTransient<ITelegramBotClientWrapper, TelegramBotClientWrapper>()
                .AddTransient<ITelegramClient, TelegramClient>()
                .AddTransient<IBulkMessagingTelegramClient, BulkMessagingTelegramClient>()
                .AddSingleton<IMonitoringManager, MonitoringManager>()
                .AddTransient<IDateTimeGenerator, DateTimeGenerator>()
                .AddTransient<IMessageSchedule, InMemoryMessageSchedule>()
                .AddTransient<IRarityRoller, RarityRoller>()
                .AddSingleton<IEventManager, EventManager>()
                .AddSingleton<IInlineKeyboardMenuParser, InlineKeyboardMenuParser>()
                .AddSingleton<IMessageTextTransformer, MessageTextTransformer>()
                .AddSingleton<IRandomFactGenerator, RandstuffruRandomFactGenerator>()
                .AddSingleton<IRandomImageProvider, YandexRandomImageProvider>()
                

                // Database
                .AddDbContext<IBoyfriendBotDbContext, BoyfriendBotDbContext>(ServiceLifetime.Transient)
                .AddTransient<IBoyfriendBotDbContextFactory, BoyfriendBotDbContextFactory>()

                // Commands
                .AddTransient<ICommandProcessor, CommandProcessor>()

                .AddTransient<NullCommand>()
                .AddTransient<SendMenuCommand>()
                .AddTransient<SetSettingCommand>()

                // Other
                .AddAutoMapper(typeof(MessageToUserDboProfile))
                .AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });
        }
    }
}
