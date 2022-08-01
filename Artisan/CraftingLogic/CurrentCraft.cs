﻿using ClickLib.Clicks;
using Artisan.RawInformation;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Excel.GeneratedSheets;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using FFXIVClientStructs.FFXIV.Client.Game;

namespace Artisan.CraftingLogic
{
    public unsafe static class CurrentCraft
    {
        public static class Skills
        {
            public const uint
                BasicSynth = 100001,
                BasicTouch = 100002,
                MastersMend = 100003,
                HastyTouch = 100355,
                RapidSynthesis = 100363,
                Observe = 100010,
                Tricks = 100371,
                WasteNot = 4631,
                Veneration = 19297,
                StandardTouch = 100004,
                GreatStrides = 260,
                Innovation = 19004,
                FinalAppraisal = 19012,
                WasteNot2 = 4639,
                ByregotsBlessing = 100339,
                PreciseTouch = 100128,
                MuscleMemory = 100379,
                CarefulSynthesis = 100203,
                Manipulation = 4574,
                PrudentTouch = 100227,
                FocusedSynthesis = 100235,
                FocusedTouch = 100243,
                Reflect = 100387,
                PreparatoryTouch = 100299,
                Groundwork = 100403,
                DelicateSynthesis = 100323,
                IntensiveSynthesis = 100315,
                TrainedEye = 100283,
                AdvancedTouch = 100411,
                PrudentSynthesis = 100427,
                TrainedFinesse = 100435;
        }

        public static class Buffs
        {
            public const ushort
                InnerQuiet = 251,
                Innovation = 2189,
                Veneration = 2226,
                GreatStrides = 254,
                Manipulation = 1164,
                WasteNot = 252,
                WasteNot2 = 257,
                FinalAppraisal = 2190,
                MuscleMemory = 2191;


        }

        public static class Multipliers
        {
            public const double
                BasicSynthesis = 1.2,
                RapidSynthesis = 5,
                MuscleMemory = 3,
                CarefulSynthesis = 1.5,
                FocusedSynthesis = 2,
                GroundWork = 3,
                DelicateSynthesis = 1,
                IntensiveSynthesis = 4,
                PrudentSynthesis = 1.8,
                BasicTouch = 1,
                HastyTouch = 1,
                StandardTouch = 1.25,
                PreciseTouch = 1.5,
                PrudentTouch = 1,
                FocusedTouch = 1.5,
                Reflect = 1,
                PrepatoryTouch = 2,
                AdvancedTouch = 1.5,
                TrainedFinesse = 1;


        }
        public static event EventHandler<int>? StepChanged;

        public static int CurrentDurability { get; set; } = 0;
        public static int MaxDurability { get; set; } = 0;
        public static int CurrentProgress { get; set; } = 0;
        public static int MaxProgress { get; set; } = 0;
        public static int CurrentQuality { get; set; } = 0;
        public static int MaxQuality { get; set; } = 0;
        public static int HighQualityPercentage { get; set; } = 0;

        public static Condition CurrentCondition { get; set; }
        private static int currentStep = 0;
        public static int CurrentStep
        {
            get { return currentStep; }
            set
            {
                if (currentStep != value)
                {
                    currentStep = value;
                    StepChanged?.Invoke(currentStep, value);
                }

            }
        }
        public static string? HQLiteral { get; set; }
        public static bool CanHQ { get; set; }
        public static string? CollectabilityLow { get; set; }
        public static string? CollectabilityMid { get; set; }
        public static string? CollectabilityHigh { get; set; }

        public static string? ItemName { get; set; }

        public static Recipe? Recipe { get; set; }

        public static uint CurrentRecommendation { get; set; }

        public static bool CraftingWindowOpen { get; set; } = false;

        public static bool JustUsedFinalAppraisal { get; set; } = false;
        public static bool JustUsedObserve { get; set; } = false;
        public static bool ManipulationUsed { get; set; } = false;
        public static bool WasteNotUsed { get; set; } = false;
        public static bool InnovationUsed { get; set; } = false;
        public static bool VenerationUsed { get; set; } = false;

