using SlackAPI;

namespace DependencyInjectionWorkshop.Models
{
    public class SlackAdapter
    {
        public void PushMessage()
        {
            #region 登入失敗發送slack通知
            var slackClient = new SlackClient("my api token");
            slackClient.PostMessage(slackResponse => { }, "my channel", "my message", "my bot name");
            #endregion
        }
    }
}