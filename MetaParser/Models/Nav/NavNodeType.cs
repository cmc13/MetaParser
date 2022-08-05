using System;

namespace MetaParser.Models
{
    public enum NavNodeType
	{
		Point = 0,
		[Obsolete]PortalObs = 1,
		Recall = 2,
		Pause = 3,
		Chat = 4,
		OpenVendor = 5,
		Portal = 6,
		NPCChat = 7,
		Checkpoint = 8,
		Jump = 9
	}
}