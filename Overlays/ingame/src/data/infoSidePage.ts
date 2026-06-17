import PlayerInfoTab from "./playerInfoTab";

export default class InfoSidePage {
    Title!: string;
    Order!: PlayerOrder;
    Players!: PlayerInfoTab[];

    constructor(o: any) {
        // null too: the backend serializes infoPage as null when the selected tab
        // has no data; both absent and null must be safe here.
        if(o === undefined || o === null)
            return;
        this.Title = o.Title;
        this.Order = PlayerOrder[o.Order as keyof typeof PlayerOrder];
        this.Players = o.Players;
    }
}

enum PlayerOrder {
    MaxToMin,
    MinToMax,
    BlueFirst,
    RedFirst
}
