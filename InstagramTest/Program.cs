using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace InstagramTest
{
    class Program
    {
        private const string USERID = "_4021708041";
        private const string TOKEN = "4021708041.f154427.1484a14dab9b4a759ed54c4cf615bf0e";

        public class DataObject
        {
            public string Name { get; set; }
        }


        static void Main(string[] args)
        {
            string recentMedia = GetAllRecentMedia();
            List<string> allMediaIds = GetPostsFromResponse(recentMedia);
            List<InstagramPost> instragramPosts = ConvertJsonResponseInObjects(allMediaIds);
        }

        private static List<InstagramPost> ConvertJsonResponseInObjects(List<string> allMediaIds)
        {
            List<InstagramPost> posts = new List<InstagramPost>();
            foreach (string post in allMediaIds)
            {
                string onePost = GetOnePost(post);
                InstagramPost instaPost = new InstagramPost(onePost);
                posts.Add(instaPost);
            }
            return posts;
        }

        private static List<string> GetPostsFromResponse(string result)
        {
            List<string> listIds = new List<string>();
            string[] resultSplitted = result.Split(',');

            for (int i = 0; i < resultSplitted.Length; i++)
            {
                if (resultSplitted[i].Contains(USERID))
                {
                    string postId = resultSplitted[i].Substring(resultSplitted[i].IndexOf("id") + 6, 30);
                    listIds.Add(postId);
                }
            }
            return listIds;
        }

        public class InstagramPost
        {
            public InstagramPost(string json)
            {
                JObject jObject = JObject.Parse(json);
                JToken jInstaPost = jObject["data"];
                id = (string)jInstaPost["id"];
                created_time = (string)jInstaPost["created_time"];
                user = new InstagramUser(jInstaPost["user"]);
                images = new InstagramImages(jInstaPost["images"]);
                caption = new InstagramCaption(jInstaPost["caption"]);
                likes = new InstagramLikes(jInstaPost["likes"]);
                link = (string)jInstaPost["link"];
            }
            public string id { get; set; }
            public string created_time { get; set; }
            public string link { get; set; }
            public InstagramUser user { get; set; }
            public InstagramImages images { get; set; }
            public InstagramCaption caption { get; set; }
            public InstagramLikes likes { get; set; }
        }

        public class InstagramUser
        {
            public string id { get; set; }
            public string full_name { get; set; }
            public string profile_picture { get; set; }
            public string username { get; set; }

            public InstagramUser(JToken jToken)
            {
                if (jToken.HasValues)
                {
                    id = (string)jToken["id"];
                    full_name = (string)jToken["full_name"];
                    profile_picture = (string)jToken["profile_picture"];
                    username = (string)jToken["username"];
                }
            }
        }

        public class InstagramLikes
        {
            public int count { get; set; }

            public InstagramLikes(JToken jToken)
            {
                count = 0;
                if (jToken.HasValues)
                {
                    count = (int)jToken["count"];
                }
            }
        }

        public class InstagramCaption
        {
            public string id { get; set; }
            public string text { get; set; }

            public InstagramCaption(JToken jToken)
            {
                id = string.Empty;
                text = string.Empty;
                if (jToken.HasValues)
                {
                    id = (string)jToken["id"];
                    text = FormatText((string)jToken["text"]);
                }
            }

            private string FormatText(string text)
            {
                string[] textSplitted = text.Split(' ');

                for (int i = 0; i < textSplitted.Length; i++)
                {
                    if (textSplitted[i].Contains('#'))
                    {
                        if (textSplitted[i].Contains(".\n"))
                        {
                            string link = textSplitted[i].Replace(".\n", "");
                            string href = "<a href=\"https://www.instagram.com/explore/tags/" + link.Replace('#', ' ').Trim().ToLower() + "/\"" + " target =\"_blank\" >" + textSplitted[i] + "</a>";
                            textSplitted[i] = href;
                        }
                        else
                        {
                            string href = "<a href=\"https://www.instagram.com/explore/tags/" + textSplitted[i].Replace('#', ' ').Trim().ToLower() + "/\"" + " target =\"_blank\" >" + textSplitted[i] + "</a>";
                            textSplitted[i] = href;
                        }
                    }
                    if (textSplitted[i].Contains('@'))
                    {
                        if (textSplitted[i].Contains(')'))
                        {
                            string link1 = textSplitted[i].Substring(textSplitted[i].IndexOf('@'), textSplitted[i].IndexOf(')') - 1);
                            string link2 = textSplitted[i].Substring(textSplitted[i].IndexOf(')') + 1);
                            string href = "<a href=\"https://www.instagram.com/" + link1.Replace('@', ' ').Trim().ToLower() + "/\"" + " target =\"_blank\" >(" + link1 + ")</a>" + link2;
                            href.Substring(0, href.LastIndexOf('\n'));
                            textSplitted[i] = href;
                        }
                        else
                        {
                            string href = "<a href=\"https://www.instagram.com/" + textSplitted[i].Replace('@', ' ').Trim().ToLower() + "/\"" + " target =\"_blank\" >" + textSplitted[i] + "</a>";
                            textSplitted[i] = href;
                        }
                    }

                    textSplitted[i] = textSplitted[i].Replace("\n", "<br>");
                }

                string result = string.Empty;
                for (int i = 0; i < textSplitted.Length; i++)
                {
                    result += textSplitted[i] + " ";
                }

                return result;
            }
        }

        public class InstagramImages
        {
            public Thumbnail thumbnail { get; set; }
            public LowRes lowRes { get; set; }
            public StandardRes standardRes { get; set; }

            public InstagramImages(JToken jToken)
            {
                if (jToken.HasValues)
                {
                    thumbnail = new Thumbnail(jToken["thumbnail"]);
                    lowRes = new LowRes(jToken["low_resolution"]);
                    standardRes = new StandardRes(jToken["standard_resolution"]);
                }
            }
        }

        public class InstagramImage
        {
            public int width { get; set; }
            public int height { get; set; }
            public string url { get; set; }
        }
        public class Thumbnail : InstagramImage
        {
            public Thumbnail(JToken jToken)
            {
                width = (int)jToken["width"];
                height = (int)jToken["height"];
                url = (string)jToken["url"];
            }
        }
        public class LowRes : InstagramImage
        {
            public LowRes(JToken jToken)
            {
                if (jToken.HasValues)
                {
                    width = (int)jToken["width"];
                    height = (int)jToken["height"];
                    url = (string)jToken["url"];
                }
            }
        }
        public class StandardRes : InstagramImage
        {
            public StandardRes(JToken jToken)
            {
                if (jToken.HasValues)
                {
                    width = (int)jToken["width"];
                    height = (int)jToken["height"];
                    url = (string)jToken["url"];
                }
            }
        }
        

        private static string GetOnePost(string postId)
        {
            string URL = "https://api.instagram.com/v1/media/" + postId + "?access_token=" + TOKEN;
            string result = string.Empty;
            result = MakeRequest(URL);
            return result;
        }

        private static string GetAllRecentMedia()
        {
            const string URL = "https://api.instagram.com/v1/users/4021708041/media/recent/?access_token=" + TOKEN;
            string result = string.Empty;
            result = MakeRequest(URL);
            return result;
        }

        private static string MakeRequest(string URL)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL);
            try
            {
                WebResponse response = request.GetResponse();
                using (Stream responseStream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
                    return reader.ReadToEnd();
                }
            }
            catch (WebException ex)
            {
                WebResponse errorResponse = ex.Response;
                using (Stream responseStream = errorResponse.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(responseStream, Encoding.GetEncoding("utf-8"));
                    String errorText = reader.ReadToEnd();
                }
                throw;
            }
        }
    }
}
