using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuraWeb.Models
{
    #region DTOs
    public class TypeNameDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class PositionDTO
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public double XMin { get; set; }
        public double XMax { get; set; }
        public double YMin { get; set; }
        public double YMax { get; set; }
        public double ZMin { get; set; }
        public double ZMax { get; set; }
    }

    public class SecurityDTO
    {
        public double Security { get; set; }
        public string SecurityClass { get; set; }
    }

    public class SolarSystemDTO
    {
        public int Id { get; set; }
        public int RegionId { get; set; }
        public string RegionName { get; set; }
        public int ConstellationId { get; set; }
        public string ConstellationName { get; set; }
        public string Name { get; set; }
        public PositionDTO Position { get; set; }
        public double Luminosity { get; set; }
        public bool IsBorder { get; set; }
        public bool IsFringe { get; set; }
        public bool IsCorridor { get; set; }
        public bool IsHub { get; set; }
        public bool IsInternational { get; set; }
        public bool IsRegional { get; set; }
        public SecurityDTO Security { get; set; }
        public int FactionId { get; set; }
        public string FactionName { get; set; }
        public double Radius { get; set; }
        public int SunTypeId { get; set; }
    }
    #endregion

    #region Other
    public class Icon
    {
        public int ID { get; set; }
        public string File { get; set; }
        public string Description { get; set; }
    }

    public class Unit
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
    }

    public class ItemType
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public double Mass { get; set; }
        public double Volume { get; set; }
        public double Capacity { get; set; }
        public int PortionSize { get; set; }
        public Race Race { get; set; }
        public double? BasePrice { get; set; }
        public int Published { get; set; }
        public MarketGroup MarketGroup { get; set; }
        public Icon Icon { get; set; }
        public int SoundID { get; set; }
        public int GraphicID { get; set; }
        public Group Group { get; set; }
        public Meta Meta { get; set; }
        public Contraband Contraband { get; set; }
        public List<Trait> Trait { get; set; }
    }

    public class Race
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Icon Icon { get; set; }
        public string ShortDescription { get; set; }
    }

    public class MarketGroup
    {
        //public int ID { get; set; }
        public int? ParentID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Icon Icon { get; set; }
        public int HasTypes { get; set; }
    }

    public class Category
    {
        //public int ID { get; set; }
        public string Name { get; set; }
        public Icon Icon { get; set; }
        public int Published { get; set; }
    }

    public class Group
    {
        //public int ID { get; set; }
        public string Name { get; set; }
        public Icon Icon { get; set; }
        public int UseBasePrice { get; set; }
        public int Anchored { get; set; }
        public int Anchorable { get; set; }
        public int FittableNonSingleton { get; set; }
        public int Published { get; set; }
        public Category Category { get; set; }
    }

    public class Meta
    {
        public int ParentTypeID { get; set; }
        public MetaGroup MetaGroup { get; set; }
    }

    public class MetaGroup
    {
        //public int ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Icon Icon { get; set; }
    }

    public class Contraband
    {
        public int FactionID { get; set; }
        public double StandingLoss { get; set; }
        public double ConfiscateMinSec { get; set; }
        public double ContrabandFineByValue { get; set; }
        public double AttackMinSec { get; set; }
    }

    public class Trait
    {
        //public int ID { get; set; }
        public int SkillID { get; set; }
        public double Bonus { get; set; }
        public string BonusText { get; set; }
        public Unit Unit { get; set; }
    }
    #endregion
}
