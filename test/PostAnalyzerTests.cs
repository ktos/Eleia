using Eleia;
using Eleia.CoyoteApi;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using Xunit;

namespace Eleia.Test
{    
    public class PostAnalyzerTests
    {
        [Fact]
        public void Analyze_PostWithQuotes_NoProblems()
        {
            var postAnalyzer = new PostAnalyzer(null);
            Post post = new Post { text = "> cmd napisał(a):\nU mnie w firmie było do przejścia jedno zadanie na tablicy, był to fizzbuzz ale to tylko dla juniorów tak? czy mid deweloperzy tez u was fizzfuzz nie przechodzili?" };

            var result = postAnalyzer.AnalyzeText(post);

            Assert.False(result.Prediction);
        }

        [Fact]
        public void Analyze_SimplePost_NoProblems()
        {
            var postAnalyzer = new PostAnalyzer(null);
            Post post = new Post { text = "A to Kotlin już nie na topie?" };

            var result = postAnalyzer.AnalyzeText(post);

            Assert.False(result.Prediction);
        }

        [Fact(Skip = "Temporary: Needs to be aligned to markdown format")]
        public void Analyze_LongPost_NoProblems()
        {
            var postAnalyzer = new PostAnalyzer(null);
            Post post = new Post { text = "" };

            var result = postAnalyzer.AnalyzeText(post);

            Assert.False(result.Prediction);
        }

        [Fact(Skip = "Temporary: Needs to be aligned to markdown format")]
        public void Analyze_PostWithProperCode_NoProblems()
        {
            var postAnalyzer = new PostAnalyzer(null);
            Post post = new Post { text = "<pre><code class=\"language-python line-numbers\">\n\n'''\n\n                            Online Python Interpreter.\n                Code, Compile, Run and Debug python program online.\nWrite your code in this editor and press \"Run\" button to execute it.\n\n'''\n\nclass Hand: # nie programuj w python2 tylko w python3 (python2 ma tylko wsparcie do konca roku)\n    \"\"\" Ręka - wszystkie karty trzymane przez gracza. \"\"\"\n    def __init__(self):\n        self.cards = []\n\n    def __str__(self):\n        if self.cards:\n           rep = \"\"\n           for card in self.cards:\n               rep += str(card) + \"\\t\"\n        else:\n            rep = \"&lt;pusta&gt;\"\n        return rep\n\n    def add(self, card: \"Card\") -&gt; None: # dodawaj typowanie argumentow i to co funkcja zwraca \n        self.cards.append(card)\n\nclass Card:\n    \"\"\" Karta do gry. \"\"\"\n    RANKS = [\"2\", \"3\", \"4\", \"5\", \"6\", \"7\", \"8\",\n             \"9\", \"10\", \"J\", \"Q\", \"K\", \"A\"]\n    SUITS = [\"pik\", \"kier\", \"trefl\", \"karo\"]\n\n    def __init__(self, rank, suit):\n        self.rank = rank\n        self.suit = suit\n\n    def __str__(self):\n        rep = f\"{self.rank} {self.suit}\" # poczytaj co to f stringi  :) \n        return rep\n\nclass WarCard(Card): # piszemy kod po angielsku. nazwy klas sa uppercase czyli WarCard nigdy War_Card to jest mic uppercase z snakecase\n    \"\"\" Karta do wojny. Ustawienie wartości kart. \"\"\"\n\n    @property\n    def value(self) -&gt; int:\n        value = WarCard.RANKS.index(self.rank)\n        return value # co to znaczy v? staraj sie pisac doslowniej, bo kiedys sie zgubisz w wiekszysc kodzie\n\nclass WarHand(Hand): # tu tak samo jak wyzej\n    \"\"\" Ręka w wojnie. \"\"\"\n    def __init__(self, name):\n        super().__init__() # w python3 nie musisz do super przekazywac klasy \n        self.name = name\n\n    def __str__(self):\n        rep = f\"{self.name}    {super(WarHand, self).__str__()}\"\n        if self.total:\n            rep += f\"(ma {self.total} punktów)\"\n        return rep\n\n    @property\n    def total(self) -&gt; int:\n        t = 0\n        for card in self.cards:\n            t += card.value\n        return t \n\ncard1 = WarCard(\"3\", \"pik\")\nprint(\"karta: \" + str(card1) + \". Jej wartość wynosi: \" + str(card1.value))\n\ncard2 = WarCard(\"2\", \"kier\")\nprint(\"karta: \" + str(card2) + \". Jej wartość wynosi: \" + str(card2.value))\n\nreka = WarHand(\"Tomasz\")\nreka.add(card1)\nprint(reka)\n\nreka = WarHand(\"Jan\")\nreka.add(card2)\nprint(reka)\n\ninput(\"\\n\\nAby zakończyć program, naciśnij klawisz Enter.\")\n\ndalem Ci pare wskazowek plus tutaj WarHand/WarCard jest zbedne i dziedziczenie po Hand/Card mozesz spokojnie w tych klasach te metody z War klas przeniesc</code></pre>" };

            var result = postAnalyzer.AnalyzeText(post);

            Assert.False(result.Prediction);
        }

