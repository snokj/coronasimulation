using System;
using System.Linq;
using System.Collections.Generic;


namespace CoronaSimulation
{
    class Program
    {
        
        static void Main(string[] args)
        {
            Console.WriteLine("Program start \r\n");

            //Sett opp en model av type ArbitraryWorld

            var singleCountry = false;

            IWorld World;

            if(!singleCountry){
                World = new ArbitraryWorld(
                    new List<int>{10000, 10000, 10000}, //Like populasjoner. Modellen fungerer best da
                    new List<int>{1, 0, 0},             //Antall smittede i hvert land
                    new List<int>{10, 10, 15},          //Antall "møter" per dag for person i hvert land
                    new List<double>{0.03, 0.02, 0.05}, //Sannsynlighet for smitte per møte i hvert land
                    new List<List<double>>{             //Sannsynlighet for reise mellom land (symmetrisk matrise)
                        new List<double>{0.98, 0.01, 0.01},
                        new List<double>{0.01, 0.96, 0.03},
                        new List<double>{0.01, 0.03, 0.96}
                    },
                    new List<string>{"Norge", "Sverige", "Danmark"},    //Navn på land
                    new List<PreventiveMeasure>{                        //Parameter som styrer forsøk på begrensning
                        new PreventiveMeasure(0.1, 3, null),
                        new PreventiveMeasure(0.2, 5, null),
                        new PreventiveMeasure(0.2, 5, null)
                    }
                );
            }
            else{
                World = new ArbitraryWorld(
                    new List<int>{10000},
                    new List<int>{1},
                    new List<int>{20},
                    new List<double>{0.03},
                    new List<List<double>>{new List<double>{1}},
                    new List<string>{"Single"}
                );  
            }

            //Variabler som styrer løkke under.
            var RunIteration = true;
            var DayNumber = 1;
            var CapPercentageInfected = 0.95;

            //Variabler som styrer "tiltak". 
            var CounterMeasuresDeployedOnDay = 0;
            var CounterMeasuresDurationInDays = 500;
            var EncountersPerDayReducedTo = 3;


            //Plassering der resultater lagres. Mappe må eksistere
            var ResultsFolderName = "C:/Users/snorr/CoronaSimulation/Results";

            while(RunIteration){

                //Formatering for skriving til kommandolinje
                Console.WriteLine($"---------------------\r\nDay {DayNumber}");

                //Gå en dag framover i modellen. Dersom ingen flere kan smitte andre personer
                //blir RunIteration satt til false, og programmet avslutter etter neste iterasjon
                RunIteration = World.Progress();


                //Igangsett "tiltak" når 10% av befolkningen er smittet
                if(singleCountry && World.GetTotalPercentageInfected() > 0.1 && CounterMeasuresDeployedOnDay == 0){
                    World.UpdateEksponeringerOgInfectionRateForNamedCountry("Single", EncountersPerDayReducedTo, 0.03);
                    CounterMeasuresDeployedOnDay = DayNumber;
                }

                //Avslutt "tiltak" etter oppgitt antall dager.
                if(CounterMeasuresDeployedOnDay != 0 && DayNumber - CounterMeasuresDeployedOnDay > CounterMeasuresDurationInDays){
                    World.UpdateEksponeringerOgInfectionRateForNamedCountry("Single", 20, 0.03);
                }

                //Formatering for skriving til kommandolinje
                Console.WriteLine();

                //Skriv resultater for gjeldende dag til fil
                World.WriteResultsForDay(ResultsFolderName, DayNumber);

                //Øk nummer på dag med en
                DayNumber = DayNumber + 1;

                //Dersom mer enn 80% av individene i verden er smittet,
                //avslutt simuleringen.
                if(World.GetTotalPercentageInfected() >= CapPercentageInfected){
                    RunIteration = false;
                }
            }

            World.MakeResultPlots(ResultsFolderName);
        }
    }
}


