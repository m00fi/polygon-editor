# Polygon Editor
Edytor wielokątów i wielokątów krzywoliniowych z ograniczeniami geometrycznymi.

## Uruchomienie

Wymagania: Windows i .NET 10 SDK.

```bash
dotnet run --project PolygonEditor
```

## Funkcje

- Wiele wielokątów na scenie, dozwolone samoprzecięcia.
- Przesuwanie wierzchołków, punktów kontrolnych i całych wielokątów; usuwanie wierzchołka i dodawanie wierzchołka w środku krawędzi.
- Ograniczenia: zadana długość krawędzi oraz równoległość dwóch krawędzi (także z różnych wielokątów), z ikonami na scenie.
- Krawędzie jako odcinki, krzywe Béziera 3. stopnia (z przesuwalnymi punktami kontrolnymi) lub łuki okręgów.
- Ciągłość G0 / G1 / C1 w wierzchołkach sąsiadujących z krzywymi (przy łukach G0 / G1), wymuszana podczas edycji.
- Rysowanie odcinków własnym algorytmem Bresenhama albo funkcją biblioteczną; krzywe Béziera i łuki liczone algorytmami przyrostowymi.

## Skrót obsługi

| Akcja | Efekt |
|---|---|
| LPM + przeciąganie na wierzchołku / punkcie kontrolnym / wnętrzu | przesuwanie wierzchołka / punktu kontrolnego / wielokąta |
| PPM na krawędzi | menu krawędzi: wierzchołek, ograniczenia, rodzaj krawędzi |
| PPM na wierzchołku | menu wierzchołka: usuwanie, ciągłość |
| Esc | anulowanie wyboru drugiej krawędzi |
| Ctrl+R | przywrócenie sceny początkowej |
| F1 | dokumentacja |

## Struktura kodu

| Folder | Zawartość |
|---|---|
| `Models` | scena, wielokąty, wierzchołki, krawędzie, punkty kontrolne, geometria łuku |
| `Constraints` | ograniczenia, rangi wierzchołków i iteracyjny solver |
| `Drawing` | renderer, Bresenham, algorytmy przyrostowe Béziera i łuku |
| `Interaction` | wykrywanie trafień myszą |
| `Scene` | scena początkowa (`DemoSceneFactory`) |
| `UI` | okno główne i okno pomocy |
