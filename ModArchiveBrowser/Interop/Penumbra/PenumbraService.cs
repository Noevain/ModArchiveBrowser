using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Penumbra.Api;
using Dalamud.Plugin;
using static FFXIVClientStructs.FFXIV.Client.Game.Character.Character.Delegates;
using Penumbra.Api.Enums;
using Penumbra.Api.Helpers;
using Penumbra.Api.IpcSubscribers;


namespace ModArchiveBrowser.Interop.Penumbra
{

    //trying to model this after Glamourer inplementation of PenumbraService
    //https://github.com/Ottermandias/Glamourer/blob/main/Glamourer/Interop/Penumbra/PenumbraService.cs
    //
    public class PenumbraService : IDisposable
    {
        public const int RequiredPenumbraBreakingVersion = 5;
        public const int RequiredPenumbraFeatureVersion = 0;

        private readonly IDalamudPluginInterface _pluginInterface;
        private global::Penumbra.Api.IpcSubscribers.GetModList? _getMods;
        private global::Penumbra.Api.IpcSubscribers.OpenMainWindow? _openModPage;
        private global::Penumbra.Api.IpcSubscribers.InstallMod? _installMod;
        private EventSubscriber<string, float, float>? _preSettingsTabBarDraw;

        private readonly IDisposable _initializedEvent;
        private readonly IDisposable _disposedEvent;

        private PenumbraWindowIntegration _windowIntegration;
        public bool Available { get; private set; }
        public int CurrentMajor { get; private set; }
        public int CurrentMinor { get; private set; }
        public DateTime AttachTime { get; private set; }
        public PenumbraService(IDalamudPluginInterface pi,Plugin plugin)
        {
            _pluginInterface = pi;
            _initializedEvent = global::Penumbra.Api.IpcSubscribers.Initialized.Subscriber(pi, Reattach);
            _disposedEvent = global::Penumbra.Api.IpcSubscribers.Disposed.Subscriber(pi, Unattach);
            _windowIntegration = new PenumbraWindowIntegration(plugin);
            _preSettingsTabBarDraw = global::Penumbra.Api.IpcSubscribers.PreSettingsTabBarDraw.Subscriber(pi,_windowIntegration.PreSettingsTabBarDraw);
            Reattach();
        }

        public PenumbraApiEc InstallMod(in string path)
        {
            if (!Available)
            {
                return PenumbraApiEc.UnknownError;
            }

            try
            {
                return(_installMod!.Invoke(path));
            }catch (Exception ex)
            {
                Plugin.Logger.Debug($"Could not queue mod for install:\n{ex}");
                return PenumbraApiEc.UnknownError;
            }
        }

        public PenumbraApiEc OpenModWindow()
        {
            if (!Available)
            {
                return PenumbraApiEc.UnknownError;
            }
            else
            {
                try
                {
                    return (_openModPage!.Invoke(TabType.Mods));
                }
                catch (Exception ex)
                {
                    Plugin.Logger.Debug($"Could not open mod window:\n{ex}");
                    return PenumbraApiEc.UnknownError;
                }
            }
        }


        
        /// <summary> Reattach to the currently running Penumbra IPC provider. Unattaches before if necessary. </summary>
        public void Reattach()
        {
            try
            {
                Unattach();

                AttachTime = DateTime.UtcNow;
                try
                {
                    (CurrentMajor, CurrentMinor) = new global::Penumbra.Api.IpcSubscribers.ApiVersion(_pluginInterface).Invoke();
                }
                catch
                {
                    try
                    {
                        (CurrentMajor, CurrentMinor) = new global::Penumbra.Api.IpcSubscribers.Legacy.ApiVersions(_pluginInterface).Invoke();
                    }
                    catch
                    {
                        CurrentMajor = 0;
                        CurrentMinor = 0;
                        throw;
                    }
                }

                if (CurrentMajor != RequiredPenumbraBreakingVersion || CurrentMinor < RequiredPenumbraFeatureVersion)
                    throw new Exception(
                        $"Invalid Version {CurrentMajor}.{CurrentMinor:D4}, required major Version {RequiredPenumbraBreakingVersion} with feature greater or equal to {RequiredPenumbraFeatureVersion}.");

                _getMods = new global::Penumbra.Api.IpcSubscribers.GetModList(_pluginInterface);
                _openModPage = new global::Penumbra.Api.IpcSubscribers.OpenMainWindow(_pluginInterface);
                _installMod = new global::Penumbra.Api.IpcSubscribers.InstallMod(_pluginInterface);
                //_preSettingsTabBarDraw = global::Penumbra.Api.IpcSubscribers.PreSettingsTabBarDraw.Subscriber(_pluginInterface, _windowIntegration.PreSettingsTabBarDraw);
                Available = true;
                Plugin.Logger.Debug("modarchivebrowser attached to Penumbra.");
            }
            catch (Exception e)
            {
                Unattach();
                Plugin.Logger.Debug($"Could not attach to Penumbra:\n{e}");
            }
        }

        /// <summary> Unattach from the currently running Penumbra IPC provider. </summary>
        public void Unattach()
        {
            if (Available)
            {
                _openModPage = null;
                _installMod = null;
                _getMods = null;
                Available = false;
                //_preSettingsTabBarDraw?.Dispose();
                Plugin.Logger.Debug("modarchivebrowser detached from Penumbra.");
            }
        }

        public void Dispose()
        {
            Unattach();
            _preSettingsTabBarDraw?.Dispose();
            _initializedEvent.Dispose();
            _disposedEvent.Dispose();
        }



    }
}
