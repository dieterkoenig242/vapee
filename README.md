# SteamLab Refill Pod System (Onshape CAD)

Parametrisches FeatureScript-Modell des Pod-Geräts (Vorbild: „Apple Juice"-Slab),
umgestaltet zu einem **nachfüllbaren System mit Klick-Verriegelung** und
**deutlich größerer Liquid-Kammer** (Standardmaße ≈ 10–12 ml statt ~2 ml —
die exakte Kapazität wird nach dem Rebuild im Feature-Dialog angezeigt).

Datei: [`steamlab_refill_pod_system.fs`](steamlab_refill_pod_system.fs)

## In Onshape einfügen

1. Neues Onshape-Dokument öffnen
2. Unten links **„+" → Feature Studio** anlegen
3. Kompletten Inhalt der `.fs`-Datei einfügen (vorhandenen Code ersetzen)
4. Oben **Commit/Build** klicken — es darf kein Fehler erscheinen
5. Ins **Part Studio** wechseln → in der Symbolleiste ganz rechts das
   Custom-Feature-Symbol → **„SteamLab Refill Pod System"** wählen
6. Alle Maße im Dialog anpassen (alles parametrisch, in mm)

Das Feature erzeugt bis zu 5 Teile nebeneinander (einzeln abschaltbar):

| Teil | Funktion |
|---|---|
| Gehäuse (Akku-Slab) | Pod-Schacht, Rast-Fenster, Batteriefach, USB-C + LED |
| Refill-Pod | große Liquid-Kammer, Luftkamin, 2 Kontakt-Pins, Rastnasen |
| Pod-Deckel | Mundstück-Oval, Kamin-Durchführung, **Füllloch** |
| Füll-Stopfen | verschließt das Füllloch (aus TPU drucken) |
| Schutzkappe | Steckkappe mit ovaler Öffnung (wie das grüne Original-Teil) |

## Klick-System

- Der Pod hat zwei **halbrunde Rastnasen** (vorn + hinten). Beim Einschieben
  federt die Gehäusewand, die Nasen rasten hörbar in die **Fenster** im
  Gehäuse ein — der Pod sitzt fest, lässt sich aber wieder herausziehen.
- Der runde Nasen-Querschnitt wirkt als Einführ- und Auszieh-Rampe zugleich.
- Der **Deckel** rastet über zwei dünne Klick-Rippen mit leichtem Übermaß in
  der Kammeröffnung ein.
- Wichtig: Gehäuse-Wandstärke ≤ 2 mm lassen, sonst federt nichts.

## Nachfüllen

1. Schnellweg: **Stopfen** aus dem Füllloch ziehen, Liquid einfüllen, Stopfen rein.
2. Gründlich: Pod aus dem Gehäuse klicken, **Deckel** abhebeln, Kammer füllen,
   Deckel wieder aufklicken.

## Druck-Empfehlungen

- **Gehäuse + Kappe:** PETG oder ABS, 0,15 mm Schichthöhe
- **Pod + Deckel:** PETG (liquidbeständig), Wände dicht drucken
  (3+ Perimeter), ggf. innen versiegeln
- **Stopfen:** TPU (flexibel); bei hartem Material das Füllloch 0,2 mm größer wählen
- Passungen: Parameter „Spiel Pod ↔ Schacht" bei Bedarf an den eigenen
  Drucker anpassen (Standard 0,2 mm pro Seite)

## Elektronik-Hinweis

Das Modell ist die reine Mechanik (Gehäuse/Pod). Akku, Platine, Coil und
Kontakte kommen aus einem Spender-Gerät oder als Ersatzteile; die Pins und
das Batteriefach sind dafür dimensioniert (Pin-Ø und -Abstand parametrisch).
