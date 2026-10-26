namespace PolygonEditor.UI;

// Treść okna pomocy (F1). Format: "## " nagłówek, "- " punkt listy, blok między ``` - czcionka stałej szerokości.
internal static class HelpContent
{
    public const string Usage = """
        ## Mysz
        - LPM na wierzchołku + przeciąganie: przesuwanie wierzchołka. Wierzchołek zawsze podąża za kursorem, a ograniczenia i ciągłość są pilnowane na bieżąco. Gdy ograniczeń nie da się spełnić, przesuwa się cały wielokąt.
        - LPM na punkcie kontrolnym (pomarańczowy kwadrat) + przeciąganie: przesuwanie punktu kontrolnego Béziera. Wierzchołki tego segmentu stoją w miejscu; ciągłość G1/C1 może przesunąć uchwyt po drugiej stronie wierzchołka.
        - LPM we wnętrzu wielokąta + przeciąganie: przesuwanie całego wielokąta.
        - PPM na krawędzi: menu krawędzi.
        - PPM na wierzchołku: menu wierzchołka.
        - Kursor: rączka nad wierzchołkiem lub punktem kontrolnym, krzyżyk nad krawędzią, strzałki nad wnętrzem.

        ## Klawiatura
        - F1: ta pomoc.
        - Ctrl+R: przywrócenie sceny początkowej.
        - Esc: anulowanie wyboru drugiej krawędzi (dodawanie / usuwanie równoległości).

        ## Menu krawędzi (PPM na krawędzi)
        - Add vertex: dodaje wierzchołek w środku krawędzi.
        - Add/update length constraint: okno z długością; wartość domyślna to aktualna długość krawędzi (kropka i przecinek dziesiętny są akceptowane). Tylko dla odcinka.
        - Add parallel constraint: następnie kliknij LPM drugą krawędź (może należeć do innego wielokąta). Esc, PPM albo klik obok krawędzi anuluje. Tylko dla odcinków, które nie mają wspólnego wierzchołka.
        - Remove length constraint: usuwa ograniczenie długości.
        - Remove parallel constraint: gdy krawędź ma jedną parę, usuwa ją od razu; w przeciwnym razie kliknij drugą krawędź pary.
        - Line segment / Bézier curve / Circular arc: rodzaj krawędzi (zaznaczony jest aktualny).

        ## Menu wierzchołka (PPM na wierzchołku)
        - Remove vertex: usuwa wierzchołek (w wielokącie zostają co najmniej 3).
        - Continuity G0 / G1 / C1: wymagana ciągłość w wierzchołku. Aktywne tylko, gdy sąsiednia krawędź jest krzywą Béziera lub łukiem; C1 tylko obok Béziera i nigdy obok łuku.

        ## Panel „Line drawing”
        - Bresenham (custom): własna implementacja algorytmu Bresenhama.
        - Library (Graphics): Graphics.DrawLine.
        - Wybór dotyczy też krzywych Béziera i łuków - są rysowane jako łamane wybranym algorytmem.

        ## Oznaczenia na scenie
        - Niebieska liczba przy krawędzi: zadana długość.
        - Dwie czerwone kreski + numer: równoległość; ten sam numer stoi przy obu krawędziach pary.
        - Zielone G0 / G1 / C1 przy wierzchołku: ciągłość (pokazywana tylko obok krzywej lub łuku).
        - Linia przerywana: wielobok Béziera (P0-P1-P2-P3) albo cięciwa łuku.
        - Pomarańczowe kwadraty: punkty kontrolne Béziera.
        """;

