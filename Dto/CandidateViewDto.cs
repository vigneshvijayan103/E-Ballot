namespace EBallotApi.Dto
{
    public class CandidateViewDto
    {
        public int CandidateId { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public string Gender { get; set; }
        public string PartyName { get; set; }
        public string Symbol { get; set; }
        public string Manifesto { get; set; }
        public string Aadhar { get; set; }           
        public string PhoneNumber { get; set; }      
        public string Photo { get; set; }
        public string ConstituencyName { get; set; }
        public string ElectionName { get; set; }
    }
}
