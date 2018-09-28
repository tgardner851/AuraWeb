﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AuraWeb.Models
{
    /* DEPRECATE START */
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
    /* DEPRECATE END */

    #region Data Models
    public class Position
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

    public class Security
    {
        public double Status { get; set; }
        public string Class { get; set; }
    }

    public class Dock
    {
        public double EntryX { get; set; }
        public double EntryY { get; set; }
        public double EntryZ { get; set; }
        public double OrientationX { get; set; }
        public double OrientationY { get; set; }
        public double OrientationZ { get; set; }
    }

    public class Icon
    {
        public int Id { get; set; }
        public string File { get; set; }
        public string Description { get; set; }
    }

    public class Unit
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
    }

    public class Race
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Icon Icon { get; set; }
        public string ShortDescription { get; set; }
    }

    public class MarketGroup
    {
        public int Id { get; set; }
        public int? ParentId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Icon Icon { get; set; }
        public int HasTypes { get; set; }
    }

    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Icon Icon { get; set; }
        public int Published { get; set; }
    }

    public class Group
    {
        public int Id { get; set; }
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
        public int ParentTypeId { get; set; }
        public MetaGroup MetaGroup { get; set; }
    }

    public class MetaGroup
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Icon Icon { get; set; }
    }

    public class Contraband
    {
        public int FactionId { get; set; }
        public double StandingLoss { get; set; }
        public double ConfiscateMinSec { get; set; }
        public double ContrabandFineByValue { get; set; }
        public double AttackMinSec { get; set; }
    }

    public class Trait
    {
        public int Id { get; set; }
        public int SkillId { get; set; }
        public double Bonus { get; set; }
        public string BonusText { get; set; }
        public Unit Unit { get; set; }
    }

    public class Region_V_Row
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Position Position { get; set; }
        public int FactionId { get; set; }
        public string FactionName { get; set; }
        public double Radius { get; set; }
    }

    public class Constellation_V_Row
    {
        public int Id { get; set; }
        public int RegionId { get; set; }
        public string RegionName { get; set; }
        public string Name { get; set; }
        public Position Position { get; set; }
        public int FactionId { get; set; }
        public string FactionName { get; set; }
        public double Radius { get; set; }
    }

    public class SolarSystem_V_Row
    {
        public int Id { get; set; }
        public int RegionId { get; set; }
        public string RegionName { get; set; }
        public int ConstellationId { get; set; }
        public string ConstellationName { get; set; }
        public string Name { get; set; }
        public Position Position { get; set; }
        public double Luminosity { get; set; }
        public int Border { get; set; }
        public int Fringe { get; set; }
        public int Corridor { get; set; }
        public int Hub { get; set; }
        public int International { get; set; }
        public int Regional { get; set; }
        public Security Security { get; set; }
        public int FactionId { get; set; }
        public string FactionName { get; set; }
        public double Radius { get; set; }
        public int SunTypeId { get; set; }
    }

    public class Station_V_Row
    {
        public int Id { get; set; }
        public int SolarSystemId { get; set; }
        public string SolarSystemName { get; set; }
        public int ConstellationId { get; set; }
        public string ConstellationName { get; set; }
        public int RegionId { get; set; }
        public string RegionName { get; set; }
        public string Name { get; set; }
        public Position Position { get; set; }
        public Security Security { get; set; }
        public double DockingCostPerVolume { get; set; }
        public double MaxShipVolumeDockable { get; set; }
        public int OfficeSlots { get; set; }
        public double OfficeRentalCost { get; set; }
        public double ReprocessingEfficiency { get; set; }
        public double ReprocessingStationsTake { get; set; }
        public int ReprocessingHangarFlag { get; set; }
        public int OperationId { get; set; }
        public int OperationActivityId { get; set; }
        public string OperationName { get; set; }
        public string OperationDescription { get; set; }
        public int StationTypeId { get; set; }
        public Dock Dock { get; set; }
        public int Conquerable { get; set; }
        public int CorporationId { get; set; }
    }

    public class ItemType_V_Row
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public double? Mass { get; set; }
        public double? Volume { get; set; }
        public double? Capacity { get; set; }
        public int? PortionSize { get; set; }
        public Race Race { get; set; }
        public double? BasePrice { get; set; }
        public int Published { get; set; }
        public MarketGroup MarketGroup { get; set; }
        public Icon Icon { get; set; }
        public int? SoundId { get; set; }
        public int? GraphicId { get; set; }
        public Group Group { get; set; }
        public Meta Meta { get; set; }
        public Contraband Contraband { get; set; }
    }

    public class Certificate
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Group Group { get; set; }
    }

    public class Skill_V_Row
    {
        public int Id { get; set; }
        public int SkillLevelInt { get; set; }
        public int SkillCertLevel { get; set; }
        public string SkillCertLevelText { get; set; }
        public Certificate Cert { get; set; }
    }
    #endregion
}
