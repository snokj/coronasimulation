namespace CoronaSimulation
{
    //Definerer metoder som vi forventer at er implementert for alle modeller av "verden".
    interface IWorld
    {
        void MovePersons();
        bool Progress();
        void WriteResultsForDay(string ResultFolderName, int DayNumber);

        double GetTotalPercentageInfected();

        bool UpdateEksponeringerOgInfectionRateForNamedCountry(string CountryName, int EksponeringerPerDag, double InfectionRate);

        void MakeResultPlots(string ResultsFolderName);
    }
}