        public unsafe static bool GetCraft()
        {
            try
            {
                IntPtr synthWindow = Service.GameGui.GetAddonByName("Synthesis", 1);
                if (synthWindow == IntPtr.Zero)
                {
                    CurrentStep = 0;
                    CharacterInfo.IsCrafting = false;
                    return false;
                }

                var craft = Marshal.PtrToStructure<AddonSynthesis>(synthWindow);
                if (craft.Equals(default(AddonSynthesis))) return false;
                if (craft.ItemName == null) { CraftingWindowOpen = false; return false; }

                CraftingWindowOpen = true;

                var cd = *craft.CurrentDurability;
                var md = *craft.StartingDurability;
                var mp = *craft.MaxProgress;
                var cp = *craft.CurrentProgress;
                var cq = *craft.CurrentQuality;
                var mq = *craft.MaxQuality;
                var hqp = *craft.HQPercentage;
                var cond = *craft.Condition;
                var cs = *craft.StepNumber;
                var hql = *craft.HQLiteral;
                var collectLow = *craft.CollectabilityLow;
                var collectMid = *craft.CollectabilityMid;
                var collectHigh = *craft.CollectabilityHigh;
                var item = *craft.ItemName;

                CharacterInfo.IsCrafting = true;
                CurrentDurability = Convert.ToInt32(cd.NodeText.ToString());
                MaxDurability = Convert.ToInt32(md.NodeText.ToString());
                CurrentProgress = Convert.ToInt32(cp.NodeText.ToString());
                MaxProgress = Convert.ToInt32(mp.NodeText.ToString());
                CurrentQuality = Convert.ToInt32(cq.NodeText.ToString());
                MaxQuality = Convert.ToInt32(mq.NodeText.ToString());
                ItemName = item.NodeText.ToString()[14..];
                ItemName = ItemName.Remove(ItemName.Length - 10, 10);
                if (ItemName[^1] == '')
                {
                    ItemName = ItemName.Remove(ItemName.Length - 1, 1).Trim();
                }
                var sheetItem = LuminaSheets.RecipeSheet?.Values.Where(x => x.ItemResult.Value.Name!.RawString.Contains(ItemName)).FirstOrDefault();

                if (sheetItem != null)
                {
                    Recipe = sheetItem;

                    if (sheetItem.CanHq)
                    {
                        CanHQ = true;
                        HighQualityPercentage = Convert.ToInt32(hqp.NodeText.ToString());
                    }
                    else
                    {
                        CanHQ = false;
                        HighQualityPercentage = 0;
                    }
                }

                CurrentCondition = cond.NodeText.ToString() switch
                {
                    "Poor" => Condition.Poor,
                    "Good" => Condition.Good,
                    "Normal" => Condition.Normal,
                    "Excellent" => Condition.Excellent,
                    _ => Condition.Unknown
                };

                CurrentStep = Convert.ToInt32(cs.NodeText.ToString());
                HQLiteral = hql.NodeText.ToString();
                CollectabilityLow = collectLow.NodeText.ToString();
                CollectabilityMid = collectMid.NodeText.ToString();
                CollectabilityHigh = collectHigh.NodeText.ToString();

                return true;

            }
            catch (Exception ex)
            {
                Dalamud.Logging.PluginLog.Error(ex, ex.StackTrace);
                return false;
            }
        }

        public static double BaseQuality()
        {
            try
            {
                if (CraftingWindowOpen)
                {
                    var baseValue = CharacterInfo.Control() * 10 / Recipe.RecipeLevelTable.Value.QualityDivider + 35;
                    if (CharacterInfo.CharacterLevel() <= Recipe.RecipeLevelTable.Value.ClassJobLevel)
                    {
                        return baseValue * Recipe.RecipeLevelTable.Value.QualityModifier * 0.01;
                    }

                    return baseValue;
                }
                return 0;
            }
            catch (Exception ex)
            {
                Dalamud.Logging.PluginLog.Error(ex, "BaseQuality");
                return 0;
            }
        }

