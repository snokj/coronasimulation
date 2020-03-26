using System;
using System.Linq;
using System.Collections.Generic;


namespace CoronaSimulation
{
    public class Person
    {
        //Er personen smittet (/har personen vært smittet)
        public bool ErSmittet; 

        //Dager siden personen ble smittet
        public int DagerSidenSmitte; 

        //Antall dager fra smitte til personen blir smittsom
        public int _Incubation;

        //Antall dager fra personen blir smittsom til personen blir frisk
        public int _Recovery;

        //Sjekk om person er ferdig med sykdomsforløpet
        public Boolean ErFriskmeldt {get => DagerSidenSmitte > _Incubation + _Recovery;}

        //Sjekk om personen er smittet og under inkubering
        public Boolean ErInkubering {get => DagerSidenSmitte < _Incubation;}

        //Sjekk om person er smittsom
        public Boolean ErSmittsom 
        {
            get => DagerSidenSmitte >= _Incubation && DagerSidenSmitte <= _Incubation + _Recovery;
        }

        //Opprett en person og sett alle verdier som må være satt.
        public Person(int Incubation, int Recovery, bool Healthy)
        {
            _Incubation = Incubation;
            _Recovery = Recovery;
            ErSmittet = Healthy;
            DagerSidenSmitte = 0;
        }
    }
}