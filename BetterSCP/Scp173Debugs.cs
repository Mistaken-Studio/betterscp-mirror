using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Exiled.API.Features;
using HarmonyLib;
using PlayableScps;
using UnityEngine;

namespace Mistaken.BetterSCP
{
    [HarmonyPatch(typeof(VisionInformation), "GetVisionInformation")]
    internal class Patch
    {
        public static void Postfix(ref VisionInformation __result, ReferenceHub source, Vector3 target, float targetRadius = 0f, float visionTriggerDistance = 0f, bool checkFog = true, bool checkLineOfSight = true, LocalCurrentRoomEffects targetLightCheck = null, int MaskLayer = 0)
        {
            var reason = __result.GetFailReason();
            Log.Debug(reason);
        }
    }
}
