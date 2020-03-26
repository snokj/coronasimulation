namespace CoronaSimulation{
    public class PreventiveMeasure
    {
        public bool UseInfectionPercentage {get => ControlMeasureInfectionPercentageStart != null;}
        public double? ControlMeasureInfectionPercentageStart;
        bool UseRateOfChangeInfection {get => ControlMeasureRateOfChangeStart != null;}
        double? ControlMeasureRateOfChangeStart;
        double? ControlMeasureRateOfChangeEnd;
        
        public int? ReducedEncountersPerDay;
        public double? ReducedInfectionRate;

        public PreventiveMeasure(
            double? controlMeasureInfectionPercentageStart,
            int? reducedEncountersPerDay,
            double? reducedInfectionRate
        )
        {
            ControlMeasureInfectionPercentageStart = controlMeasureInfectionPercentageStart;
            ReducedEncountersPerDay = reducedEncountersPerDay;
            ReducedInfectionRate = reducedInfectionRate;
        }

        // //Semi-major implementation changes to move
        // bool DisallowTravel
        // //Semi-major implementation changes to move
        // bool DisallowEntryForSickPersons
        // bool CheckIncubationAtEntry
        // double FalseNegativeChanceIncubationAtEntry
        // double FalsePositiveChanceIncubationAtEntry
    }
}