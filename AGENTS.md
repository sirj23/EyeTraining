# AGENTS.md

## 1. Język komunikacji

- Komunikuj się z użytkownikiem po polsku.
- Opisy zmian, podsumowania, komentarze dotyczące wykonanej pracy i rekomendacje również podawaj po polsku.
- Nazwy klas, metod, pól, interfejsów, enumów, plików kodu i innych elementów technicznych mogą być po angielsku zgodnie z dobrymi praktykami C# i Unity.

## 2. Charakter projektu

Projekt `EyeTraining` jest aplikacją do treningu sprawności wzrokowej przeznaczoną przede wszystkim na duże ekrany.

Docelowe platformy:
- Windows
- macOS
- Google TV
- Apple TV

Technologia:
- Unity 6.5
- C#
- Universal 2D / URP
- projekt 2D z możliwością lekkich elementów 2.5D
- bez SQLite na początkowym etapie

Aplikacja nie jest projektowana jako aplikacja mobilna na telefon lub tablet.

## 3. Główny cel aplikacji

Aplikacja ma wspierać regularny trening:
- śledzenia wzrokiem,
- spostrzegawczości,
- reakcji,
- koordynacji wzrokowej,
- widzenia peryferyjnego w ramach ćwiczeń,
- rozpoznawania symboli i kontrastu,
- koncentracji,
- dobrych nawyków podczas pracy z ekranami.

Aplikacja przygotowuje użytkownikowi gotowe sesje ćwiczeń i stopniowo rozwija ich zawartość oraz poziom trudności.

## 4. Brak obietnic medycznych

To jest twarda zasada projektu.

Aplikacja:
- nie diagnozuje chorób,
- nie zastępuje okulisty, optometrysty ani innego specjalisty,
- nie obiecuje leczenia wad wzroku,
- nie może twierdzić, że poprawia krótkowzroczność, nadwzroczność, astygmatyzm lub inne schorzenia,
- nie może sugerować, że użytkownik może zrezygnować z okularów lub soczewek.

Dozwolone są komunikaty dotyczące wyników w ćwiczeniach, np.:
- poprawa czasu reakcji,
- wyższa skuteczność,
- wyższy poziom ćwiczenia,
- większa regularność.

Niedozwolone są komunikaty typu:
- „Twój wzrok poprawił się o 20%”.
- „Twoja ostrość wzroku jest lepsza”.
- „Wyleczyłeś wadę wzroku”.

## 5. Bezpieczeństwo użytkownika

Bezpieczeństwo ma pierwszeństwo przed:
- progresją,
- rangami,
- punktami doświadczenia,
- nagrodami,
- osiągnięciami,
- wynikami.

System powinien kontrolować:
- maksymalny czas sesji,
- czas pojedynczego ćwiczenia,
- tempo,
- intensywność,
- liczbę bodźców,
- długość przerw,
- tempo zwiększania trudności.

Każde ćwiczenie powinno umożliwiać natychmiastowe:
- zatrzymanie,
- pauzę,
- pominięcie,
- zakończenie sesji.

Jeżeli użytkownik zgłasza dyskomfort, aplikacja nie powinna zwiększać trudności.

Nie stosować mechanizmów, które zmuszają użytkownika do dalszego ćwiczenia mimo dyskomfortu.

## 6. Progresja

Progresja nie oznacza wyłącznie:
- szybszego ruchu,
- dłuższego czasu.

Może obejmować między innymi:
- czas ćwiczenia,
- tempo,
- wielkość obiektu,
- kontrast,
- zakres ruchu,
- złożoność trajektorii,
- liczbę bodźców,
- liczbę możliwych odpowiedzi,
- czas ekspozycji,
- liczbę rozpraszaczy,
- przewidywalność ruchu,
- liczbę śledzonych celów.

Zmiany progresji powinny być:
- niewielkie,
- stopniowe,
- odwracalne.

Jeżeli to możliwe, nie zwiększaj wielu głównych parametrów trudności jednocześnie.

## 7. Codzienna sesja

Użytkownik nie konfiguruje ręcznie treningu.

System przygotowuje gotową sesję.

Typowy przebieg:
1. wybór profilu,
2. wybór sposobu prowadzenia sesji,
3. przygotowanie przed sesją,
4. główne ćwiczenia,
5. przerwy i ćwiczenia odpoczynkowe,
6. opcjonalne zgłoszenie oceny konkretnego ćwiczenia,
7. zakończenie sesji,
8. krótkie podsumowanie,
9. statystyki i nagrody.

Przed każdą sesją użytkownik wybiera:
- lektora,
- albo instrukcje tekstowe.

## 8. Przygotowanie przed sesją

