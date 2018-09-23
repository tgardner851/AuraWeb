using EVEStandard.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuraWeb.Models
{
    public class CharacterPageViewModel
    {
        public string CharacterName { get; set; }
        public string CorporationName { get; set; }
        public int CharacterLocationId { get; set; }
        public string CharacterLocationName { get; set; }

        public string CharacterPortrait { get; set; }

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
