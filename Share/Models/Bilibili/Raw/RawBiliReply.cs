using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Mzr.Share.Models.Bilibili.Raw
{
    public class RawBiliReply
    {
        [JsonPropertyName("content")]
        public RawBiliReplyContent Content { get; set; } = null!;

        [JsonPropertyName("count")]
        public int Count { get; set; }

        [JsonPropertyName("ctime")]
        public long Timestamp { get; set; }

        [JsonPropertyName("dialog")]
        public long Dialog { get; set; }

        [JsonPropertyName("folder")]
        public RawBiliReplyFolder Folder { get; set; } = null!;

        [JsonPropertyName("invisible")]
        public bool Invisible { get; set; }

        [JsonPropertyName("like")]
        public int Like { get; set; }

        [JsonPropertyName("member")]
        public RawBiliReplyMember Member { get; set; } = null!;

        [JsonPropertyName("oid")]
        public long ThreadId { get; set; }

        [JsonPropertyName("parent")]
        public long Parent { get; set; }

        [JsonPropertyName("rpid")]
        public long ReplyId { get; set; }

        [JsonPropertyName("root")]
        public long Root { get; set; }

        [JsonPropertyName("floor")]
        public int Floor { get; set; } = -1;

        [JsonPropertyName("rcount")]
        public int ReplyCount { get; set; }

        [JsonPropertyName("replies")]
        public List<RawBiliReply> Replies { get; set; } = new List<RawBiliReply>();

        [JsonIgnore]
        public DateTime Time { get { return DateTimeOffset.FromUnixTimeSeconds(Timestamp).DateTime; } }


    }

    public class RawBiliReplyContent
    {
        [JsonPropertyName("device")]
        public string Device { get; set; } = null!;

        [JsonPropertyName("message")]
        public string Message { get; set; } = null!;

        [JsonPropertyName("plat")]
        public int Plat { get; set; }
    }

    public class RawBiliReplyFolder
    {
        [JsonPropertyName("has_folded")]
        public bool HasFolded { get; set; }

        [JsonPropertyName("is_folded")]
        public bool IsFolded { get; set; }
    }

    public class RawBiliReplyMember
    {
        [JsonPropertyName("avatar")]
        public string Avatar { get; set; } = null!;

        [JsonPropertyName("level_info")]
        public RawBiliReplyMemberLevelInfo LevelInfo { get; set; } = null!;

        [JsonPropertyName("mid")]
        public string Mid { get; set; } = null!;

        [JsonPropertyName("pendant")]
        public RawBiliReplyMemberPendant Pendant { get; set; } = null!;

        [JsonPropertyName("user_sailing")]
        public RawBiliReplyMemberUserSailing? UserSailing { get; set; }

        [JsonPropertyName("sign")]
        public string Sign { get; set; } = null!;

        [JsonPropertyName("sex")]
        public string Sex { get; set; } = null!;

        [JsonPropertyName("uname")]
        public string Username { get; set; } = null!;

        [JsonPropertyName("vip")]
        public RawBiliReplyMemberVIP VIP { get; set; } = null!;

        [JsonIgnore]
        public long UserId { get { return Convert.ToInt64(Mid); } }

        public int ParsedSex
        {
            get
            {
                switch (Sex)
                {
                    case "女":
                        return 0;
                    case "男":
                        return 1;
                    default:
                        return 2;
                }
            }
        }
    }


    public class RawBiliReplyMemberLevelInfo
    {
        [JsonPropertyName("current_level")]
        public int Level { get; set; }
    }

    public class RawBiliReplyMemberPendant
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = null!;
    }

    public class RawBiliReplyMemberUserSailing
    {
        [JsonPropertyName("cardbg")]
        public RawBiliReplyMemberUserSailingCardBG? CardBG { get; set; }
    }

    public class RawBiliReplyMemberUserSailingCardBG
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = null!;    

        [JsonPropertyName("fan")]
        public RawBiliReplyMemberUserSailingCardBGFan? Fan { get; set; }
    }

    public class RawBiliReplyMemberUserSailingCardBGFan
    {
        [JsonPropertyName("is_fan")]
        public int _isFan { get; set; }

        [JsonPropertyName("number")]
        public int Number { get; set; }

        [JsonIgnore]
        public bool IsFan { get { return _isFan == 1 ? true : false; } }
    }

    public class RawBiliReplyMemberVIP
    {
        [JsonPropertyName("vipType")]
        public int Type { get; set; }
    }
}
