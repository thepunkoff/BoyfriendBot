using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AutoMapper;
using BoyfriendBot.Domain.AppSettings;
using BoyfriendBot.Domain.Commands;
using BoyfriendBot.Domain.Core;
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
            var logPath = Configuration.GetValue<string>("LoggingAbsolutePath");
            var listeningServiceFullLogPath = Path.Combine(logPath, "boyfriend-bot-listeningService.log");
            var generalFullLogPath = Path.Combine(logPath, "boyfriend-bot-general.log");

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Error)

                .WriteTo.Logger(listeningServiceLoggerConfig => listeningServiceLoggerConfig
                    .WriteTo.RollingFile(listeningServiceFullLogPath,
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}")
                    .Filter.ByIncludingOnly(x => x.MessageTemplate.Text.Contains(Const.Serilog.ListeningService))
                )
                .WriteTo.Logger(generalLoggerConfig => generalLoggerConfig
                    .WriteTo.RollingFile(generalFullLogPath,
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}")
                    .Filter.ByExcluding(x => x.MessageTemplate.Text.Contains(Const.Serilog.ListeningService))
                )
                .CreateLogger();

            services
                // Configuration
                .Configure<DatabaseAppSettings>(Configuration.GetSection("Database"))
                .Configure<ScheduledMessageServiceAppSettings>(Configuration.GetSection("ScheduledMessageService"))
                .Configure<ListeningServiceAppSettings>(Configuration.GetSection("ListeningService"))
                .Configure<DateTimeGeneratorAppSettings>(Configuration.GetSection("DateTimeGenerator"))
                .Configure<ResourceManagerAppSettings>(Configuration.GetSection("ResourceManager"))
                
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
                .AddTransient<IInlineKeyboardMenuParser, InlineKeyboardMenuParser>()
                .AddTransient<IMessageTextTransformer, MessageTextTransformer>()
                .AddTransient<IRandomFactGenerator, RandstuffruRandomFactGenerator>()
                .AddTransient<IQueryImageDownloader, YandexQueryImageDownloader>()
                .AddTransient<IInputProcessor, InputProcessor>()
                .AddTransient<IStringAnalyzer, StringAnalyzer>()
                .AddTransient<IExpressionBuilder, ExpressionBuilder>()
                .AddTransient<IResourceManager, ResourceManager>()
                .AddTransient<IImageProvider, ImageProvider>()
                .AddSingleton<ISessionManagerSingleton, SessionManagerSingleton>()
                .AddTransient<ISessionDataProcessor, SessionDataProcessor>()
                .AddTransient<ISessionBootstrapper, SessionBootstrapper>()

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