    public const string Assumptions = """
        ## Wariant
        - Nazwisko z grupy A-K: ograniczenie dwóch krawędzi to równoległość, a domyślna ciągłość po zamianie krawędzi na Béziera to G1.

        ## Ograniczenia
        - Ograniczenia długości i równoległości dotyczą wyłącznie odcinków. Zmiana rodzaju krawędzi usuwa jej ograniczenia, a na krzywej lub łuku nie da się ich dodać.
        - Równoległość krawędzi sąsiednich (ze wspólnym wierzchołkiem) jest zabroniona - leżałyby na jednej prostej. Krawędź nie może być równoległa do samej siebie.
        - Jedna krawędź może mieć długość i dowolną liczbę równoległości. Ponowne ustawienie długości zastępuje poprzednią.
        - Dodanie wierzchołka dzieli krawędź na dwa odcinki (także gdy była krzywą lub łukiem), a usunięcie wierzchołka łączy jego krawędzie w jeden odcinek. Ograniczenia tych krawędzi są usuwane, a domyślna ciągłość sąsiednich wierzchołków przeliczana.
        - Gdy ograniczeń nie da się spełnić:
          - przy przeciąganiu wierzchołka przesuwany jest cały wielokąt (translacja zachowuje wszystkie ograniczenia), więc wierzchołek nadal podąża za kursorem;
          - przeciąganie punktu kontrolnego, dodanie ograniczenia, zmiana ciągłości i zmiana rodzaju krawędzi są odrzucane - scena wraca do stanu sprzed operacji.

        ## Ciągłość
        - Ciągłość jest cechą wierzchołka i ma znaczenie tylko wtedy, gdy sąsiednia krawędź jest krzywą Béziera lub łukiem. G0 jest zawsze spełnione (krawędzie mają wspólny wierzchołek).
        - Domyślna ciągłość: G1 obok Béziera, G0 obok łuku (polecenie określa wartość domyślną tylko dla Béziera, więc łuk startuje jako półokrąg). Ciągłość wybrana przez użytkownika nie jest nadpisywana przy zmianie rodzaju krawędzi.
        - Przeciąganie punktu kontrolnego nie przesuwa wierzchołków jego segmentu. Wyjątek dopuszczony przez polecenie: gdy po drugiej stronie wierzchołka z G1/C1 jest odcinek, obraca się on (a przy C1 także zmienia długość) wokół wspólnego wierzchołka, co może przenieść się dalej przez ograniczenia.

        ## Łuki
        - Dozwolone G0 i G1; G1 tylko na jednym końcu łuku - ustawienie G1 na jednym końcu przełącza drugi koniec na G0. C1 sąsiadujące z nowym łukiem zamienia się na G1.
        - G0 na obu końcach: półokrąg o promieniu równym połowie długości krawędzi.
        - G1 na jednym końcu: łuk jest styczny do sąsiedniej krawędzi. Gdy styczna jest równoległa do cięciwy (łuk byłby odcinkiem), łuk pozostaje półokręgiem.
        - Gdy dwa łuki mają G1 we wspólnym wierzchołku, każdy bierze styczną z półokręgu drugiego (przerwanie wzajemnej zależności).

        ## Pozostałe
        - Nowa krzywa Béziera i nowy łuk wyginają się na zewnątrz wielokąta. Strona „na zewnątrz” wynika ze znaku pola wielokąta - dla wielokątów z samoprzecięciami to przybliżenie.
        - Samoprzecięcia są dozwolone; wnętrze wielokąta (do przeciągania całości) wyznacza reguła even-odd.
        - Wielokąt ma co najmniej 3 wierzchołki. Wielokąty są tworzone w kodzie sceny początkowej (DemoSceneFactory) - polecenie nie wymaga tworzenia ich myszą.
        """;

    public const string DataModel = """
        ## Struktura
        ```
        Scene
        ├── Polygons : List<Polygon>
        └── Constraints : ConstraintStore     ograniczenia użytkownika (długość, równoległość)

        Polygon
        ├── Vertices : List<Vertex>
        └── Edges    : List<Edge>             Edges[i] biegnie od Vertices[i] do Vertices[(i+1) mod n]

        Vertex       Position, Continuity (G0/G1/C1), ContinuityUserSet, Incoming, Outgoing
        Edge         Start, End, Kind (Line / CubicBezier / CircularArc),
                     Control1, Control2 (Bézier), ArcSide (strona wygięcia półokręgu)
        ControlPoint Edge, Owner (wierzchołek), Offset;  Position = Owner.Position + Offset
        ArcGeometry  Center, Radius, StartAngle, SweepAngle  (liczona na bieżąco, nie przechowywana)
        ```

        ## Najważniejsze decyzje
        - Topologia jest dwukierunkowa: wierzchołek zna krawędź wchodzącą i wychodzącą, krawędź zna swoje końce. Dzięki temu ograniczenia i ciągłość sięgają do sąsiadów bez szukania w listach.
        - Punkt kontrolny jest obiektem (ma tożsamość - może być trafiony myszą, przeciągany i być kotwicą solvera) i pamięta przesunięcie względem swojego wierzchołka (Control1 należy do Start, Control2 do End). Każdy ruch wierzchołka, także wykonany przez solver, niesie jego punkty kontrolne, więc kształt krzywej przy wierzchołku się nie zmienia.
        - Łuk nie ma punktów kontrolnych - jego geometria jest liczona z końców, ciągłości i stycznej sąsiada przy każdym użyciu.
        - Operacje zmieniające topologię idą przez Scene: SetEdgeKind, InsertVertex, RemoveVertex, SetContinuity. Tam w jednym miejscu są reguły usuwania ograniczeń i ciągłości domyślnej.
        - ConstraintStore przechowuje tylko ograniczenia zdefiniowane przez użytkownika. Ograniczenia ciągłości są generowane przez solver z ustawień wierzchołków przy każdym rozwiązywaniu - stan ciągłości jest w jednym miejscu (Vertex) i nie wymaga synchronizacji.
        - SceneSnapshot zapamiętuje położenia wierzchołków, przesunięcia punktów kontrolnych, ciągłość i listę ograniczeń - pozwala wycofać operację, której nie da się pogodzić z ograniczeniami.

        ## Warstwy (foldery)
        - Models: model danych (bez zależności od interfejsu).
        - Constraints: ograniczenia, kontekst rozwiązywania (rangi) i solver.
        - Drawing: Renderer, Bresenham, algorytmy przyrostowe Béziera i łuku.
        - Interaction: wykrywanie trafień myszą.
        - Scene: scena początkowa (DemoSceneFactory) - nowy wielokąt to jedno wywołanie AddPolygon + nazwane krawędzie.
        - UI: okno główne, menu kontekstowe, ta pomoc.
        """;

