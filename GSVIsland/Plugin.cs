
using ClassIsland.Core;
using ClassIsland.Core.Abstractions;
using ClassIsland.Core.Attributes;
using ClassIsland.Core.Controls.CommonDialog;
using ClassIsland.Core.Extensions.Registry;
using GSVIsland.Controls.SpeechProviderSettingsControls;
using GSVIsland.Services.SpeechService;
using GSVIsland.Shared;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO;

namespace GSVIsland
{
    [PluginEntrance]
    public class Plugin : PluginBase
    {
        public override void Initialize(HostBuilderContext context, IServiceCollection services)
        {
            services.AddSpeechProvider<GSVIService, GSVISpeechServiceSettingsControl>();
            GlobalConstants.PluginConfigFolder = PluginConfigFolder;
            Console.WriteLine(Path.GetDirectoryName(PluginConfigFolder));
        }
    }
}
