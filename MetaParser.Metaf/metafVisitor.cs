using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using MetaParser.Models;

namespace MetaParser.Metaf;

public static class metafVisitorExtensions
{
    public static string UnescapeStringValue(this string value) =>
        value.Substring(1, value.Length - 2).Replace("{{", "{").Replace("}}", "}");
}

public class metafVisitor : metafBaseVisitor<object>
{
    private class EmbeddedNavRouteMetaActionWithTransform
        : EmbeddedNavRouteMetaAction
    {
        public double[]? Transform { get; set; }
    }

    private readonly Dictionary<string, List<EmbeddedNavRouteMetaAction>> navReferences = new();
    private static readonly Dictionary<string, RecallSpellId> RecallSpellList = new()
    {
        { "Primary Portal Recall", RecallSpellId.PrimaryPortalRecall },
        { "Secondary Portal Recall", RecallSpellId.SecondaryPortalRecall },
        { "Lifestone Recall", RecallSpellId.LifestoneRecall },
        { "Lifestone Sending", RecallSpellId.LifestoneSending },
        { "Portal Recall", RecallSpellId.PortalRecall },
        { "Recall Aphus Lassel", RecallSpellId.RecallAphusLassel },
        { "Recall the Sanctuary", RecallSpellId.RecalltheSanctuary },
        { "Recall to the Singularity Caul", RecallSpellId.RecalltotheSingularityCaul },
        { "Glenden Wood Recall", RecallSpellId.GlendenWoodRecall },
        { "Aerlinthe Recall", RecallSpellId.AerlintheRecall },
        { "Mount Lethe Recall", RecallSpellId.MountLetheRecall },
        { "Ulgrim's Recall", RecallSpellId.UlgrimsRecall },
        { "Bur Recall", RecallSpellId.BurRecall },
        { "Paradox-touched Olthoi Infested Area Recall", RecallSpellId.ParadoxTouchedOlthoiInfestedAreaRecall },
        { "Call of the Mhoire Forge", RecallSpellId.CalloftheMhoireForge },
        { "Colosseum Recall", RecallSpellId.ColosseumRecall },
        { "Facility Hub Recall", RecallSpellId.FacilityHubRecall },
        { "Gear Knight Invasion Area Camp Recall", RecallSpellId.GearKnightInvasionAreaCampRecall },
        { "Lost City of Neftet Recall", RecallSpellId.LostCityofNeftetRecall },
        { "Return to the Keep", RecallSpellId.ReturntotheKeep },
        { "Rynthid Recall", RecallSpellId.RynthidRecall },
        { "Viridian Rise Recall", RecallSpellId.ViridianRiseRecall },
        { "Viridian Rise Great Tree Recall", RecallSpellId.ViridianRiseGreatTreeRecall },
        { "Celestial Hand Stronghold Recall", RecallSpellId.CelestialHandStrongholdRecall },
        { "Radiant Blood Stronghold Recall", RecallSpellId.RadiantBloodStrongholdRecall },
        { "Eldrytch Web Stronghold Recall", RecallSpellId.EldrytchWebStrongholdRecall }
    };

    public override object VisitXfArg([NotNull] metafParser.XfArgContext context)
    {
        return Enumerable.Range(0, 7).Select(i => double.Parse(context.DOUBLE(i).GetText())).ToArray();
    }

    public override object VisitPointDef([NotNull] metafParser.PointDefContext context)
    {
        return (
                double.Parse(context.DOUBLE(0).GetText()),
                double.Parse(context.DOUBLE(0).GetText()),
                double.Parse(context.DOUBLE(0).GetText())
            );
    }

    public override object VisitPointNode([NotNull] metafParser.PointNodeContext context) => context.nodeType.Text switch
    {
        "pnt" => new NavNodePoint() { Point = ((double, double, double))Visit(context.pointDef()) },
        "chk" => new NavNodeCheckpoint() { Point = ((double, double, double))Visit(context.pointDef()) },
        _ => throw new MetaParserException("Invalid Node point identifier", "pnt|chk", context.nodeType.Text)
    };

