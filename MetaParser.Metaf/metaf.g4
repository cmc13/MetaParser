grammar metaf;

prog
	: state* nav* EOF
	;

state
	: 'STATE:' STRING RN rule*
	;

rule
	: '\tIF:' RN? conditionBlock '\t\tDO:' RN? actionBlock
	;

conditionBlock
	: '\t'* condition RN
	;

conditionBlockList
	: conditionBlock*
	;

condition
	: multipleCondition
	| notCondition
	| emptyCondition
	| stringCondition
	| intCondition
	| noMobsInDistCondition
	| chatCaptureCondition
	| mobsInDistPriorityCondition
	| mobsInDistNameCondition
	| landCellCondition
	| distToRouteCondition
	| secsOnSpellCondition
	;

multipleCondition
	: conditionType=( 'All' | 'Any' ) RN conditionBlockList
	;

notCondition
	: 'Not' RN? conditionBlock
	;

emptyCondition
	: 'Always'
	| 'Never'
	| 'NavEmpty'
	| 'Death'
	| 'VendorOpen'
	| 'VendorClosed'
	| 'IntoPortal'
	| 'ExitPortal'
	;

stringCondition
	: conditionType=( 'Expr' | 'ChatMatch' ) STRING
	;

intCondition
	: intConditionType INT
	;

intConditionType
	: 'PSecsInStateGE'
	| 'SecsInStateGE'
	| 'BuPercentGE'
	| 'MainSlotsLE'
	;

noMobsInDistCondition
	: 'NoMobsInDist' DOUBLE
	;

chatCaptureCondition
	: 'ChatCapture' STRING STRING
	;

mobsInDistPriorityCondition
	: 'MobsInDist_Priority' INT DOUBLE INT
	;

mobsInDistNameCondition
	: 'MobsInDist_Name' INT DOUBLE STRING
	;

landCellCondition
	: conditionType=( 'CellE' | 'BlockE' ) HEXINT
	;

distToRouteCondition
	: 'DistToRteGE' DOUBLE
	;

secsOnSpellCondition
	: 'SecsOnSpellGE' INT INT
	;

actionBlock
	: '\t'* action RN
	;

action
	: multipleAction
	| emptyAction
	| stringAction
	| optionAction
	| embedNavAction
	| setWatchdogAction
	;

multipleAction
	: 'DoAll' RN actionBlockList
	;

actionBlockList
	: actionBlock*
	;

emptyAction
	: 'None'
	| 'Return'
	| 'DestroyAllViews'
	;

stringAction
	: stringActionType STRING
	;

stringActionType
	: 'Chat'
	| 'SetState'
	| 'ChatExpr'
	| 'DoExpr'
	| 'DestroyView'
	;

optionAction
	: actionType=( 'SetOpt' | 'GetOpt' | 'CallState' | 'CreateView' ) STRING STRING
	;

embedNavAction
	: 'EmbedNav' ID STRING xfArg?
	;

setWatchdogAction
	: 'SetWatchdog' DOUBLE INT STRING
	;

nav
	: 'NAV:' ID navtype RN navdef
	;

navtype
	: 'circular'
	| 'linear'
	| 'once'
	| 'follow'
	;

navdef
	: navfollow
	| navnodeBlockList
	;

navnodeBlockList
	: navnodeBlock*
	;

navfollow
	: '\t'* 'flw' HEXINT STRING RN
	;

navnodeBlock
	: '\t'* navnode RN
	;

navnode
	: nodeType=( 'pnt' | 'chk' ) pointDef						#pointNode
	| 'prt' pointDef INT										#portalObsNode
	| 'rcl' pointDef STRING 									#recallNode
	| 'pau' pointDef DOUBLE										#pauseNode
	| 'cht' pointDef STRING									#chatNode
	| 'vnd' pointDef HEXINT STRING							#vendorNode
	| 'ptl' pointDef pointDef INT STRING						#portalNode
	| 'tlk' pointDef pointDef INT STRING						#npcChatNode
	| 'jmp' pointDef DOUBLE shift=( 'True' | 'False') DOUBLE	#jumpNode
	;

pointDef
	: DOUBLE DOUBLE DOUBLE
	;

xfArg
	: '{' DOUBLE DOUBLE DOUBLE DOUBLE DOUBLE DOUBLE DOUBLE '}'
	;

fragment ESC: '{{' | '}}' ;
STRING: '{' ~[{}]* | ESC '}' ;
fragment SIGN: [+-] ;
DOUBLE: SIGN? (INT* '.')? INT+ ;
ID: [a-zA-Z_][a-zA-Z0-9_]+ ;
INT: [0-9]+ ;
HEXINT: [0-9a-fA-F]+ ;
RN: '\r'? '\n' ;

COMMENTS: '~~' ~[\r\n]* -> skip ;
NEWLINE: RN -> skip ;
WS: [ \t]* -> skip ;