namespace OpenTelmetry.Api
{
    public class LabelSet
    {
        static public readonly string[] empty_keyvalues = {};
        static public LabelSet Empty { get; } = new LabelSet();
        
        private string[] keyvalues;

        public LabelSet()
        {
            this.keyvalues = empty_keyvalues;
        }

        public LabelSet(string[] keyvalues)
        {
            this.keyvalues = keyvalues;
        }

        /// <summary>
        /// Returns KeyValue pairs in a flatten string array. 
        /// (i.e. [key1, value1, key2, value2, key3, value3 ])
        /// </summary>
        public virtual string[] GetKeyValues()
        {
            return keyvalues;
        }
    }
}