        public static double BaseProgression()
        {
            try
            {
                if (CraftingWindowOpen)
                {
                    var baseValue = CharacterInfo.Craftsmanship() * 10 / Recipe.RecipeLevelTable.Value.ProgressDivider + 2;

                    return baseValue;
                }
                return 0;
            }
            catch (Exception ex)
            {
                Dalamud.Logging.PluginLog.Error(ex, "BaseProgression");
                return 0;
            }
        }

        public static double GetMultiplier(uint id, bool isQuality = false)
        {
            double baseMultiplier = id switch
            {
                Skills.BasicSynth => Multipliers.BasicSynthesis,
                Skills.RapidSynthesis => Multipliers.RapidSynthesis,
                Skills.MuscleMemory => Multipliers.MuscleMemory,
                Skills.CarefulSynthesis => Multipliers.CarefulSynthesis,
                Skills.FocusedSynthesis => Multipliers.FocusedSynthesis,
                Skills.Groundwork => Multipliers.GroundWork,
                Skills.DelicateSynthesis => Multipliers.DelicateSynthesis,
                Skills.IntensiveSynthesis => Multipliers.IntensiveSynthesis,
                Skills.PrudentSynthesis => Multipliers.PrudentSynthesis,
                Skills.BasicTouch => Multipliers.BasicTouch,
                Skills.HastyTouch => Multipliers.HastyTouch,
                Skills.StandardTouch => Multipliers.StandardTouch,
                Skills.PreciseTouch => Multipliers.PreciseTouch,
                Skills.PrudentTouch => Multipliers.PrudentTouch,
                Skills.FocusedTouch => Multipliers.FocusedTouch,
                Skills.Reflect => Multipliers.Reflect,
                Skills.PreparatoryTouch => Multipliers.PrepatoryTouch,
                Skills.AdvancedTouch => Multipliers.AdvancedTouch,
                Skills.TrainedFinesse => Multipliers.TrainedFinesse,
                _ => 1
            };

            if (!isQuality) return baseMultiplier;

            var conditionMod = CurrentCondition switch
            {
                Condition.Poor => 0.5,
                Condition.Normal => 1,
                Condition.Good => 1.5,
                Condition.Excellent => 4,
                Condition.Unknown => 1,
                _ => 1
            };

            return conditionMod * baseMultiplier;
        }

        public static uint CalculateNewQuality(uint id)
        {
            var multiplier = id == Skills.ByregotsBlessing ? ByregotMultiplier() : GetMultiplier(id);
            int IQStacks = Convert.ToInt32(GetStatus(251)?.StackCount);
            double innovation = GetStatus(Buffs.Innovation) != null ? 1.5 : 1;
            double IQMultiplier = 1 + (IQStacks * 0.1);
            return (uint)Math.Floor(CurrentQuality + (BaseQuality() * multiplier) * IQMultiplier * innovation);

        }

        public static uint CalculateNewProgress(uint id)
        {
            var multiplier = GetMultiplier(id, false);
            double veneration = GetStatus(Buffs.Veneration) != null ? 1.5 : 1;
            double muscleMemory = GetStatus(Buffs.MuscleMemory) != null ? 2 : 1;
            return (uint)Math.Floor(CurrentProgress + (BaseProgression() * multiplier) * veneration * muscleMemory);

        }