    public override object VisitPortalObsNode([NotNull] metafParser.PortalObsNodeContext context)
    {
        return new NavNodePortalObs()
        {
            Point = ((double, double, double))Visit(context.pointDef()),
            Data = int.Parse(context.INT().GetText())
        };
    }

    public override object VisitRecallNode([NotNull] metafParser.RecallNodeContext context)
    {
        var spellName = context.STRING().GetText().UnescapeStringValue();
        return new NavNodeRecall()
        {
            Point = ((double, double, double))Visit(context.pointDef()),
            Data = RecallSpellList.ContainsKey(spellName) ? RecallSpellList[spellName] : throw new MetaParserException($"Invalid spell name: {spellName}")
        };
    }

    public override object VisitPauseNode([NotNull] metafParser.PauseNodeContext context)
    {
        return new NavNodePause()
        {
            Point = ((double, double, double))Visit(context.pointDef()),
            Data = double.Parse(context.DOUBLE().GetText())
        };
    }

    public override object VisitChatNode([NotNull] metafParser.ChatNodeContext context)
    {
        return new NavNodeChat()
        {
            Point = ((double, double, double))Visit(context.pointDef()),
            Data = context.STRING().GetText().UnescapeStringValue()
        };
    }

    public override object VisitVendorNode([NotNull] metafParser.VendorNodeContext context)
    {
        return new NavNodeOpenVendor()
        {
            Point = ((double, double, double))Visit(context.pointDef()),
            Data = (
                int.Parse(context.HEXINT().GetText(), System.Globalization.NumberStyles.HexNumber),
                context.STRING().GetText().UnescapeStringValue()
            )
        };
    }

    public override object VisitPortalNode([NotNull] metafParser.PortalNodeContext context)
    {
        var target = ((double x, double y, double z))Visit(context.pointDef(1));
        return new NavNodePortal()
        {
            Point = ((double, double, double))Visit(context.pointDef(0)),
            Data = (
                context.STRING().GetText().UnescapeStringValue(),
                (ObjectClass)int.Parse(context.INT().GetText()),
                target.x,
                target.y,
                target.z
            )
        };
    }

    public override object VisitNpcChatNode([NotNull] metafParser.NpcChatNodeContext context)
    {
        var target = ((double x, double y, double z))Visit(context.pointDef(1));
        return new NavNodePortal()
        {
            Point = ((double, double, double))Visit(context.pointDef(0)),
            Data = (
                context.STRING().GetText().UnescapeStringValue(),
                (ObjectClass)int.Parse(context.INT().GetText()),
                target.x,
                target.y,
                target.z
            )
        };
    }

    public override object VisitJumpNode([NotNull] metafParser.JumpNodeContext context)
    {
        return new NavNodeJump()
        {
            Point = ((double, double, double))Visit(context.pointDef()),
            Data = (
                double.Parse(context.DOUBLE(0).GetText()),
                context.shift.Text == bool.TrueString,
                double.Parse(context.DOUBLE(1).GetText())
            )
        };
    }

    public override object VisitNavfollow([NotNull] metafParser.NavfollowContext context)
    {
        return new NavFollow()
        {
            TargetId = int.Parse(context.HEXINT().GetText(), System.Globalization.NumberStyles.HexNumber),
            TargetName = context.STRING().GetText().UnescapeStringValue()
        };
    }

    public override object VisitNavnodeBlockList([NotNull] metafParser.NavnodeBlockListContext context)
    {
        return context.navnodeBlock().Select(c => (NavNode)Visit(c)).ToList();
    }

    public override object VisitNavnodeBlock([NotNull] metafParser.NavnodeBlockContext context)
    {
        return (NavNode)Visit(context.navnode());
    }

