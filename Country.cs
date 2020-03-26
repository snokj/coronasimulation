using System;
using System.Linq;
using System.Collections.Generic;

namespace CoronaSimulation
{
    public class Country
    {
        //Navn på land
        public string Name;

        //Liste over personer som befinner seg i landet
        public List<Person> Borgere;

        //Parameters to adress disease prevention measure in the various countries
        public PreventiveMeasure _PreventiveMeasure;

        //Henter antall personer som befinner seg i landet
        public int AntallBorgere {get => Borgere.Count();}

        //Henter antall smittede personer som befinner seg i landet
        public int Smittede {get => Borgere.Where(b => b.ErSmittet).Count();}

        //Henter antall smittsomme personer som befinner seg i landet
        public int Smittsomme {get => Borgere.Where(b => b.ErSmittsom).Count();}

        //Setter antall "møter" som en person i landet har med andre personer i landet per dag
        public int _EksponeringerPerDag;

        //Setter sannsynlighet for at en frist personen blir smittet dersom den møter en syk person
        public double _InfectionRate;

        //Setter inkuberingstid til 4 dager. Kunne vært satt et annet sted, men har lagt den her
        public int GetIncubation(){return 4;}

        //Setter tid fra person blir smittsom til person blir frist til 14 dager.
        //Kunne vært satt et annet sted, men har lagt den her.
        public int GetRecovery(){return 14;}

        //Opprett landet og sett alle verdier som må være satt.
        //Inkluderer å opprette personene som skal befinne seg i landet ved start av simulering
        public Country(
            int InitialBorgerCount, 
            int InitialInfected, 
            int EksponeringerPerDag, 
            double InfectionRate, 
            string name = null,
            PreventiveMeasure preventiveMeasure = null
        )
        {
            _InfectionRate = InfectionRate;
            _EksponeringerPerDag = EksponeringerPerDag;
            Borgere = new List<Person>();
            _PreventiveMeasure = preventiveMeasure;
            Name = name;

            for(int i = 0; i < InitialBorgerCount; i++){
                Borgere.Add(new Person(GetIncubation(), GetRecovery(), (i<InitialInfected)));
            }
        }

        private void ApplyPreventiveMeasuresIfRequired(){
            if(
                _PreventiveMeasure.UseInfectionPercentage 
                && 
                _PreventiveMeasure.ControlMeasureInfectionPercentageStart <= ((double)Smittede/AntallBorgere))
            {
                if(_PreventiveMeasure.ReducedEncountersPerDay != null){
                    _EksponeringerPerDag = (int)_PreventiveMeasure.ReducedEncountersPerDay;
                }
                if(_PreventiveMeasure.ReducedInfectionRate != null){
                    _InfectionRate = (double)_PreventiveMeasure.ReducedInfectionRate;
                }

            }

        }

        //Infiser personer i landet
        public void Infect(){

            ApplyPreventiveMeasuresIfRequired();

            //For each person in country, expose to EksponeringerPerDag other persons
            var RandomForPerson = new Random();
            var InfectionRandom = new Random();

            //For hver "person" i landet
            foreach(var person in Borgere){

                //Dersom "person" ikke allerede er smittet
                if(!person.ErSmittet){

                    //For antall "møter" per dag i dette landet
                    for(int i = 0; i < _EksponeringerPerDag; i++){ 

                        //Hent en tilfeldig annen borger i landet. Kall denne "encounter"
                        var index = RandomForPerson.Next(Borgere.Count); 
                        var encounter = Borgere[index];

                        //IMPLEMENTASJONSDETALJ: smitte tillates kun FRA "encounter" TIL "person".
                        //Det er sannsynligvis litt feil, men gjør koden enklere.

                        //Hvis "encounter" er smittsom
                        if(encounter.ErSmittsom){

                            //Hent et tilfeldig tall mellom 0 og 1. Dersom tallet er mindre enn
                            //infeksjonsraten har "person" blitt smittet av "encounter".
                            //Sett i så fall relevante verdier på "person". Slutt å iterer over
                            //"møter" for "person", man kan ikke bli smittet mer enn 1 gang.
                            var val = InfectionRandom.NextDouble();
                            if(val <= _InfectionRate){
                                person.ErSmittet = true;
                                person.DagerSidenSmitte = 0;
                                break;
                            }
                        }
                    }
                }
                //Inkrementer tid på personobjektet
                if(person.ErSmittet){
                    //Dette gjør at personer som har blitt smittet etter hvert vil bli smittsomme
                    //og deretter friske.
                    person.DagerSidenSmitte = person.DagerSidenSmitte + 1;
                }
            }
        }
    }
}