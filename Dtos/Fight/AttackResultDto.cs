namespace dotnet_rpg.Dtos.Fight
{
    public class AttackResultDto
    {

        public string Attacker { get; set; }
        public string Opponent { get; set; }
        public int AttackerHitPoint { get; set; }
        public int OpponentHitPoints { get; set; }
        public int Damage { get; set; }

    }
}