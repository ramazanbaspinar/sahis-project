namespace Orion.Models
{
    public class FamilyTreeDto
    {
        public PersonDto Person { get; set; } = new();
        public List<PersonDto> Parents { get; set; } = new();
        public List<PersonDto> Children { get; set; } = new();
        public List<PersonDto> Siblings { get; set; } = new();
    }
}