    public const string Constraints = """
        ## Interfejs ograniczenia
        ```
        interface IConstraint
            void Satisfy(SolveContext context)   jeden krok korekty
            bool IsSatisfied()                   sprawdzenie z tolerancją
        ```
        - LengthConstraint: |długość − zadana| ≤ 0.5 px.
        - ParallelConstraint: |sin kąta między krawędziami| ≤ 0.005 (ok. 0.3°).
        - ContinuityConstraint: G1 - |sin| ≤ 0.005 i zgodny zwrot; C1 - położenie uchwytu z dokładnością 0.5 px.

        ## Solver (relaksacja)
        1. Lista ograniczeń = ograniczenia użytkownika + ciągłość G1/C1 wygenerowana dla wierzchołków sąsiadujących z Bézierem.
        2. Jeśli wszystkie są spełnione - koniec, scena się nie zmienia.
        3. Wyznaczenie rang wierzchołków względem kotwicy (niżej).
        4. Do 100 iteracji: każde ograniczenie wykonuje Satisfy; po każdej iteracji sprawdzane jest, czy wszystkie są spełnione.
        5. Wynik true/false. Przy false wywołujący przywraca zapamiętany SceneSnapshot.

        ## Kotwica i rangi
        - Kotwica to element, który ma zostać w miejscu: przeciągany wierzchołek; przy przeciąganiu punktu kontrolnego - oba wierzchołki jego segmentu; przy dodawaniu ograniczenia lub zmianie rodzaju krawędzi - wskazana krawędź.
        - Ranga wierzchołka = odległość (przeszukiwanie wszerz, BFS) od kotwicy w grafie, którego krawędzie to boki wielokątów i powiązania przez ograniczenia dwóch krawędzi (np. równoległość między wielokątami). Kotwica ma rangę 0 i nigdy się nie przesuwa.
        - Zasada: ograniczenie przesuwa wierzchołek o większej randze. Dzięki temu korekty rozchodzą się od przeciąganego wierzchołka na zewnątrz, w obie strony obwodu, zamiast się „przeciągać”. Przy równych rangach oba końce przesuwają się symetrycznie względem środka krawędzi.

        ## Długość
        - Koniec o większej randze przesuwa się wzdłuż kierunku krawędzi tak, by jej długość była równa zadanej.

        ## Równoległość
        - Nieruchoma zostaje krawędź bliższa kotwicy (mniejsza ranga, potem suma rang). Druga obraca się do kierunku pierwszej, zachowując swoją długość i zwrot - wokół końca o mniejszej randze albo wokół środka.

        ## Ciągłość G1 / C1 w wierzchołku V
        - Uchwyt to punkt wyznaczający styczną przy V: punkt kontrolny Béziera (Control2 krawędzi wchodzącej, Control1 wychodzącej) albo drugi koniec sąsiedniego odcinka.
        - G1: uchwyt wchodzący, V i uchwyt wychodzący leżą na jednej prostej, V pomiędzy. Przesuwany uchwyt zachowuje swoją odległość od V.
        - C1: równe wektory styczne. Pochodna Béziera w końcu to 3·(P1 − P0), a odcinka (End − Start), więc Out = V + (V − In)·s_in / s_out, gdzie s = 3 dla Béziera i 1 dla odcinka. Obok odcinka uchwyt Béziera ma zatem 1/3 długości odcinka.
        - Przesuwany jest uchwyt, który nie jest kotwicą; gdy oba są wolne - punkt kontrolny (jego ruch nie psuje innych ograniczeń). Koniec odcinka przesuwa się tylko wtedy, gdy uchwyt po drugiej stronie jest unieruchomiony (np. przeciągany punkt kontrolny).

        ## Łuk a ciągłość
        - Łuk nie potrzebuje ograniczenia w solverze: jego geometria jest liczona przy każdym użyciu ze stycznej sąsiada, więc zawsze jest z nim zgodna.
        - G1 na początku: środek C leży na prostej przez początek P prostopadłej do stycznej t (normalna n) i jest równo odległy od obu końców: C = P + λ·n, λ = |d|² / (2·n·d), d = koniec − P. Kierunek obiegu dobierany tak, by łuk wychodził z P zgodnie z t.
        - G1 na końcu: ten sam wzór dla łuku liczonego od końca ze styczną −t, potem odwrócony.

        ## Wycofywanie operacji
        - Przeciąganie wierzchołka: kotwica = wierzchołek; przy porażce przywrócenie stanu i translacja całego wielokąta o ruch kursora.
        - Przeciąganie punktu kontrolnego: kotwica = punkt; przy porażce przywrócenie stanu (punkt zostaje).
        - Dodanie długości: kotwica = początek krawędzi; dodanie równoległości: kotwica = pierwsza wybrana krawędź. Przy porażce ograniczenie nie zostaje dodane.
        - Zmiana ciągłości: kotwica = wierzchołek; zmiana rodzaju krawędzi: kotwica = krawędź. Przy porażce poprzedni stan wraca.
        - Przeciąganie całego wielokąta nie wywołuje solvera - translacja nie narusza żadnego ograniczenia.
        """;

