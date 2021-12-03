using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Buddy.Coroutines;
using Clio.Utilities;
using ff14bot;
using ff14bot.AClasses;
using ff14bot.Behavior;
using ff14bot.Enums;
using ff14bot.Managers;
using ff14bot.Navigation;
using ff14bot.Pathing.Service_Navigation;
using ff14bot.RemoteWindows;
using LlamaLibrary;
using LlamaLibrary.Enums;
using LlamaLibrary.Extensions;
using LlamaLibrary.Helpers;
using LlamaLibrary.JsonObjects;
using LlamaLibrary.Logging;
using LlamaLibrary.Memory;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.RemoteAgents;
using LlamaLibrary.RemoteWindows;
using LlamaLibrary.Retainers;
using LlamaLibrary.Structs;
using LlamaLibrary.Utilities;
using Newtonsoft.Json;
using TreeSharp;
using static ff14bot.RemoteWindows.Talk;

namespace LlamaBotBases.Tester
{
    public class TesterBase : BotBase
    {
        private static readonly LLogger Log = new LLogger("Tester", Colors.Pink);

        private readonly SortedDictionary<string, List<string>> luaFunctions = new SortedDictionary<string, List<string>>();

        private Composite _root;

        public Dictionary<string, List<Composite>> hooks;

        private static readonly InventoryBagId[] RetainerBagIds =
        {
            InventoryBagId.Retainer_Page1, InventoryBagId.Retainer_Page2, InventoryBagId.Retainer_Page3,
            InventoryBagId.Retainer_Page4, InventoryBagId.Retainer_Page5, InventoryBagId.Retainer_Page6,
            InventoryBagId.Retainer_Page7
        };

        private static readonly InventoryBagId[] SaddlebagIds =
        {
            (InventoryBagId) 0xFA0, (InventoryBagId) 0xFA1 //, (InventoryBagId) 0x1004,(InventoryBagId) 0x1005
        };

        private static readonly ItemUiCategory[] GatheringCategories =
        {
            ItemUiCategory.Lumber, ItemUiCategory.Stone, ItemUiCategory.Reagent, ItemUiCategory.Reagent, ItemUiCategory.Bone, ItemUiCategory.Ingredient
        };

        public TesterBase()
        {
            Task.Factory.StartNew(() =>
            {
                Init();
                Log.Information("INIT DONE");
            });
        }

        public override string Name => "Tester";
        public override PulseFlags PulseFlags => PulseFlags.All;

        public override bool IsAutonomous => true;
        public override bool RequiresProfile => false;

        public override Composite Root => _root;

        public override bool WantButton { get; } = true;

        private static Random _rand = new Random();

        public override void OnButtonPress()
        {
            /*
             DumpLuaFunctions();
            StringBuilder sb1 = new StringBuilder();
            foreach (var obj in luaFunctions.Keys.Where(obj => luaFunctions[obj].Count >= 1))
            {
                sb1.AppendLine(obj);
                foreach (var funcName in luaFunctions[obj])
                {
                    sb1.AppendLine($"\t{funcName}");
                }
            }

            Log.Information($"\n {sb1}");
            */
            DumpOffsets();
            DumpLLOffsets();
        }

        internal void Init()
        {
            OffsetManager.Init();
            OffsetManager.SetOffsetClasses();
        }

        private static T LoadResource<T>(string text)
        {
            return JsonConvert.DeserializeObject<T>(text);
        }

        public override void Start()
        {
            Navigator.PlayerMover = new SlideMover();
            Navigator.NavigationProvider = new ServiceNavigationProvider();
            _root = new ActionRunCoroutine(r => Run());
        }

        public override void Stop()
        {
            _root = null;
            (Navigator.NavigationProvider as IDisposable)?.Dispose();
            Navigator.NavigationProvider = null;
        }

        private async Task<bool> Run()
        {
            Log.Information("Nothing to test, this does nothing right now");

            //await HelperFunctions.ForceGetRetainerData();

            var retainers = await HelperFunctions.GetOrderedRetainerArray(true);

            foreach (var retainer in retainers)
            {
                await RetainerRoutine.SelectRetainer(retainer.Unique);

                Log.Information("Should be at the retainer");

                //await Coroutine.Sleep(5000);

                var belts = InventoryManager.GetBagByInventoryBagId(InventoryBagId.Retainer_Market).Where(i => i.Item.EquipmentCatagory == ItemUiCategory.Waist);

                foreach (var belt in belts)
                {
                    belt.RetainerRetrieveQuantity(belt.Count);
                    await Coroutine.Sleep(1000);
                }



                await RetainerRoutine.DeSelectRetainer();
            }

            TreeRoot.Stop("Stop Requested");

            return true;
        }

        private void LogPtr(IntPtr instancePointer)
        {
            Log.Information(instancePointer.ToString("X"));
        }

