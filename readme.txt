Dette er en konsollapplikasjon (skriver tekst til kommandolinje(f.eks. i Visual Studio Code))

FOR Å KJØRE PROSJEKTET:
------------------------------------------------
Oppstart (mener ikke at det er noe mer som må gjøres, men er ikke 100% sikker.)
    1. Last ned VisualStudioCode
    2. Åpne mappen som inneholder denne filen i VisualStudioCode (via File -> Open Folder)
    3. hvis du ikke ser oversikt over filer "Explorer" på venstre side, trykk på filikonet
        ytters til venstre (et ark med et eseløre på foran et annet ark)
    4. Gå til Terminal (øverst til venstre) -> New Terminal. Nederst i skjermbildet der terminalen dukker opp,
        legg inn følgende linjer og trykk enter:
            -> "dotnet add package Scottplot"
    5. Åpne fila Program.cs
    6. Gå til linja med tekst "var ResultsFolderName". Mellom fnuttene oppgir du her en 
        mappe som finnes på din PC (der resultater skal lagres).
    7. Trykk på Run -> Run without debugging
    8. Koden kjører.

Kjøre i debug:
    1. Sett et "breakpoint" i Program.cs. Du gjør dette ved å gå til Program.cs
        og venstreklikke i margen til venstre for linjenumrene. Da skal det vises en rød prikk.
        Sett f.eks. breakpoint inne i "while"-løkka.
    2. Trykk på Run -> Start Debugging.
    3. Koden blir nå "Bygget" og vil begynne å kjøre.
    4. Hvis alt går bra blir breakpointet du satte i steg 1 omkranset av en gul pil.
        Pila viser hvor i fila du befinner deg for debuggingsformål.
    5. For å stegvis gå gjennom koden bruker du F10 for å gå til neste kodelinje, eller
        F11 for å gå inn i kall som gjøres i gjeldende kodelinje (for å se hva som skjer når
        metoder kjøres, kunne f.eks. brukes på denne linja i Program.cs: 
        "RunIteration = World.Progress();"), F5 for å fortsette til neste breakpoint (eller
        slutten av programmet hvis det ikke finnes flere breakpoint) eller shift+F5 for å stanse
        debugging. Alt dette kan også gjøres fra verktøylinje høyt oppe i skjermbildet.
    6. Øverst i skjermbildet har du også en verktøylinje som hjelper deg med dette.



OBJEKTER OG INTERFACER:
------------------------------------------------
Person (Person.cs), objekt:
Dette objektet inneholder informasjon om tilstanden til personen

Country (Country.cs), objekt:
Dette objektet inneholder en liste over personer og noen metoder som gir informasjon om denne listen
f.eks. antall smittede. Inneholder også informasjon om hyppighet av møter mellom personer i landet,
og hvor ofte et møte mellom personer resulterer i smitte.
Country har også en metode for å eksponere personer for andre personer i samme land og gi en sjanse
for smitte dersom personen man blir eksponert for er smittsom (smittet og ikke ferdig med sykdommen).

IWorld (IWorld.cs), interface:
Definerer hvilke metoder vi trenger i en modell av "Verden" (simuleringen).
Følgende metoder trengs:
    - MovePersons(), flytter personer mellom land
    - Progress(), gå en dag fremover i tid
    - WriteResultsForDay(), skrive resultater fra gjeldende dag til fil

SmallWorld (SmallWorld.cs), objekt:
Implementerer IWorld (arver fra IWorld. Det betyr at SmallWorld er en type IWorld)
Modellerer verden som kun to land

ArbitraryWorld (ArbitraryWorld.cs), objekt:
Implementerer IWorld (arver fra IWorld. Det betyr at ArbitraryWorld er en type IWorld)
Modellerer verden som et arbitrært antall land. Hvert land har et gitt antall innbyggere
og et gitt antall smittede ved start på simulering. I tillegg har hvert land et antall
møter mellom personer per dag (avhengig av land) og en gitt sannsynlighet for overføring
av sykdom ved hvert møte. Objektet trenger også en matrise som definerer rater for flytting 
mellom land. Denne matrisen bør være symmetrisk (dvs. hvis matrisen er A og i og j angir
henholdsvis radnummer og kolonnenummer så bør A(i,j) = A(j, i)), ellers vil det hope seg 
opp med folk i landet der tilflyttingsraten er størst.


------------------------------------------
HVA ER BRUKT:

Programmeringsspråk:
C#, dette språket er:
    1. et høynivå programmeringsspråk (les: bruker slipper å ta hånd om mange tekniske detaljer,
        slik som minnehåndtering etc.).
    2. en dialekt av C. Svært likt Java (ikke JavaScript).
    3. sterkt typet, for alle variabler må det være klart hvilken struktur dataen har.

Datatyper:
Boolean: data av type "true" eller "false"
int: heltall
string: tekststreng
double: desimaltall
List<T>: lager en liste over data med datatype T (mest brukt for List<Person>)
Exception: brukt i throw new Exception(). Dette gjøres for å avslutte programmet
        og returnere en feil dersom man havner i en uforventet situasjon.
var: Ikke egentlig en datatype. Man kan bruke denne som enkel notasjon når datatypen
        er "åpenbar" for kompilatoren.

Namespace:
Dette er et lite prosjekt, så jeg har lagt alle klasser i samme Namespace.
Effekten av dette er at alle klassene i prosjektet kan brukes i alle andre klasser
uten at man trenger å gjøre noe mer.


Teknologi:

LINQ: tillater å gjøre operasjoner som .Where(x => f(x)) på lister. Denne metoden returnerer
kun elementer fra listen der elementet x er slik at f(x) = true (f(x) er en funksjon som returnerer
en boolean, dvs "true" eller "false"). Tillater også å gjøre sorteringer etc.

Skriving til fil: bruker File.WriteAllText og StreamWriter for å skrive resultater til csv-filer
(comma separated values). Disse filene kan åpnes f.eks. i Excel.


Forslag til utvidelser:
1. Støtte i ArbitraryWorld for å spesifisere ulike grupper med mennesker i landene, slik at vi f.eks. kan gi
ulike verdier for infeksjonsrate og antall eksponeringer per dag for gamle og unge mennesker
2. Støtte spesifikasjon av smitteverntiltak også i tilfeller der det er mer enn ett land (flytte denne funksjonaliteten 
inn i ArbitraryWorld og ut av Program)