        public static double ByregotMultiplier()
        {
            int IQStacks = Convert.ToInt32(GetStatus(251)?.StackCount);
            return 1 + (IQStacks * 0.2);
        }
        public static uint GetRecommendation()
        {
            if (CanUse(Skills.TrainedEye)) return Skills.TrainedEye;
           
            if (MaxQuality == 0)
            {
                if (CurrentStep == 1 && CanUse(Skills.MuscleMemory)) return Skills.MuscleMemory;
                if (GetStatus(Buffs.Veneration) == null && CanUse(Skills.Veneration)) return Skills.Veneration;

                return Skills.Groundwork;
            }

            if (CurrentDurability <= 10 && CanUse(Skills.MastersMend)) return Skills.MastersMend;


            if (MaxDurability >= 60)
            {
                if (CurrentQuality < MaxQuality)
                {

                    if (CurrentStep == 1 && CanUse(Skills.MuscleMemory)) return Skills.MuscleMemory;
                    if (CurrentStep == 2 && CanUse(Skills.FinalAppraisal) && !JustUsedFinalAppraisal) { JustUsedFinalAppraisal = true; return Skills.FinalAppraisal; }
                    if (GetStatus(Buffs.MuscleMemory) != null) return Skills.Groundwork;
                    if (CurrentCondition == Condition.Poor && CanUse(Skills.Observe)) { JustUsedObserve = true; return Skills.Observe; }
                    if (GetStatus(Buffs.InnerQuiet)?.StackCount == 10 && GetStatus(Buffs.GreatStrides) is null && CanUse(Skills.GreatStrides)) return Skills.GreatStrides;
                    if (GetStatus(Buffs.InnerQuiet)?.StackCount == 10 && GetStatus(Buffs.GreatStrides) is not null && CanUse(Skills.ByregotsBlessing)) return Skills.ByregotsBlessing;
                    if (!ManipulationUsed && GetStatus(Buffs.Manipulation) is null && CanUse(Skills.Manipulation)) { ManipulationUsed = true; return Skills.Manipulation; }
                    if (!WasteNotUsed && GetStatus(Buffs.WasteNot2) is null && CanUse(Skills.WasteNot2)) { WasteNotUsed = true; return Skills.WasteNot2; }
                    if (!InnovationUsed && GetStatus(Buffs.Innovation) is null && CanUse(Skills.Innovation)) { InnovationUsed = true; return Skills.Innovation; }
                    return CharacterInfo.HighestLevelTouch();

                }
            }

            if (MaxDurability == 40)
            {
                if (CurrentQuality < MaxQuality)
                {
                    if (CurrentStep == 1 && CanUse(Skills.Reflect)) return Skills.Reflect;
                    if (!ManipulationUsed && GetStatus(Buffs.Manipulation) is null && CanUse(Skills.Manipulation)) { ManipulationUsed = true; return Skills.Manipulation; }
                    if (!WasteNotUsed && CanUse(Skills.WasteNot2)) { WasteNotUsed = true; return Skills.WasteNot2; }
                    if (!InnovationUsed && CanUse(Skills.Innovation)) { InnovationUsed = true; return Skills.Innovation; }
                    if (GetStatus(Buffs.InnerQuiet)?.StackCount == 8 && GetStatus(Buffs.GreatStrides) is null && CanUse(Skills.GreatStrides)) return Skills.GreatStrides;
                    if (CurrentCondition == Condition.Poor) return Skills.Observe;
                    if (GetStatus(Buffs.InnerQuiet)?.StackCount == 8 && GetStatus(Buffs.GreatStrides) is not null && CanUse(Skills.ByregotsBlessing)) return Skills.ByregotsBlessing;
                    return CharacterInfo.HighestLevelTouch();
                }

                if (CanUse(Skills.Groundwork) && CalculateNewProgress(Skills.Groundwork) >= MaxProgress) return Skills.Groundwork;
                if (CanUse(Skills.CarefulSynthesis) && CalculateNewProgress(Skills.CarefulSynthesis) >= MaxProgress) return Skills.CarefulSynthesis;
                if (!VenerationUsed && CanUse(Skills.Veneration)) { VenerationUsed = true; return Skills.Veneration; }
                if (CanUse(Skills.Groundwork)) return Skills.Groundwork;
            }

            if (CurrentDurability > MaxDurability / 2 && CanUse(Skills.Groundwork)) return Skills.Groundwork;
            if (CanUse(Skills.CarefulSynthesis)) return Skills.CarefulSynthesis;


            return Skills.BasicSynth;

        }

