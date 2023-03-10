using System.Collections.Generic;
using UnityEngine;

using FrigidBlackwaters.Utility;
using FrigidBlackwaters.Core;

namespace FrigidBlackwaters.Game
{
    public abstract class TiledAreaMobGeneratorStrategy : FrigidScriptableObject
    {
        [SerializeField]
        private string presetId;
        [SerializeField]
        private bool canSpawnAdditionalWaves;
        [SerializeField]
        [ShowIfBool("canSpawnAdditionalWaves", true)]
        private TiledAreaWaveSignal waveSignalPrefab;
        [SerializeField]
        private FloatSerializedReference waveSpawnDelayDuration;

        public string PresetID
        {
            get
            {
                return this.presetId;
            }
        }

        public bool CanSpawnAdditionalWaves
        {
            get
            {
                return this.canSpawnAdditionalWaves;
            }
        }

        public TiledAreaWaveSignal WaveSignalPrefab
        {
            get
            {
                return this.waveSignalPrefab;
            }
        }

        public float WaveSpawnDelayDuration
        {
            get
            {
                return this.waveSpawnDelayDuration.MutableValue;
            }
        }

        public List<Mob> DoGenerationStrategy(TiledLevelPlanArea planArea, TiledArea tiledArea, HashSet<TiledAreaMobSpawnPoint> spawnPoints, int waveIndex)
        {
            List<Mob> generationMobs = new List<Mob>();
            Dictionary<TiledAreaMobSpawnPoint, MobSpawnable> mobSpawns = DetermineMobSpawnsInTiledArea(planArea, tiledArea, spawnPoints, waveIndex);
            foreach (KeyValuePair<TiledAreaMobSpawnPoint, MobSpawnable> mobSpawn in mobSpawns)
            {
                TiledAreaMobSpawnPoint spawnPoint = mobSpawn.Key;
                MobSpawnable mobSpawnable = mobSpawn.Value;
                if (spawnPoint.CanSpawnHere(mobSpawnable, tiledArea.AbsoluteCenterPosition))
                {
                    /* Will need to fix later. TODO MOBS_V2
                    generationMobs.Add(mobSpawnable.SpawnMob(tiledArea.AbsoluteCenterPosition + spawnPoint.LocalPosition, -spawnPoint.LocalPosition.normalized, initiallyActivated));
                    */
                }
            }
            return generationMobs;
        }

        public virtual int GetMaxNumberWaves(TiledLevelPlanArea planArea, TiledArea tiledArea) { return 1; }

        public virtual int GetCurrentNumberWaves(TiledLevelPlanArea planArea, TiledArea tiledArea) { return 1; }

        public virtual bool CanAdvanceToNextWave(int waveIndex, List<Mob> mobsInPreviousWaves) { return true; }

        public virtual bool ShouldLockEntrances(List<Mob> mobsInPreviousWaves, List<Mob> mobsInWavesToAdvance) { return false; }

        protected abstract Dictionary<TiledAreaMobSpawnPoint, MobSpawnable> DetermineMobSpawnsInTiledArea(TiledLevelPlanArea planArea, TiledArea tiledArea, HashSet<TiledAreaMobSpawnPoint> spawnPoints, int waveIndex);
    }
}
