//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     ANTLR Version: 4.12.0
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from C:\Users\Cole\source\repos\MetaParser\MetaParser.WPF\Expressions\MetaExpressions.g4 by ANTLR 4.12.0

// Unreachable code detected
#pragma warning disable 0162
// The variable '...' is assigned but its value is never used
#pragma warning disable 0219
// Missing XML comment for publicly visible type or member '...'
#pragma warning disable 1591
// Ambiguous reference in cref attribute
#pragma warning disable 419

using System;
using System.IO;
using System.Text;
using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Misc;
using DFA = Antlr4.Runtime.Dfa.DFA;

[System.CodeDom.Compiler.GeneratedCode("ANTLR", "4.12.0")]
[System.CLSCompliant(false)]
public partial class MetaExpressionsLexer : Lexer {
	protected static DFA[] decisionToDFA;
	protected static PredictionContextCache sharedContextCache = new PredictionContextCache();
	public const int
		T__0=1, T__1=2, T__2=3, T__3=4, T__4=5, T__5=6, T__6=7, T__7=8, T__8=9, 
		T__9=10, T__10=11, T__11=12, T__12=13, T__13=14, T__14=15, T__15=16, T__16=17, 
		T__17=18, T__18=19, T__19=20, T__20=21, T__21=22, T__22=23, T__23=24, 
		T__24=25, T__25=26, T__26=27, T__27=28, T__28=29, T__29=30, T__30=31, 
		BOOL=32, MINUS=33, NUMBER=34, HEXNUMBER=35, STRING=36, WHITESPACE=37;
	public static string[] channelNames = {
		"DEFAULT_TOKEN_CHANNEL", "HIDDEN"
	};

	public static string[] modeNames = {
		"DEFAULT_MODE"
	};

	public static readonly string[] ruleNames = {
		"T__0", "T__1", "T__2", "T__3", "T__4", "T__5", "T__6", "T__7", "T__8", 
		"T__9", "T__10", "T__11", "T__12", "T__13", "T__14", "T__15", "T__16", 
		"T__17", "T__18", "T__19", "T__20", "T__21", "T__22", "T__23", "T__24", 
		"T__25", "T__26", "T__27", "T__28", "T__29", "T__30", "BOOL", "MINUS", 
		"NUMBER", "HEXNUMBER", "STRING", "WHITESPACE", "DIGIT", "BSLASH"
	};


	public MetaExpressionsLexer(ICharStream input)
	: this(input, Console.Out, Console.Error) { }

	public MetaExpressionsLexer(ICharStream input, TextWriter output, TextWriter errorOutput)
	: base(input, output, errorOutput)
	{
		Interpreter = new LexerATNSimulator(this, _ATN, decisionToDFA, sharedContextCache);
	}

	private static readonly string[] _LiteralNames = {
		null, "';'", "'('", "')'", "'$'", "'@'", "'&'", "'{'", "':'", "'}'", "'~'", 
		"'>>'", "'<<'", "'^'", "'|'", "'*'", "'/'", "'%'", "'+'", "'#'", "'='", 
		"'>'", "'<'", "'>='", "'<='", "'=='", "'!='", "'&&'", "'||'", "'['", "','", 
		"']'", null, "'-'"
	};
	private static readonly string[] _SymbolicNames = {
		null, null, null, null, null, null, null, null, null, null, null, null, 
		null, null, null, null, null, null, null, null, null, null, null, null, 
		null, null, null, null, null, null, null, null, "BOOL", "MINUS", "NUMBER", 
		"HEXNUMBER", "STRING", "WHITESPACE"
	};
	public static readonly IVocabulary DefaultVocabulary = new Vocabulary(_LiteralNames, _SymbolicNames);

	[NotNull]
	public override IVocabulary Vocabulary
	{
		get
		{
			return DefaultVocabulary;
		}
	}

	public override string GrammarFileName { get { return "MetaExpressions.g4"; } }