    public const string Drawing = """
        ## Odcinek - algorytm Bresenhama
        - Wersja całkowitoliczbowa dla wszystkich oktantów: błąd err = dx − dy; w każdym kroku e2 = 2·err; jeśli e2 > −dy, to x += sx i err −= dy; jeśli e2 < dx, to y += sy i err += dx. Piksele są stawiane bezpośrednio w bitmapie.
        - Alternatywa (radio button): Graphics.DrawLine.

        ## Krzywa Béziera - algorytm przyrostowy
        - Przejście do bazy potęgowej B(t) = a·t³ + b·t² + c·t + d:
        ```
        a = −P0 + 3P1 − 3P2 + P3
        b = 3P0 − 6P1 + 3P2
        c = 3(P1 − P0)
        d = P0
        ```
        - Różnice przednie dla kroku h = 1/n:
        ```
        Δ1 = a·h³ + b·h² + c·h
        Δ2 = 6a·h³ + 2b·h²
        Δ3 = 6a·h³
        ```
        - Pętla bez mnożeń: P += Δ1; Δ1 += Δ2; Δ2 += Δ3. Ostatni punkt jest ustawiany dokładnie w P3.
        - Liczba kroków n = długość wieloboku kontrolnego / 4 px (od 8 do 1000) - wielobok ogranicza z góry długość krzywej, więc kolejne punkty są najwyżej ~4 px od siebie.
        - Kolejne punkty łączone są wybranym algorytmem odcinka (Bresenham albo biblioteka).

        ## Łuk okręgu - obrót przyrostowy
        - Wektor promienia (rx, ry) jest obracany o stały kąt Δ = kąt łuku / n; cos Δ i sin Δ liczone są raz, w pętli: (rx, ry) ← (rx·cosΔ − ry·sinΔ, rx·sinΔ + ry·cosΔ).
        - n = długość łuku / 4 px (od 8 do 1000); ostatni punkt dokładnie w końcu krawędzi; punkty łączone wybranym algorytmem odcinka.

        ## Wykrywanie trafień
        - Kolejność: wierzchołek (8 px) > punkt kontrolny (7 px) > krawędź (10 px) > wnętrze.
        - Krawędź jest sprawdzana jako ta sama łamana, która jest rysowana (odcinek, Bézier lub łuk), więc trafia się dokładnie w to, co widać.
        - Wnętrze: reguła even-odd na obwodzie złożonym z łamanych wszystkich krawędzi (uwzględnia krzywe i łuki, dopuszcza samoprzecięcia).

        ## Kolejność rysowania
        - Krawędzie → wieloboki Béziera i cięciwy łuków → wierzchołki → etykiety ciągłości → ikony ograniczeń. Scena jest rysowana na bitmapie, która trafia na ekran w całości (bez migotania).
        """;
}