        private Task TestHook()
        {
            Log.Information("LL hook");
            return Task.CompletedTask;
        }

        private void DumpLLOffsets()
        {
            var sb = new StringBuilder();
            var sb1 = new StringBuilder();
            var sb2 = new StringBuilder();
            foreach (var patternItem in OffsetManager.patterns.OrderBy(k => k.Key))
            {
                var name = patternItem.Key;
                var pattern = patternItem.Value.Replace("Search ", "");

                if (name.ToLowerInvariant().Contains("vtable") && name.ToLowerInvariant().Contains("agent"))
                {
                    Log.Information($"Agent_{name}, {pattern}");
                    sb1.AppendLine($"{name.Replace("Vtable", "").Replace("vtable", "").Replace("VTable", "").Replace("_", "")}, {pattern}");
                }
                else if (!name.ToLowerInvariant().Contains("exd"))
                {
                    Log.Information($"{name}, {pattern}");
                    sb.AppendLine($"{name}, {pattern}");
                }
            }

            foreach (var patternItem in OffsetManager.constants)
            {
                var name = patternItem.Key;
                var pattern = patternItem.Value.Replace("Search ", "");
                sb2.AppendLine($"{name}, {pattern}");
            }

            using (var outputFile = new StreamWriter(@"G:\LLOffsets\AgentLL.csv", false))
            {
                outputFile.Write(sb1.ToString());
            }

            using (var outputFile = new StreamWriter(@"G:\LLOffsets\LL.csv", false))
            {
                outputFile.Write(sb.ToString());
            }

            using (var outputFile = new StreamWriter(@"G:\LLOffsets\Constants.csv", false))
            {
                outputFile.Write(sb2.ToString());
            }

            sb = new StringBuilder();
            var i = 0;
            foreach (var vtable in AgentModule.AgentVtables)
            {
                sb.AppendLine($"Model_{i},{Core.Memory.GetRelative(vtable).ToString("X")}");
                i++;
            }

            using (var outputFile = new StreamWriter(@"G:\AgentOffsets.csv", false))
            {
                outputFile.Write(sb.ToString());
            }
        }

        private async Task BuyHouse()
        {
            var _rnd = new Random();

            var placard = GameObjectManager.GetObjectsByNPCId(2002736).OrderBy(i => i.Distance()).FirstOrDefault();
            if (placard != null)
            {
                do
                {
                    if (!HousingSignBoard.Instance.IsOpen)
                    {
                        placard.Interact();
                        await Coroutine.Wait(3000, () => HousingSignBoard.Instance.IsOpen);
                    }

                    if (HousingSignBoard.Instance.IsOpen)
                    {
                        if (HousingSignBoard.Instance.IsForSale)
                        {
                            await Coroutine.Sleep(_rnd.Next(200, 400));
                            HousingSignBoard.Instance.ClickBuy();
                            await Coroutine.Wait(3000, () => Conversation.IsOpen);
                            if (Conversation.IsOpen)
                            {
                                await Coroutine.Sleep(_rnd.Next(50, 300));
                                Conversation.SelectLine(0);
                                await Coroutine.Wait(3000, () => SelectYesno.IsOpen);
                                SelectYesno.Yes();
                                await Coroutine.Sleep(_rnd.Next(23, 600));
                            }
                        }
                    }

                    await Coroutine.Sleep(_rnd.Next(1500, 3000));
                    placard.Interact();
                    await Coroutine.Wait(3000, () => HousingSignBoard.Instance.IsOpen);
                }
                while (HousingSignBoard.Instance.IsForSale);

                await Coroutine.Wait(3000, () => HousingSignBoard.Instance.IsOpen);
                HousingSignBoard.Instance.Close();
                await Coroutine.Wait(3000, () => !HousingSignBoard.Instance.IsOpen);
                Lua.DoString("return _G['EventHandler']:Shutdown();");
            }
        }

        private void DumpOffsets()
        {
            var off = typeof(Core).GetProperty("Offsets", BindingFlags.NonPublic | BindingFlags.Static);
            var stringBuilder = new StringBuilder();
            var i = 0;
            var p1 = 0;
            var p2 = 0;
            foreach (var p in off.PropertyType.GetFields())
            {
                var tp = p.GetValue(off.GetValue(null));
                p1 = 0;
                p2 = 0;
                foreach (var t in p.FieldType.GetFields())
                {
                    //stringBuilder.Append(string.Format("\nField: {0} \t", p2));

                    if (t.FieldType == typeof(IntPtr))
                    {
                        //IntPtr ptr = new IntPtr(((IntPtr) t.GetValue(tp)).ToInt64() - Core.Memory.ImageBase.ToInt64());
                        var ptr = (IntPtr) t.GetValue(tp);
                        stringBuilder.Append($"Struct{i + 88}_IntPtr{p1}, {Core.Memory.GetRelative(ptr).ToInt64()}\n");

                        //stringBuilder.Append(string.Format("\tPtr Offset_{0}: 0x{1:x}", p1, ptr.ToInt64()));

                        p1++;
                    }

                    p2++;
                }

                //stringBuilder.Append("\n");
                i++;
            }

            using (var outputFile = new StreamWriter($@"G:\LLOffsets\RB{Assembly.GetEntryAssembly().GetName().Version.Build}.csv", false))
            {
                outputFile.Write(stringBuilder.ToString());
            }
        }

