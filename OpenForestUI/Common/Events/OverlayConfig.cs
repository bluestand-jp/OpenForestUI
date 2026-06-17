using OpenForestUI.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenForestUI.Common.Events
{
    public abstract class OverlayConfig : LeagueEvent
    {
        public FrontEndType type;

        public OverlayConfig() : base("OverlayConfig") { }
    }
}
