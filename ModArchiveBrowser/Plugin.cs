using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.IO;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using ModArchiveBrowser.Windows;
using HtmlAgilityPack;
using ModArchiveBrowser.Interop.Penumbra;
using Dalamud.Interface.ImGuiFileDialog;

namespace ModArchiveBrowser;

public sealed class Plugin : IDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ITextureProvider TextureProvider { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] internal static IPluginLog Logger { get; private set; } = null!;

    private const string CommandName = "/pmycommand";

    public Configuration Configuration { get; init; }

    public readonly WindowSystem WindowSystem = new("ModArchiveBrowser");

    public readonly PenumbraService penumbra;
    private ConfigWindow ConfigWindow { get; init; }
    private MainWindow MainWindow { get; init; }

    public SearchWindow searchWindow { get; init; }

    public FileDialogManager fileDialogManager = new FileDialogManager();
    public ModWindow modWindow { get; init; }

    public ImageHandler imageHandler = null!;
    public ModHandler modHandler = null!;

    public Plugin()
    {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        // you might normally want to embed resources and load them from the manifest stream
        //var goatImagePath = Path.Combine(PluginInterface.AssemblyLocation.Directory?.FullName!, "goat.png");
        imageHandler = new ImageHandler(Configuration.CacheImagePath);
        modHandler = new ModHandler(Configuration.CacheModPath,this);
        ConfigWindow = new ConfigWindow(this);
        modWindow = new ModWindow(this);
        MainWindow = new MainWindow(this);
        searchWindow = new SearchWindow(this);
        WindowSystem.AddWindow(ConfigWindow);
        WindowSystem.AddWindow(MainWindow);
        WindowSystem.AddWindow(modWindow);
        WindowSystem.AddWindow(searchWindow);
        penumbra = new PenumbraService(PluginInterface,this);

        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "A useful message to display in /xlhelp"
        });

        PluginInterface.UiBuilder.Draw += DrawUI;

        // This adds a button to the plugin installer entry of this plugin which allows
        // to toggle the display status of the configuration ui
        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUI;

        // Adds another button that is doing the same but for the main ui of the plugin
        PluginInterface.UiBuilder.OpenMainUi += ToggleMainUI;
    }

    public void Dispose()
    {
        WindowSystem.RemoveAllWindows();

        ConfigWindow.Dispose();
        MainWindow.Dispose();
        modWindow.Dispose();
        searchWindow.Dispose();
        CommandManager.RemoveHandler(CommandName);
        modHandler.Dispose();
        penumbra.Dispose();
    }

    private void OnCommand(string command, string args)
    {
        foreach(string path in modHandler._modNameToThumbnail.Keys)
        {
            Logger.Debug(path);
        }
    }

    private void DrawUI() => WindowSystem.Draw();

    public void ToggleConfigUI() => ConfigWindow.Toggle();
    public void ToggleMainUI() => MainWindow.Toggle();
}