    public override object VisitNavtype([NotNull] metafParser.NavtypeContext context) => context.GetText() switch
    {
        "circular" => NavType.Circular,
        "linear" => NavType.Linear,
        "once" => NavType.Once,
        "follow" => NavType.Follow,
        string s => throw new MetaParserException("Invalid nav type", "circular|follow|linear|once", s)
    };

    public override object VisitNav([NotNull] metafParser.NavContext context)
    {
        // navReferences
        var route = new NavRoute()
        {
            Type = (NavType)Visit(context.navtype()),
            Data = Visit(context.navdef())
        };

        if (navReferences.TryGetValue(context.ID().GetText(), out var list))
        {
            list.ForEach(action => action.Data = (action.Data.name, route));
        }

        return route;
    }

    public override object VisitSetWatchdogAction([NotNull] metafParser.SetWatchdogActionContext context)
    {
        return new WatchdogSetMetaAction()
        {
            Range = double.Parse(context.DOUBLE().GetText()),
            Time = (double)int.Parse(context.INT().GetText()),
            State = context.STRING().GetText().UnescapeStringValue()
        };
    }

    public override object VisitEmbedNavAction([NotNull] metafParser.EmbedNavActionContext context)
    {
        var action = context.xfArg() != null ?
            new EmbeddedNavRouteMetaActionWithTransform() :
            new EmbeddedNavRouteMetaAction();

        // need to use navReferences
        if (context.xfArg() != null)
        {
            ((EmbeddedNavRouteMetaActionWithTransform)action).Transform = (double[])Visit(context.xfArg());
        }

        action.Data = (context.STRING().GetText().UnescapeStringValue(), null);

        if (!navReferences.TryGetValue(context.ID().GetText(), out var list))
        {
            list = new();
            navReferences[context.ID().GetText()] = list;
        }

        list.Add(action);

        return action;
    }

    public override object VisitOptionAction([NotNull] metafParser.OptionActionContext context) => context.actionType.Text switch
    {
        "SetOpt" => new SetVTOptionMetaAction()
        {
            Option = context.STRING(0).GetText().UnescapeStringValue(),
            Value = context.STRING(1).GetText().UnescapeStringValue()
        },
        "GetOpt" => new GetVTOptionMetaAction()
        {
            Option = context.STRING(0).GetText().UnescapeStringValue(),
            Variable = context.STRING(1).GetText().UnescapeStringValue()
        },
        "CallState" => new CallStateMetaAction()
        {
            CallState = context.STRING(0).GetText().UnescapeStringValue(),
            ReturnState = context.STRING(1).GetText().UnescapeStringValue()
        },
        "CreateView" => new CreateViewMetaAction()
        {
            ViewName = context.STRING(0).GetText().UnescapeStringValue(),
            ViewDefinition = context.STRING(1).GetText().UnescapeStringValue()
        }
    };

    public override object VisitStringActionType([NotNull] metafParser.StringActionTypeContext context) => context.GetText() switch
    {
        "Chat" => ActionType.ChatCommand,
        "SetState" => ActionType.SetState,
        "ChatExpr" => ActionType.ChatWithExpression,
        "DoExpr" => ActionType.ExpressionAct,
        "DestroyView" => ActionType.DestroyView,
        string s => throw new MetaParserException("Invalid string action type", "Chat|SetState|ChatExpr|DoExpr|DestroyView", s)
    };

    public override object VisitStringAction([NotNull] metafParser.StringActionContext context)
    {
        var type = (ActionType)Visit(context.stringActionType());
        var action = (MetaAction<string>)MetaAction.CreateMetaAction(type);
        action.Data = context.STRING().GetText().UnescapeStringValue();
        return action;
    }

