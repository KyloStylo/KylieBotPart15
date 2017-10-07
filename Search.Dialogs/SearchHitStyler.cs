namespace Search.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;
    using Search.Models;

    [Serializable]
    public class SearchHitStyler : PromptStyler
    {
        public override void Apply<T>(ref IMessageActivity message, string prompt, IReadOnlyList<T> options, IReadOnlyList<string> descriptions = null, string speak = null)
        {
            var hits = options as IList<SearchHit>;
            if (hits != null)
            {
                var cards = hits.Select(h => new ThumbnailCard
                {
                    Title = h.Title,
                    Images = new[] { new CardImage("https://static.wixstatic.com/media/85fb2e_f0bfb249c6044df189ed617533f21a84~mv2.png/v1/fill/w_120,h_189,al_c/85fb2e_f0bfb249c6044df189ed617533f21a84~mv2.png") },
                    Buttons = new[] { new CardAction(ActionTypes.OpenUrl, "More details", value: h.SourceLink) },
                    Text =
                    "**Product**: " + h.Product + "\n\n" +
                    "**Version**: " + h.Version + "\n\n" +
                    "**ArticleDate**: " + h.ArticleDate + "\n\n" +
                    "**Category**: " + h.Category + "\n\n" +
                    "**Rating**: " + h.Rating.ToString() + "\n\n" +
                    "**Source**: " + h.Source + "\n\n"
                });

                message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                message.Attachments = cards.Select(c => c.ToAttachment()).ToList();
                message.Text = prompt;
                message.Speak = speak;
            }
            else
            {
                base.Apply<T>(ref message, prompt, options, descriptions, speak);
            }
        }
    }
}