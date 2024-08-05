using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using UnityEngine;
using System.Collections;
using UnitBrains;
using Model.Runtime.ReadOnly;

namespace Assets.Scripts.UnitBrains
{
    public class PathAndTargetCoordinator : BaseUnitBrain
    {
        private static PathAndTargetCoordinator _instance;

        private IReadOnlyRuntimeModel _runtimeModel;
        private TimeUtil _timeUtil;
        private Vector2Int? _priorityTargetPosition = null;
        private Vector2Int? _prioritySelfPosition = null;

        private PathAndTargetCoordinator()
        {
            _runtimeModel = ServiceLocator.Get<IReadOnlyRuntimeModel>();
            _timeUtil = ServiceLocator.Get<TimeUtil>();

            _timeUtil.AddFixedUpdateAction(getPriorityTargetPosition);
            _timeUtil.AddFixedUpdateAction(getPrioritySelfPosition);
        }

        public static PathAndTargetCoordinator GetInstance()
        {
            return _instance ?? (_instance = new PathAndTargetCoordinator());
        }

        /*
            Рекомендуемая цель: если на нашей половине карты есть враги, то юнитам рекомендуется атаковать ближайшего к нашей базе.
                                В противном случае целью становится враг с наименьшим количеством здоровья.

            Рекомендуемая точка: если на нашей половине карты есть враги, то рекомендуемая точка устанавливается перед базой. 
                                 Иначе, рекомендуемая точка находится на расстоянии выстрела от ближайшего к базе врага. 
        */

        public void getPriorityTargetPosition(float deltaTime, BaseUnitBrain unitBrain)
        {
            List<Vector2Int> enemiesCloseToBase = getEnemiesCloseToBase(unitBrain);
            if (enemiesCloseToBase.Count > 0)
            {
                _priorityTargetPosition =  enemiesCloseToBase[0];
            }

            List<Vector2Int> enemiesWithLowHealth = getEnemiesWithLowHealth(unitBrain);
            if (enemiesWithLowHealth.Count > 0)
            {
                _priorityTargetPosition =  enemiesWithLowHealth[0];
            }

            _priorityTargetPosition =  null;
        }

        public void getPrioritySelfPosition(float deltaTime, BaseUnitBrain unitBrain)
        {
            List<Vector2Int> enemiesCloseToBase = getEnemiesCloseToBase(unitBrain);

            if (enemiesCloseToBase.Count > 0)
            {
                _prioritySelfPosition = unitBrain.IsPlayerUnitBrain
                ? _runtimeModel.RoMap.Bases[RuntimeModel.PlayerId] + Vector2Int.right
                : _runtimeModel.RoMap.Bases[RuntimeModel.BotPlayerId] + Vector2Int.left;
            }

            _prioritySelfPosition = null;
        }

        private List<Vector2Int> getEnemiesCloseToBase(BaseUnitBrain unitBrain)
        {
            Vector2Int unitBase = _runtimeModel.RoMap.Bases[unitBrain.IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId];
            IEnumerable<IReadOnlyUnit> enemyUnits = unitBrain.IsPlayerUnitBrain
                ? _runtimeModel.RoBotUnits
                : _runtimeModel.RoPlayerUnits;

            int middleX = (int)Math.Round(_runtimeModel.RoMap.Width / 2f);

            return enemyUnits
                .Where((enemy) => unitBrain.IsPlayerUnitBrain ? enemy.Pos.x < middleX : enemy.Pos.x > middleX)
                .Select((enemy) => enemy.Pos)
                .OrderBy((enemyPosition) => Vector2Int.Distance(enemyPosition, unitBase))
                .ToList();
        }

        private List<Vector2Int> getEnemiesWithLowHealth(BaseUnitBrain unitBrain)
        {
            IEnumerable<IReadOnlyUnit> enemyUnits = unitBrain.IsPlayerUnitBrain
                ? _runtimeModel.RoBotUnits
                : _runtimeModel.RoPlayerUnits;

            return enemyUnits
                .OrderBy((enemy) => enemy.Health)
                .Select((enemy) => enemy.Pos)
                .ToList();
        }

        public void Dispose()
        {
            _timeUtil.RemoveFixedUpdateAction(getPriorityTargetPosition);
            _timeUtil.RemoveFixedUpdateAction(getPrioritySelfPosition);
        }
    }
}
