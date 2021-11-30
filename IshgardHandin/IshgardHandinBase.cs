using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using Buddy.Coroutines;
using ff14bot;
using ff14bot.AClasses;
using ff14bot.Behavior;
using ff14bot.Managers;
using ff14bot.Navigation;
using ff14bot.Pathing.Service_Navigation;
using LlamaLibrary.Extensions;
using LlamaLibrary.Helpers;
using LlamaLibrary.Logging;
using LlamaLibrary.Memory;
using LlamaLibrary.RemoteWindows;
using LlamaLibrary.ScriptConditions;
using TreeSharp;

namespace LlamaBotBases.IshgardHandin
{
    public class IshgardHandinBase : BotBase
    {
        private static readonly string _name = "Ishgard Handin";
        public override string Name => _name;

        private static readonly LLogger Log = new LLogger(_name, Colors.Aquamarine);

        public override PulseFlags PulseFlags => PulseFlags.All;
        public override bool IsAutonomous => true;
        public override bool RequiresProfile => false;
        public override bool WantButton { get; } = false;

        private Composite _root;
        public override Composite Root => _root;

        public IshgardHandinBase()
        {
            OffsetManager.Init();
        }

        public override void Start()
        {
            _root = new ActionRunCoroutine(r => Run());
        }

        private async Task<bool> Run()
        {
            await LlamaLibrary.Utilities.Ishgard.Handin();

            TreeRoot.Stop("Stop Requested");
            return true;
        }

        public override void Stop()
        {
            _root = null;
        }

    }
}