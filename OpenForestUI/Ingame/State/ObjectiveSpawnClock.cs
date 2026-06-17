using OpenForestUI.Common.Data.Config;
using OpenForestUI.Ingame.Events;
using System.Collections.Generic;

namespace OpenForestUI.Ingame.State
{
    /// <summary>
    /// Derives every objective spawn countdown (Dragon / Baron / Herald) from first
    /// principles on EVERY tick: patch timing constants + the raw kill events in
    /// /eventdata. This replaces the old "seed a timer, decrement it per tick, and
    /// special-case rewinds" approach, which had three failure modes this kills by
    /// construction: the Baron timer was never seeded at game start (stuck at 0 until
    /// the first Baron kill), the rewind recompute inverted its elapsed-time sign
    /// (420 - (kill - now) = 420 + elapsed), and mid-game joins started from whatever
    /// the decrementing state happened to hold.
    ///
    /// remaining(objective) = max(0, nextSpawnAt - gameTime)
    ///   nextSpawnAt = no kill yet ? FirstSpawn : lastKill + Respawn
    ///
    /// remaining == 0 means "spawned (presumed alive)" — the frontend's
    /// ObjectiveTimerVisual already renders that state via its alive-icon config.
    /// Zero-crossings (prev > 0 → 0 while time moves forward) emit spawn events for
    /// the pop-up pipeline; rewinds and the first observed tick never fire them.
    /// </summary>
    public class ObjectiveSpawnClock
    {
        public struct ClockTick
        {
            public double DragonRemaining;
            public double BaronRemaining;
            public double HeraldRemaining;
            public bool DragonSpawned;
            public bool BaronSpawned;
            public bool HeraldSpawned;
            // True when the NEXT dragon is the Elder (a team has soul point: 4 drakes).
            public bool NextDragonIsElder;
        }

        // Previous tick's remaining values; -1 = no previous observation (first tick /
        // after Reset), which suppresses spawn events until a real countdown is seen.
        private double prevDragon = -1;
        private double prevBaron = -1;
        private double prevHerald = -1;

        public void Reset()
        {
            prevDragon = prevBaron = prevHerald = -1;
        }

        /// <param name="gameTime">Current game clock (post-seek value during replays).</param>
        /// <param name="timeAdvanced">False on rewinds — suppresses spawn events.</param>
        /// <param name="events">The cumulative raw /eventdata batch (pastIngameEvents).</param>
        public ClockTick Recompute(double gameTime, bool timeAdvanced, List<RiotEvent> events,
                                   int blueDrakes, int redDrakes, ObjectiveTimingsConfig t)
        {
            // Latest raw kill per objective. EventTime is game time; the list also
            // carries synthetic ObjectiveKilled markers (EventID == -1) but the raw
            // Riot names used here never collide with them.
            double lastDragonKill = -1, lastBaronKill = -1, lastHeraldKill = -1;
            if (events != null)
            {
                foreach (var e in events)
                {
                    switch (e.EventName)
                    {
                        case "DragonKill":
                            if (e.EventTime > lastDragonKill) lastDragonKill = e.EventTime;
                            break;
                        case "BaronKill":
                            if (e.EventTime > lastBaronKill) lastBaronKill = e.EventTime;
                            break;
                        case "HeraldKill":
                            if (e.EventTime > lastHeraldKill) lastHeraldKill = e.EventTime;
                            break;
                        default:
                            break;
                    }
                }
            }

            bool nextDragonIsElder = blueDrakes >= 4 || redDrakes >= 4;
            double dragonRespawn = nextDragonIsElder ? t.ElderRespawn : t.DragonRespawn;
            double dragonAt = lastDragonKill < 0 ? t.FirstDragonSpawn : lastDragonKill + dragonRespawn;
            double baronAt = lastBaronKill < 0 ? t.FirstBaronSpawn : lastBaronKill + t.BaronRespawn;

            double dragonRemaining = System.Math.Max(0, dragonAt - gameTime);
            double baronRemaining = System.Math.Max(0, baronAt - gameTime);

            // Herald spawns once and never respawns; it leaves the map for good when
            // killed or at its despawn time (Baron replaces it). Before its window the
            // countdown runs; once irrelevant it pins to 0 with no spawn event.
            bool heraldRelevant = lastHeraldKill < 0 && gameTime < t.HeraldDespawn;
            double heraldRemaining = heraldRelevant ? System.Math.Max(0, t.FirstHeraldSpawn - gameTime) : 0;

            var tick = new ClockTick
            {
                DragonRemaining = dragonRemaining,
                BaronRemaining = baronRemaining,
                HeraldRemaining = heraldRemaining,
                DragonSpawned = timeAdvanced && prevDragon > 0 && dragonRemaining == 0,
                BaronSpawned = timeAdvanced && prevBaron > 0 && baronRemaining == 0,
                HeraldSpawned = timeAdvanced && prevHerald > 0 && heraldRemaining == 0 && heraldRelevant,
                NextDragonIsElder = nextDragonIsElder,
            };

            prevDragon = dragonRemaining;
            prevBaron = baronRemaining;
            prevHerald = heraldRemaining;
            return tick;
        }
    }
}
