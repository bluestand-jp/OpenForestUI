export default class FrontEndTeam {
    Name: string;
    Icon: string;
    Score: number;

    Kills: number;
    Towers: number;
    Gold: number;
    Dragons: string[];

    // PRM top-bar additions (optional; absent in legacy backends).
    VoidGrubs: number;
    Baron: number;          // baron-takedown count (OCR'd; not in spectator/replay /eventdata)
    DragonCount: number;    // dragon count (OCR'd; separate from the typed Dragons list)
    Inhibitors: number;
    PlatesDestroyed: number;
    Region: string;
    Seed: string;
    Flag: string;

    constructor(o:any) {
        this.Name = o.Name;
        this.Icon = o.Icon;
        this.Score = o.Score;

        this.Kills = o.Kills;
        this.Towers = o.Towers;
        this.Gold = o.Gold;
        this.Dragons = o.Dragons;

        this.VoidGrubs = o.VoidGrubs;
        this.Baron = o.Baron;
        this.DragonCount = o.DragonCount;
        this.Inhibitors = o.Inhibitors;
        this.PlatesDestroyed = o.PlatesDestroyed;
        this.Region = o.Region;
        this.Seed = o.Seed;
        this.Flag = o.Flag;
    }
}
