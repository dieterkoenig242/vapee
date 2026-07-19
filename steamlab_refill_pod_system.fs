// ============================================================
// STEAM-LAB REFILL POD SYSTEM — Onshape FeatureScript
// ------------------------------------------------------------
// Parametrisches CAD-Modell nach Vorbild des "Apple Juice"
// Pod-Geräts (flacher Akku-Slab + Pod + Steckkappe), komplett
// umgestaltet:
//
//   NEU GEGENÜBER DEM ORIGINAL:
//   • NACHFÜLLBAR:  Pod hat abnehmbaren Deckel (Klick-Sitz)
//                   + Füllloch mit Silikon-Stopfen
//   • KLICK-SYSTEM: Rastnasen am Pod rasten hörbar in
//                   Fenster im Gehäuse ein (statt Magnete)
//   • GRÖSSER:      Liquid-Kammer nutzt fast den ganzen Pod,
//                   Standardmaße vergrößert (ca. 10-12 ml
//                   statt ~2 ml) — Kapazität wird nach dem
//                   Rebuild unten im Feature-Dialog angezeigt
//
// TEILE (alle in einem Feature, nebeneinander platziert):
//   1. Gehäuse (Akku-Slab)  — mit Pod-Schacht, Rast-Fenstern,
//                             Batterie-Fach, USB-C + LED-Loch
//   2. Refill-Pod           — Liquid-Kammer, Luftkamin,
//                             2 Kontakt-Pins, Rastnasen
//   3. Pod-Deckel           — mit Mundstück-Oval, Kamin-
//                             Durchführung und Füllloch
//   4. Füll-Stopfen         — verschließt das Füllloch
//   5. Schutzkappe          — Steckkappe mit ovaler Öffnung
//
// INSTALLATION IN ONSHAPE:
//   1. Neues Dokument → "+" (unten links) → "Feature Studio"
//   2. Diesen Code komplett einfügen (alten Code ersetzen)
//   3. Oben rechts auf "Commit" / Häkchen klicken (Build)
//   4. Im Part Studio: Symbolleiste → ganz rechts das
//      Custom-Feature-Symbol → "SteamLab Refill Pod System"
//   5. Alle Maße im Dialog anpassen — alles ist parametrisch
//
// DRUCK-HINWEISE:
//   • Gehäuse + Kappe: PETG/ABS, 0.15 mm Schichthöhe
//   • Pod + Deckel: PETG (liquidbeständig), 100 % Infill
//     an den Wänden für Dichtigkeit, ggf. innen versiegeln
//   • Stopfen: TPU/Silikon (flexibel) — bei hartem Material
//     Füllloch-Ø 0.2 mm größer wählen
//   • Rastnasen funktionieren über die Elastizität der
//     Gehäusewand — Wandstärke nicht über 2 mm wählen
// ============================================================

FeatureScript 2151;
import(path : "onshape/std/geometry.fs", version : "2151.0");

// ------------------------------------------------------------
// Hilfsfunktion: gerundetes Rechteck als geschlossenes Profil
// (4 Linien + 4 Viertelkreis-Bögen) in eine Skizze zeichnen
// ------------------------------------------------------------
function skRoundedRect(sketch is Sketch, prefix is string,
                       cx is ValueWithUnits, cy is ValueWithUnits,
                       w is ValueWithUnits, h is ValueWithUnits,
                       rIn is ValueWithUnits)
{
    var r = min(rIn, min(w, h) * 0.45); // Radius sicher begrenzen
    const x1 = cx - w / 2;
    const x2 = cx + w / 2;
    const y1 = cy - h / 2;
    const y2 = cy + h / 2;
    const d  = r * (1 - sqrt(0.5)); // Abstand des Bogen-Mittelpunkts von der Ecke

    skLineSegment(sketch, prefix ~ "L1", { "start" : vector(x1 + r, y1), "end" : vector(x2 - r, y1) });
    skLineSegment(sketch, prefix ~ "L2", { "start" : vector(x2, y1 + r), "end" : vector(x2, y2 - r) });
    skLineSegment(sketch, prefix ~ "L3", { "start" : vector(x2 - r, y2), "end" : vector(x1 + r, y2) });
    skLineSegment(sketch, prefix ~ "L4", { "start" : vector(x1, y2 - r), "end" : vector(x1, y1 + r) });

    skArc(sketch, prefix ~ "A1", { "start" : vector(x2 - r, y1), "mid" : vector(x2 - d, y1 + d), "end" : vector(x2, y1 + r) });
    skArc(sketch, prefix ~ "A2", { "start" : vector(x2, y2 - r), "mid" : vector(x2 - d, y2 - d), "end" : vector(x2 - r, y2) });
    skArc(sketch, prefix ~ "A3", { "start" : vector(x1 + r, y2), "mid" : vector(x1 + d, y2 - d), "end" : vector(x1, y2 - r) });
    skArc(sketch, prefix ~ "A4", { "start" : vector(x1, y1 + r), "mid" : vector(x1 + d, y1 + d), "end" : vector(x1 + r, y1) });
}

