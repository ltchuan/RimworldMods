﻿using Harmony;
using RimWorld;
using System;
using Verse;
using Verse.AI;

namespace AnimalsLogic
{
    /*
     * Adds more awareness about predators. Predator hunting one of your pawns (humanlike or animal) is considered hostile to whole your faction, like manhunter would be.
     * 
     * TODO: check if fixed in vanilla
     */

    [HarmonyPatch(typeof(GenHostility), "HostileTo", new Type[] { typeof(Thing), typeof(Thing) })]
    static class GenHostility_IsPredatorHostileTo_Patch
    {
        static bool Prefix(ref object[] __state, Thing a, Thing b)
        {
            __state = new object[] { a, b };
            return true;
        }

        static void Postfix(ref bool __result, ref object[] __state)
        {
            if (__state == null || !Settings.hostile_predators)
            {
                return;
            }

            Thing a = (Thing)__state[0];
            Thing b = (Thing)__state[1];

            if (CheckHostile(a, b) || CheckHostile(b, a))
            {
                __result = true;
            }
        }

        private static bool CheckHostile(Thing who, Thing to)
        {
            if (!(who is Pawn) || to.Faction == null)
            {
                return false;
            }

            Pawn agressor = who as Pawn;

            if (to.Faction.HasPredatorRecentlyAttackedAnyone(agressor) || GetPreyOfMyFaction(agressor, to.Faction) != null)
            {
                return true;
            }

            return false;
        }

        // copy-paste from GenHostility
        private static Pawn GetPreyOfMyFaction(Pawn predator, Faction myFaction)
        {
            Job curJob = predator.CurJob;
            if (curJob != null && curJob.def == JobDefOf.PredatorHunt && !predator.jobs.curDriver.ended)
            {
                Pawn pawn = curJob.GetTarget(TargetIndex.A).Thing as Pawn;
                if (pawn != null && pawn.Faction == myFaction)
                {
                    return pawn;
                }
            }
            return null;
        }
    }
}
