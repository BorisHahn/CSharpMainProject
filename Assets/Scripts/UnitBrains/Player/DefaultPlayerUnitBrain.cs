﻿using System;
using System.Collections.Generic;
using Assets.Scripts.UnitBrains;
using Model;
using Model.Runtime.Projectiles;
using UnitBrains.Pathfinding;
using UnityEngine;

namespace UnitBrains.Player
{
    public class DefaultPlayerUnitBrain : BaseUnitBrain
    {
        protected float DistanceToOwnBase(Vector2Int fromPos) =>
            Vector2Int.Distance(fromPos, runtimeModel.RoMap.Bases[RuntimeModel.PlayerId]);

        protected void SortByDistanceToOwnBase(List<Vector2Int> list)
        {
            list.Sort(CompareByDistanceToOwnBase);
        }
        
        private int CompareByDistanceToOwnBase(Vector2Int a, Vector2Int b)
        {
            var distanceA = DistanceToOwnBase(a);
            var distanceB = DistanceToOwnBase(b);
            return distanceA.CompareTo(distanceB);
        }

        public override Vector2Int GetNextStep()
        {
            PathAndTargetCoordinator pathAndTargetCoordinator = PathAndTargetCoordinator.GetInstance();
            Vector2Int? priorityTarget = pathAndTargetCoordinator.PriorityTargetPosition;
            Vector2Int? priorityPosition = pathAndTargetCoordinator.PrioritySelfPosition;
            BaseUnitPath activePath;
            var target = runtimeModel.RoMap.Bases[
                IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId];
            if (priorityTarget != null)
            {
                target = priorityTarget.Value;
            }

            activePath = new BgUnitPath(runtimeModel, unit.Pos, target);
            return activePath.GetNextStepFrom(unit.Pos) != null ? activePath.GetNextStepFrom(unit.Pos) : base.GetNextStep();           
        }

        ~DefaultPlayerUnitBrain()
        {
            PathAndTargetCoordinator pathAndTargetCoordinator = PathAndTargetCoordinator.GetInstance();
            pathAndTargetCoordinator.Dispose();
        }
    }
}