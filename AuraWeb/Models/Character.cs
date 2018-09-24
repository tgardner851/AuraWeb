using EVEStandard.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuraWeb.Models
{
    public class CharacterPageViewModel
    {

        public CharacterInfo Character { get; set; }
        public EVEStandard.Models.Icons Portrait { get; set; }
        public CorporationInfo Corporation { get; set; }


        public int LocationSystemId { get; set; }
        public EVEStandard.Models.System LocationSystem { get; set; }

        

        public Fatigue CharacterJumpFatigue { get; set; }
    }

    public class CharacterKillsLossesViewModel
    {
        public List<Killmail> KillMails { get; set; }
    }

    public class CharacterBookmarkDataModel
    {
        public BookmarkFolder Folder { get; set; }
        public List<Bookmark> Bookmarks { get; set; }
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
        public List<SkillQueue> SkillQueue { get; set; }
        public CharacterSkills Skills { get; set; }
    }
}
