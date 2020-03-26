using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Globalization;

namespace CoronaSimulation{
    class ArbitraryWorld : IWorld
    {

        //1xn vektor. Liste over land
        public List<Country> _Countries; 

        //nxn matrise. Reiserater
        public List<List<double>> _TravelRate; 

        //Total populasjon i "verden". Denne er invariant i modellen.
        Double _WorldPopulation;

        //Hent antall personer fra alle land som enten er smittsomme eller som er under inkubering
        int SmittsommeOgInkuberingIVerden
        {
            get => _Countries.SelectMany(c => c.Borgere).Where(b => b.ErSmittsom || b.ErInkubering).Count();
        }

        //Hent antall smittede totalt i "verden"
        int TotalInfected 
        {
            get => _Countries.Select(c => c.Smittede).Sum();
        }

        //Finn prosentandel av verden som er smittet
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
            var CountryToEdit = _Countries.Where(c => c.Name == CountryName)?.FirstOrDefault();

            if(CountryToEdit == null){
                return false;
            }

            var index = _Countries.IndexOf(CountryToEdit);

            CountryToEdit._EksponeringerPerDag = EksponeringerPerDag;
            CountryToEdit._InfectionRate = InfectionRate;

            _Countries[index] = CountryToEdit;

            return true;
        }

        //Validering av data brukt til å konstruere modellen.
        private bool ValidateConstructorInput(
            List<int> InitialPopulations,
            List<int> InitiallyInfected,
            List<int> DailyEncounters,
            List<double> InfectionProbability,
            List<List<Double>> TravelRate,
            List<string> CountryNames = null,
            List<PreventiveMeasure> PreventiveMeasures = null
        )
        {
            //Ensure appropriate size of vectors and matrices
            var NumberOfCountries = InitialPopulations.Count();

            if(
                InitiallyInfected.Count() != NumberOfCountries
                || DailyEncounters.Count() != NumberOfCountries
                || InfectionProbability.Count() != NumberOfCountries
                || (PreventiveMeasures.Count() != NumberOfCountries && PreventiveMeasures != null)
                || (CountryNames != null && CountryNames.Count() != NumberOfCountries)
                || TravelRate.Count() != NumberOfCountries
                || TravelRate.Any(t => t.Count() != NumberOfCountries)
            )
            {
                return false;
            }

            if(InfectionProbability.Any(i => i < 0 || i > 1))
            {
                return false;
            }

            for(int i = 0; i < NumberOfCountries; i++){
                if(InitiallyInfected[i] > InitialPopulations[i])
                    return false;
            }

            return true;
        }

        //Opprett modellen og sett alle verdier som må være satt.
        //Inkluderer å opprette alle land.
        public ArbitraryWorld(
            List<int> InitialPopulations,
            List<int> InitiallyInfected,
            List<int> DailyEncounters,
            List<double> InfectionProbability,
            List<List<Double>> TravelRate,
            List<string> CountryNames = null,
            List<PreventiveMeasure> PreventiveMeasures = null
        )
        {
            
            var InputIsOk = ValidateConstructorInput(InitialPopulations, InitiallyInfected, 
                DailyEncounters, InfectionProbability, TravelRate, CountryNames, PreventiveMeasures);

            if(!InputIsOk)
                throw new Exception();
            
            _WorldPopulation = InitialPopulations.Sum();
            _TravelRate = TravelRate;
            _Countries = new List<Country>();

            for(int i = 0; i < InitialPopulations.Count(); i++){
                var CountryToAdd = new Country(
                    InitialPopulations[i], 
                    InitiallyInfected[i], 
                    DailyEncounters[i], 
                    InfectionProbability[i], 
                    CountryNames != null ? CountryNames[i] : $"Land {i}",
                    PreventiveMeasures != null ? PreventiveMeasures[i] : null
                );
                _Countries.Add(CountryToAdd);
            }
        }

        //Flytt personer mellom land (reise).
        public void MovePersons(){
            //From the current country list,
            //  -> ADRESSED. Randomly find people to move according to TravelRate
            //  -> ADRESSED. Do this in a way that does not bias moving to low-index countries.
            //  -> Do this in a way that does not promote equalizing country populations

            //Kan ikke flytte folk fra et land til det samme landet.
            if(_Countries.Count() == 1){
                return;
            }

            //Hent lister over personer i alle land i modellen (dette er en liste av lister)
            var CurrentPopulations = _Countries.Select(c => c.Borgere).ToList();
            //Opprett tilsvarende, tom oversikt som lagrer tilstand etter reiser for gjeldende dag.
            var NewPopulations = new List<List<Person>>();

            var rnd = new Random();

            //For hvert land CurrentCountry
            for(int CurrentCountry = 0; CurrentCountry < _Countries.Count(); CurrentCountry++){
                
                //Finn hvor mange personer som skal flyttes til alle andre land i modellen
                //Dette regnes ut som TravelRate FRA CurrentCountry TIL nytt land
                //ganger antall personer i CurrentCountry rundet til nærmeste heltall
                var NumberOfPersonsToMove = _TravelRate[CurrentCountry]
                    .Select(x => (int)Math.Round(x * CurrentPopulations[CurrentCountry].Count())).ToList();

                //Stokk om på listen over personer i CurrentCountry.
                //Lagre som ny liste som kommer til å bli endret på.
                var MutablePopulation = CurrentPopulations[CurrentCountry]
                    .OrderBy(x => rnd.Next()).ToList();

                //For hvert land NewCountry
                for(int NewCountry = 0; NewCountry < _Countries.Count(); NewCountry++){
                    //Hvis vi er på det første landet i den ytre løkka må vi opprette en ny liste
                    //og legge den til i NewPopulations her.
                    if(CurrentCountry == 0){
                        NewPopulations.Add(new List<Person>());
                    }

                    //Hvis vi er på siste land. Legg til alle person i MutablePopulation
                    //som personer i dette landet for neste dag.
                    //Dette for å ta hånd om feil i avrunding
                    if(NewCountry == _Countries.Count() - 1
                        && NumberOfPersonsToMove[NewCountry] > MutablePopulation.Count()){
                            NewPopulations[NewCountry].AddRange(MutablePopulation);
                        }
                    //Ellers: trekk de n første elementene fra MutablePopulation og legg dem til 
                    //i liste over personer i NewCountry fra neste dag. Fjern deretter disse
                    //personene fra MutablePopulation.
                    else{
                        NewPopulations[NewCountry].AddRange(MutablePopulation.Take(NumberOfPersonsToMove[NewCountry]));
                        MutablePopulation = MutablePopulation.Skip(NumberOfPersonsToMove[NewCountry]).ToList();
                    }
                }
            }

            //Etter at vi har gjort alle flytt vi skal, oppdater liste over personer i samtlige land.
            for(var CountryIndex = 0; CountryIndex < _Countries.Count(); CountryIndex++){
                _Countries[CountryIndex].Borgere = NewPopulations[CountryIndex];
            }
        }

