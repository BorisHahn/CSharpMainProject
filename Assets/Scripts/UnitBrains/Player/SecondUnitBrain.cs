using System;
using System.Collections.Generic;
using System.Linq;
using Model;
using Model.Runtime;
using Model.Runtime.Projectiles;
using UnityEngine;
using Utilities;

namespace UnitBrains.Player
{
    public class SecondUnitBrain : DefaultPlayerUnitBrain
    {
        public override string TargetUnitName => "Cobra Commando";
        private const float OverheatTemperature = 3f;
        private const float OverheatCooldown = 2f;
        private float _temperature = 0f;
        private float _cooldownTime = 0f;
        private bool _overheated;
        private List<Vector2Int> _priorityNotReachableTargets;

        public SecondUnitBrain()
        {
            _priorityNotReachableTargets = new List<Vector2Int>();
        }

        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            float overheatTemperature = OverheatTemperature;
            ///////////////////////////////////////
            // Homework 1.3 (1st block, 3rd module)
            ///////////////////////////////////////
            float currentTemperature = GetTemperature();
            if (currentTemperature >= overheatTemperature)
            {
                return;
            } 
            
            for (int i = 0; i <= currentTemperature; i++)
            {
                var projectile = CreateProjectile(forTarget);
                AddProjectileToList(projectile, intoList);
            }

            IncreaseTemperature();
            ///////////////////////////////////////
        }

        public override Vector2Int GetNextStep()
        {
            Vector2Int targetPosition;
            List<Vector2Int> reachableTargets = GetReachableTargets();
            if (reachableTargets.Contains(_priorityNotReachableTargets.LastOrDefault()))
            {
                targetPosition = unit.Pos;
            }
            else
            {
                targetPosition = unit.Pos.CalcNextStepTowards(_priorityNotReachableTargets.LastOrDefault());
            }
            return targetPosition;
        }

        protected override List<Vector2Int> SelectTargets()
        {
            ///////////////////////////////////////
            // Homework 1.4 (1st block, 4rd module)
            ///////////////////////////////////////

            var botPlayerId = RuntimeModel.BotPlayerId;
            var baseCoords = runtimeModel.RoMap.Bases[botPlayerId];
            List<Vector2Int> allTargets = GetAllTargets().ToList();
            List<Vector2Int> reachableTargets = GetReachableTargets();
            (float, int) minTargetDistanceValue = (float.MaxValue, 0);
            if (allTargets.Count > 0 ) 
            {
                for (var i = 0; i < allTargets.Count; i++)
                {
                    float targetDistance = DistanceToOwnBase(allTargets[i]);
                    
                    if (targetDistance < minTargetDistanceValue.Item1)
                    {
                        minTargetDistanceValue.Item1 = targetDistance;
                        minTargetDistanceValue.Item2 = i;
                    }
                }
                var priorityTarget = allTargets[minTargetDistanceValue.Item2];
                _priorityNotReachableTargets.Clear();
                _priorityNotReachableTargets.Add(priorityTarget);
            }
            else
            {
                _priorityNotReachableTargets.Add(baseCoords);
            }

            return reachableTargets.Contains(_priorityNotReachableTargets.LastOrDefault()) ? _priorityNotReachableTargets : reachableTargets;
            ///////////////////////////////////////
        }

        public override void Update(float deltaTime, float time)
        {
            if (_overheated)
            {              
                _cooldownTime += Time.deltaTime;
                float t = _cooldownTime / (OverheatCooldown/10);
                _temperature = Mathf.Lerp(OverheatTemperature, 0, t);
                if (t >= 1)
                {
                    _cooldownTime = 0;
                    _overheated = false;
                }
            }
        }

        private int GetTemperature()
        {
            if(_overheated) return (int) OverheatTemperature;
            else return (int)_temperature;
        }

        private void IncreaseTemperature()
        {
            _temperature += 1f;
            if (_temperature >= OverheatTemperature) _overheated = true;
        }
    }
}