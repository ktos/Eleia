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
            Post post = new Post { text = "<blockquote>\n<h5><a href=\"https://4programmers.net/Forum/1625684\">cmd napisał(a)</a>:</h5>\n<p>U mnie w firmie było do przejścia jedno zadanie na tablicy, był to fizzbuzz</p>\n</blockquote>\n<p>ale to tylko dla juniorów tak? czy mid deweloperzy tez u was fizzfuzz nie przechodzili?</p>" };

            var result = postAnalyzer.Analyze(post);

            Assert.Empty(result);
        }

        [Fact]
        public void Analyze_SimplePost_NoProblems()
        {
            var postAnalyzer = new PostAnalyzer(null);
            Post post = new Post { text = "<p>A to Kotlin już nie na topie?</p>" };

            var result = postAnalyzer.Analyze(post);

            Assert.Empty(result);
        }

        [Fact]
        public void Analyze_LongPost_NoProblems()
        {
            var postAnalyzer = new PostAnalyzer(null);
            Post post = new Post { text = "<blockquote>\n<p>Głupie rozmowy z HR i menadżerami, gdzie zadawane są mi pytania typu \"a jak pan zobaczy błąd w aplikacji, to co pan zrobi?\". Czasami mam ochotę odpowiedzieć \"zesram się i ucieknę\".</p>\n</blockquote>\n<p>IMO pytanie jak pytanie, w sumie nawet dobre.  Może sprawdzają czy ktoś ma w dupie wszystko albo czy będzie pracował dobrze jako członek większego zespołu.  Moja odpowiedź na takie pytanie byłaby taka że jak to bug w kodzie to zgłoszę do osoby odpowiedzialnej.  Jak ją znajdę - zależy, może po prostu wiem kto się tym zajmuje, może jest coś w stylu MAINTAINERS z Linuksa a w ostateczności zobaczę git blame kto ostatnio dotykał tego fragmentu.  Jak to bug nie w kodzie tylko w zachowaniu aplikacji to gorzej, odłożenie na TODO listę i ticket na lokalnym bug trackerze.</p>\n<blockquote>\n<p>Lwia część ludzi przeprowadzających te rozmowy nie powinna być na nie wogóle dopuszczona. I nie chodzi o to że są ubrani jak lumpy pod monopolem, bo to nie jest dla mnie problem. Większym problemem jest to, że te rozmowy nie wiadomo do czego prowadzą. Ostatnio byłem na jednej, gdzie przeszedłem test z podstaw programowania, gdzie pytania były typu \"czym się różni interfejs od klasy abstrakcyjnej (klasyk na rozmowach)\",. Co oni chcą sprawdzić u kandydata dając mu zadania i pytania na poziomie II roku studiów?</p>\n</blockquote>\n<p>Może nauczeni smutnym doświadczeniem wiedzą że są tacy którzy tego nie wiedzą. Albo co gorsze dla nich, nie potrafią <em>zwięźle</em> wyjaśnić.  Teraz dużo chodzę na rozmowy - takie proste pytania czasem pochodzą od osób które się nie za bardzo znają i chcą zadać jakiekolwiek najprostsze pytanie na które znają odpowiedź. Albo jest to pytanie które jest wstępem do kolejnego pytania - kiedy się czego do czego używa albo jak jest coś zaimplementowane pod spodem, jaki ma overhead etc.</p>\n<blockquote>\n<p>No i best part, czyli hajs. Mieszkam i pracuję w Warszawie, od kilku lat pracuję w .net, ostatnio w net core, znajomość front, angular, ef itd, na rozmowie odpowiadam na przynajmniej 80% głupich pytań, a jak mówię że chcę zarabiać 14lk netto na fakturę, to robią czasami oczy. Ostatnio mnie odrzucili z jednej firmy i po uzyskaniu feedbacku dowiedziałem się że chodzi o wymagania finansowe. To dlaczego firmy nie negocjują? Ja zawsze mówię stawkę 10-15% wyższą niż chcę, mogą zadzwonić i ponegocjować, jeżeli praca mi się podoba to mogę zejść z wymagań.</p>\n</blockquote>\n<p>No ale co się dziwisz, 14k to od cholery pieniędzy, pewnie mają tańszych.  Ja po 8 latach \"kariery\" w embedded szukam nowej pracy i jak na razie czekam na odpowiedź od firmy gdzie wstępnie powiedziałem 120 PLN/h ale od razu powiedzieli że bez płatnych urlopów bo się boją kontroli, i to do tej pory była największa oferta finansowa ale wątpię czy tyle dostane.  We wcześniejszej chcieli dać 750 PLN dziennie a jak powiedziałem że więcej chcę to się przestali odzywać.</p>\n<p>EDIT: aha, no i wiesz, 4 lata doświadczenia to naprawdę mało.</p>" };

            var result = postAnalyzer.Analyze(post);

            Assert.Empty(result);
        }

        [Fact]
        public void Analyze_PostWithProperCode_NoProblems()
        {
            var postAnalyzer = new PostAnalyzer(null);
            Post post = new Post { text = "<pre><code class=\"language-python line-numbers\">\n\n'''\n\n                            Online Python Interpreter.\n                Code, Compile, Run and Debug python program online.\nWrite your code in this editor and press \"Run\" button to execute it.\n\n'''\n\nclass Hand: # nie programuj w python2 tylko w python3 (python2 ma tylko wsparcie do konca roku)\n    \"\"\" Ręka - wszystkie karty trzymane przez gracza. \"\"\"\n    def __init__(self):\n        self.cards = []\n\n    def __str__(self):\n        if self.cards:\n           rep = \"\"\n           for card in self.cards:\n               rep += str(card) + \"\\t\"\n        else:\n            rep = \"&lt;pusta&gt;\"\n        return rep\n\n    def add(self, card: \"Card\") -&gt; None: # dodawaj typowanie argumentow i to co funkcja zwraca \n        self.cards.append(card)\n\nclass Card:\n    \"\"\" Karta do gry. \"\"\"\n    RANKS = [\"2\", \"3\", \"4\", \"5\", \"6\", \"7\", \"8\",\n             \"9\", \"10\", \"J\", \"Q\", \"K\", \"A\"]\n    SUITS = [\"pik\", \"kier\", \"trefl\", \"karo\"]\n\n    def __init__(self, rank, suit):\n        self.rank = rank\n        self.suit = suit\n\n    def __str__(self):\n        rep = f\"{self.rank} {self.suit}\" # poczytaj co to f stringi  :) \n        return rep\n\nclass WarCard(Card): # piszemy kod po angielsku. nazwy klas sa uppercase czyli WarCard nigdy War_Card to jest mic uppercase z snakecase\n    \"\"\" Karta do wojny. Ustawienie wartości kart. \"\"\"\n\n    @property\n    def value(self) -&gt; int:\n        value = WarCard.RANKS.index(self.rank)\n        return value # co to znaczy v? staraj sie pisac doslowniej, bo kiedys sie zgubisz w wiekszysc kodzie\n\nclass WarHand(Hand): # tu tak samo jak wyzej\n    \"\"\" Ręka w wojnie. \"\"\"\n    def __init__(self, name):\n        super().__init__() # w python3 nie musisz do super przekazywac klasy \n        self.name = name\n\n    def __str__(self):\n        rep = f\"{self.name}    {super(WarHand, self).__str__()}\"\n        if self.total:\n            rep += f\"(ma {self.total} punktów)\"\n        return rep\n\n    @property\n    def total(self) -&gt; int:\n        t = 0\n        for card in self.cards:\n            t += card.value\n        return t \n\ncard1 = WarCard(\"3\", \"pik\")\nprint(\"karta: \" + str(card1) + \". Jej wartość wynosi: \" + str(card1.value))\n\ncard2 = WarCard(\"2\", \"kier\")\nprint(\"karta: \" + str(card2) + \". Jej wartość wynosi: \" + str(card2.value))\n\nreka = WarHand(\"Tomasz\")\nreka.add(card1)\nprint(reka)\n\nreka = WarHand(\"Jan\")\nreka.add(card2)\nprint(reka)\n\ninput(\"\\n\\nAby zakończyć program, naciśnij klawisz Enter.\")\n\ndalem Ci pare wskazowek plus tutaj WarHand/WarCard jest zbedne i dziedziczenie po Hand/Card mozesz spokojnie w tych klasach te metody z War klas przeniesc</code></pre>" };

            var result = postAnalyzer.Analyze(post);

            Assert.Empty(result);
        }

        [Fact]
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

            var result = postAnalyzer.Analyze(post);

            Assert.Empty(result);
        }

        [Fact]
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

            var result = postAnalyzer.Analyze(post);

            Assert.Single(result);
            Assert.IsType<NotFormattedCodeFound>(result[0]);
        }

        [Fact]
        public void Analyze_PostWithUnformattedCode_UnformattedCodeFound()
        {
            var postAnalyzer = new PostAnalyzer(null);
            Post post = new Post { text = "<p>Witam zrobilem zadanie , z systemu 10 na system 16, ma ktoś jakiś pomysł na lepsze rozwiązanie?<br />\n#include &lt;iostream&gt;</p>\n<p>using namespace std;</p>\n<p>int main()<br />\n{<br />\nint n;<br />\ncin&gt;&gt;n;<br />\nint w;<br />\nint c=0;<br />\nint tab[c];<br />\nwhile(n&gt;0)<br />\n{<br />\nc++;<br />\nw=n%16;<br />\nn=n/16;</p>\n<pre><code>tab[c]=w;</code></pre>\n<p>}<br />\ncout&lt;&lt;\"System szesnastkowy to: \";<br />\nfor(int i=c; i&gt;0; i--)<br />\n{<br />\nif(tab[i]==10)<br />\n{<br />\ncout&lt;&lt;\"A\";<br />\n}<br />\nelse if(tab[i]==11)<br />\n{<br />\ncout&lt;&lt;\"B\";<br />\n}<br />\nelse if(tab[i]==12)<br />\n{<br />\ncout&lt;&lt;\"C\";<br />\n}<br />\nelse if(tab[i]==13)<br />\n{<br />\ncout&lt;&lt;\"D\";<br />\n}<br />\nelse if(tab[i]==14)<br />\n{<br />\ncout&lt;&lt;\"E\";<br />\n}<br />\nelse if(tab[i]==15)<br />\n{<br />\ncout&lt;&lt;\"F\";<br />\n}<br />\nelse<br />\n{<br />\ncout&lt;&lt;tab[i];<br />\n}<br />\n}<br />\n}</p>" };

            var result = postAnalyzer.Analyze(post);

            Assert.Single(result);
            Assert.IsType<NotFormattedCodeFound>(result[0]);
        }
    }
}