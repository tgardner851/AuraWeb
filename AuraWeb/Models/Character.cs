using EVEStandard.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuraWeb.Models
{
    public class CharacterPageViewModel
    {
        public int Id { get; set; }
        public CharacterInfo Character { get; set; }
        public EVEStandard.Models.Icons Portrait { get; set; }
        public CorporationInfo Corporation { get; set; }

        public int LocationSystemId { get; set; }
        public EVEStandard.Models.System LocationSystem { get; set; }

        public Fatigue CharacterJumpFatigue { get; set; }

        public List<SkillQueueDataModel> SkillsQueue { get; set; }
    }

    public class CharacterKillsLossesViewModel
    {
        public List<Killmail> KillMails { get; set; }
    }

    public class BookmarkDataModel
    {
        public long Id { get; set; }
        public EVEStandard.Models.Position Coordinates { get; set; }
        public DateTime Created { get; set; }
        public long? FolderId { get; set; }
        public int ItemTypeId { get; set; }
        public string ItemTypeName { get; set; }
        public string Label { get; set; }
        public long LocationId { get; set; }
        public string LocationName { get; set; }
        public string Notes { get; set; }

    }

    public class CharacterBookmarkDataModel
    {
        public BookmarkFolder Folder { get; set; }
        public List<BookmarkDataModel> Bookmarks { get; set; }
    }

    public class CharacterBookmarksViewModel
    {
        public List<CharacterBookmarkDataModel> BookmarkFolders { get; set; }
    }

    public class CharacterFleetViewModel
    {
        public bool CharacterInFleet { get; set; }
        public CharacterFleetInfo Fleet { get; set; }
    }

    public class CharacterSkillsViewModel
    {
        public List<SkillQueueDataModel> SkillQueue { get; set; }
        public SkillFinishedDataModel Skills { get; set; }
    }

    public class SkillQueueDataModel
    {
        public int Sequence { get; set; }
        public Skill_V_Row Skill { get; set; }
        public SkillQueue Skill_API { get; set; }
    }

    public class SkillFinishedDataModel
    {
        public long TotalSp { get; set; }
        public int? UnallocatedSp { get; set; }
        public List<SkillFinishedSkillDataModel> Skills { get; set; }
    }

    public class SkillFinishedSkillDataModel
    {
        public Skill_V_Row Skill { get; set; }
        public Skill Skill_API { get; set; }
    }

    public class CharacterDataModel
    {
        public int Id { get; set; }
        public EVEStandard.Models.CharacterInfo Character { get; set; }
    }

    public class AssetDataModel
    {
        public Asset Asset_API { get; set; }
        public ItemType_V_Row ItemType { get; set; }
        public SolarSystem_V_Row System { get; set; }
        public Station_V_Row Station { get; set; }
    }

    public class AssetsPageViewModel
    {
        public List<AssetDataModel> Assets { get; set; }
    }

    public class WalletPageViewModel
    {
        public List<CharacterWalletJournal> Journal { get; set; }
        public List<WalletTransaction> Transactions { get; set; }
    }
}
