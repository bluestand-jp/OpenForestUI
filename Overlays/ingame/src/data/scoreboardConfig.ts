import FrontEndTeam from "./frontEndTeam";
import PlayerScoreboardEntry from "./playerScoreboardEntry";

export default class ScoreboardConfig {
    BlueTeam: FrontEndTeam;
    RedTeam: FrontEndTeam;
    GameTime: number;
    SeriesGameCount: number;
    TournamentName: string;
    Players: PlayerScoreboardEntry[];

    constructor(o: any) {
        this.BlueTeam = new FrontEndTeam(o.BlueTeam);
        this.RedTeam = new FrontEndTeam(o.RedTeam);
        this.GameTime = o.GameTime;
        this.SeriesGameCount = o.SeriesGameCount;
        this.TournamentName = o.TournamentName;
        this.Players = Array.isArray(o.Players) ? o.Players.map((p: any) => new PlayerScoreboardEntry(p)) : [];
    }
}
