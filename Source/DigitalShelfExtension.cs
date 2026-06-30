using Verse;

namespace DigitalStorage
{
    // Marks a storage building as a powered digital shelf and sets how much reading
    // bonus each stored book contributes (0.2 matches a vanilla bookcase shelf cell).
    public class DigitalShelfExtension : DefModExtension
    {
        public float readingBonusPerBook;
    }
}