        [Fact(Skip = "Temporary: Needs to be aligned to markdown format")]
        public void Analyze_PostWithLinksThreshold099_NoProblems()
        {
            var myConfiguration = new Dictionary<string, string>
            {
                {"threshold", "0.99"}
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(myConfiguration)
                .Build();

            var postAnalyzer = new PostAnalyzer(configuration);
            Post post = new Post { text = "<pre><code class=\"language-csharp line-numbers\">protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)\r\n{\r\n    optionsBuilder\r\n        .ConfigureWarnings(w =&gt; w.Throw(RelationalEventId.QueryClientEvaluationWarning));\r\n}</code></pre>\r\n<p>Źródło: </p>\r\n<ol>\r\n<li><a href=\"https://compiledexperience.com/blog/posts/ef-core-client-side-eval\">https://compiledexperience.com/blog/posts/ef-core-client-side-eval</a></li>\r\n<li><a href=\"https://docs.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.dbcontextoptionsbuilder.configurewarnings?view=efcore-2.2\">https://docs.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.dbcontextoptionsbuilder.configurewarnings?view=efcore-2.2</a></li>\r\n</ol>" };

            var result = postAnalyzer.AnalyzeText(post);

            Assert.False(result.Prediction);
        }

        [Fact(Skip = "Temporary: Needs to be aligned to markdown format")]
        public void Analyze_PostWithLinksThreshold08_ProblemsFound()
        {
            var myConfiguration = new Dictionary<string, string>
            {
                {"threshold", "0.8"}
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(myConfiguration)
                .Build();

            var postAnalyzer = new PostAnalyzer(configuration);
            Post post = new Post { text = "<pre><code class=\"language-csharp line-numbers\">protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)\r\n{\r\n    optionsBuilder\r\n        .ConfigureWarnings(w =&gt; w.Throw(RelationalEventId.QueryClientEvaluationWarning));\r\n}</code></pre>\r\n<p>Źródło: </p>\r\n<ol>\r\n<li><a href=\"https://compiledexperience.com/blog/posts/ef-core-client-side-eval\">https://compiledexperience.com/blog/posts/ef-core-client-side-eval</a></li>\r\n<li><a href=\"https://docs.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.dbcontextoptionsbuilder.configurewarnings?view=efcore-2.2\">https://docs.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.dbcontextoptionsbuilder.configurewarnings?view=efcore-2.2</a></li>\r\n</ol>" };

            var result = postAnalyzer.AnalyzeText(post);

            Assert.True(result.Prediction);            
        }

        [Fact(Skip = "Temporary: Needs to be aligned to markdown format")]
        public void Analyze_PostWithUnformattedCodeThreshold097_UnformattedCodeFound()
        {
            var myConfiguration = new Dictionary<string, string>
            {
                {"threshold", "0.97"}
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(myConfiguration)
                .Build();

            var postAnalyzer = new PostAnalyzer(configuration);
            Post post = new Post { text = "<p>Witam zrobilem zadanie , z systemu 10 na system 16, ma ktoś jakiś pomysł na lepsze rozwiązanie?<br />\n#include &lt;iostream&gt;</p>\n<p>using namespace std;</p>\n<p>int main()<br />\n{<br />\nint n;<br />\ncin&gt;&gt;n;<br />\nint w;<br />\nint c=0;<br />\nint tab[c];<br />\nwhile(n&gt;0)<br />\n{<br />\nc++;<br />\nw=n%16;<br />\nn=n/16;</p>\n<pre><code>tab[c]=w;</code></pre>\n<p>}<br />\ncout&lt;&lt;\"System szesnastkowy to: \";<br />\nfor(int i=c; i&gt;0; i--)<br />\n{<br />\nif(tab[i]==10)<br />\n{<br />\ncout&lt;&lt;\"A\";<br />\n}<br />\nelse if(tab[i]==11)<br />\n{<br />\ncout&lt;&lt;\"B\";<br />\n}<br />\nelse if(tab[i]==12)<br />\n{<br />\ncout&lt;&lt;\"C\";<br />\n}<br />\nelse if(tab[i]==13)<br />\n{<br />\ncout&lt;&lt;\"D\";<br />\n}<br />\nelse if(tab[i]==14)<br />\n{<br />\ncout&lt;&lt;\"E\";<br />\n}<br />\nelse if(tab[i]==15)<br />\n{<br />\ncout&lt;&lt;\"F\";<br />\n}<br />\nelse<br />\n{<br />\ncout&lt;&lt;tab[i];<br />\n}<br />\n}<br />\n}</p>" };

            var result = postAnalyzer.AnalyzeText(post);

            Assert.True(result.Prediction);            
        }
    }
}