    public override object VisitEmptyAction([NotNull] metafParser.EmptyActionContext context) => context.GetText() switch
    {
        "None" => MetaAction.CreateMetaAction(ActionType.None),
        "Return" => MetaAction.CreateMetaAction(ActionType.ReturnFromCall),
        "DestroyAllViews" => MetaAction.CreateMetaAction(ActionType.DestroyAllViews),
        string s => throw new MetaParserException("Invalid empty action type", "None|Return|DestroyAllViews", s)
    };

    public override object VisitMultipleAction([NotNull] metafParser.MultipleActionContext context)
    {
        return new AllMetaAction()
        {
            Data = (List<MetaAction>)Visit(context.actionBlockList())
        };
    }

    public override object VisitActionBlockList([NotNull] metafParser.ActionBlockListContext context)
    {
        return context.actionBlock().Select(c => (MetaAction)Visit(c)).ToList();
    }

    public override object VisitActionBlock([NotNull] metafParser.ActionBlockContext context)
    {
        return (MetaAction)Visit(context.action());
    }

    public override object VisitSecsOnSpellCondition([NotNull] metafParser.SecsOnSpellConditionContext context)
    {
        return new TimeLeftOnSpellGECondition()
        {
            SpellId = int.Parse(context.INT(0).GetText()),
            Seconds = int.Parse(context.INT(1).GetText())
        };
    }

    public override object VisitDistToRouteCondition([NotNull] metafParser.DistToRouteConditionContext context)
    {
        return new DistanceToAnyRoutePointGECondition()
        {
            Distance = double.Parse(context.DOUBLE().GetText())
        };
    }

    public override object VisitLandCellCondition([NotNull] metafParser.LandCellConditionContext context)
    {
        var cond =  (Condition<int>)(context.conditionType.Text switch
        {
            "CellE" => Condition.CreateCondition(ConditionType.LandCellE),
            "BlockE" => Condition.CreateCondition(ConditionType.LandBlockE),
            _ => throw new MetaParserException("Invalid landcell/block condition type", "CellE|BlockE", context.conditionType.Text)
        });

        cond.Data = int.Parse(context.HEXINT().GetText(), System.Globalization.NumberStyles.HexNumber);

        return cond;
    }

    public override object VisitMobsInDistNameCondition([NotNull] metafParser.MobsInDistNameConditionContext context)
    {
        return new MonsterCountWithinDistanceCondition()
        {
            MonsterNameRx = context.STRING().GetText().UnescapeStringValue(),
            Count = int.Parse(context.INT().GetText()),
            Distance = double.Parse(context.DOUBLE().GetText())
        };
    }

    public override object VisitMobsInDistPriorityCondition([NotNull] metafParser.MobsInDistPriorityConditionContext context)
    {
        return new MonstersWithPriorityWithinDistanceCondition()
        {
            Count = int.Parse(context.INT(0).GetText()),
            Distance = double.Parse(context.DOUBLE().GetText()),
            Priority = int.Parse(context.INT(1).GetText())
        };
    }

    public override object VisitChatCaptureCondition([NotNull] metafParser.ChatCaptureConditionContext context)
    {
        return new ChatMessageCaptureCondition()
        {
            Pattern = context.STRING(0).GetText().UnescapeStringValue(),
            Color = context.STRING(1).GetText().UnescapeStringValue()
        };
    }

    public override object VisitNoMobsInDistCondition([NotNull] metafParser.NoMobsInDistConditionContext context)
    {
        return new NoMonstersInDistanceCondition()
        {
            Distance = double.Parse(context.DOUBLE().GetText())
        };
    }

    public override object VisitIntConditionType([NotNull] metafParser.IntConditionTypeContext context) => context.GetText() switch
    {
        "PSecsInStateGE" => ConditionType.SecondsInStatePersistGE,
        "SecsInStateGE" => ConditionType.SecondsInStateGE,
        "BuPercentGE" => ConditionType.BurdenPercentGE,
        "MainSlotsLE" => ConditionType.MainPackSlotsLE,
        string s => throw new MetaParserException("Invalid int condition type", "PSecsInStateGE|SecsInStateGE|BuPercentGE|MainSlotsLE", s)
    };

