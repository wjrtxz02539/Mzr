using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Mzr.Share.Interfaces.Bilibili.Raw;
using Mzr.Share.Models.Bilibili;
using Mzr.Share.Models.Bilibili.Raw;
using System.Text.Json.Nodes;

namespace Mzr.Share.Repositories.Bilibili.Raw
{
    public class RawBiliDynamicRepository : RawBiliRepository<RawBiliDynamic>, IRawBiliDynamicRepository
    {

        public RawBiliDynamicRepository(ILogger<RawBiliDynamicRepository> logger, IHost host) : base(logger, host)
        {
        }

        public async Task<BiliDynamic?> FromCardDocument(RawBiliDynamicDataCard cardDocument)
        {
            var dynamicType = cardDocument.Desc.Type;
            switch (dynamicType)
            {
                case 1:
                    return await TransformRetweet(cardDocument);
                case 2:
                    return await TransformNormal(cardDocument);
                case 4:
                    return await TransformNormal2(cardDocument);
                case 8:
                    return await TransformVideo(cardDocument);
                case 64:
                    return await TransformReport(cardDocument);
                case 256:
                    return await TransformMusic(cardDocument);
                case 2048:
                    return await TransformTopic(cardDocument);
                default:
                    Logger.LogError("Unknown dynamic type {type} for {dynamicId}.", dynamicType, cardDocument.Desc?.DynamicId);
                    return null;
            }
        }
        public async Task<BiliDynamic?> GetBiliDynamicAsync(RawBiliDynamic raw)
        {
            return await FromCardDocument(raw.Data.Card);

        }

        private static async Task<BiliDynamic> TransformBase(RawBiliDynamicDataCardDesc desc)
        {
            await Task.Yield();
            return new BiliDynamic()
            {
                DynamicId = desc.DynamicId,
                Time = desc.Date,
                UserId = desc.Uid,
                View = desc.View,
                Like = desc.Like
            };
        }

        private static async Task<BiliDynamic> TransformNormal(RawBiliDynamicDataCard card)
        {
            var desc = card.Desc;
            var document = await TransformBase(desc);

            document.DynamicType = 2;
            document.ReportId = Convert.ToInt64(desc.Rid);
            document.ThreadId = document.ReportId;

            if (JsonNode.Parse(card.Card) is JsonNode cardJson
                && cardJson["item"] is JsonNode item
                && item["description"] is JsonNode description)
            {
                document.Description = description.GetValue<string>();
            }

            return document;
        }

        private static async Task<BiliDynamic> TransformNormal2(RawBiliDynamicDataCard card)
        {
            var desc = card.Desc;
            var document = await TransformBase(desc);

            document.DynamicType = 4;
            document.ReportId = Convert.ToInt64(desc.Rid);
            document.ThreadId = document.DynamicId;

            if (JsonNode.Parse(card.Card) is JsonNode cardJson
                && cardJson["item"] is JsonNode item
                && item["content"] is JsonNode content)
            {
                document.Description = content.GetValue<string>();
            }

            return document;
        }

        private static async Task<BiliDynamic> TransformRetweet(RawBiliDynamicDataCard card)
        {
            var desc = card.Desc;
            var document = await TransformBase(desc);

            document.DynamicType = 1;
            document.ReportId = Convert.ToInt64(desc.Rid);
            document.ThreadId = desc.DynamicId;
            document.OriginalDynamicId = desc.OriginalById;
            document.OriginalType = desc.OriginalType;

            if (JsonNode.Parse(card.Card) is JsonNode cardJson
                && cardJson["item"] is JsonNode item
                && item["content"] is JsonNode content)
            {
                document.Description = content.GetValue<string>();
            }

            return document;
        }

        private static async Task<BiliDynamic> TransformVideo(RawBiliDynamicDataCard card)
        {
            var desc = card.Desc;
            var document = await TransformBase(desc);

            document.DynamicType = 8;
            document.ReportId = Convert.ToInt64(desc.Rid);
            document.ThreadId = document.ReportId;
            document.VideoId = desc.ExtensionData?["bvid"].GetString();

            if (JsonNode.Parse(card.Card) is JsonNode cardJson
                && cardJson["desc"] is JsonNode descJson)
            {
                document.Description = descJson.GetValue<string>();
            }

            return document;
        }

        private static async Task<BiliDynamic> TransformReport(RawBiliDynamicDataCard card)
        {
            var desc = card.Desc;
            var document = await TransformBase(desc);

            document.DynamicType = 64;
            document.ReportId = Convert.ToInt64(desc.Rid);
            document.ThreadId = document.ReportId;

            if (JsonNode.Parse(card.Card) is JsonNode cardJson
                && cardJson["summary"] is JsonNode summary)
            {
                document.Description = summary.GetValue<string>();
            }

            return document;
        }

        private static async Task<BiliDynamic> TransformTopic(RawBiliDynamicDataCard card)
        {
            var desc = card.Desc;
            var document = await TransformBase(desc);

            document.DynamicType = 2048;
            document.ReportId = Convert.ToInt64(desc.Rid);
            document.ThreadId = document.DynamicId;

            if (JsonNode.Parse(card.Card) is JsonNode cardJson)
            {
                if (cardJson["vest"] is JsonNode vest && vest["content"] is JsonNode content)
                    document.Description = content.GetValue<string>();
                if (cardJson["sketch"] is JsonNode sketch && sketch["target_url"] is JsonNode target_url)
                    document.TargetUrl = target_url.GetValue<string>();
            }

            return document;
        }

        private static async Task<BiliDynamic> TransformMusic(RawBiliDynamicDataCard card)
        {
            var desc = card.Desc;
            var document = await TransformBase(desc);

            document.DynamicType = 256;
            document.ReportId = Convert.ToInt64(desc.Rid);
            document.ThreadId = document.ReportId;

            if (JsonNode.Parse(card.Card) is JsonNode cardJson
                && cardJson["intro"] is JsonNode intro)
            {
                document.Description = intro.GetValue<string>();
            }

            return document;
        }
    }
}
