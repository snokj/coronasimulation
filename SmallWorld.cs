using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;

namespace CoronaSimulation{
    //SmallWorld is one possible model for the world.
    //Hence it implements the more general interface: IWorld
    public class SmallWorld : IWorld
    {
        public Country Land_A;
        public Country Land_B;
        Double _TravelRate;
        Double _WorldPopulation;

        int SmittsommeOgInkuberingIVerden
        {
            get => 
                Land_A.Borgere.Where(p => p.ErSmittsom || p.ErInkubering).Count()
                + Land_B.Borgere.Where(p => p.ErSmittsom || p.ErInkubering).Count();
        }
        int TotalInfected 
        {
            get => Land_A.Smittede + Land_B.Smittede;
        }

        public double TotalPercentageInfected
        {
            get => (Double)TotalInfected/_WorldPopulation;
        }

        public double GetTotalPercentageInfected()
        {
            return TotalPercentageInfected;
        }

        public bool UpdateEksponeringerOgInfectionRateForNamedCountry(string CountryName, int EksponeringerPerDag, double InfectionRate)
        {
            //Ønsker ikke å bruke dette for denne enkle modellen.
            return true;
        }

        public SmallWorld(
            int Inhabitants_A, 
            int Inhabitants_B, 
            int InitiallyInfected_A, 
            Double TravelRate
        )
        {
            _WorldPopulation = Inhabitants_A + Inhabitants_B;
            _TravelRate = TravelRate;
            Land_A = new Country(Inhabitants_A, InitiallyInfected_A, 20, 0.03);
            Land_B = new Country(Inhabitants_B, 0, 20, 0.03);
        }

        public void MovePersons(){
            //Not Implemented

            var PersonerILandA = Land_A.Borgere;
            var PersonerFlyttetFraLandA = new List<Person>();
            var PersonerIkkeFlyttetFraLandA = new List<Person>();

            var FlytteRandom = new Random();

            foreach(var person in PersonerILandA){
                if(FlytteRandom.NextDouble() < _TravelRate){
                    PersonerFlyttetFraLandA.Add(person);
                }
                else{
                    PersonerIkkeFlyttetFraLandA.Add(person);
                }
            }

            var PersonerILandB = Land_B.Borgere;
            var PersonerFlyttetFraLandB = new List<Person>();
            var PersonerIkkeFlyttetFraLandB = new List<Person>();

            foreach(var person in PersonerILandB){
                if(FlytteRandom.NextDouble() < _TravelRate){
                    PersonerFlyttetFraLandB.Add(person);
                }
                else{
                    PersonerIkkeFlyttetFraLandB.Add(person);
                }
            }

            Land_A.Borgere = PersonerIkkeFlyttetFraLandA.Concat(PersonerFlyttetFraLandB).ToList();
            Land_B.Borgere = PersonerIkkeFlyttetFraLandB.Concat(PersonerFlyttetFraLandA).ToList();

            Console.WriteLine($"{PersonerFlyttetFraLandA.Count} personer flyttet fra Land A"
                + $"({PersonerFlyttetFraLandA.Where(p => p.ErSmittet).Count()} smittede,"
                + $"{PersonerFlyttetFraLandA.Where(p => p.ErSmittsom).Count()} smittsomme)");
            Console.WriteLine($"{PersonerFlyttetFraLandB.Count} personer flyttet fra Land B"
                + $"({PersonerFlyttetFraLandB.Where(p => p.ErSmittet).Count()} smittede,"
                + $"{PersonerFlyttetFraLandB.Where(p => p.ErSmittsom).Count()} smittsomme)");
        }

        public bool Progress(){
            MovePersons();
            Land_A.Infect();
            Land_B.Infect();
            Console.WriteLine($"Land A: {Land_A.Smittede} smittet, {Land_A.Borgere.Where(b => b.ErSmittsom).Count()} smittsomme. {Land_A.Borgere.Count()} borgere.");
            Console.WriteLine($"Land B: {Land_B.Smittede} smittet, {Land_B.Borgere.Where(b => b.ErSmittsom).Count()} smittsomme. {Land_B.Borgere.Count()} borgere.");           
            return SmittsommeOgInkuberingIVerden > 0;
        }

        public void WriteResultsForDay(string ResultFolderName, int DayNumber){

            var FilenameLandA = ResultFolderName + "/" + "LandA.csv";
            var FilenameLandB = ResultFolderName + "/" + "LandB.csv";            

            var InfectedPersonsInLandA = Land_A.Smittede;
            var TotalPersonsInLandA = Land_A.AntallBorgere;

            var InfectedPersonsInLandB = Land_B.Smittede;
            var TotalPersonsInLandB = Land_B.AntallBorgere;

            //Overwrite files if new run was started.
            if(DayNumber == 1){
                File.WriteAllText(FilenameLandA,
                    $"{DayNumber}, {InfectedPersonsInLandA}, {TotalPersonsInLandA}\r\n");
                File.WriteAllText(FilenameLandB,
                    $"{DayNumber}, {InfectedPersonsInLandB}, {TotalPersonsInLandB}\r\n");
            }
            //Update files if still in same run.
            else{
                using (StreamWriter sw = File.AppendText(FilenameLandA)) 
                {
                    sw.WriteLine($"{DayNumber}, {InfectedPersonsInLandA}, {TotalPersonsInLandA}");
                }
                using (StreamWriter sw = File.AppendText(FilenameLandB)) 
                {
                    sw.WriteLine($"{DayNumber}, {InfectedPersonsInLandB}, {TotalPersonsInLandB}");
                }	
            }
        }
        public void MakeResultPlots(string ResultsFolderName){
            throw new Exception("Not implemented!");
        }
    }
}