    public override object VisitIntCondition([NotNull] metafParser.IntConditionContext context)
    {
        var cond = (Condition<int>)Condition.CreateCondition((ConditionType)Visit(context.intConditionType()));
        cond.Data = int.Parse(context.INT().GetText());
        return cond;
    }

    public override object VisitStringCondition([NotNull] metafParser.StringConditionContext context)
    {
        var cond = (Condition<string>)Condition.CreateCondition(context.conditionType.Text switch
        {
            "Expr" => ConditionType.Expression,
            "ChatMatch" => ConditionType.ChatMessage
        });

        cond.Data = context.STRING().GetText().UnescapeStringValue();

        return cond;
    }

    public override object VisitEmptyCondition([NotNull] metafParser.EmptyConditionContext context) => context.GetText() switch
    {
        "Always" => Condition.CreateCondition(ConditionType.Always),
        "Never" => Condition.CreateCondition(ConditionType.Never),
        "NavEmpty" => Condition.CreateCondition(ConditionType.NavrouteEmpty),
        "Death" => Condition.CreateCondition(ConditionType.Died),
        "VendorOpen" => Condition.CreateCondition(ConditionType.VendorOpen),
        "VendorClosed" => Condition.CreateCondition(ConditionType.VendorClosed),
        "IntoPortal" => Condition.CreateCondition(ConditionType.PortalspaceEnter),
        "ExitPortal" => Condition.CreateCondition(ConditionType.PortalspaceExit),
        string s => throw new MetaParserException("Invalid empty condition type", "Always|Never|NavEmpty|Death|VendorOpen|VendorClosed|IntoPortal|ExitPortal", s)
    };

    public override object VisitNotCondition([NotNull] metafParser.NotConditionContext context)
    {
        var cond = (NotCondition)Condition.CreateCondition(ConditionType.Not);
        cond.Data = (Condition)Visit(context.conditionBlock());
        return cond;
    }

    public override object VisitMultipleCondition([NotNull] metafParser.MultipleConditionContext context)
    {
        var type = context.conditionType.Text switch
        {
            "All" => ConditionType.All,
            "Any" => ConditionType.Any,
            string s => throw new MetaParserException("Invalid multiple condition type", "All|Any", s)
        };

        var cond = (MultipleCondition)Condition.CreateCondition(type);

        cond.Data = (List<Condition>)Visit(context.conditionBlockList());

        return cond;
    }

    public override object VisitConditionBlock([NotNull] metafParser.ConditionBlockContext context)
    {
        return (Condition)Visit(context.condition());
    }

    public override object VisitConditionBlockList([NotNull] metafParser.ConditionBlockListContext context)
    {
        return context.conditionBlock().Select(c => (Condition)Visit(c)).ToList();
    }

    public override object VisitRule([NotNull] metafParser.RuleContext context)
    {
        return new Rule()
        {
            Condition = (Condition)Visit(context.conditionBlock()),
            Action = (MetaAction)Visit(context.actionBlock()),
        };
    }

    public override object VisitState([NotNull] metafParser.StateContext context)
    {
        var state = context.STRING().GetText().UnescapeStringValue();

        var rules = new List<Rule>();
        foreach (var rc in context.rule())
        {
            var rule = (Rule)Visit(rc);
            rule.State = state;
            rules.Add(rule);
        }

        return rules;
    }

    public override object VisitProg([NotNull] metafParser.ProgContext context)
    {
        var m = new Meta();
        foreach (var rule in context.state().SelectMany(c => (List<Rule>)Visit(c)).OrderBy(r => r.State))
        {
            m.Rules.Add(rule);
        }

        return m;
    }

    public override object VisitErrorNode(IErrorNode node)
    {
        throw new MetaParserException($"Parsing error on line {node.Symbol.Line} at index {node.Symbol.Column}");
    }
}