// Richtungs-Konstanten
const DIR_X = vector(1, 0, 0);
const DIR_Y = vector(0, 1, 0);
const DIR_Z = vector(0, 0, 1);

annotation { "Feature Type Name" : "SteamLab Refill Pod System",
             "Feature Type Description" : "Nachfüllbares Pod-System mit Klick-Verriegelung: Gehäuse, Pod, Deckel, Stopfen und Kappe — komplett parametrisch." }
export const steamLabRefillSystem = defineFeature(function(context is Context, id is Id, definition is map)
    precondition
    {
        annotation { "Group Name" : "Bauteile", "Collapsed By Default" : false }
        {
            annotation { "Name" : "Gehäuse (Akku-Slab) bauen", "Default" : true }
            definition.buildGehaeuse is boolean;

            annotation { "Name" : "Refill-Pod bauen", "Default" : true }
            definition.buildPod is boolean;

            annotation { "Name" : "Pod-Deckel bauen", "Default" : true }
            definition.buildDeckel is boolean;

            annotation { "Name" : "Füll-Stopfen bauen", "Default" : true }
            definition.buildStopfen is boolean;

            annotation { "Name" : "Schutzkappe bauen", "Default" : true }
            definition.buildKappe is boolean;
        }

        annotation { "Group Name" : "Gehäuse", "Collapsed By Default" : false }
        {
            annotation { "Name" : "Breite (X)" }
            isLength(definition.gWidth, { (millimeter) : [40, 50, 70] });

            annotation { "Name" : "Dicke (Y)" }
            isLength(definition.gThick, { (millimeter) : [10, 15, 22] });

            annotation { "Name" : "Höhe (Z)" }
            isLength(definition.gHeight, { (millimeter) : [60, 82, 120] });

            annotation { "Name" : "Eckenradius" }
            isLength(definition.gCorner, { (millimeter) : [2, 4, 7] });

            annotation { "Name" : "Wandstärke" }
            isLength(definition.wall, { (millimeter) : [1.2, 1.8, 2.5] });

            annotation { "Name" : "Bodenstärke" }
            isLength(definition.bottomWall, { (millimeter) : [1.5, 2.5, 4] });

            annotation { "Name" : "Pod-Schacht-Tiefe" }
            isLength(definition.bayDepth, { (millimeter) : [12, 24, 40] });

            annotation { "Name" : "USB-C + LED-Loch (unten)", "Default" : true }
            definition.addUSBC is boolean;
        }

        annotation { "Group Name" : "Pod (nachfüllbar)", "Collapsed By Default" : false }
        {
            annotation { "Name" : "Pod-Überstand über Gehäuse" }
            isLength(definition.podExtra, { (millimeter) : [6, 14, 30] });

            annotation { "Name" : "Pod-Wandstärke" }
            isLength(definition.podWall, { (millimeter) : [0.8, 1.2, 2.5] });

            annotation { "Name" : "Pod-Bodenstärke" }
            isLength(definition.podFloor, { (millimeter) : [2, 3, 5] });

            annotation { "Name" : "Luftkamin Außen-Ø" }
            isLength(definition.chimOD, { (millimeter) : [4, 6, 9] });

            annotation { "Name" : "Kontakt-Pin-Ø" }
            isLength(definition.pinDia, { (millimeter) : [1.5, 2.0, 3.0] });

            annotation { "Name" : "Pin-Abstand (Mitte-Mitte)" }
            isLength(definition.pinSpacing, { (millimeter) : [4, 6.5, 12] });

            annotation { "Name" : "Spiel Pod ↔ Schacht (pro Seite)" }
            isLength(definition.clearance, { (millimeter) : [0.1, 0.2, 0.5] });
        }

        annotation { "Group Name" : "Klick-System", "Collapsed By Default" : false }
        {
            annotation { "Name" : "Rastnasen-Breite" }
            isLength(definition.snapWidth, { (millimeter) : [4, 8, 14] });

            annotation { "Name" : "Rastnasen-Vorstand" }
            isLength(definition.snapBump, { (millimeter) : [0.8, 1.2, 2.0] });
        }

        annotation { "Group Name" : "Füllsystem", "Collapsed By Default" : false }
        {
            annotation { "Name" : "Füllloch-Ø (Deckel)" }
            isLength(definition.fillDia, { (millimeter) : [3, 4.5, 7] });
        }
    }
    {
        // ----------------------------------------------------
        // Abgeleitete Maße
        // ----------------------------------------------------
        const Z0     = 0 * millimeter;
        const lugZ   = 4 * millimeter;                          // Rastnasen-Höhe über Pod-Unterkante
        const bayW   = definition.gWidth - 2 * definition.wall; // Schacht innen
        const bayD   = definition.gThick - 2 * definition.wall;
        const bayR   = max(1 * millimeter, definition.gCorner - definition.wall);
        const podW   = bayW - 2 * definition.clearance;         // Pod außen
        const podD   = bayD - 2 * definition.clearance;
        const podR   = max(1 * millimeter, bayR - definition.clearance);
        const podH   = definition.bayDepth + definition.podExtra;
        const chamW  = podW - 2 * definition.podWall;           // Liquid-Kammer
        const chamD  = podD - 2 * definition.podWall;
        const chamR  = max(0.8 * millimeter, podR - definition.podWall);
        const chimID = max(1.5 * millimeter, definition.chimOD - 2.5 * millimeter); // Luftkanal innen
        const battW  = bayW - 3 * millimeter;                   // Batteriefach (schmaler → Auflage-Absatz)
        const battD  = bayD - 2 * millimeter;
        const bayFloorZ = definition.gHeight - definition.bayDepth; // Absatz, auf dem der Pod sitzt
        const winW   = definition.snapWidth + 0.4 * millimeter; // Rast-Fenster im Gehäuse
        const winH   = 2 * definition.snapBump + 0.3 * millimeter;
        const winZ   = bayFloorZ + lugZ;
        const lidPlugH = 2.5 * millimeter;                      // Deckel: Einsteck-Teil
        const lidTopH  = 1.5 * millimeter;                      // Deckel: Deckplatte

        // Platzierung der Teile nebeneinander (X-Versatz)
        const podOffX = definition.gWidth / 2 + podW / 2 + 15 * millimeter;
        const lidOffX = podOffX + podW + 15 * millimeter;
        const plugOffX = lidOffX + podW / 2 + 15 * millimeter;
        const capOutW = definition.gWidth + 3.6 * millimeter;
        const capOffX = -(definition.gWidth / 2 + capOutW / 2 + 15 * millimeter);

        // ====================================================
        // 1) GEHÄUSE (Akku-Slab)
        // ====================================================
        if (definition.buildGehaeuse)
        {
            // Grundkörper
            var sk = newSketchOnPlane(context, id + "gBaseSk", {
                "sketchPlane" : plane(vector(Z0, Z0, Z0), DIR_Z, DIR_X)
            });
            skRoundedRect(sk, "gBase", Z0, Z0, definition.gWidth, definition.gThick, definition.gCorner);
            skSolve(sk);
            extrude(context, id + "gBase", {
                "entities"      : qSketchRegion(id + "gBaseSk", true),
                "endBound"      : BoundingType.BLIND,
                "depth"         : definition.gHeight,
                "operationType" : NewBodyOperationType.NEW
            });
            const gBody = qCreatedBy(id + "gBase", EntityType.BODY);

            // Pod-Schacht von oben
            sk = newSketchOnPlane(context, id + "gBaySk", {
                "sketchPlane" : plane(vector(Z0, Z0, definition.gHeight), DIR_Z, DIR_X)
            });
            skRoundedRect(sk, "gBay", Z0, Z0, bayW, bayD, bayR);
            skSolve(sk);
            extrude(context, id + "gBayCut", {
                "entities"          : qSketchRegion(id + "gBaySk", true),
                "endBound"          : BoundingType.BLIND,
                "depth"             : definition.bayDepth,
                "oppositeDirection" : true,
                "operationType"     : NewBodyOperationType.REMOVE,
                "defaultScope"      : false,
                "booleanScope"      : gBody
            });

            // Batteriefach (schmaler als der Schacht → umlaufender
            // Absatz auf dem der Pod aufsitzt; Pins + Luft gehen
            // durch die offene Mitte nach unten durch)
            sk = newSketchOnPlane(context, id + "gBattSk", {
                "sketchPlane" : plane(vector(Z0, Z0, definition.gHeight), DIR_Z, DIR_X)
            });
            skRoundedRect(sk, "gBatt", Z0, Z0, battW, battD, 1.5 * millimeter);
            skSolve(sk);
            extrude(context, id + "gBattCut", {
                "entities"          : qSketchRegion(id + "gBattSk", true),
                "endBound"          : BoundingType.BLIND,
                "depth"             : definition.gHeight - definition.bottomWall,
                "oppositeDirection" : true,
                "operationType"     : NewBodyOperationType.REMOVE,
                "defaultScope"      : false,
                "booleanScope"      : gBody
            });

            // USB-C-Schlitz + LED-Loch im Boden (dient zugleich
            // als Lufteinlass für den Zug)
            if (definition.addUSBC)
            {
                sk = newSketchOnPlane(context, id + "gUsbSk", {
                    "sketchPlane" : plane(vector(Z0, Z0, Z0), DIR_Z, DIR_X)
                });
                skRoundedRect(sk, "gUsb", Z0, Z0, 9 * millimeter, 3.2 * millimeter, 1.4 * millimeter);
                skCircle(sk, "gLed", {
                    "center" : vector(definition.gWidth / 6, Z0),
                    "radius" : 0.75 * millimeter
                });
                skSolve(sk);
                extrude(context, id + "gUsbCut", {
                    "entities"      : qSketchRegion(id + "gUsbSk", true),
                    "endBound"      : BoundingType.BLIND,
                    "depth"         : definition.bottomWall + 2 * millimeter,
                    "operationType" : NewBodyOperationType.REMOVE,
                    "defaultScope"  : false,
                    "booleanScope"  : gBody
                });
            }

            // Rast-Fenster: ein Durchgangs-Schnitt erzeugt die
            // Fenster in Vorder- UND Rückwand (Schachtmitte ist eh hohl)
            sk = newSketchOnPlane(context, id + "gWinSk", {
                "sketchPlane" : plane(vector(Z0, -definition.gThick / 2 - 1 * millimeter, winZ), DIR_Y, DIR_X)
            });
            skRectangle(sk, "gWin", {
                "firstCorner"  : vector(-winW / 2, -winH / 2),
                "secondCorner" : vector( winW / 2,  winH / 2)
            });
            skSolve(sk);
            extrude(context, id + "gWinCut", {
                "entities"      : qSketchRegion(id + "gWinSk", true),
                "endBound"      : BoundingType.BLIND,
                "depth"         : definition.gThick + 2 * millimeter,
                "operationType" : NewBodyOperationType.REMOVE,
                "defaultScope"  : false,
                "booleanScope"  : gBody
            });

            setProperty(context, {
                "entities" : gBody,
                "property" : Property.NAME,
                "value"    : "Gehäuse (Akku-Slab)"
            });
        }

        // ====================================================
        // 2) REFILL-POD (große Liquid-Kammer, Klick-Rastnasen)
        // ====================================================
        if (definition.buildPod)
        {
            // Grundkörper
            var sk = newSketchOnPlane(context, id + "pBaseSk", {
                "sketchPlane" : plane(vector(Z0, Z0, Z0), DIR_Z, DIR_X)
            });
            skRoundedRect(sk, "pBase", podOffX, Z0, podW, podD, podR);
            skSolve(sk);
            extrude(context, id + "pBase", {
                "entities"      : qSketchRegion(id + "pBaseSk", true),
                "endBound"      : BoundingType.BLIND,
                "depth"         : podH,
                "operationType" : NewBodyOperationType.NEW
            });
            const pBody = qCreatedBy(id + "pBase", EntityType.BODY);

            // Liquid-Kammer (oben offen → Deckel), nutzt fast
            // den kompletten Pod = maximale Füllmenge
            sk = newSketchOnPlane(context, id + "pChamSk", {
                "sketchPlane" : plane(vector(Z0, Z0, podH), DIR_Z, DIR_X)
            });
            skRoundedRect(sk, "pCham", podOffX, Z0, chamW, chamD, chamR);
            skSolve(sk);
            extrude(context, id + "pChamCut", {
                "entities"          : qSketchRegion(id + "pChamSk", true),
                "endBound"          : BoundingType.BLIND,
                "depth"             : podH - definition.podFloor,
                "oppositeDirection" : true,
                "operationType"     : NewBodyOperationType.REMOVE,
                "defaultScope"      : false,
                "booleanScope"      : pBody
            });

            // Luftkamin: Rohr in der Kammermitte, vom Boden bis
            // zur Oberkante (trennt Luftweg vom Liquid)
            sk = newSketchOnPlane(context, id + "pChimSk", {
                "sketchPlane" : plane(vector(Z0, Z0, definition.podFloor - 0.5 * millimeter), DIR_Z, DIR_X)
            });
            skCircle(sk, "pChim", { "center" : vector(podOffX, Z0), "radius" : definition.chimOD / 2 });
            skSolve(sk);
            extrude(context, id + "pChimAdd", {
                "entities"      : qSketchRegion(id + "pChimSk", true),
                "endBound"      : BoundingType.BLIND,
                "depth"         : podH - definition.podFloor + 0.5 * millimeter,
                "operationType" : NewBodyOperationType.ADD,
                "defaultScope"  : false,
                "booleanScope"  : pBody
            });

            // Luftkanal: durchgehende Bohrung durch Kamin und
            // Boden (Zug: Boden-Einlass → Kamin → Mundstück)
            sk = newSketchOnPlane(context, id + "pAirSk", {
                "sketchPlane" : plane(vector(Z0, Z0, podH + 1 * millimeter), DIR_Z, DIR_X)
            });
            skCircle(sk, "pAir", { "center" : vector(podOffX, Z0), "radius" : chimID / 2 });
            skSolve(sk);
            extrude(context, id + "pAirCut", {
                "entities"          : qSketchRegion(id + "pAirSk", true),
                "endBound"          : BoundingType.BLIND,
                "depth"             : podH + 2 * millimeter,
                "oppositeDirection" : true,
                "operationType"     : NewBodyOperationType.REMOVE,
                "defaultScope"      : false,
                "booleanScope"      : pBody
            });

            // Kontakt-Pins (unten, wie beim rosa Original-Pod)
            sk = newSketchOnPlane(context, id + "pPinSk", {
                "sketchPlane" : plane(vector(Z0, Z0, 1 * millimeter), DIR_Z, DIR_X)
            });
            skCircle(sk, "pPin0", { "center" : vector(podOffX - definition.pinSpacing / 2, Z0), "radius" : definition.pinDia / 2 });
            skCircle(sk, "pPin1", { "center" : vector(podOffX + definition.pinSpacing / 2, Z0), "radius" : definition.pinDia / 2 });
            skSolve(sk);
            extrude(context, id + "pPinAdd", {
                "entities"          : qSketchRegion(id + "pPinSk", true),
                "endBound"          : BoundingType.BLIND,
                "depth"             : 4 * millimeter,
                "oppositeDirection" : true,
                "operationType"     : NewBodyOperationType.ADD,
                "defaultScope"      : false,
                "booleanScope"      : pBody
            });

            // KLICK-SYSTEM: halbrunde Rastnasen auf Vorder- und
            // Rückseite. Runder Querschnitt = Einführ- und
            // Auszieh-Rampe in einem; rastet in die Gehäuse-
            // Fenster ein ("Klick"), Pod bleibt aber entnehmbar.
            sk = newSketchOnPlane(context, id + "pLugSk", {
                "sketchPlane" : plane(vector(podOffX - definition.snapWidth / 2, Z0, Z0), DIR_X, DIR_Y)
            });
            skCircle(sk, "pLug0", {
                "center" : vector( (podD / 2 - 0.2 * millimeter), lugZ),
                "radius" : definition.snapBump + 0.2 * millimeter
            });
            skCircle(sk, "pLug1", {
                "center" : vector(-(podD / 2 - 0.2 * millimeter), lugZ),
                "radius" : definition.snapBump + 0.2 * millimeter
            });
            skSolve(sk);
            extrude(context, id + "pLugAdd", {
                "entities"      : qSketchRegion(id + "pLugSk", true),
                "endBound"      : BoundingType.BLIND,
                "depth"         : definition.snapWidth,
                "operationType" : NewBodyOperationType.ADD,
                "defaultScope"  : false,
                "booleanScope"  : pBody
            });

            setProperty(context, {
                "entities" : pBody,
                "property" : Property.NAME,
                "value"    : "Refill-Pod"
            });

            // Füllmenge berechnen und im Feature-Dialog anzeigen
            const liqH = podH - definition.podFloor - lidPlugH;
            const vol  = chamW * chamD * liqH - PI * (definition.chimOD / 2) ^ 2 * liqH;
            const ml   = roundToPrecision(vol / centimeter ^ 3, 1);
            reportFeatureInfo(context, id,
                "Liquid-Kapazität: ca. " ~ toString(ml) ~ " ml. " ~
                "Zum Nachfüllen: Stopfen ziehen ODER Deckel abklicken.");
        }

        // ====================================================
        // 3) POD-DECKEL (Klick-Sitz, Mundstück, Füllloch)
        // ====================================================
        if (definition.buildDeckel)
        {
            const plugW = chamW - 0.2 * millimeter;
            const plugD = chamD - 0.2 * millimeter;

            // Einsteck-Teil (taucht in die Kammeröffnung ein)
            var sk = newSketchOnPlane(context, id + "dPlugSk", {
                "sketchPlane" : plane(vector(Z0, Z0, Z0), DIR_Z, DIR_X)
            });
            skRoundedRect(sk, "dPlug", lidOffX, Z0, plugW, plugD, max(0.8 * millimeter, chamR - 0.1 * millimeter));
            skSolve(sk);
            extrude(context, id + "dPlug", {
                "entities"      : qSketchRegion(id + "dPlugSk", true),
                "endBound"      : BoundingType.BLIND,
                "depth"         : lidPlugH,
                "operationType" : NewBodyOperationType.NEW
            });
            const dBody = qCreatedBy(id + "dPlug", EntityType.BODY);

            // Deckplatte (liegt auf dem Pod-Rand auf)
            sk = newSketchOnPlane(context, id + "dTopSk", {
                "sketchPlane" : plane(vector(Z0, Z0, lidPlugH), DIR_Z, DIR_X)
            });
            skRoundedRect(sk, "dTop", lidOffX, Z0, podW, podD, podR);
            skSolve(sk);
            extrude(context, id + "dTopAdd", {
                "entities"      : qSketchRegion(id + "dTopSk", true),
                "endBound"      : BoundingType.BLIND,
                "depth"         : lidTopH,
                "operationType" : NewBodyOperationType.ADD,
                "defaultScope"  : false,
                "booleanScope"  : dBody
            });

            // Ovales Mundstück (wie die Öffnung der Steckkappe)
            sk = newSketchOnPlane(context, id + "dMouthSk", {
                "sketchPlane" : plane(vector(Z0, Z0, lidPlugH + lidTopH), DIR_Z, DIR_X)
            });
            skEllipse(sk, "dMouth", {
                "center"      : vector(lidOffX, Z0),
                "majorRadius" : definition.chimOD / 2 + 3 * millimeter,
                "minorRadius" : min(definition.chimOD / 2 + 1.5 * millimeter, podD / 2 - 1 * millimeter)
            });
            skSolve(sk);
            extrude(context, id + "dMouthAdd", {
                "entities"      : qSketchRegion(id + "dMouthSk", true),
                "endBound"      : BoundingType.BLIND,
                "depth"         : 6 * millimeter,
                "operationType" : NewBodyOperationType.ADD,
                "defaultScope"  : false,
                "booleanScope"  : dBody
            });

            // Kamin-Durchführung: unten weites Loch (stülpt sich
            // über den Kamin), oben enger Luftkanal
            sk = newSketchOnPlane(context, id + "dChimSk", {
                "sketchPlane" : plane(vector(Z0, Z0, Z0), DIR_Z, DIR_X)
            });
            skCircle(sk, "dChim", { "center" : vector(lidOffX, Z0), "radius" : (definition.chimOD + 0.3 * millimeter) / 2 });
            skSolve(sk);
            extrude(context, id + "dChimCut", {
                "entities"      : qSketchRegion(id + "dChimSk", true),
                "endBound"      : BoundingType.BLIND,
                "depth"         : lidPlugH,
                "operationType" : NewBodyOperationType.REMOVE,
                "defaultScope"  : false,
                "booleanScope"  : dBody
            });

            sk = newSketchOnPlane(context, id + "dAirSk", {
                "sketchPlane" : plane(vector(Z0, Z0, lidPlugH + lidTopH + 7 * millimeter), DIR_Z, DIR_X)
            });
            skCircle(sk, "dAir", { "center" : vector(lidOffX, Z0), "radius" : chimID / 2 });
            skSolve(sk);
            extrude(context, id + "dAirCut", {
                "entities"          : qSketchRegion(id + "dAirSk", true),
                "endBound"          : BoundingType.BLIND,
                "depth"             : lidPlugH + lidTopH + 8 * millimeter,
                "oppositeDirection" : true,
                "operationType"     : NewBodyOperationType.REMOVE,
                "defaultScope"      : false,
                "booleanScope"      : dBody
            });

            // FÜLLLOCH: durch Deckplatte und Einsteck-Teil,
            // seitlich neben dem Kamin → direkt in die Kammer
            sk = newSketchOnPlane(context, id + "dFillSk", {
                "sketchPlane" : plane(vector(Z0, Z0, lidPlugH + lidTopH + 1 * millimeter), DIR_Z, DIR_X)
            });
            skCircle(sk, "dFill", { "center" : vector(lidOffX + podW / 4, Z0), "radius" : definition.fillDia / 2 });
            skSolve(sk);
            extrude(context, id + "dFillCut", {
                "entities"          : qSketchRegion(id + "dFillSk", true),
                "endBound"          : BoundingType.BLIND,
                "depth"             : lidPlugH + lidTopH + 2 * millimeter,
                "oppositeDirection" : true,
                "operationType"     : NewBodyOperationType.REMOVE,
                "defaultScope"      : false,
                "booleanScope"      : dBody
            });

            // Klick-Rippen am Einsteck-Teil: leichtes Übermaß
            // gegen die Kammerwand → Deckel rastet spürbar ein
            sk = newSketchOnPlane(context, id + "dRibSk", {
                "sketchPlane" : plane(vector(lidOffX - 4 * millimeter, Z0, Z0), DIR_X, DIR_Y)
            });
            skCircle(sk, "dRib0", { "center" : vector( (plugD / 2 - 0.15 * millimeter), lidPlugH / 2), "radius" : 0.45 * millimeter });
            skCircle(sk, "dRib1", { "center" : vector(-(plugD / 2 - 0.15 * millimeter), lidPlugH / 2), "radius" : 0.45 * millimeter });
            skSolve(sk);
            extrude(context, id + "dRibAdd", {
                "entities"      : qSketchRegion(id + "dRibSk", true),
                "endBound"      : BoundingType.BLIND,
                "depth"         : 8 * millimeter,
                "operationType" : NewBodyOperationType.ADD,
                "defaultScope"  : false,
                "booleanScope"  : dBody
            });

            setProperty(context, {
                "entities" : dBody,
                "property" : Property.NAME,
                "value"    : "Pod-Deckel (Füllsystem)"
            });
        }

        // ====================================================
        // 4) FÜLL-STOPFEN (am besten aus TPU drucken)
        // ====================================================
        if (definition.buildStopfen)
        {
            var sk = newSketchOnPlane(context, id + "sShaftSk", {
                "sketchPlane" : plane(vector(Z0, Z0, Z0), DIR_Z, DIR_X)
            });
            skCircle(sk, "sShaft", { "center" : vector(plugOffX, Z0), "radius" : (definition.fillDia + 0.1 * millimeter) / 2 });
            skSolve(sk);
            extrude(context, id + "sShaft", {
                "entities"      : qSketchRegion(id + "sShaftSk", true),
                "endBound"      : BoundingType.BLIND,
                "depth"         : 3.2 * millimeter,
                "operationType" : NewBodyOperationType.NEW
            });
            const sBody = qCreatedBy(id + "sShaft", EntityType.BODY);

            sk = newSketchOnPlane(context, id + "sCapSk", {
                "sketchPlane" : plane(vector(Z0, Z0, 3.2 * millimeter), DIR_Z, DIR_X)
            });
            skCircle(sk, "sCap", { "center" : vector(plugOffX, Z0), "radius" : definition.fillDia / 2 + 1.75 * millimeter });
            skSolve(sk);
            extrude(context, id + "sCapAdd", {
                "entities"      : qSketchRegion(id + "sCapSk", true),
                "endBound"      : BoundingType.BLIND,
                "depth"         : 1.5 * millimeter,
                "operationType" : NewBodyOperationType.ADD,
                "defaultScope"  : false,
                "booleanScope"  : sBody
            });

            setProperty(context, {
                "entities" : sBody,
                "property" : Property.NAME,
                "value"    : "Füll-Stopfen"
            });
        }

        // ====================================================
        // 5) SCHUTZKAPPE (Steckkappe mit ovaler Öffnung,
        //    wie das grüne Original-Teil)
        // ====================================================
        if (definition.buildKappe)
        {
            const capClr   = 0.3 * millimeter;
            const capInW   = definition.gWidth + 2 * capClr;
            const capInD   = definition.gThick + 2 * capClr;
            const capOutD  = capInD + 3 * millimeter;
            const capInnerDepth = definition.podExtra + 10 * millimeter;
            const capTotal = capInnerDepth + 1.5 * millimeter;

            var sk = newSketchOnPlane(context, id + "kBaseSk", {
                "sketchPlane" : plane(vector(Z0, Z0, Z0), DIR_Z, DIR_X)
            });
            skRoundedRect(sk, "kBase", capOffX, Z0, capOutW, capOutD, definition.gCorner + 1.8 * millimeter);
            skSolve(sk);
            extrude(context, id + "kBase", {
                "entities"      : qSketchRegion(id + "kBaseSk", true),
                "endBound"      : BoundingType.BLIND,
                "depth"         : capTotal,
                "operationType" : NewBodyOperationType.NEW
            });
            const kBody = qCreatedBy(id + "kBase", EntityType.BODY);

            // Innenraum (von unten offen, steckt auf dem Gehäuse)
            sk = newSketchOnPlane(context, id + "kInSk", {
                "sketchPlane" : plane(vector(Z0, Z0, Z0), DIR_Z, DIR_X)
            });
            skRoundedRect(sk, "kIn", capOffX, Z0, capInW, capInD, definition.gCorner + capClr);
            skSolve(sk);
            extrude(context, id + "kInCut", {
                "entities"      : qSketchRegion(id + "kInSk", true),
                "endBound"      : BoundingType.BLIND,
                "depth"         : capInnerDepth,
                "operationType" : NewBodyOperationType.REMOVE,
                "defaultScope"  : false,
                "booleanScope"  : kBody
            });

            // Ovale Öffnung oben (Mundstück-Durchlass)
            sk = newSketchOnPlane(context, id + "kOvalSk", {
                "sketchPlane" : plane(vector(Z0, Z0, capTotal), DIR_Z, DIR_X)
            });
            skEllipse(sk, "kOval", {
                "center"      : vector(capOffX, Z0),
                "majorRadius" : definition.chimOD / 2 + 3.5 * millimeter,
                "minorRadius" : definition.chimOD / 2 + 2 * millimeter
            });
            skSolve(sk);
            extrude(context, id + "kOvalCut", {
                "entities"          : qSketchRegion(id + "kOvalSk", true),
                "endBound"          : BoundingType.BLIND,
                "depth"             : 2.5 * millimeter,
                "oppositeDirection" : true,
                "operationType"     : NewBodyOperationType.REMOVE,
                "defaultScope"      : false,
                "booleanScope"      : kBody
            });

            setProperty(context, {
                "entities" : kBody,
                "property" : Property.NAME,
                "value"    : "Schutzkappe"
            });
        }

        // Aufräumen: alle Hilfs-Skizzen ausblenden/löschen
        opDeleteBodies(context, id + "cleanupSketches", {
            "entities" : qSketchFilter(qCreatedBy(id, EntityType.BODY), SketchObject.YES)
        });
    });