Przed właściwym treningiem może pojawić się krótki zestaw przygotowawczy, np.:
- ustawienie pozycji,
- rozluźnienie barków,
- spokojne ruchy szyi,
- rozluźnienie szczęki i czoła,
- spokojne pełne mruganie,
- krótkie zamknięcie oczu,
- spojrzenie w dal.

Przygotowanie ma być krótkie i różnorodne.

Nie należy stale wydłużać przygotowania bez potrzeby.

## 9. Główne rodziny ćwiczeń

Projekt przewiduje między innymi:

### Ćwiczenia ruchowe
- poziomo,
- pionowo,
- przekątne,
- okrąg,
- półokręgi,
- kwadrat,
- prostokąt,
- zygzaki,
- fale,
- ósemki,
- spirale,
- diament,
- trójkąt,
- wielokąty,
- łączenie tras,
- zmiany kierunku,
- zatrzymania,
- zmienne tempo.

### Spostrzegawczość i reakcja
- reakcja na zmianę koloru,
- reakcja na pojawienie się obiektu,
- wskazanie miejsca bodźca,
- znajdź inny symbol,
- kierunek strzałki,
- Go / No-Go,
- zapamiętaj pozycję,
- śledzenie właściwego obiektu.

### Widzenie peryferyjne
- błyski lewo/prawo,
- 4 kierunki,
- 8 kierunków,
- kolor,
- kształt,
- ruch na obrzeżach,
- bodźce wśród rozpraszaczy,
- centralny punkt + peryferyjny bodziec.

### Landolt C
- 4 kierunki,
- 8 kierunków,
- zmiana wielkości,
- zmiana kontrastu,
- czas ekspozycji,
- losowe pozycje,
- obrót,
- wyszukiwanie innego symbolu.

### Ćwiczenia odpoczynkowe
- spokojne mruganie,
- zamknięcie oczu,
- spojrzenie w dal,
- blisko–daleko,
- rozluźnienie wzroku,
- mikroprzerwy,
- spokojny reset po bardziej wymagających ćwiczeniach.

### Ćwiczenia bardziej „growe”
- Trzy kubki,
- Meteoryty,
- Orbitujące elektrony.

Nie zakładaj, że wszystkie ćwiczenia muszą znaleźć się w MVP.

## 10. Trzy kubki

Ćwiczenie polega na:
- pokazaniu kulki,
- ukryciu jej pod kubkiem,
- przemieszczaniu kubków,
- wskazaniu końcowego położenia.

Progresja może obejmować:
- 3, 4, 5, 6 i więcej kubków,
- większą liczbę zamian,
- większą prędkość,
- wiele rzędów i kolumn,
- ruch pionowy i poziomy,
- później wiele śledzonych kulek.

## 11. Meteoryty

Ćwiczenie polega na śledzeniu wybranej kuli wśród ruchomych rozpraszaczy.

Progresja może obejmować:
- większą liczbę meteorytów,
- większe podobieństwo kolorów,
- bardziej zbliżone rozmiary,
- bardziej złożone trajektorie,
- więcej przecięć torów,
- więcej niż jeden śledzony cel.

Po zatrzymaniu użytkownik wskazuje właściwy obiekt.

## 12. Orbitujące elektrony

Ćwiczenie polega na śledzeniu wybranego elektronu poruszającego się:
- wokół jąder,
- pomiędzy jądrami,
- wśród innych elektronów.

Progresja może obejmować:
- większą liczbę jąder,
- większą liczbę elektronów,
- większe podobieństwo rozpraszaczy,
- większą liczbę przejść,
- bardziej złożone orbity,
- śledzenie wielu elektronów jednocześnie.

## 13. Ocena ćwiczenia

Nie wyświetlaj obowiązkowej ankiety po każdym ćwiczeniu.

Po zakończeniu ćwiczenia użytkownik powinien mieć opcjonalny przycisk:
- „Oceń ćwiczenie”.

Przy pierwszym użyciu należy wyjaśnić, że funkcja służy do zgłaszania:
- zbyt szybkiego tempa,
- zbyt wolnego tempa,
- zbyt dużej trudności,
- problemów z widocznością lub kontrastem,
- dyskomfortu.

Brak oceny nie oznacza automatycznie, że można agresywnie zwiększać trudność.

Podczas ćwiczenia powinna istnieć również możliwość natychmiastowego przerwania z powodu dyskomfortu.

## 14. Sesje powrotne

Krótka nieobecność nie powinna powodować cofania poziomu.

Po dłuższej przerwie aplikacja może zaproponować:
- normalną sesję według dotychczasowego planu,
- spokojniejszą sesję powrotną.

Spokojniejsza sesja nie może usuwać trwałego poziomu użytkownika.