        private void DumpLuaFunctions()
        {
            var func = "local values = {} for key,value in pairs(_G) do table.insert(values, key); end return unpack(values);";

            var retValues = Lua.GetReturnValues(func);
            foreach (var ret in retValues.Where(ret => !ret.StartsWith("_") && !ret.StartsWith("Luc") && !ret.StartsWith("Stm") && !char.IsDigit(ret[ret.Length - 1]) && !char.IsLower(ret[0])))
            {
                if (ret.Contains(":"))
                {
                    var name = ret.Split(':')[0];
                    if (luaFunctions.ContainsKey(name))
                    {
                        continue;
                    }

                    luaFunctions.Add(name, GetSubFunctions(name));
                }
                else
                {
                    if (luaFunctions.ContainsKey(ret))
                    {
                        continue;
                    }

                    luaFunctions.Add(ret, GetSubFunctions(ret));
                }
            }
        }

        private static List<string> GetSubFunctions(string luaObject)
        {
            var func = $"local values = {{}} for key,value in pairs(_G['{luaObject}']) do table.insert(values, key); end return unpack(values);";
            var functions = new List<string>();
            try
            {
                var retValues = Lua.GetReturnValues(func);
                functions.AddRange(retValues.Where(ret => !ret.Contains("_") && !ret.Contains("OnSequence") && !ret.StartsWith("On") && !ret.Contains("className") && !ret.Contains("referenceCount") && !ret.Contains("ACTOR")));
            }
            catch
            {
            }

            functions.Sort();
            return functions;
        }

        public async Task<bool> CheckVentures()
        {
            if (!SelectString.IsOpen)
            {
                return false;
            }

            if (SelectString.Lines().Contains(Translator.VentureCompleteText))
            {
                Log.Information("Venture Done");
                SelectString.ClickLineEquals(Translator.VentureCompleteText);

                await Coroutine.Wait(5000, () => RetainerTaskResult.IsOpen);

                if (!RetainerTaskResult.IsOpen)
                {
                    Log.Error("RetainerTaskResult didn't open");
                    return false;
                }

                var taskId = AgentRetainerVenture.Instance.RetainerTask;

                var task = ResourceManager.VentureData.Value.First(i => i.Id == taskId);

                Log.Information($"Finished Venture {task.Name}");
                Log.Information($"Reassigning Venture {task.Name}");

                RetainerTaskResult.Reassign();

                await Coroutine.Wait(5000, () => RetainerTaskAsk.IsOpen);
                if (!RetainerTaskAsk.IsOpen)
                {
                    Log.Error("RetainerTaskAsk didn't open");
                    return false;
                }

                await Coroutine.Wait(2000, RetainerTaskAskExtensions.CanAssign);
                if (RetainerTaskAskExtensions.CanAssign())
                {
                    RetainerTaskAsk.Confirm();
                }
                else
                {
                    Log.Error($"RetainerTaskAsk Error: {RetainerTaskAskExtensions.GetErrorReason()}");
                    RetainerTaskAsk.Close();
                }

                await Coroutine.Wait(1500, () => DialogOpen);
                await Coroutine.Sleep(200);
                if (DialogOpen)
                {
                    Next();
                }

                await Coroutine.Sleep(200);
                await Coroutine.Wait(5000, () => SelectString.IsOpen);
            }
            else
            {
                Log.Information("Venture Not Done");
            }

            return true;
        }

        public static async Task LowerQualityAndCombine(InventoryBagId[] bags, ItemUiCategory[] categories)
        {
            var HQslots = InventoryManager.GetBagsByInventoryBagId(bags).Select(i => i.FilledSlots).SelectMany(x => x).Where(slot => categories.Contains(slot.Item.EquipmentCatagory) && slot.IsHighQuality);

            foreach (var slot in HQslots)
            {
                var itemId = slot.RawItemId;
                Log.Information($"Retrieve {slot.Name}");
                await slot.TryRetrieveFromRetainer(slot.Count);
                var newSlot = InventoryManager.FilledSlots.First(i => i.RawItemId == itemId);
                Log.Information($"Lowering {newSlot.Name}");
                newSlot.LowerQuality();
                await Coroutine.Sleep(1000);
                Log.Information($"Entrust {newSlot.Name}");
                await newSlot.TryEntrustToRetainer(newSlot.Count);
                await Coroutine.Sleep(100);
            }
        }
    }
}