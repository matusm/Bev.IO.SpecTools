namespace Bev.IO.PerkinElmerSP
{
    public enum BlockCodes : short
    {
        DSet2DC1DI = 120,               // Blocks
        HistoryRecord = 121,            // Blocks
        InstrHdrHistoryRecord = 122,    // Blocks
        InstrumentHeader = 123,         // Blocks
        IRInstrumentHeader = 124,       // Blocks
        UVInstrumentHeader = 125,       // Blocks
        FLInstrumentHeader = 126,       // Blocks

        DataSetDataType = -29839,       // Members
        DataSetAbscissaRange = -29838,  // Members
        DataSetOrdinateRange = -29837,  // Members
        DataSetInterval = -29836,       // Members
        DataSetNumPoints = -29835,      // Members
        DataSetSamplingMethod = -29834, // Members
        DataSetXAxisLabel = -29833,     // Members
        DataSetYAxisLabel = -29832,     // Members
        DataSetXAxisUnitType = -29831,  // Members
        DataSetYAxisUnitType = -29830,  // Members
        DataSetFileType = -29829,       // Members
        DataSetData = -29828,           // Members
        DataSetName = -29827,           // Members
        DataSetChecksum = -29826,       // Members
        DataSetHistoryRecord = -29825,  // Members
        DataSetInvalidRegion = -29824,  // Members
        DataSetAlias = -29823,          // Members
        DataSetVXIRAccyHdr = -29822,    // Members
        DataSetVXIRQualHdr = -29821,    // Members
        DataSetEventMarkers = -29820,   // Members
        
        Short = 29999,                  // TypeCodes
        UShort = 29998,                 // TypeCodes
        Int = 29997,                    // TypeCodes
        UInt = 29996,                   // TypeCodes
        Long = 29995,                   // TypeCodes
        Bool = 29988,                   // TypeCodes
        Char = 29987,                   // TypeCodes
        CvCoOrdPoint = 29986,           // TypeCodes
        StdFont = 29985,                // TypeCodes
        CvCoOrdDimension = 29984,       // TypeCodes
        CvCoOrdRectangle = 29983,       // TypeCodes
        RGBColor = 29982,               // TypeCodes
        CvCoOrdRange = 29981,           // TypeCodes
        Double = 29980,                 // TypeCodes
        CvCoOrd = 29979,                // TypeCodes
        ULong = 29978,                  // TypeCodes
        Peak = 29977,                   // TypeCodes
        CoOrd = 29976,                  // TypeCodes
        Range = 29975,                  // TypeCodes
        CvCoOrdArray = 29974,           // TypeCodes
        Enum = 29973,                   // TypeCodes
        LogFont = 29972                 // TypeCodes
    }
}