Po długiej przerwie powrót do pełnych parametrów może odbywać się przez kilka sesji.

## 15. Profile użytkowników

Na start:
- jeden profil = jeden użytkownik,
- brak rankingów rodzinnych,
- brak turniejów,
- brak porównywania wyników użytkowników.

Jeden profil przechowuje własne:
- postępy,
- poziomy,
- historię,
- rangę,
- XP,
- odznaki,
- nagrody,
- ustawienia,
- informacje z „Oceń ćwiczenie”.

Przy tworzeniu profilu użytkownik wybiera kategorię:
- dziecko,
- nastolatek,
- dorosły,
- senior.

Kategoria wpływa na:
- startową intensywność,
- tempo progresji,
- długość serii,
- wielkość elementów,
- sposób instrukcji.

Kategoria jest tylko punktem startowym.

Później system powinien reagować na rzeczywiste wyniki i feedback użytkownika.

## 16. Rangi, XP i nagrody

System motywacyjny ma nagradzać przede wszystkim:
- regularność,
- ukończone sesje,
- poznawanie nowych ćwiczeń,
- realizację planu,
- powrót po przerwie.

Ranga nie oznacza jakości wzroku.

System rang ma być inspirowany wizualnie stopniami wojskowymi:
- belki,
- oznaczenia,
- później inne symbole,
- czytelna gradacja.

Aktualny stopień powinien być widoczny od razu po uruchomieniu aplikacji.

Ranga nie spada z powodu przerwy.

## 17. Statystyki

Statystyki mają być minimalistyczne.

Po sesji:
- pokazuj przede wszystkim wyniki ćwiczeń wykonanych w tej sesji.

Na głównym ekranie statystyk:
- regularność,
- liczba sesji,
- aktualna ranga,
- najważniejsze trendy i rozwój.

Nie zasypuj użytkownika pełną telemetrią.

System może przechowywać więcej danych technicznych niż pokazuje użytkownikowi.

Nie twórz jednego wskaźnika typu:
- „Twój wzrok: 82%”.

Pokazuj realnie zmierzone wyniki ćwiczeń.

## 18. Sterowanie

Główne metody wejścia:
- Windows/macOS: mysz,
- Google TV/Apple TV: pilot.

Klawiatura ma być używana minimalnie i pomocniczo.

Projektuj ćwiczenia najpierw z myślą o:
- wskazaniu obiektu,
- wyborze odpowiedzi,
- prostym nawigowaniu.

Nie projektuj podstawowych ćwiczeń w oparciu o konieczność zapamiętywania wielu klawiszy.

## 19. Duży ekran

Podstawowym środowiskiem aplikacji są:
- monitory komputerowe,
- laptopy,
- telewizory.

Telefon i tablet nie są podstawową platformą treningową.

Telefon może ewentualnie później pełnić rolę pomocniczą, np.:
- kontrolera,
- drugiego punktu do ćwiczeń blisko–daleko.

## 20. Prowadzenie głosowe

Prowadzenie głosowe jest funkcją startową projektu.

Przed każdą sesją użytkownik wybiera:
- lektor,
- tekst.

Lektor:
- wyjaśnia ćwiczenie,
- prowadzi przygotowanie,
- prowadzi odpoczynek,
- zapowiada ważne zmiany,
- podczas właściwego pomiaru lub zadania zwykle milczy.

Nie może zdradzać poprawnych odpowiedzi.

Pierwsza wersja powinna być przygotowana pod systemowy TTS na poszczególnych platformach.

## 21. Personalizacja

Personalizacja ma niski priorytet.

Na początku tylko wybrane funkcje, jeśli są potrzebne do komfortu i dostępności.

Nie pozwalaj użytkownikowi ręcznie zmieniać:
- czasu ćwiczeń,
- prędkości wynikającej z planu,
- poziomu trudności,
- kolejności ćwiczeń,
- progresji.

Użytkownik personalizuje doświadczenie.
System personalizuje trening.

## 22. Rozwój zawartości

Nie pokazuj całej biblioteki od pierwszej sesji.

Nowe:
- ćwiczenia,
- warianty,
- poziomy,
- mini-gry,
- nagrody,
- statystyki

mogą być odblokowywane stopniowo.

Biblioteka może rosnąć, ale długość sesji nie powinna rosnąć bez końca.

## 23. Dane

Na początku:
- bez SQLite,
- bez backendu,
- bez kont online.

Dane mają być przechowywane lokalnie.

Architektura zapisu powinna być tak zaprojektowana, aby późniejsza migracja do SQLite była możliwa bez przebudowy całego projektu.

Nie zapisuj danych bezpośrednio z wielu miejsc w kodzie.

Użyj jednej warstwy odpowiedzialnej za zapis i odczyt.

