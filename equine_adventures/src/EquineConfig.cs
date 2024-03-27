
namespace EquineAdventures {
    public class EquineConfig {
        public string Units = "CUSTOMARY_METRIC";

        public string WeightSuffix() {
            if (Units.Equals("IMPERIAL") || Units.Equals("CUSTOMARY")) {
                return "_lbs";
            }
            if (Units.Equals("METRIC")) {
                return "_kg";
            }
            if (Units.Equals("METRIC_IMPERIAL") || Units.Equals("METRIC_CUSTOMARY")) {
                return "_kg_lbs";
            }
            return "_lbs_kg";
        }
    }
    public enum MeasurementSystem {
        IMPERIAL,
        METRIC,
        METRIC_IMPERIAL,
        IMPERIAL_METRIC
    }
}