        //Gå 1 dag fram i tid i modellen.
        public bool Progress(){

            //Flytt personer mellom land.
            MovePersons();

            //I hvert land, infiser personer. 
            //Skriv ut info om gjeldende tilstand til kommandolinje
            foreach(var Land in _Countries){
                Land.Infect();
                Console.WriteLine($"{Land.Name}: {Land.Smittede} smittet, {Land.Borgere.Where(b => b.ErSmittsom).Count()} smittsomme. {Land.Borgere.Count()} borgere.");
            }

            //Dersom det forstatt finnes personer som kan smitte nå eller i framtiden
            //gir det mening å fortsette å kjøre simuleringen. I motsatt fall kan vi avslutte.
            return _Countries.Any(c => c.Borgere.Any(b => !(b.ErFriskmeldt || !b.ErSmittet)));
        }

        //Skriv resultater til fil. Vi bruker 1 fil for hvert land, 1 linje for hver dag.
        public void WriteResultsForDay(string ResultFolderName, int DayNumber){
            //Hvis dette er første dag i modellen, overskriv innhold i eventuelle eksisterende filer.
            if(DayNumber == 1){
                foreach(var Land in _Countries){
                    File.WriteAllText(ResultFolderName + "/" + Land.Name + ".csv",
                        $"Dag, Antall_smittede, Antall smittsomme, Antall_borgere, Eksponeringer_per_dag, Infeksjonsrate\r\n" +
                        $"{DayNumber}, {Land.Smittede}, {Land.Smittsomme}, {Land.AntallBorgere}, {Land._EksponeringerPerDag}, {Land._InfectionRate.ToString("G", CultureInfo.InvariantCulture)}\r\n");
                }
            }
            //Hvis vi ikke er på dag 1 i modellen: legg til data for gjeldende dag i filer.
            else{
                foreach(var Land in _Countries){
                    using (StreamWriter sw = File.AppendText(ResultFolderName + "/" + Land.Name + ".csv")) 
                    {
                        sw.WriteLine($"{DayNumber}, {Land.Smittede}, {Land.Smittsomme}, {Land.AntallBorgere}, {Land._EksponeringerPerDag}, {Land._InfectionRate.ToString("G", CultureInfo.InvariantCulture)}");
                    }
                }
            }
        }

        public void MakeResultPlots(string ResultFolderName){

            foreach(var country in _Countries){
                int counter = 0;  
                string line;  
    
                // Read the file and display it line by line.  
                System.IO.StreamReader file =
                    new System.IO.StreamReader(ResultFolderName + "/" + country.Name + ".csv");

                var data = new List<List<double>>();
                var dataHeadings = new List<string>();

                while((line = file.ReadLine()) != null)  
                {  
                    var values = line.Split(",");

                    if(counter == 0){
                        dataHeadings = values.ToList();
                    }

                    for(int i = 0; i < values.Count(); i++){
                        if(counter == 0){
                            data.Add(new List<double>());
                        }
                        else{
                            //Konverter til tall (Fikser ogs formatering slik at dette fungerer)
                            data[i].Add(Convert.ToDouble(values[i].Replace(".", ",")));
                        }

                    }
                    counter++;  
                }
    
                file.Close();

                

                double[] xAxisData = data[0].ToArray();
                double[] totalInfectedData = data[1].ToArray();
                double[] infectionusData = data[2].ToArray();
                double[] totalPopulation = data[3].ToArray();

                var derivative = new double[totalInfectedData.Count()];

                for(int i = 0; i < totalInfectedData.Count(); i++){
                    if(i == 0){
                        derivative[i] = 0;
                        continue;
                    }
                    derivative[i] = totalInfectedData[i] - totalInfectedData[i-1];
                }

                var plt = new ScottPlot.Plot(1200, 800);
                plt.PlotScatter(xAxisData, totalInfectedData);
                plt.PlotScatter(xAxisData, infectionusData);
                plt.PlotScatter(xAxisData, derivative);
                plt.PlotScatter(xAxisData, totalPopulation);
                plt.SaveFig(ResultFolderName + "/" + country.Name + ".png");
            }
        }
    }
}