## 24. Architektura kodu

Preferuj:
- małe klasy,
- pojedynczą odpowiedzialność,
- zależności przez interfejsy,
- czytelne serwisy,
- separację logiki od warstwy UI,
- separację logiki ćwiczeń od mechaniki sesji,
- konfigurację poziomów przez dane zamiast wartości wpisanych na sztywno.

Docelowo warto rozdzielić co najmniej:
- zarządzanie sesją,
- progresję,
- ćwiczenia,
- profile,
- zapis danych,
- input,
- audio/TTS,
- statystyki,
- nagrody,
- UI.

Nie twórz dużych klas typu `GameManager`, które zarządzają całą aplikacją.

## 25. Unity

Projekt używa:
- Unity 6.5,
- C#,
- Universal 2D / URP.

Używaj aktualnych API Unity 6.5.

Nie wprowadzaj starszych lub deprecated rozwiązań, jeśli Unity 6.5 ma zalecany zamiennik.

Nie instaluj nowych pakietów bez uzasadnienia.

Przed dodaniem zależności:
- sprawdź, czy problem można rozwiązać standardowymi możliwościami Unity,
- wyjaśnij po co pakiet jest potrzebny.

## 26. Struktura projektu

Twórz spójną strukturę katalogów.

Preferowany kierunek:

`Assets/EyeTraining/`

a wewnątrz między innymi:
- `Art`
- `Audio`
- `Data`
- `Prefabs`
- `Scenes`
- `Scripts`
- `Settings`
- `UI`

W `Scripts` można później wydzielać:
- `Core`
- `Exercises`
- `Input`
- `Profiles`
- `Progression`
- `Save`
- `Statistics`
- `Audio`
- `UI`

Nie twórz dużej liczby pustych katalogów bez realnej potrzeby.

## 27. Testowanie

Po zmianach:
- sprawdź błędy kompilacji,
- sprawdź Unity Console,
- wykonaj dostępne testy,
- sprawdź `git diff --check`,
- sprawdź `git status`.

Jeżeli nie można wykonać testu automatycznie, napisz jasno, co użytkownik powinien sprawdzić ręcznie w Unity.

Nie deklaruj, że coś działa, jeśli nie zostało sprawdzone.

## 28. Git

Repozytorium używa Git.

Nie commituj:
- `Library`
- `Temp`
- `Logs`
- lokalnych ustawień edytora
- innych artefaktów generowanych automatycznie przez Unity.

Możesz tworzyć commity, jeśli:
- zmiana tworzy logicznie zamknięty etap,
- projekt kompiluje się,
- sprawdziłeś zmiany,
- repozytorium nie zawiera przypadkowych artefaktów.

Commity powinny być małe i logiczne.

Preferowane przykłady:
- `chore: skonfigurowano projekt Unity`
- `feat: dodano bazę systemu sesji`
- `feat: dodano pierwsze ćwiczenie śledzenia`
- `fix: poprawiono nawigację pilotem`

Po commicie zawsze pokaż:
- hash,
- opis,
- wynik `git status`.

Nie wykonuj `push`, jeśli użytkownik nie poprosi o to wyraźnie.

## 29. Zasady pracy

Przed większą zmianą:
1. przeanalizuj istniejący kod,
2. sprawdź powiązane pliki,
3. zaproponuj najprostsze rozwiązanie zgodne z architekturą,
4. wykonaj zmianę,
5. sprawdź rezultat.

Nie zmieniaj zaakceptowanych zasad produktu samodzielnie.

Jeżeli zadanie wymaga decyzji produktowej, której ten plik nie rozstrzyga:
- zatrzymaj się,
- opisz problem,
- zapytaj użytkownika.

Nie dodawaj funkcji „przy okazji”, jeśli nie są potrzebne do bieżącego zadania.

## 30. Priorytety projektu

Kolejność priorytetów:

1. bezpieczeństwo użytkownika,
2. poprawność działania,
3. czytelność i prostota UX,
4. płynność działania,
5. łatwa rozbudowa,
6. estetyka,
7. dodatkowe efekty i personalizacja.

Nie poświęcaj bezpieczeństwa ani czytelności dla bardziej efektownej animacji.

## 31. Aktualny etap projektu

Projekt jest na bardzo wczesnym etapie konfiguracji.

Nie implementuj jeszcze całej aplikacji bez wyraźnego polecenia.

Pierwsze działania powinny koncentrować się na:
- poprawnym `.gitignore`,
- bazowej strukturze projektu,
- podstawowej architekturze,
- konfiguracji Unity,
- przygotowaniu fundamentów pod kolejne moduły.

Przed rozpoczęciem większego modułu zawsze upewnij się, że użytkownik chce przejść do jego implementacji.