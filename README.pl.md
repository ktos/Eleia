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

## Użycie (CLI)

Począwszy od wersji 0.6, Eleia obsługuje zestaw parametrów wiersza polecenia,
gdzie każdy jest w formacie `--parametr <wartość>` lub skróconej `-p <wartość>`.

* `-u` lub `--username` -> nazwa użytkownika do zalogowania do systemu Coyote.
Domyślnie jest pusta i może być pusta tylko jeśli `-c` nie jest `true`.
* `-p` lub `--password` -> hasło do zalogowania do systemu Coyote,
* `-t` lub `--timeBetweenUpdates` -> czas (w minutach) do odczekania przed
pobraniem kolejnego zestawu postów do oceny, o ile `-s` nie zostało podane,
ani `-r` nie jest `true`. Jeśli zostanie podany 0, zachowuje się tak samo, jak
przy `-r true`, czyli uruchamia się i kończy działanie po wykonaniu jednej
analizy. Domyślnie 60 (minut).
* `-n` lub `--nagMessage` -> wiadomość, która zostanie umieszczona w komentarzu
do posta, który został sklasyfikowany jako zawierający błędnie formatowany kod.
Można użyć `{0}` w miejsce którego będzie wstawione prawdopodobieństwo wyliczone
przez algorytm.
* `-d` lub `--useDebug4p` -> `true` lub `false`. Jeśli ustawione `true`,
będzie wykorzystany `dev.4programmers.info`' zamiast `4programmers.net`.
Domyślnie `true`.
* `-c` lub `--postComments` -> `true` lub `false`. Jeśli ustawiono `true`, to po
znalezieniu problemu z postem zostanie do niego dodany komentarz o treści jak
ustawiono w `-n`. Domyślnie `false`.
* `-r` lub `--runOnce` -> `true` lub `false`. Jeśli ustawiono `true` to
aplikacja wykona analizę tylko raz, na raz pobranym zestawie nowych postów.
Dmyślnie `false`.
* `-s` lub `--runOnSet` -> uruchamia pojedynczą analizę, ale tylko na postach,
których id zostały przekazane w wartości polecenia (rozdzielone przecinkami).
Na przykład `-s 1,2` przeanalizuje posty o id 1 oraz 2. Implikuje `-r`.
* `--blacklist` -> oddzielana przecinkami lista identyfikatorów subforów, z
których posty będą ignorowane,
* `--ignoreAlreadyAnalyzed` -> ignoruje bazę danych już zanalizowanych postów,
nie tworzy jej przy wyjściu, nie ładuje przy uruchomieniu,
* `--help` -> wyświetla ekran pomocy i kończy działanie,
* `--version` -> wyświetla informacje o wersji i kończy działanie.

### Przykłady

```bash
# uruchomi się raz, pobierze nowe posty, przeanalizuje je i skomentuje - na
# serwerze testowym
eleia --u someuser -p someSECRETpassw0rd --useDebug4p true --postComments true --runOnce true

# będzie ciągle analiziwać posty, czekając 15 minut przed pobraniem nowych do
# analizy, nie będzie komentować, używa serwera produkcyjnego
eleia -d false -c false -t 15

# przeanalizuje posty o id 221 oraz 14122
eleia --runOnSet 221,14122
```

## Konfiguracja z wykorzystaniem pliku konfiguracyjnego i zmiennych środowiskowych

Poza parametrami dostarczanymi przez wiersz polecenia, można skonfigurować
aplikację z wykorzystaniem pliku konfiguracyjnego lub zmiennych środowiskowych
aby ustawić np. używaną nazwę użytkownika czy hasło. Te opcje zostały głównie
pomyślane do wykorzystania w kontenerach.

**Uwaga:** Konfiguracja ma priorytet ponad opcjami wiersza polecenia. Przykładowo,
ustawiając nazwę użytkownika w opcji `-u` oraz w pliku konfiguracyjnym, zostanie
użyta wartość z pliku.

Dostępne opcje konfiguracyjne:

* `username` -> (string) nazwa użytkownika do logowania,
* `password` -> (string) hasło do logowania,
* `useDebug4p` -> (bool) czy używać `dev.4programmers.info` czy `4programmers.net`?,
* `postComments` -> (bool) czy wysyłać komentarze?,
* `timeBetweenUpdates` -> (int) jaki jest czas przerwy pomiędzy pobraniem kolejnych
postów (w minutach)?,
* `threshold` -> (float) jaki jest próg klasyfikacji, po którym post zostanie
oceniony jako zawierający problem? (domyślnie: 0.99),
* `nagMessage` -> (string) treść wiadomości która będzie wysłana w komentarzu
do posta zawierającego problem (`{0}` zostanie zamienione na prawdopodobieństwo).
* `blacklist` -> (string) oddzielana przecinkami lista identyfikatorów subforów
z których posty będą ignorowane.

Wszystkie te opcje mogą być ustawione w pliku `appsettings.json` w katalogu
roboczym. Poza tym, możliwe jest ustawienie poziomu logowania informacji, poprzez `Logging:LogLevel:Default` (dostępne wartości: `Debug`, `Information`, `Warning`
oraz `Error`).

Przykładowy plik konfiguracyjny:

```json
{
  "username": "jakiś użytkownik",
  "password": "bardzoTAJNEhasl0",
  "useDebug4p": true,
  "postComments": false,
  "timeBetweenUpdates": 30,
  "threshold": 0.95,
  "nagMessage": "Hej! Coś jest nie tak z twoim postem!",
  "Logging": {
    "IncludeScopes": false,
    "LogLevel": {
      "Default": "Debug"
    }
  }
}
```

Jeśli wolisz użyć zmiennych środowiskowych, muszą być poprzedzone prefiksem
`ELEIA_`, na przykład:

```bash
export ELEIA_username=jakisuzytkownik
export ELEIA_useDebug4p=false

# jeśli chcesz ustawić poziom logów, musisz użyć __ jako separatora sekcji
export ELEIA_Logging__LogLevel__Default=Error
```

## Uczenie Maszynowe

Został wykonany ML.NET do wykorzystania modelu binarnej klasyfikacji, natomiast
AutoML (tj. `mlnet auto-train`) zostało wykorzystane do wygenerowania modelu.

[trainingdata2.tsv](https://github.com/ktos/Eleia/blob/master/trainingdata2.tsv)
to plik, na którym model był trenowany.

[trainingdata2.log](https://github.com/ktos/Eleia/blob/master/trainingdata2.log)
to log z autotreningu, SdcaLogisticRegressionBinary z dokładnością 0,9585 został
ostatecznie wykorzystany.

Uruchamiałem też dłuższe sesje trenujące niż 180 sekund, ale nadal ten algorytm
był uznawany za najlepszy, dłuższy czas treningu nie zmieniał nic.

## Twój wkład

Zachęcam do rozwijania projektu!

Zacznij od stworzenia nowego issue, gdzie będziemy mogli przedyskutować, co
próbujesz osiągnąć. A potem klasycznie: stwórz fork tego repozytorium,
naprawiaj błędy lub dodawaj nowe funkcje, i na końcu wyślij mi pull request.

Każdy PR jest automatycznie budowany i testowany przez Azure Pipelines.

Wersje końcowe są budowane przez Azure Pipelines przy każdym nowym tagu,
używam SemVer do określenia numeru wersji wydania.
