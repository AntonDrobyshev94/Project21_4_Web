namespace Project19.Entitys
{
    /// <summary>
    /// Анемичная модель
    /// </summary>
    /// </summary>
    public class Contact
    {
        public int ID { get; set; }
        public required string Surname { get; set; }
        public required string Name { get; set; }
        public required string FatherName { get; set; }
        public required string TelephoneNumber { get; set; }
        public required string ResidenceAdress { get; set; }
        public required string Description { get; set; }
    }
}
