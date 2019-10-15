# Eleia

[![Build Status](https://dev.azure.com/ktos/Eleia/_apis/build/status/ktos.Eleia?branchName=master)](https://dev.azure.com/ktos/Eleia/_build/latest?definitionId=5&branchName=master)

Napędzany uczeniem maszynowym bot do detekcji i komentowania kiedy znajdzie
w poście na forum 4programmers.net kod, który nie jest oznaczony jako kod.

## Krótki opis

Eleia śledzi nowe posty na forum 4programmers.net i analizuje każdy z nich
poprzez usunięcie wpierw wszystkich tagów HTML (włączając w to `<code>`),
następnie podzielenie na akapity i klasyfikację każdego z akapitów pomiędzy
"code" i "text".

Jeśli którykolwiek z akapitów został sklasyfikowany jako kod (i pewność
klasyfikacji przekracza określony próg) bot może wysłać komentarz do tego postu,
nakłaniajac autora do poprawy.

Domyślnie, Eleia następnie "pójdzie spać" i po upłynięciu określonego czasu
znów pobierze nowe posty, znów je zanalizuje i tak dalej i dalej. Posty, które
zostały już ocenione nie są analizowana ponownie, ale ta lista jest
przechowywana w pamięci tymczasowej.

## Użycie

Musisz dostarczyć aplikacji konfiguracji, zanim zaczniesz jej używać. Możesz
wykorzystać plik konfiguracyjny `appconfig.json`, zmienne środowiskowe o nazwach
z prefiksem `ELEIA_` lub parametry wywołania. Wszystkie są równoważne.

Na przykład, aby tylko uruchomić aplikację i zobaczyć jak działa na serwerze
testowym, po prostu uruchom:

```batch
eleia
```

Aby uruchomić na serwerze produkcyjnym:

```batch
eleia --useDebug4p false
```

Aby wysyłać komentarze, musisz podać nazwę użytkownika, hasło i opcję `postComments`:

```batch
eleia --username "Jakiś Użytkownik" --password "sekretne hasło" --useDebug4p false --postComments true
```

Dostępne opcje konfiguracyjne:

* `username` -> nazwa użytkownika do zalogowania w Coyote,
* `password` -> hasło użytkownika do zalogowania,
* `useDebug4p` -> używać `dev.4programmers.info` czy `4programmers.net`?,
* `postComments` -> czy powinien wysyłać komentarze?,
* `timeBetweenUpdates` -> jaki jest czas oczekiwania przed pobraniem kolejnej listy postów?,
* `treshold` -> jaki jest próg klasyfikacji po którym zostaje wysłany komentarz (domyślnie: 0,99).

## Uczenie Maszynowe

Został wykorzystany framework ML.NET, oraz AutoML do wygenerowania modelu.

[basictrainingdata.tsv](https://github.com/ktos/Eleia/blob/master/basictrainingdata.tsv)
to plik, na którym model był trenowany.

## Twój wkład

Zachęcam do rozwijania projektu!

Zacznij od stworzenia nowego issue, gdzie będziemy mogli przedyskutować, co
próbujesz osiągnąć. A potem klasycznie: stwórz fork tego repozytorium,
naprawiaj błędy lub dodawaj nowe funkcje, i na końcu wyślij mi pull request.

Każdy PR jest automatycznie budowany i testowany przez Azure Pipelines.
