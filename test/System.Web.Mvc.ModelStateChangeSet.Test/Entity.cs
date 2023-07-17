namespace System.Web.Mvc.ModelStateChangeSet.Test
{
    class Address
    {
        public int Id { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
    }

    class Account
    {
        public int Id { get; set; }
        public int AddressId { get; set; }
        public string Name { get; set; }
        public string Number { get; set; }
        public Address Address { get; set; }
    }

    class Entity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Account Account { get; set; }
    }
}