        public static void RepeatTrialCraft()
        {
            try
            {
                var recipeWindow = Service.GameGui.GetAddonByName("RecipeNote", 1);
                if (recipeWindow == IntPtr.Zero)
                    return;

                var addonPtr = (AddonRecipeNote*)recipeWindow;
                if (addonPtr == null)
                    return;

                var synthButton = addonPtr->TrialSynthesisButton;

                if (synthButton != null && !synthButton->IsEnabled)
                {
                    Dalamud.Logging.PluginLog.Debug("AddonRecipeNote: Enabling trial synth button");
                    synthButton->AtkComponentBase.OwnerNode->AtkResNode.Flags ^= 1 << 5;
                }
                else
                {
                    return;
                }

                Dalamud.Logging.PluginLog.Debug("AddonRecipeNote: Selecting trial");
                ClickRecipeNote.Using(recipeWindow).TrialSynthesis();
            }
            catch (Exception ex)
            {
                Dalamud.Logging.PluginLog.Error(ex, "RepeatTrialCraft");
            }
        }

        public static void RepeatActualCraft()
        {
            try
            {
                var recipeWindow = Service.GameGui.GetAddonByName("RecipeNote", 1);
                if (recipeWindow == IntPtr.Zero)
                    return;

                var addonPtr = (AddonRecipeNote*)recipeWindow;
                if (addonPtr == null)
                    return;

                var synthButton = addonPtr->SynthesizeButton;

                if (synthButton != null && !synthButton->IsEnabled)
                {
                    Dalamud.Logging.PluginLog.Debug("AddonRecipeNote: Enabling synth button");
                    synthButton->AtkComponentBase.OwnerNode->AtkResNode.Flags ^= 1 << 5;
                }
                else
                {
                    return;
                }

                Dalamud.Logging.PluginLog.Debug("AddonRecipeNote: Selecting synth");
                ClickRecipeNote.Using(recipeWindow).Synthesize();
            }
            catch (Exception ex)
            {
                Dalamud.Logging.PluginLog.Error(ex, "RepeatActualCraft");
            }
        }
        internal static Dalamud.Game.ClientState.Statuses.Status? GetStatus(uint statusID)
        {
            if (Service.ClientState.LocalPlayer is null) return null;

            foreach (var status in Service.ClientState.LocalPlayer?.StatusList)
            {
                if (status.StatusId == statusID)
                    return status;
            }

            return null;
        }

        internal static int GetResourceCost(uint actionID)
        {
            if (actionID < 100000)
            {
                var cost = LuminaSheets.ActionSheet[actionID].PrimaryCostValue;
                return cost;
            }
            else
            {
                var cost = LuminaSheets.CraftActions[actionID].Cost;
                return cost;

            }  
        }

        internal unsafe static uint CanUse2(uint id)
        {
            ActionManager* actionManager = ActionManager.Instance();
            if (actionManager == null)
                return 0;

            if (LuminaSheets.ActionSheet.TryGetValue(id, out var act1))
            {
                var canUse = actionManager->GetActionStatus(ActionType.Spell, id);
                return canUse;
            }
            if (LuminaSheets.CraftActions.TryGetValue(id, out var act2))
            {
                var canUse = actionManager->GetActionStatus(ActionType.CraftAction, id);
                return canUse;
            }

            return 0;
        }

        internal static bool CanUse(uint id)
        {
            if (LuminaSheets.ActionSheet.TryGetValue(id, out var act1))
            {
                string skillName = act1.Name;
                var allOfSameName = LuminaSheets.ActionSheet.Where(x => x.Value.Name == skillName).Select(x => x.Key);
                foreach (var dupe in allOfSameName)
                {
                    if (CanUse2(dupe) == 0) return true;
                }
                return false;
            }

            if (LuminaSheets.CraftActions.TryGetValue(id, out var act2))
            {
                string skillName = act2.Name;
                var allOfSameName = LuminaSheets.CraftActions.Where(x => x.Value.Name == skillName).Select(x => x.Key);
                foreach (var dupe in allOfSameName)
                {
                    if (CanUse2(dupe) == 0) return true;
                }
                return false;
            }

            return false;
        }

        public enum Condition
        {
            Poor,
            Normal,
            Good,
            Excellent,
            Unknown
        }
    }
}