using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Media;
using Buddy.Coroutines;
using ff14bot;
using ff14bot.AClasses;
using ff14bot.Behavior;
using ff14bot.Enums;
using ff14bot.Managers;
using ff14bot.Navigation;
using ff14bot.NeoProfiles;
using ff14bot.Pathing.Service_Navigation;
using ff14bot.RemoteWindows;
using LlamaLibrary.Enums;
using LlamaLibrary.Extensions;
using LlamaLibrary.Helpers;
using LlamaLibrary.Logging;
using LlamaLibrary.Memory;
using LlamaLibrary.RemoteAgents;
using LlamaLibrary.RemoteWindows;
using LlamaLibrary.Structs;
using Newtonsoft.Json;
using TreeSharp;

namespace LlamaBotBases.GCExpertTurnin
{
    public class GCExpertGrindBase : BotBase
    {
        private static string _name = "GCExpertGrind";
        public override string Name => _name;

        public override PulseFlags PulseFlags => PulseFlags.All;

        public override bool IsAutonomous => true;
        public override bool RequiresProfile => false;

        private GCExpertSettingsFrm settings;

        public override Composite Root => _root;

        private Composite _root;

        public override bool WantButton => true;

        private static readonly LLogger Log = new LLogger(_name, Colors.SeaGreen);

        public GCExpertGrindBase()
        {
            OffsetManager.Init();
        }

        public override void OnButtonPress()
        {
            if (settings == null || settings.IsDisposed)
            {
                settings = new GCExpertSettingsFrm();
            }

            try
            {
                settings.Show();
                settings.Activate();
            }
            catch (ArgumentOutOfRangeException)
            {
            }
        }

        public override void Start()
        {
            Navigator.PlayerMover = new SlideMover();
            Navigator.NavigationProvider = new ServiceNavigationProvider();
            _root = new ActionRunCoroutine(r => Run());
        }

        private async Task<bool> Run()
        {
            if (!GCExpertSettings.Instance.AcceptedRisk)
            {
                Log.Error("**************************Attention*********************************");
                Log.Error("Please go to settings");
                Log.Error("This botbase will turn in any gear that's in your inventory to the GC expert delivery npc");
                Log.Error("You need to set Accepted to true to show you understand the risk");
                TreeRoot.Stop("Stop Requested");
                return true;
            }

            await GeneralFunctions.StopBusy();

            Log.Information("Turning in current items:");

            await HandInExpert();

            if (GCExpertSettings.Instance.Craft)
            {
                Log.Information("Crafting set to true");
                Log.Information($"{Core.Me.MaxGCSeals()} - {GCExpertSettings.Instance.SealReward} < {Core.Me.GCSeals()}");
                while (Core.Me.GCSeals() < Core.Me.MaxGCSeals() - GCExpertSettings.Instance.SealReward)
                {
                    Log.Information("Generating Lisbeth order");
                    var currentSeals = Core.Me.GCSeals();
                    var neededSeals = (Core.Me.MaxGCSeals() - currentSeals) - (GCExpertSettings.Instance.SealReward * ConditionParser.ItemCount((uint)GCExpertSettings.Instance.ItemId));
                    var qty = (int)(neededSeals / GCExpertSettings.Instance.SealReward);
                    Log.Information($"Need {qty}");
                    var freeSlots = InventoryManager.FreeSlots;
                    var couldCraft = Math.Min(freeSlots - 10, qty);
                    Log.Information($"Order Would be for {couldCraft}");

                    var outList = new List<LisbethOrder>
                    {
                        new LisbethOrder(0, 1, GCExpertSettings.Instance.ItemId, (int)couldCraft, ((ClassJobType)DataManager.GetItem((uint)GCExpertSettings.Instance.ItemId).RepairClass).ToString(), true)
                    };

                    var order = JsonConvert.SerializeObject(outList, Formatting.None);

                    await GeneralFunctions.StopBusy();
                    Log.Information($"Calling Lisbeth with {order}");
                    try
                    {
                        await Lisbeth.ExecuteOrders(order);
                    }
                    catch (Exception e)
                    {
                        Log.Error($"{e}");
                    }

                    await GeneralFunctions.StopBusy();
                    await HandInExpert();
                }
            }

            TreeRoot.Stop("Stop Requested");
            return true;
        }

        public static async Task HandInExpert()
        {
            if (GCExpertSettings.Instance.Craft && ConditionParser.ItemCount((uint)GCExpertSettings.Instance.ItemId) == 0)
            {
                return;
            }

            await LlamaLibrary.Helpers.GrandCompanyHelper.GCHandInExpert();


        }
    }
}