	public override string[] RuleNames { get { return ruleNames; } }

	public override string[] ChannelNames { get { return channelNames; } }

	public override string[] ModeNames { get { return modeNames; } }

	public override int[] SerializedAtn { get { return _serializedATN; } }

	static MetaExpressionsLexer() {
		decisionToDFA = new DFA[_ATN.NumberOfDecisions];
		for (int i = 0; i < _ATN.NumberOfDecisions; i++) {
			decisionToDFA[i] = new DFA(_ATN.GetDecisionState(i), i);
		}
	}
	private static int[] _serializedATN = {
		4,0,37,229,6,-1,2,0,7,0,2,1,7,1,2,2,7,2,2,3,7,3,2,4,7,4,2,5,7,5,2,6,7,
		6,2,7,7,7,2,8,7,8,2,9,7,9,2,10,7,10,2,11,7,11,2,12,7,12,2,13,7,13,2,14,
		7,14,2,15,7,15,2,16,7,16,2,17,7,17,2,18,7,18,2,19,7,19,2,20,7,20,2,21,
		7,21,2,22,7,22,2,23,7,23,2,24,7,24,2,25,7,25,2,26,7,26,2,27,7,27,2,28,
		7,28,2,29,7,29,2,30,7,30,2,31,7,31,2,32,7,32,2,33,7,33,2,34,7,34,2,35,
		7,35,2,36,7,36,2,37,7,37,2,38,7,38,1,0,1,0,1,1,1,1,1,2,1,2,1,3,1,3,1,4,
		1,4,1,5,1,5,1,6,1,6,1,7,1,7,1,8,1,8,1,9,1,9,1,10,1,10,1,10,1,11,1,11,1,
		11,1,12,1,12,1,13,1,13,1,14,1,14,1,15,1,15,1,16,1,16,1,17,1,17,1,18,1,
		18,1,19,1,19,1,20,1,20,1,21,1,21,1,22,1,22,1,22,1,23,1,23,1,23,1,24,1,
		24,1,24,1,25,1,25,1,25,1,26,1,26,1,26,1,27,1,27,1,27,1,28,1,28,1,29,1,
		29,1,30,1,30,1,31,1,31,1,31,1,31,1,31,1,31,1,31,1,31,1,31,3,31,159,8,31,
		1,32,1,32,1,33,5,33,164,8,33,10,33,12,33,167,9,33,1,33,3,33,170,8,33,1,
		33,4,33,173,8,33,11,33,12,33,174,1,34,1,34,1,34,1,34,4,34,181,8,34,11,
		34,12,34,182,1,35,1,35,1,35,1,35,1,35,5,35,190,8,35,10,35,12,35,193,9,
		35,1,35,1,35,1,35,1,35,1,35,4,35,200,8,35,11,35,12,35,201,1,35,4,35,205,
		8,35,11,35,12,35,206,1,35,1,35,1,35,5,35,212,8,35,10,35,12,35,215,9,35,
		3,35,217,8,35,1,36,4,36,220,8,36,11,36,12,36,221,1,36,1,36,1,37,1,37,1,
		38,1,38,0,0,39,1,1,3,2,5,3,7,4,9,5,11,6,13,7,15,8,17,9,19,10,21,11,23,
		12,25,13,27,14,29,15,31,16,33,17,35,18,37,19,39,20,41,21,43,22,45,23,47,
		24,49,25,51,26,53,27,55,28,57,29,59,30,61,31,63,32,65,33,67,34,69,35,71,
		36,73,37,75,0,77,0,1,0,14,2,0,84,84,116,116,2,0,82,82,114,114,2,0,85,85,
		117,117,2,0,69,69,101,101,2,0,70,70,102,102,2,0,65,65,97,97,2,0,76,76,
		108,108,2,0,83,83,115,115,3,0,48,57,65,70,97,102,1,0,96,96,5,0,34,34,39,
		39,65,90,95,95,97,122,7,0,32,32,34,34,39,39,48,57,65,90,95,95,97,122,3,
		0,9,10,13,13,32,32,1,0,48,57,240,0,1,1,0,0,0,0,3,1,0,0,0,0,5,1,0,0,0,0,
		7,1,0,0,0,0,9,1,0,0,0,0,11,1,0,0,0,0,13,1,0,0,0,0,15,1,0,0,0,0,17,1,0,
		0,0,0,19,1,0,0,0,0,21,1,0,0,0,0,23,1,0,0,0,0,25,1,0,0,0,0,27,1,0,0,0,0,
		29,1,0,0,0,0,31,1,0,0,0,0,33,1,0,0,0,0,35,1,0,0,0,0,37,1,0,0,0,0,39,1,
		0,0,0,0,41,1,0,0,0,0,43,1,0,0,0,0,45,1,0,0,0,0,47,1,0,0,0,0,49,1,0,0,0,
		0,51,1,0,0,0,0,53,1,0,0,0,0,55,1,0,0,0,0,57,1,0,0,0,0,59,1,0,0,0,0,61,
		1,0,0,0,0,63,1,0,0,0,0,65,1,0,0,0,0,67,1,0,0,0,0,69,1,0,0,0,0,71,1,0,0,
		0,0,73,1,0,0,0,1,79,1,0,0,0,3,81,1,0,0,0,5,83,1,0,0,0,7,85,1,0,0,0,9,87,
		1,0,0,0,11,89,1,0,0,0,13,91,1,0,0,0,15,93,1,0,0,0,17,95,1,0,0,0,19,97,
		1,0,0,0,21,99,1,0,0,0,23,102,1,0,0,0,25,105,1,0,0,0,27,107,1,0,0,0,29,
		109,1,0,0,0,31,111,1,0,0,0,33,113,1,0,0,0,35,115,1,0,0,0,37,117,1,0,0,
		0,39,119,1,0,0,0,41,121,1,0,0,0,43,123,1,0,0,0,45,125,1,0,0,0,47,128,1,
		0,0,0,49,131,1,0,0,0,51,134,1,0,0,0,53,137,1,0,0,0,55,140,1,0,0,0,57,143,
		1,0,0,0,59,145,1,0,0,0,61,147,1,0,0,0,63,158,1,0,0,0,65,160,1,0,0,0,67,
		165,1,0,0,0,69,176,1,0,0,0,71,216,1,0,0,0,73,219,1,0,0,0,75,225,1,0,0,
		0,77,227,1,0,0,0,79,80,5,59,0,0,80,2,1,0,0,0,81,82,5,40,0,0,82,4,1,0,0,
		0,83,84,5,41,0,0,84,6,1,0,0,0,85,86,5,36,0,0,86,8,1,0,0,0,87,88,5,64,0,
		0,88,10,1,0,0,0,89,90,5,38,0,0,90,12,1,0,0,0,91,92,5,123,0,0,92,14,1,0,
		0,0,93,94,5,58,0,0,94,16,1,0,0,0,95,96,5,125,0,0,96,18,1,0,0,0,97,98,5,
		126,0,0,98,20,1,0,0,0,99,100,5,62,0,0,100,101,5,62,0,0,101,22,1,0,0,0,
		102,103,5,60,0,0,103,104,5,60,0,0,104,24,1,0,0,0,105,106,5,94,0,0,106,
		26,1,0,0,0,107,108,5,124,0,0,108,28,1,0,0,0,109,110,5,42,0,0,110,30,1,
		0,0,0,111,112,5,47,0,0,112,32,1,0,0,0,113,114,5,37,0,0,114,34,1,0,0,0,
		115,116,5,43,0,0,116,36,1,0,0,0,117,118,5,35,0,0,118,38,1,0,0,0,119,120,
		5,61,0,0,120,40,1,0,0,0,121,122,5,62,0,0,122,42,1,0,0,0,123,124,5,60,0,
		0,124,44,1,0,0,0,125,126,5,62,0,0,126,127,5,61,0,0,127,46,1,0,0,0,128,
		129,5,60,0,0,129,130,5,61,0,0,130,48,1,0,0,0,131,132,5,61,0,0,132,133,
		5,61,0,0,133,50,1,0,0,0,134,135,5,33,0,0,135,136,5,61,0,0,136,52,1,0,0,
		0,137,138,5,38,0,0,138,139,5,38,0,0,139,54,1,0,0,0,140,141,5,124,0,0,141,
		142,5,124,0,0,142,56,1,0,0,0,143,144,5,91,0,0,144,58,1,0,0,0,145,146,5,
		44,0,0,146,60,1,0,0,0,147,148,5,93,0,0,148,62,1,0,0,0,149,150,7,0,0,0,
		150,151,7,1,0,0,151,152,7,2,0,0,152,159,7,3,0,0,153,154,7,4,0,0,154,155,
		7,5,0,0,155,156,7,6,0,0,156,157,7,7,0,0,157,159,7,3,0,0,158,149,1,0,0,
		0,158,153,1,0,0,0,159,64,1,0,0,0,160,161,5,45,0,0,161,66,1,0,0,0,162,164,
		3,75,37,0,163,162,1,0,0,0,164,167,1,0,0,0,165,163,1,0,0,0,165,166,1,0,
		0,0,166,169,1,0,0,0,167,165,1,0,0,0,168,170,5,46,0,0,169,168,1,0,0,0,169,
		170,1,0,0,0,170,172,1,0,0,0,171,173,3,75,37,0,172,171,1,0,0,0,173,174,
		1,0,0,0,174,172,1,0,0,0,174,175,1,0,0,0,175,68,1,0,0,0,176,177,5,48,0,
		0,177,178,5,120,0,0,178,180,1,0,0,0,179,181,7,8,0,0,180,179,1,0,0,0,181,
		182,1,0,0,0,182,180,1,0,0,0,182,183,1,0,0,0,183,70,1,0,0,0,184,191,5,96,
		0,0,185,186,3,77,38,0,186,187,5,96,0,0,187,190,1,0,0,0,188,190,8,9,0,0,
		189,185,1,0,0,0,189,188,1,0,0,0,190,193,1,0,0,0,191,189,1,0,0,0,191,192,
		1,0,0,0,192,194,1,0,0,0,193,191,1,0,0,0,194,217,5,96,0,0,195,200,7,10,
		0,0,196,197,3,77,38,0,197,198,9,0,0,0,198,200,1,0,0,0,199,195,1,0,0,0,
		199,196,1,0,0,0,200,201,1,0,0,0,201,199,1,0,0,0,201,202,1,0,0,0,202,213,
		1,0,0,0,203,205,7,11,0,0,204,203,1,0,0,0,205,206,1,0,0,0,206,204,1,0,0,
		0,206,207,1,0,0,0,207,212,1,0,0,0,208,209,3,77,38,0,209,210,9,0,0,0,210,
		212,1,0,0,0,211,204,1,0,0,0,211,208,1,0,0,0,212,215,1,0,0,0,213,211,1,
		0,0,0,213,214,1,0,0,0,214,217,1,0,0,0,215,213,1,0,0,0,216,184,1,0,0,0,
		216,199,1,0,0,0,217,72,1,0,0,0,218,220,7,12,0,0,219,218,1,0,0,0,220,221,
		1,0,0,0,221,219,1,0,0,0,221,222,1,0,0,0,222,223,1,0,0,0,223,224,6,36,0,
		0,224,74,1,0,0,0,225,226,7,13,0,0,226,76,1,0,0,0,227,228,5,92,0,0,228,
		78,1,0,0,0,15,0,158,165,169,174,182,189,191,199,201,206,211,213,216,221,
		1,6,0,0
	};

	public static readonly ATN _ATN =
		new ATNDeserializer().Deserialize(_serializedATN);


}
