namespace StreetLibrary
{
    [Serializable]
    public class Street
    {
        public string Name { get; set; }
        public int ZipCode { get; set; }

        public Street(string name, int zipCode)
        {
            Name = name;
            ZipCode = zipCode;
        }
    }
}