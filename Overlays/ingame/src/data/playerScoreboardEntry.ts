// One player's row in the PRM bottom comparison bar (Phase 2). Mirrors the C#
// PlayerScoreboardEntry. All EXACT from /playerlist; per-player gold is intentionally
// absent (estimated -> hidden under strict-accuracy). Icons are built from DataDragon.
export default class PlayerScoreboardEntry {
    Team: string;          // "ORDER" | "CHAOS"
    Position: string;      // TOP/JUNGLE/MIDDLE/BOTTOM/UTILITY
    Name: string;
    ChampionID: string;    // DataDragon champion key (e.g. "Shen", "MonkeyKing")
    ChampionName: string;
    Level: number;
    Kills: number;
    Deaths: number;
    Assists: number;
    CreepScore: number;
    // Player gold (backend item+score estimator, displayed as the player's gold
    // per operator decision) — feeds the per-lane gold diff chip.
    Gold: number;
    Items: number[];       // item IDs, slot order 0..6; 0 = empty
    Spells: string[];      // two DataDragon summoner-spell keys

    constructor(o: any) {
        this.Team = o.Team;
        this.Position = o.Position;
        this.Name = o.Name;
        this.ChampionID = o.ChampionID;
        this.ChampionName = o.ChampionName;
        this.Level = o.Level;
        this.Kills = o.Kills;
        this.Deaths = o.Deaths;
        this.Assists = o.Assists;
        this.CreepScore = o.CreepScore;
        this.Gold = o.Gold;
        this.Items = o.Items || [];
        this.Spells = o.Spells || [];
    }
}
