using HarmonyLib;
using MGSC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using static HarmonyLib.Code;

namespace ShowHunger
{
    [HarmonyPatch(typeof(StarvationBar), nameof(StarvationBar.Initialize))]
    public static class StarvationBar_Initialize_Patch
    {

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> original = instructions.ToList();

            //Goal:  Replace the simple calorie count text to have missing amount + amount

            //Target:  
            // -- start Keep
            //IL_0043: beq.s IL_005e
            //  IL_0045: ldarg.0
            //  IL_0046: ldfld class [Unity.TextMeshPro]TMPro.TextMeshProUGUI MGSC.StarvationBar::_starvationValue
            //  IL_004b: ldarg.1  --keep
            // -- End Keep
            // -- Start remove
            //IL_004c: callvirt instance int32 MGSC.StarvationEffect::get_CurrentLevel()
            //IL_0051: stloc.2
            //IL_0052: ldloca.s 2
            //IL_0054: call instance string [mscorlib]System.Int32::ToString()
            // -- End remove
            // +++++ Insert call to Plugin.StarvationText
            //---- keep  IL_0059: callvirt instance void [Unity.TextMeshPro]TMPro.TMP_Text::set_text(string)

            //Utils.LogIL(original, @"c:\work\before.il");

            List<CodeInstruction> result = new CodeMatcher(original)
                .MatchEndForward(
                    new CodeMatch(OpCodes.Beq_S),
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldfld),
                    new CodeMatch(OpCodes.Ldarg_1),
                    // -- Start remove
                    new CodeMatch(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(StarvationEffect), nameof(StarvationEffect.CurrentLevel))),
                    new CodeMatch(OpCodes.Stloc_2),
                    new CodeMatch(OpCodes.Ldloca_S),
                    new CodeMatch(OpCodes.Call, AccessTools.PropertyGetter(typeof(Int32), nameof(Int32.ToString))),
                    // -- End remove
                    new CodeMatch(OpCodes.Callvirt, AccessTools.PropertySetter(typeof(TMP_Text), nameof(TMP_Text.text)))
                )
                .ThrowIfNotMatch("Unable to find StarvationBar and level")
                .Advance(-4)
                .RemoveInstructions(4)
                .Insert(CodeInstruction.Call(() => Plugin.StarvationText(default)))
                .InstructionEnumeration()
                .ToList();

            //Utils.LogIL(result , @"c:\work\after.il");

            return result;

        }
    }
}
